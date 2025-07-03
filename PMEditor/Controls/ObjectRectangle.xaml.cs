using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using PMEditor.Util;
using Range = PMEditor.Util.Range;

namespace PMEditor.Controls;

public partial class ObjectRectangle
{
    
    public static readonly ObjectPool<ObjectRectangle> ObjectRectanglePool = new(() => new ObjectRectangle(), 50);
    
    public bool IsResizable = true;
    
    public ObjectAdapter Data;
    
    public double StartTime => Data.StartTime;
    public double LengthTime => Data.LengthTime;

    public int Rail => Data.Rail;
    
    public Color Color
    {
        set
        {
            Rect.Fill = new SolidColorBrush(value);
        }
    }
    
    public Color HighLightColor
    {
        set
        {
            HighLightBorder.BorderBrush = new SolidColorBrush(value);
        }
    }

    public Line ParentLine;

    public ObjectPanel ParentPanel;
    
    public ObjectRectangle()
    {
        InitializeComponent();
    }
    
    private bool highLight;
    public bool HighLight
    {
        get => highLight;
        set
        {
            highLight = value;
            //高亮
            HighLightBorder.BorderThickness = highLight ? new Thickness(2) : new Thickness(0);
        }
    }

    public void SetVisible(ObjectVisible visible)
    {
        HighLightBorder.Opacity = visible switch
        {
            ObjectVisible.Visible => 1,
            ObjectVisible.Translucent => 0.5,
            ObjectVisible.Hidden => 0,
            _ => throw new ArgumentOutOfRangeException(nameof(visible), visible, null)
        };
    }

    //选中此事件
    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        //TrackEditorPage.Instance.InfoFrame.Content = new EventPropertyPanel(Event);
        ParentPanel.CurrTool.OnRectangleLeftClick(this);
    }

    public void SetFromObject(object obj)
    {
        switch (obj)
        {
            case FakeCatch fakeCatch:
                SetFromFakeCatch(fakeCatch);
                break;
            case Note note:
                SetFromNote(note);
                break;
            case Event e:
                SetFromEvent(e);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(obj), obj, null);
        }
    }

    public void SetFromNote(Note note)
    {
        Data = new ObjectAdapter(note);
        IsResizable = note.type == NoteType.Hold;
        Color = note.type switch
        {
            NoteType.Tap => EditorColors.tapColor,
            NoteType.Hold => EditorColors.holdColor,
            _ => EditorColors.catchColor
        };
        HighLightColor = note.type switch
        {
            NoteType.Tap => EditorColors.tapHighlightColor,
            NoteType.Hold => EditorColors.holdHighlightColor,
            _ => EditorColors.catchHighlightColor
        };
    }

    public static ObjectRectangle FromNote(ObjectPanel panel, Note note)
    {
        var rect = new ObjectRectangle
        {
            Data = new ObjectAdapter(note),
            IsResizable = note.type == NoteType.Hold,
            Color = note.type switch
            {
                NoteType.Tap => EditorColors.tapColor,
                NoteType.Hold => EditorColors.holdColor,
                _ => EditorColors.catchColor
            },
            HighLightColor = note.type switch
            {
                NoteType.Tap => EditorColors.tapHighlightColor,
                NoteType.Hold => EditorColors.holdHighlightColor,
                _ => EditorColors.catchHighlightColor
            }
        };
        if (note.type == NoteType.Hold)
        {
            rect.Height = panel.GetTopYFromTime(note.ActualTime) -
                          panel.GetTopYFromTime(note.ActualTime + note.ActualHoldTime);
        }
        else
        {
            rect.Height = 10;
        }
        rect.Width = panel.ActualWidth / 9;
        rect.ParentLine = note.ParentLine;
        return rect;
    }

    public void SetFromEvent(Event e)
    {
        Data = new ObjectAdapter(e);
        Color = EditorColors.GetEventColor(e.Type);
        ParentLine = e.ParentList.ParentLine;
    }

    public static ObjectRectangle FromEvent(ObjectPanel panel, Event e)
    {
        var rect = new ObjectRectangle
        {
            Data = new ObjectAdapter(e),
            Color = EditorColors.GetEventColor(e.Type),
            Height = panel.GetTopYFromTime(e.StartTime) - panel.GetTopYFromTime(e.EndTime),
            Width = panel.ActualWidth / 9,
            ParentLine = e.ParentList.ParentLine
        };
        return rect;
    }

    public void SetFromFakeCatch(FakeCatch fakeCatch)
    {
        Data = new ObjectAdapter(fakeCatch);
        Color = FakeCatch.GetColor(fakeCatch.Height);
        ParentLine = fakeCatch.ParentLine;
    }

    public static ObjectRectangle FromFakeCatch(ObjectPanel panel, FakeCatch fakeCatch)
    {
        var rect = new ObjectRectangle
        {
            Data = new ObjectAdapter(fakeCatch),
            Color = FakeCatch.GetColor(fakeCatch.Height),
            Height = 10,
            Width = panel.ActualWidth / 9,
            ParentLine = fakeCatch.ParentLine
        };
        return rect;
    }
    
    public void UpdateText()
    {
        if(Data.Value is not Event e) return;
        StartValue.Text = e.StartValue.ToString(CultureInfo.InvariantCulture);
        EndValue.Text = e.EndValue.ToString(CultureInfo.InvariantCulture);
    }
    
    public static void DrawFunction(List<ObjectRectangle> eventRectangles)
    {
        //获取曲线的较大点和较小点
        double max = double.MinValue, min = double.MaxValue;
        foreach(var eventRectangle in eventRectangles)
        {
            max = Math.Max(max, ((Event)eventRectangle.Data.Value!).StartValue);
            max = Math.Max(max, ((Event)eventRectangle.Data.Value).EndValue);
            min = Math.Min(min, ((Event)eventRectangle.Data.Value).StartValue);
            min = Math.Min(min, ((Event)eventRectangle.Data.Value).EndValue);
        }
        //宽度
        var width = eventRectangles[0].ActualWidth;
        foreach(var er in eventRectangles)
        {
            if(er.FunctionPath.Data is not PathGeometry)
            {
                PathGeometry pg = new();
                pg.Figures.Add(new PathFigure());
                er.FunctionPath.Data = pg;
            }
            (er.FunctionPath.Data as PathGeometry)!.Figures[0].Segments.Clear();
            var e = (Event)er.Data.Value!;
            var pathGeometry = (er.FunctionPath.Data as PathGeometry)!;
            var pathFigure = pathGeometry.Figures[0];
            var height = er.ActualHeight;
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

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (TrackEditorPage.Instance!.CurrPanel.IsDragging)
        {
            TrackEditorPage.Instance.CurrPanel.CurrTool.OnRectangleDragOver(this);
        }
    }
}