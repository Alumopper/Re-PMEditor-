﻿using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Windows.Documents;
using PMEditor.Util;

namespace PMEditor
{
    public partial class Line
    {
        public bool IsNoteOverLap(Note note)
        {
            if(note is FakeCatch f)
            {
                return fakeCatch.Contains(f);
            }
            if (notes.Contains(note))
            {
                return true;
            }
            else
            {
                //检查note是否重叠
                foreach (var item in notes)
                {
                    if(item.IsOverlap(note))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool ClickOnNote(double time, int rail)
        {
            foreach (var item in notes)
            {
                if (item.Rail != rail) continue;
                if(item.type == NoteType.Hold)
                {
                    if(item.ActualTime < time && time < item.ActualTime + item.ActualHoldTime)
                    {
                        return true;
                    }
                }
                else
                {
                    if(item.ActualTime == time)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool ClickOnFakeCatch(double time, int rail)
        {
            return fakeCatch.Any(item => item.Rail == rail && item.ActualTime == time);
        }

        public bool ClickOnEvent(double time, int rail)
        {
            if (eventLists.Count <= rail) return false;
            return eventLists[rail].Events.Any(item => item.StartTime < time && time < item.EndTime);
        }

        public bool ClickOnFunction(double time, int rail)
        {
            return functions.Any(item => item.Rail == rail && item.Time == time);
        }

        public double GetLastEventValue(int rail)
        {
            var qwq = eventLists[rail].type;
            if(qwq == EventType.Unknown)
            {
                return double.NaN;
            }
            var endValue = Event.GetDefaultValue(qwq);
            double endTime = 0;
            foreach(var item in eventLists[rail].Events)
            {
                if(item.EndTime > endTime)
                {
                    endTime = item.EndTime;
                    endValue = item.EndTime;
                }
            }
            return endValue;
        }

        public EventType GetType(int index)
        {
            return index >= eventLists.Count ? EventType.Unknown : eventLists[index].type;
        }

        public void SetType(EventType type, int index)
        {
            while (eventLists.Count < index + 1)
            {
                eventLists.Add(new(this));
            }
            eventLists[index].type = type;
        }

        /// <summary>
        /// 获得此时的速度
        /// </summary>
        /// <param name="time">时间，单位秒</param>
        /// <returns></returns>
        public double GetSpeed(double time)
        {
            var speed = Event.GetDefaultValue(EventType.Speed);
            foreach (var list in eventLists)
            {
                if(list.type != EventType.Speed) continue;
                double value = 0;
                var isDefaultSpeed = true;
                foreach(var e in list.Events)
                {
                    if (e.EndTime <= time)
                    {
                        isDefaultSpeed = false;
                        value = e.EndValue;
                    }
                    if (e.StartTime <= time && time <= e.EndTime)
                    {
                        isDefaultSpeed = false;
                        value = EaseFunctions.Interpolate(e.StartValue, e.EndValue, (time - e.StartTime)/(e.EndTime - e.StartTime), e.EaseFunction);
                        break;
                    }
                }
                if (isDefaultSpeed)
                {
                    value = list.IsMainEvent() ? Event.GetDefaultValue(EventType.Speed) : 0;
                }
                if (list.IsMainEvent())
                {
                    speed = value;
                }
                else
                {
                    speed += value;
                }
            }
            return speed;
        }

        public void Init(Track track)
        {
            foreach (var note in notes)
            {
                note.ParentLine = this;
            }
            foreach (var item in fakeCatch)
            {
                item.ParentLine = this;
            }
            for (int i = 0; i < eventLists.Count; i++)
            {
                eventLists[i].parentLine = this;
                eventLists[i].Events.ForEach(e =>
                {
                    e.ParentList = eventLists[i];
                    SetType(e.Type, i);
                });
                eventLists[i].GroupEvent();
            }
            foreach (var function in functions)
            {
                function.TryLink(track);
            }
        }
    }
}
