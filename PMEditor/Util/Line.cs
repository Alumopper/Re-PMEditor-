using PMEditor.Util;
using System.ComponentModel;
using System.Drawing;
using System.Xml.Serialization;

namespace PMEditor
{
    public partial class Line
    {
        public bool IsNoteOverLap(Note note)
        {
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
    }
}
