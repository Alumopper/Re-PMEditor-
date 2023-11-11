using PMEditor.Controls;
using PMEditor.Operation;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;

namespace PMEditor
{
    public partial class Event
    {
        public static EventType puttingEvent = EventType.Speed;

        public Line parentLine;

        public EventRectangle rectangle;

        public EventType type;

        public Func<double, double> easeFunction;

        public Color Color
        {
            set
            {
                rectangle.Fill = new SolidColorBrush(value);
            }
        }

        public Event(double startTime, double endTime, int rail, string easeFunction)
            : this(startTime, endTime, rail, (int)puttingEvent, easeFunction, InitProperties(puttingEvent)) { }

        //右键删除此event
        private void Rectangle_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            parentLine.events.Remove(this);
            TrackEditorPage.Instance.eventPanel.Children.Remove(rectangle);
            OperationManager.AddOperation(new RemoveEventOperation(this, parentLine));
        }


        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TrackEditorPage.Instance.infoFrame.Content = new EventPropertyPanel(this);
            TrackEditorPage.Instance.UpdateSelectedEvent(this);
        }

        public static Dictionary<string, object> InitProperties(EventType type)
        {
            Dictionary<string, object> re = new();
            return re;
        }

        public override string ToString()
        {
            return $"Event[line={parentLine.Id},type={type}]";
        }

        public static string typeString(EventType type)
        {
            return type switch
            {
                EventType.Speed => "速度",
                EventType.YPosition => "Y",
                _ => "未知"
            };
        }
    }

    public enum EventType
    {
        Speed, YPosition, Unknown
    }
}
