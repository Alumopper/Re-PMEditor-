using PMEditor.Util;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace PMEditor
{
    public class EventGroup
    {
        public List<Event> Events;
        public EventType Type;
        public Event HeaderEvent;
        public double MinValue;
        public double MaxValue;

        public EventGroup(List<Event> events)
        {
            Events = events;
            Type = events[0].Type;
            HeaderEvent = events[0];
            //寻找最大值
            double max = double.MinValue, min = double.MaxValue;
            foreach (var e in events)
            {
                var (fmax, fmin) = EaseFunctions.FunctionMaxValues[e.EaseFunctionID];
                var emax = e.StartValue + (e.EndValue - e.StartValue) * fmax;
                var emin = e.StartValue + (e.EndValue - e.StartValue) * fmin;
                if (emax > max) max = emax;
                if (emax < min) min = emax;
                if (emin > max) max = emin;
                if (emin < min) min = emin;
            }
            MaxValue = max;
            MinValue = min;
        }

        public static void BuildGroup(List<Event> events)
        {
            if(events.Count == 0) return;
            var groups = new EventGroup(events);
            events.ForEach(e => e.EventGroup = groups);
        }
    }
    
    public partial class Event
    {

        [JsonIgnore] public int Rail;
        
        public EventList ParentList;

        public static EventType puttingEvent = EventType.Speed;

        public bool IsHeaderEvent;

        public EventGroup EventGroup;

        public EventType Type;

        public Func<double, double> EaseFunction;

        public Event(double startTime, double endTime, string easeFunction, double startValue, double endValue)
            : this(startTime, endTime, (int)puttingEvent, easeFunction, InitProperties(puttingEvent), startValue, endValue) { }

        public void SetType(EventType type)
        {
            this.Type = type;
            this.typeId = (int)type;
            EaseFunction = EaseFunctions.Functions[easeFunctionID];
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

        public Event Clone()
        {
            return new Event(StartTime, EndTime, EaseFunctionID, StartValue, EndValue);
        }
        
        
        public bool IsOverlap(Event e)
        {
            if(!(e.Rail == Rail && e.ParentList == ParentList)) return false;
            return StartTime <= e.StartTime && e.StartTime <= EndTime ||
                   StartTime <= e.EndTime && e.EndTime <= StartTime;

        }
    }

    public enum EventType
    { 
        Speed, YPosition, Function, Unknown
    }
}
