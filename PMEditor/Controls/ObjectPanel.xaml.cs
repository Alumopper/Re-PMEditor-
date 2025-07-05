using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.Logging.Abstractions;
using PMEditor.EditorTool;
using PMEditor.Operation;
using PMEditor.Util;
using Point = System.Windows.Point;

namespace PMEditor.Controls;

/// <summary>
/// NotePanel.xaml 的交互逻辑
/// </summary>
public partial class ObjectPanel
{
    public readonly ArrowTool Arrow = new();
    public readonly ResizeTool Resize = new();
    public readonly MoveTool Move = new();
    public readonly PutTool Put = new();
    public readonly EraserTool Eraser = new();

    public AbstractTool CurrTool;

    public MousePosInfo? DragStartInfo;

    public bool IsDragging => DragStartInfo != null;
    private bool isClick = true;
    
    public readonly List<ObjectRectangle> ObjectRectangles = new();
    
    private List<ObjectRectangle> selectedObjectRectangles = new();

    private static readonly List<ObjectAdapter> Clipboard = new();

    private static double _clipBoardStartTime;

    public readonly List<ObjectAdapter> Objects = new();
    
    private static TrackEditorPage Page => TrackEditorPage.Instance!;
    private static EditorWindow Window => EditorWindow.Instance;

    public Line ParentLine;

    private List<ObjectRectangle> displayingRect = new();

    private bool isFirstLoaded = true;

    public ObjectPanel()
    {
        InitializeComponent();
        CurrTool = Arrow;
    }

    public void SetVisible(ObjectVisible visible)
    {
        switch (visible)
        {
            case ObjectVisible.Visible:
                this.Visibility = Visibility.Visible;
                foreach (var obj in ObjectRectangles)
                {
                    obj.SetVisible(ObjectVisible.Visible);
                }
                Panel.SetZIndex(this, 1);
                break;
            case ObjectVisible.Translucent:
                this.Visibility = Visibility.Visible;
                foreach (var obj in ObjectRectangles)
                {
                    obj.SetVisible(ObjectVisible.Translucent);
                }
                Panel.SetZIndex(this, 0);
                break;
            case ObjectVisible.Hidden:
                this.Visibility = Visibility.Hidden;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(visible), visible, null);
        }
    }
    
    public void AddObj(ObjectAdapter obj)
    {
        Objects.Add(obj);
    } 
    
    public void RemoveObj(ObjectAdapter obj)
    {
        Objects.Remove(obj);
    } 
    
    public void Update()
    {
        //目前显示的时间范围
        var minTime = Window.playerTime;
        var maxTime = minTime + Page.CurrDisplayLength;
        foreach (var obj in Objects)
        {
            if (obj.EndTime < minTime || obj.StartTime > maxTime)
            {
                if (obj.IsInViewRange)
                {
                    var owo = obj.ExitViewRange();
                    if (owo != null)
                    {
                        ObjCanvas.Children.Remove(owo);
                        ObjectRectangles.Remove(owo);
                    }
                }
            }
            else
            {
                if(!obj.IsInViewRange)
                {
                    var owo = obj.EnterViewRange();
                    owo.ParentPanel = this;
                    ObjCanvas.Children.Add(owo);
                    ObjectRectangles.Add(owo);
                }
            }
            //在判定线上方
            if (obj.Value is Note note)
            {
                if (minTime <= obj.StartTime)
                {
                    if (obj.IsJudged) obj.IsJudged = false;
                }
                else
                {
                    //如果未被判定过且谱面正在被播放
                    if (!obj.IsJudged && Window.isPlaying)
                    {
                        obj.OnJudged?.Invoke(note);
                        obj.IsJudged = true;
                    }
                }
            }
            var uwu = obj.Rect;
            if(uwu == null) continue;
            var qwq = Math.Max(minTime, obj.StartTime); //末端
            uwu.Width = ActualWidth / 9;
            uwu.Height = obj.LengthTime == 0
                ? 10
                : GetTopYFromTime(qwq) - GetTopYFromTime(obj.StartTime + obj.LengthTime);
            Canvas.SetTop(uwu, GetTopYFromTime(qwq) - uwu.Height);
            Canvas.SetLeft(uwu, obj.Rail * ActualWidth / 9);
        }
    }

    
    //滚轮滚动
    public void OnMouseWheel(object _, MouseWheelEventArgs e)
    {
        var info = GetAlignedPoint(e.GetPosition(this));
        CurrTool.OnMouseWheel(this, new ToolWheelArgs(info, e.Delta, selectedObjectRectangles));
    }
    
    public void OnMouseMove(object _, MouseEventArgs e)
    {
        var dragEndInfo = GetAlignedPoint(e.GetPosition(this));
        if (IsDragging)
        {
            CurrTool.OnMouseDrag(this, new ToolDragArgs(DragStartInfo!, dragEndInfo, selectedObjectRectangles));
            if (isClick && dragEndInfo!.Time - DragStartInfo!.Time != 0)
            {
                isClick = false;
            }
        }
        else
        {
            CurrTool.OnMouseMove(this, new ToolMoveArgs(dragEndInfo));
        }
    }
    
    public void OnMouseLeave(object _, MouseEventArgs e)
    {
        Page.UpdateObjPreview(Visibility.Collapsed);
    }

    //放置note
    public void OnMouseLeftButtonDown(object _, MouseButtonEventArgs e)
    {
        if (Window.isPlaying) return;
        if (DragStartInfo != null) return;
        //获取鼠标位置
        DragStartInfo = GetAlignedPoint(e.GetPosition(this));
        isClick = true;
    }

    //放置note，完成拖动
    public void OnMouseLeftButtonUp(object _, MouseButtonEventArgs e)
    {
        if (Window.isPlaying) return;
        if (DragStartInfo == null) return;

        //获取鼠标位置
        var endInfo = GetAlignedPoint(e.GetPosition(this));
        if (isClick)
        {
            CurrTool.OnMouseClick(this, new ToolClickArgs(endInfo));
        }
        else
        {
            CurrTool.OnMouseDragEnd(this, new ToolDragArgs(DragStartInfo, endInfo, selectedObjectRectangles));
        }
        DragStartInfo = null;
    }

    //调整note大小
    public void OnSizeChanged(object _, SizeChangedEventArgs e)
    {
        foreach (var obj in ObjectRectangles)
        {
            obj.Width = ActualWidth / 9;
        }
    }
    
    public void UpdateSelectedObj(List<ObjectRectangle> obj)
    {
        if (obj.Count == 1) UpdateSelectedObj(obj[0]);
        selectedObjectRectangles.ForEach(n => { n.HighLight = false; });
        selectedObjectRectangles = obj;
        selectedObjectRectangles.ForEach(n => { n.HighLight = true; });
    }

    public void UpdateSelectedObj(ObjectRectangle note, bool multiSelect = false)
    {
        if (!multiSelect)
        {
            selectedObjectRectangles.ForEach(n => { n.HighLight = false; });
            selectedObjectRectangles.Clear();
        }

        selectedObjectRectangles.Add(note);
        note.HighLight = true;
    }
    
    public void UpdateSelectingBorder(double left, double top, double width, double height)
    {
        SelectBorder.Visibility = Visibility.Visible;
        SelectBorder.Width = width;
        SelectBorder.Height = height;
        Canvas.SetLeft(SelectBorder, left);
        Canvas.SetTop(SelectBorder, top);
    }

    public void UpdateSelectingBorder(bool visible)
    {
        SelectBorder.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
    }
    
    /// <summary>
    /// 坐标对齐转换。返回的x坐标是对齐的，y坐标是相对于顶部的长度
    /// </summary>
    /// <returns>
    /// 对齐后的时间，对齐后的小节数，对齐后的拍数，对齐后鼠标的位置
    /// </returns>
    ///
    /// 
    public MousePosInfo GetAlignedPoint(Point p)
    {
        //获取时间
        //获取鼠标位置，生成note位置预览
        var mousePos = p;
        var width = ActualWidth / 9;
        //x坐标对齐
        var rail = (int)Math.Round(mousePos.X / width);
        
        //TODO 如果是Tap，额外需要对齐主线
        
        mousePos.X = rail * width;
        //获取当前小节数
        var (measure, deltime) = Window.track.GetMeasureFromTime(
            (ActualHeight - mousePos.Y) / ActualHeight * Page.CurrDisplayLength + Window.playerTime
        );
        //获取当前bpm
        var bpm = Window.track.GetBPM(measure);
        //获取当前拍数
        var beat = (int)(deltime / 60 * bpm * Page.DivideNum);
        //计算y坐标
        var time = Window.track.GetTimeRange(measure).startTime + beat/(double)Page.DivideNum * (60 / bpm);
        mousePos.Y = GetTopYFromTime(time);
        return new MousePosInfo(p, mousePos, time, measure, beat, rail);
    }

    /// <summary>
    /// 坐标时间转换
    /// </summary>
    /// <param name="y">相对于底部的长度</param>
    /// <returns></returns>
    public double GetTimeFromTopY(double y)
    {
        return (ActualHeight - y) / ActualHeight * Page.CurrDisplayLength + Window.player.Position.TotalSeconds;
    }

    public double GetTopYFromTime(double time)
    {
        return ActualHeight - (time - Window.playerTime) / Page.CurrDisplayLength * ActualHeight;
    }

    internal static void OnNoteJudged(Note note)
    {
        if (note.type == NoteType.Catch)
        {
            Window.catchSoundManager.AppendSoundRequest();
        }
        else
        {
            Window.tapSoundManager.AppendSoundRequest();
        }
    }
    
    private static ObjectAdapter? PutNote(PutTool.PutArgs note)
    {
        Note willPutNote;
        if (note.LengthTime > 0 && Window.puttingTap)
        {
            willPutNote = new Note(
                note.Rail,
                (int)NoteType.Hold,
                0,
                false,
                note.StartTime,
                actualHoldTime: note.LengthTime
            );
        }
        else if(note.LengthTime >= 0)
        {
            willPutNote = new Note(
                note.Rail,
                (int)(Window.puttingTap ? NoteType.Tap : NoteType.Catch),
                0,
                false,
                note.StartTime
            );
        }
        else
        {
            return null;
        }

        if (Page.CurrLine == Window.track.FreeLine) willPutNote.ExpressionString = "[t]";

        //OperationManager.AddOperation(new PutNoteOperation(willPutNote, Page.CurrLine));
        
        willPutNote.ParentLine = Page.CurrLine;
        if (Page.CurrLine.IsNoteOverLap(willPutNote)) return null;
        Page.CurrLine.AddNote(willPutNote);
        return new ObjectAdapter(willPutNote);
    }

    private static void MoveNote(List<ObjectRectangle> list)
    {
        foreach (var rect in list)
        {
            var note = (Note)rect.Data.Value!;
            note.ActualTime = rect.StartTime;
            note.Rail = rect.Rail;
        }
    }

    private static void ResizeNote(List<ObjectRectangle> list)
    {
        foreach (var rect in list)
        {
            var note = (Note)rect.Data.Value!;
            note.ActualHoldTime = rect.LengthTime;
        }
    }
    
    public static ObjectPanel NewNotePanel(Line line)
    {
        var panel = new ObjectPanel();
        panel.Put.OnPut += PutNote;
        panel.Move.OnMoveEnd += MoveNote;
        panel.Resize.OnResizeEnd += ResizeNote;
        panel.Loaded += (_, _) =>
        {
            if(!panel.isFirstLoaded) return;
            panel.isFirstLoaded = false;
            foreach (var note in line.Notes)
            {
                panel.AddObj(new ObjectAdapter(note));
            }
        };
        panel.ParentLine = line;
        return panel;
    }

    private static ObjectAdapter? PutEvent(PutTool.PutArgs rect)
    {
        if (rect.LengthTime == 0) return null;
            
        //获取事件类型
        var type = Page.CurrLine.GetType(rect.Rail);
        double v;
        if (type == EventType.Unknown)
        {
            type = EventType.Speed;
            v = Event.GetDefaultValue(type);
            Page.CurrLine.SetType(type, rect.Rail + Page.EventPageIndex * 9);
        }
        else
        {
            v = Page.CurrLine.GetLastEventValue(rect.Rail);
        }

        Event.puttingEvent = type;

        var willPutEvent = new Event(
            rect.StartTime,
            rect.StartTime + rect.LengthTime,
            EaseFunctions.linearName,
            v,
            v
        );
        if (Page.CurrLine.EventLists[rect.Rail].Events.Contains(willPutEvent)) return null;
            
        Page.CurrLine.EventLists[rect.Rail].Events.Add(willPutEvent);
        willPutEvent.ParentList = Page.CurrLine.EventLists[rect.Rail];
        //OperationManager.AddOperation(new PutEventOperation(willPutEvent, Page.CurrLine.EventLists[rect.Rail]));
        //更新这个轨道的事件组
        Page.CurrLine.EventLists[rect.Rail].GroupEvent();

        return new ObjectAdapter(willPutEvent);
    }

    private static void MoveEvent(List<ObjectRectangle> list)
    {
        foreach (var rect in list)
        {
            var e = (Event)rect.Data.Value!;
            e.StartTime = rect.StartTime;
            e.EndTime = rect.StartTime + rect.LengthTime;
        }
    }

    private static void ResizeEvent(List<ObjectRectangle> list)
    {
        foreach (var rect in list)
        {
            var e = (Event)rect.Data.Value!;
            e.EndTime = rect.StartTime + rect.LengthTime;
        }
    }
    
    public static ObjectPanel NewEventPanel(Line line)
    {
        var panel = new ObjectPanel();
        panel.Put.OnPut += PutEvent;
        panel.Move.OnMoveEnd += MoveEvent;
        panel.Resize.OnResizeEnd += ResizeEvent;
        panel.Loaded += (_, _) =>
        {
            if(!panel.isFirstLoaded) return;
            panel.isFirstLoaded = false;
            foreach (var e in line.EventLists.SelectMany(list => list.Events))
            {
                panel.AddObj(new ObjectAdapter(e));
            }
        };
        panel.ParentLine = line;
        return panel;
    }

    private static ObjectAdapter PutFakeCatch(PutTool.PutArgs rect)
    {
        var willPutNote = new FakeCatch(
            rect.Rail,
            0,
            rect.StartTime,
            Page.CatchHeight
        );
        if (Page.CurrLine == Window.track.FreeLine) willPutNote.ExpressionString = "[t]";
        Page.CurrLine.FakeCatch.Add(willPutNote);
        return new ObjectAdapter(willPutNote);
    }

    private static void MoveFakeCatch(List<ObjectRectangle> list)
    {
        foreach (var rect in list)
        {
            var f = (FakeCatch)rect.Data.Value!;
            f.ActualTime = rect.StartTime;
            f.Rail = rect.Rail;
        }
    }
    
    private static void ResizeFakeCatch(List<ObjectRectangle> list) { }

    public static ObjectPanel NewFakeCatchPanel(Line line)
    {
        var panel = new ObjectPanel();
        panel.Put.OnPut += PutFakeCatch;
        panel.Move.OnMoveEnd += MoveFakeCatch;
        panel.Resize.OnResizeEnd += ResizeFakeCatch;
        panel.Loaded += (_, _) =>
        {
            if(!panel.isFirstLoaded) return;
            panel.isFirstLoaded = false;
            foreach (var f in line.FakeCatch)
            {
                panel.AddObj(new ObjectAdapter(f));
            }
        };
        panel.ParentLine = line;
        return panel;
    }

    public void DeleteObj(List<ObjectAdapter> objs)
    {
        foreach (var obj in objs)
        {
            RemoveObj(obj);
            // OperationManager.AddOperation(new RemoveNoteOperation(note, note.ParentLine));
        }
        selectedObjectRectangles.Clear();
    }
    
    public void CopyObj(List<ObjectAdapter> objs)
    {
        Clipboard.Clear();
        if (objs.Count == 0) return;
        Clipboard.AddRange(objs);
        _clipBoardStartTime = objs.Min(obj => obj.StartTime);
    }

    public void CutObj(List<ObjectAdapter> objs)
    {
        Clipboard.Clear();
        if (objs.Count == 0) return;
        Clipboard.AddRange(objs);
        _clipBoardStartTime = objs.Min(obj => obj.StartTime);
        DeleteObj(objs);
    }

    public void PasteObj(List<ObjectAdapter> objs, double startTime)
    {
        foreach (var obj in objs)
        {
            obj.StartTime += startTime - _clipBoardStartTime;
            AddObj(obj);
        }
        if(objs.Count != 0) Update();
        UpdateSelectedObj(objs.Select(obj => obj.Rect!).ToList());
    }

    public void CopyClick(object sender, RoutedEventArgs e)
    {
        CopyObj(selectedObjectRectangles.Select(obj => obj.Data).ToList());
    }

    public void CutClick(object sender, RoutedEventArgs e)
    {
        CutObj(selectedObjectRectangles.Select(obj => obj.Data).ToList());
    }

    public void DeleteClick(object sender, RoutedEventArgs e)
    {
        DeleteObj(selectedObjectRectangles.Select(obj => obj.Data).ToList());
    }

    public void PasteClick(object sender, RoutedEventArgs e)
    {
        double time = GetAlignedPoint(contextMenuOpenPoint).Time;
        PasteObj(Clipboard, time);
    }
    
    public void CopyCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = selectedObjectRectangles.Count > 0;
    }

    public void CopyExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        CopyObj(selectedObjectRectangles.Select(obj => obj.Data).ToList());
    }

    public void PasteCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = Clipboard.Count > 0;
    }

    public void PasteExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        PasteObj(Clipboard, GetAlignedPoint(contextMenuOpenPoint).Time);
    }

    public void CutCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = selectedObjectRectangles.Count > 0;
    }

    public void CutExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        CutObj(selectedObjectRectangles.Select(obj => obj.Data).ToList());
    }

    public void DeleteCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = selectedObjectRectangles.Count > 0;
    }

    public void DeleteExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        DeleteObj(selectedObjectRectangles.Select(obj => obj.Data).ToList());
    }
    
    private Point contextMenuOpenPoint;
    private void ContextMenu_OnOpened(object sender, RoutedEventArgs e)
    {
        contextMenuOpenPoint = Mouse.GetPosition(this);
        CommandManager.InvalidateRequerySuggested();
    }
}

public enum ObjectVisible
{
    Visible, Translucent, Hidden
}