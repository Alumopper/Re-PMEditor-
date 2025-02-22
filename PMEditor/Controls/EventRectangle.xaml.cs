using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PMEditor.Util;
using PMEditor.Operation;
using System.Windows.Input;
using PMEditor.Controls;

namespace PMEditor
{
    /// <summary>
    /// NoteRectangle.xaml 的交互逻辑
    /// </summary>
    public partial class EventRectangle : UserControl
    {
        /// <summary>
        /// 这个矩形对应的note
        /// </summary>
        public Event Event;

        public EventRectangle(Event @event)
        {
            InitializeComponent();
            this.Event = @event;
            startValue.Text = @event.StartValue.ToString();
            endValue.Text = @event.EndTime.ToString();
        }

        public Brush Fill
        {
            get => rect.Fill; set => rect.Fill = value;
        }

        public Brush HighLightBorderBrush
        {
            get => highLightBorder.BorderBrush; set => highLightBorder.BorderBrush = value; 
        }

        private bool highLight;
        public bool HighLight
        {
            get => highLight;
            set
            {
                highLight = value;
                //高亮
                if (highLight)
                {
                    highLightBorder.BorderThickness = new(2);
                }
                else
                {
                    highLightBorder.BorderThickness = new(0);
                }
            }
        }

        public short IsResizing { get; set; } = 0;

        public double EventHeight
        {
            get {
                var a = TrackEditorPage.Instance!.GetBottomYFromTime(Event.StartTime);
                var b = TrackEditorPage.Instance.GetBottomYFromTime(Event.EndTime);
                return Math.Abs(a - b);
            }
        }

        public static void DrawFunction(List<EventRectangle> eventRectangles)
        {
            //获取曲线的较大点和较小点
            double max = double.MinValue, min = double.MaxValue;
            foreach(var eventRectangle in eventRectangles)
            {
                max = Math.Max(max, eventRectangle.Event.StartValue);
                max = Math.Max(max, eventRectangle.Event.EndValue);
                min = Math.Min(min, eventRectangle.Event.StartValue);
                min = Math.Min(min, eventRectangle.Event.EndValue);
            }
            //宽度
            var width = eventRectangles[0].ActualWidth;
            foreach(var er in eventRectangles)
            {
                if(er.functionPath.Data is not PathGeometry)
                {
                    PathGeometry pg = new();
                    pg.Figures.Add(new PathFigure());
                    er.functionPath.Data = pg;
                }
                (er.functionPath.Data as PathGeometry)!.Figures[0].Segments.Clear();
                var e = er.Event;
                var pathGeometry = (er.functionPath.Data as PathGeometry)!;
                var pathFigure = pathGeometry.Figures[0];
                var height = er.EventHeight;
                for(double i = 0; i <= height; i++)
                {
                    var value = EaseFunctions.Interpolate(e.StartValue, e.EndValue, i / height, e.EaseFunction);
                    Point point = new((value - min) / (max - min) * width, height - i);
                    if(pathFigure.Segments.Count == 0)
                    {
                        pathFigure.StartPoint = point;
                    }
                    pathFigure.Segments.Add(new LineSegment(point, true));
                }
            }
        }

        public void UpdateText()
        {
            startValue.Text = Event.StartValue.ToString(CultureInfo.InvariantCulture);
            endValue.Text = Event.EndValue.ToString(CultureInfo.InvariantCulture);
        }

        //右键删除此event
        private void Rectangle_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Event.ParentList.GroupEvent();
            Event.ParentList.Events.Remove(Event);
            TrackEditorPage.Instance!.eventPanel.Children.Remove(this);
            OperationManager.AddOperation(new RemoveEventOperation(Event, Event.ParentList));
        }

        //选中此事件
        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TrackEditorPage.Instance.infoFrame.Content = new EventPropertyPanel(Event);
            TrackEditorPage.Instance.UpdateSelectedEvent(Event); 
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                Point currentPosition = e.GetPosition(rect);
                if (rect.ActualHeight - currentPosition.Y < 20)
                {
                    IsResizing = -1;
                    return;
                }
                if (currentPosition.Y < 20)
                {
                    IsResizing = 1;
                    return;
                }
            }
            IsResizing = 0;
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if(Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                Point currentPosition = e.GetPosition(rect);
                if (rect.ActualHeight - currentPosition.Y < 20)
                {
                    rect.Cursor = Cursors.SizeNS;
                    return;
                }
                if(currentPosition.Y < 20)
                {
                    rect.Cursor = Cursors.SizeNS;
                    return;
                }
            }
            rect.Cursor = Cursors.Arrow;
        }
    }
}
