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
                if (item.rail != rail) continue;
                if(item.type == NoteType.Hold)
                {
                    if(item.actualTime < time && time < item.actualTime + item.actualHoldTime)
                    {
                        return true;
                    }
                }
                else
                {
                    if(item.actualTime == time)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool ClickOnFakeCatch(double time, int rail)
        {
            foreach (var item in fakeCatch)
            {
                if (item.rail == rail && item.actualTime == time)
                {
                    return true;
                }
            }
            return false;
        }

        public bool ClickOnEvent(double time, int rail)
        {
            if (eventLists.Count <= rail) return false;
            foreach(var item in eventLists[rail].events)
            {
                if(item.startTime < time && time < item.endTime)
                {
                    return true;
                }
            }
            return false;
        }

        public bool ClickOnFunction(double time, int rail)
        {
            foreach (var item in functions)
            {
                if (item.rail == rail && item.time == time)
                {
                    return true;
                }
            }
            return false;
        }

        public double GetLastEventValue(int rail)
        {
            var qwq = eventLists[rail].type;
            if(qwq == EventType.Unknown)
            {
                return double.NaN;
            }
            double endValue = Event.GetDefaultValue(qwq);
            double endTime = 0;
            foreach(Event item in eventLists[rail].events)
            {
                if(item.endTime > endTime)
                {
                    endTime = item.endTime;
                    endValue = item.endValue;
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
            double speed = Event.GetDefaultValue(EventType.Speed);
            foreach (var list in eventLists)
            {
                if(list.type != EventType.Speed) continue;
                double value = 0;
                bool isDefaultSpeed = true;
                foreach(var e in list.Events)
                {
                    if (e.endTime <= time)
                    {
                        isDefaultSpeed = false;
                        value = e.endValue;
                    }
                    if (e.startTime <= time && time <= e.endTime)
                    {
                        isDefaultSpeed = false;
                        value = EaseFunctions.Interpolate(e.startValue, e.endValue, (time - e.startTime)/(e.endTime - e.startTime), e.easeFunction);
                        break;
                    }
                    if (e.endTime > time) continue;
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
                note.parentLine = this;
            }
            foreach (var item in fakeCatch)
            {
                item.parentLine = this;
            }
            for (int i = 0; i < eventLists.Count; i++)
            {
                eventLists[i].parentLine = this;
                eventLists[i].events.ForEach(e =>
                {
                    e.parentList = eventLists[i];
                    SetType(e.type, i);
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
