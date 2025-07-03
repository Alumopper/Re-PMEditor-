using System.Collections.Generic;
using PMEditor.Util;

namespace PMEditor
{
    public partial class EventList
    {
        public EventType Type;

        public Line ParentLine;

        public EventList(Line parent, EventType type = EventType.Unknown)
        {
            this.ParentLine = parent;
            this.Type = type;
            this.events = new List<Event>();
        }

        public bool IsMainEvent()
        {
            int index = ParentLine.EventLists.IndexOf(this);
            for(int i = 0; i < ParentLine.EventLists.Count; i++)
            {
                if (ParentLine.EventLists[i].Type == Type)
                {
                    return i == index;
                }
            }
            throw new System.Exception();
        }

        public void GroupEvent()
        {
            double lastEndTime = -1;
            Event? headEvent = null;
            foreach (var item in events)
            {
                if(item.StartTime > lastEndTime)
                {
                    item.IsHeaderEvent = true;
                    item.EventGroup.Add(item);
                    headEvent = item;
                }
                else if (item.StartTime == lastEndTime)
                {
                    item.IsHeaderEvent = false;
                    headEvent!.EventGroup.Add(item);

                }
                lastEndTime = item.EndTime;
            }
        }
    }
}
