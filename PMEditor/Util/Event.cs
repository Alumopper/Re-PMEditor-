using PMEditor.Util;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace PMEditor
{
    public partial class Event
    {
        public EventList ParentList;

        public static EventType puttingEvent = EventType.Speed;

        public readonly EventRectangle Rectangle;

        public bool IsHeaderEvent;

        public readonly List<Event> EventGroup = new();

        public EventType Type;

        public Func<double, double> EaseFunction;

        public Color Color
        {
            set
            {
                Rectangle.Fill = new SolidColorBrush(value);
            }
        }

        public Event(double startTime, double endTime, string easeFunction, double startValue, double endValue)
            : this(startTime, endTime, (int)puttingEvent, easeFunction, InitProperties(puttingEvent), startValue, endValue) { }

        public void SetType(EventType type)
        {
            this.Type = type;
            this.typeId = (int)type;
            EaseFunction = EaseFunctions.functions[easeFunctionID];
            Rectangle.UpdateText();
            this.Rectangle.Fill = new SolidColorBrush(EditorColors.GetEventColor(type));
            this.Rectangle.HighLightBorderBrush = new SolidColorBrush(EditorColors.GetEventHighlightColor(type));
        }

        public static Dictionary<string, object> InitProperties(EventType type)
        {
            Dictionary<string, object> re = new();
            return re;
        }

        public override string ToString()
        {
            return $"Event[line={ParentList.parentLine.Id},type={Type}]";
        }

        public static string TypeString(EventType type)
        {
            return type switch
            {
                EventType.Speed => "速度",
                EventType.YPosition => "Y",
                _ => "未知"
            };
        }

        public static double GetDefaultValue(EventType type)
        {
            return type switch
            {
                EventType.Speed => 10,
                EventType.YPosition => 0,
                _ => double.NaN
            };

        }    
    }

    public enum EventType
    { 
        Speed, YPosition, Unknown
    }
}
