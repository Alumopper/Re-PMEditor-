using PMEditor.Util;

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

        public void SetType(EventType type)
        {
            this.type = type;
            this.typeId = (int)type;
            foreach (var item in events)
            {
                item.type = type;
                item.TypeId = (int)type;
                item.rectangle.Fill = new System.Windows.Media.SolidColorBrush(EditorColors.GetEventColor(type));
                item.rectangle.HighLightBorderBrush = new System.Windows.Media.SolidColorBrush(EditorColors.GetEventHighlightColor(type));
            }
        }

        public bool IsMainEvent()
        {
            int index = parentLine.EventLists.IndexOf(this);
            for(int i = 0; i < parentLine.EventLists.Count; i++)
            {
                if (parentLine.EventLists[i].type == type)
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
                    item.isHeaderEvent = true;
                    item.EventGroup.Add(item);
                    headEvent = item;
                }
                else if (item.StartTime == lastEndTime)
                {
                    item.isHeaderEvent = false;
                    headEvent!.EventGroup.Add(item);

                }
                lastEndTime = item.EndTime;
            }
        }
    }
}
