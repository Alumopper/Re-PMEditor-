using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
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

    public Point BeforeMovePos;
    public Point BeforeResizeSize;
    
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

    public ObjectPanel ParentPanel;

    public bool IsNote => Data.Value is Note;
    public bool IsEvent => Data.Value is Event;
    public bool IsFakeCatch => Data.Value is FakeCatch;
    public ObjectVisible Visible = ObjectVisible.Visible;
    
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
            if (highLight)
            {
                BeforeMovePos = new Point(Canvas.GetLeft(this), Canvas.GetTop(this));
                BeforeResizeSize = new Point(Width, Height);
            }
        }
    }

    public void SetVisible(ObjectVisible visible)
    {
        Visible = visible;
        Rect.Fill.Opacity = visible switch
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
        e.Handled = true;
    }
    
    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
    }
    
    public void SetFromObject(object obj, ObjectPanel panel)
    {
        switch (obj)
        {
            case FakeCatch fakeCatch:
                SetFromFakeCatch(fakeCatch, panel);
                break;
            case Note note:
                SetFromNote(note, panel);
                break;
            case Event e:
                SetFromEvent(e, panel);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(obj), obj, null);
        }
    }

    public void SetFromNote(Note note, ObjectPanel panel)
    {
        Data = new ObjectAdapter(note);
        IsResizable = note.type == NoteType.Hold;
        Width = panel.ActualWidth / 9;
        Height = note.type == NoteType.Hold
            ? panel.GetTopYFromTime(note.ActualTime) -
              panel.GetTopYFromTime(note.ActualTime + note.ActualHoldTime)
            : 10;
        ParentPanel = panel;
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
        StartValue.Visibility = Visibility.Collapsed;
        EndValue.Visibility = Visibility.Collapsed;
        PathCanvas.Visibility = Visibility.Collapsed;
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
            },
            StartValue =
            {
                Visibility = Visibility.Collapsed
            },
            EndValue =
            {
                Visibility = Visibility.Collapsed
            },
            PathCanvas =
            {
                Visibility = Visibility.Collapsed
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
        return rect;
    }

    public void SetFromEvent(Event e, ObjectPanel panel)
    {
        Data = new ObjectAdapter(e);
        Width = panel.ActualWidth / 9;
        Height = panel.GetTopYFromTime(e.StartTime) - panel.GetTopYFromTime(e.EndTime);
        ParentPanel = panel;
        Color = EditorColors.GetEventColor(e.Type);
        StartValue.Visibility = Visibility.Visible;
        EndValue.Visibility = Visibility.Visible;
        PathCanvas.Visibility = Visibility.Visible;
        UpdateText();
        DrawFunction();
    }

    public static ObjectRectangle FromEvent(ObjectPanel panel, Event e)
    {
        var rect = new ObjectRectangle
        {
            Data = new ObjectAdapter(e),
            Color = EditorColors.GetEventColor(e.Type),
            Height = panel.GetTopYFromTime(e.StartTime) - panel.GetTopYFromTime(e.EndTime),
            Width = panel.ActualWidth / 9,
            StartValue =
            {
                Visibility = Visibility.Visible
            },
            EndValue =
            {
                Visibility = Visibility.Visible
            },
            PathCanvas =
            {
                Visibility = Visibility.Visible
            }
        };
        return rect;
    }

    public void SetFromFakeCatch(FakeCatch fakeCatch, ObjectPanel panel)
    {
        Data = new ObjectAdapter(fakeCatch);
        Color = FakeCatch.GetColor(fakeCatch.Height);
        Height = 10;
        Width = panel.ActualWidth / 9;
        ParentPanel = panel;
        StartValue.Visibility = Visibility.Collapsed;
        EndValue.Visibility = Visibility.Collapsed;
        PathCanvas.Visibility = Visibility.Collapsed;
    }

    public static ObjectRectangle FromFakeCatch(ObjectPanel panel, FakeCatch fakeCatch)
    {
        var rect = new ObjectRectangle
        {
            Data = new ObjectAdapter(fakeCatch),
            Color = FakeCatch.GetColor(fakeCatch.Height),
            Height = 10,
            Width = panel.ActualWidth / 9,
            StartValue =
            {
                Visibility = Visibility.Collapsed
            },
            EndValue =
            {
                Visibility = Visibility.Collapsed
            },
            PathCanvas =
            {
                Visibility = Visibility.Collapsed
            }
        };
        return rect;
    }
    
    public void UpdateText()
    {
        if(Data.Value is not Event e) return;
        StartValue.Text = e.StartValue.ToString(CultureInfo.InvariantCulture);
        EndValue.Text = e.EndValue.ToString(CultureInfo.InvariantCulture);
    }
    
    public void DrawFunction()
    {
        //获取曲线的较大点和较小点
        if(Data.Value is not Event e) return;
        double max = e.EventGroup.MaxValue, min = e.EventGroup.MinValue;
        //宽度
        var width = Width;
        if(FunctionPath.Data is not PathGeometry)
        {
            PathGeometry pg = new();
            pg.Figures.Add(new PathFigure());
            FunctionPath.Data = pg;
        }
        (FunctionPath.Data as PathGeometry)!.Figures[0].Segments.Clear();
        var pathGeometry = (FunctionPath.Data as PathGeometry)!;
        var pathFigure = pathGeometry.Figures[0];
        var height = Height;
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

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (TrackEditorPage.Instance!.CurrPanel.IsDragging)
        {
            TrackEditorPage.Instance.CurrPanel.CurrTool.OnRectangleDragOver(this);
        }
    }
}