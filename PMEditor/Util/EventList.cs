﻿using System.Management.Automation.Language;

namespace PMEditor
{
    public partial class EventList
    {
        public EventType type;

        public Line parentLine;

        public EventList(Line parent)
        {
            this.parentLine = parent;
            this.type = EventType.Unknown;
            this.events = new();
        }

        public bool IsMainEvent()
        {
            int index = parentLine.eventLists.IndexOf(this);
            for(int i = 0; i < parentLine.eventLists.Count; i++)
            {
                if (parentLine.eventLists[i].type == type)
                {
                    return i == index;
                }
            }
            throw new System.Exception();
        }

        public void GroupEvent()
        {
            double lastEndTime = -1;
            Event headEvent = null;
            foreach (var item in events)
            {
                if(item.startTime > lastEndTime)
                {
                    item.isHeaderEvent = true;
                    item.EventGroup.Add(item);
                    headEvent = item;
                }
                else if (item.startTime == lastEndTime)
                {
                    item.isHeaderEvent = false;
                    headEvent.EventGroup.Add(item);

                }
                lastEndTime = item.endTime;
            }
        }
    }
}
