using PMEditor.Util;
using System.Drawing;

namespace PMEditor
{
    public partial class Line
    {
        //事件种类区别
        public EventList eventList = new();

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
                    if(item.actualTime <= time && time <= item.actualTime + item.actualHoldTime)
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
            foreach(var item in events)
            {
                if (item.rail != rail) continue;
                if(item.startTime <= time && time <= item.endTime)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
