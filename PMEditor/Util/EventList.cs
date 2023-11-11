using System.Collections.Generic;

namespace PMEditor.Util
{
    public class EventList
    {
        public int Count { get => typeList.Count; }

        private List<EventType> typeList = new();

        public void ChangeType(EventType type, int index)
        {
            while (typeList.Count < index + 1)
            {
                typeList.Add(EventType.Unknown);
            }
            typeList[index] = type;
        }

        public EventType GetType(int index)
        {
            return index >= typeList.Count ? EventType.Unknown : typeList[index];
        }

        public bool isMainEvent(int index)
        {
            return typeList.IndexOf(typeList[index]) == index;
        }
    }
}
