using PMEditor.Controls;
using PMEditor.Operation;
using PMEditor.Util;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;

namespace PMEditor
{
    public partial class Event
    {
        public EventList parentList;

        public static EventType puttingEvent = EventType.Speed;

        public EventRectangle rectangle;

        public bool isHeaderEvent;

        public List<Event> EventGroup = new();

        public EventType type;

        public Func<double, double> easeFunction;

        public Color Color
        {
            set
            {
                rectangle.Fill = new SolidColorBrush(value);
            }
        }

        public Event(double startTime, double endTime, string easeFunction, double startValue, double endValue)
            : this(startTime, endTime, (int)puttingEvent, easeFunction, InitProperties(puttingEvent), startValue, endValue) { }

        public void SetType(EventType type)
        {
            this.type = type;
            this.typeId = (int)type;
            easeFunction = EaseFunctions.functions[easeFunctionID];
            rectangle.UpdateText();
            this.rectangle.Fill = new SolidColorBrush(EditorColors.GetEventColor(type));
            this.rectangle.HighLightBorderBrush = new SolidColorBrush(EditorColors.GetEventHighlightColor(type));
        }

        public static Dictionary<string, object> InitProperties(EventType type)
        {
            Dictionary<string, object> re = new();
            return re;
        }

        public override string ToString()
        {
            return $"Event[line={parentList.parentLine.Id},type={type}]";
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
