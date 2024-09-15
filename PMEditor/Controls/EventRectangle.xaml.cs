using System;
using System.Collections.Generic;
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
        public Event @event;

        public EventRectangle(Event @event)
        {
            InitializeComponent();
            this.@event = @event;
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
                double a = TrackEditorPage.Instance.GetYFromTime(@event.StartTime);
                double b = TrackEditorPage.Instance.GetYFromTime(@event.EndTime);
                return Math.Abs(a - b);
            }
        }

        public static void DrawFunction(List<EventRectangle> eventRectangles)
        {
            //获取曲线的较大点和较小点
            double max = double.MinValue, min = double.MaxValue;
            foreach(EventRectangle eventRectangle in eventRectangles)
            {
                max = Math.Max(max, eventRectangle.@event.StartValue);
                max = Math.Max(max, eventRectangle.@event.EndValue);
                min = Math.Min(min, eventRectangle.@event.StartValue);
                min = Math.Min(min, eventRectangle.@event.EndValue);
            }
            //宽度
            double width = eventRectangles[0].ActualWidth;
            foreach(var er in eventRectangles)
            {
                if(er.functionPath.Data is not PathGeometry)
                {
                    PathGeometry pg = new();
                    pg.Figures.Add(new());
                    er.functionPath.Data = pg;
                }
                (er.functionPath.Data as PathGeometry)!.Figures[0].Segments.Clear();
                Event e = er.@event;
                PathGeometry pathGeometry = (er.functionPath.Data as PathGeometry)!;
                PathFigure pathFigure = pathGeometry.Figures[0];
                double height = er.EventHeight;
                for(double i = 0; i <= height; i++)
                {
                    double value = EaseFunctions.Interpolate(e.StartValue, e.EndValue, i / height, e.easeFunction);
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
            startValue.Text = @event.StartValue.ToString();
            endValue.Text = @event.EndValue.ToString();
        }

        //右键删除此event
        private void Rectangle_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            @event.parentList.GroupEvent();
            @event.parentList.Events.Remove(@event);
            TrackEditorPage.Instance.eventPanel.Children.Remove(this);
            OperationManager.AddOperation(new RemoveEventOperation(@event, @event.parentList));
        }

        //选中此事件
        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TrackEditorPage.Instance.infoFrame.Content = new EventPropertyPanel(@event);
            TrackEditorPage.Instance.UpdateSelectedEvent(@event); 
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
