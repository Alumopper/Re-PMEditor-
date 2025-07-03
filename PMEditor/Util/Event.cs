using PMEditor.Util;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace PMEditor
{
    public partial class Event
    {

        [JsonIgnore] public int Rail;
        
        public EventList ParentList;

        public static EventType puttingEvent = EventType.Speed;

        public bool IsHeaderEvent;

        public readonly List<Event> EventGroup = new();

        public EventType Type;

        public Func<double, double> EaseFunction;

        public Event(double startTime, double endTime, string easeFunction, double startValue, double endValue)
            : this(startTime, endTime, (int)puttingEvent, easeFunction, InitProperties(puttingEvent), startValue, endValue) { }

        public void SetType(EventType type)
        {
            this.Type = type;
            this.typeId = (int)type;
            EaseFunction = EaseFunctions.functions[easeFunctionID];
        }

        public static Dictionary<string, object> InitProperties(EventType type)
        {
            Dictionary<string, object> re = new();
            return re;
        }

        public override string ToString()
        {
            return $"Event[line={ParentList.ParentLine.Id},type={Type}]";
        }

        public static string TypeString(EventType type)
        {
            return type switch
            {
                EventType.Speed => "速度",
                EventType.YPosition => "Y",
                EventType.Function => "函数",
                _ => "未知"
            };
        }

        public static double GetDefaultValue(EventType type)
        {
            return type switch
            {
                EventType.Speed => 10,
                EventType.YPosition => 0,
                EventType.Function => 0,
                _ => double.NaN
            };

        }    
    }

    public enum EventType
    { 
        Speed, YPosition, Function, Unknown
    }
}
