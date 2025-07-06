using System.Collections.Generic;
using System.Linq;
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
            List<Event> group = new();
            foreach (var item in events)
            {
                if(item.StartTime > lastEndTime)
                {
                    EventGroup.BuildGroup(group);
                    group = new List<Event>();
                    item.IsHeaderEvent = true;
                    group.Add(item);
                }
                else if (item.StartTime == lastEndTime)
                {
                    item.IsHeaderEvent = false;
                    group.Add(item);
                }
                lastEndTime = item.EndTime;
            }
        }

        public List<EventGroup> GetEventGroups()
        {
            return (from item in events where item.IsHeaderEvent select item.EventGroup).ToList();
        }
    }
}
