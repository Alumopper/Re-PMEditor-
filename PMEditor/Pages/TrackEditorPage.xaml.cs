﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using PMEditor.Operation;
using PMEditor.Pages;
using PMEditor.Util;

// ReSharper disable once CheckNamespace
namespace PMEditor;

/// <summary>
///     TrackEditorPage.xaml 的交互逻辑
/// </summary>
// ReSharper disable once RedundantExtendsListEntry
public partial class TrackEditorPage : Page
{
    private delegate void InputEvent(); 
    
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once MemberCanBePrivate.Global
    public const double FPS = double.NaN;
    
    // ReSharper disable once InconsistentNaming
    private const byte NOTE = 0;
    // ReSharper disable once InconsistentNaming
    private const byte EVENT = 1;
    // ReSharper disable once InconsistentNaming
    private const byte FAKE_CATCH = 2;
    // ReSharper disable once InconsistentNaming
    private const byte FUNCTION = 3;
    // ReSharper disable once InconsistentNaming
    private const byte MAX_MODE = 4;

    private static readonly SolidColorBrush TapBrush = new(
        Color.FromArgb(102, 109, 209, 213)
    );

    private static readonly SolidColorBrush CatchBrush = new(
        Color.FromArgb(102, 227, 214, 76)
    );

    private static readonly SolidColorBrush FakeCatchBrush = new(
        Color.FromArgb(102, 255, 255, 255)
    );

    //可能会加入的，事件可以翻页，防止事件太多放不下
    private readonly int page = 0;

    //当前能看到的总谱面时长
    private double currDisplayLength = 4;

    private double catchHeight = 1.0;

    //节拍分割
    private int divideNum = 4;

    private Point dragStartPoint = new(-1, -1);

    /// <summary>
    ///     0-note 1-event 2-catch
    /// </summary>
    private byte editingMode;

    private bool eventChange = true;
    private bool fakeCatchChange;
    private bool functionChange;

    private bool init;

    private bool isDeleting;

    private bool isDragging;

    private bool isMouseDown;

    private bool isSelecting;

    //渲染主循环
    private DateTime lastRenderTime = DateTime.MinValue;

    private NBTTrack nbtTrack;

    private bool noteChange;

    private bool previewing;

    //可预览的范围
    private double previewRange;

    //可预览的起始时间点
    private double previewStartTime;
    private List<Event> selectedEvents = new();
    private List<Function> selectedFunctions = new();

    private List<Note> selectedNotes = new();

    private bool selectingFreeLine;

    private Point startPos = new(-1, -1);
    private Event? willPutEvent;
    private Function? willPutFunction;

    private Note? willPutNote;

    public static TrackEditorPage? Instance { get; private set; }

    private static EditorWindow Window => EditorWindow.Instance;

    //private static double SecondsPreBeat => 60.0 / Window.track.BaseBpm;

    //private double SecondsPreDivideBeat => SecondsPreBeat / divideNum;

    //private double PixelPreBeat => pixelPreDividedBeat * divideNum;
 
    // ReSharper disable once MemberCanBePrivate.Global
    public int LineIndex { get; private set; }
    
    public double WheelSpeed = 0.1;

    //public List<double> BeatLineTimes;

    //public List<double> DivideBeatLineTimes;

    private Line CurrLine
    {
        get
        {
            return selectingFreeLine ? Window.track.FreeLine : Window.track.Lines[LineIndex];
        }
    }
    
    private Thread inputThread;
    private ConcurrentQueue<InputEvent> inputQueue = new();
    private bool isRunning = true;

#pragma warning disable CS8618
    public TrackEditorPage()
#pragma warning restore CS8618
    {
        InitializeComponent();
        Instance = this;
        TimeDis.Content = "0.00" + " / " + Window.track.Length.ToString("0.00");
        CompositionTarget.Rendering += Tick;
        //添加note渲染和事件渲染
        foreach (var line in Window.track.AllLines)
        {
            line.Notes.ForEach(note =>
            {
                note.rectangle.Visibility = Visibility.Hidden;
                notePanel.Children.Add(note.rectangle);
                if (note.ActualHoldTime != 0)
                    note.rectangle.Height = GetBottomYFromTime(note.ActualTime + note.ActualHoldTime) -
                                            GetBottomYFromTime(note.ActualTime);
                else
                    note.rectangle.Height = 10;
                note.rectangle.Width = notePanel.ActualWidth / 9;
            });
            foreach (var t in line.EventLists)
            {
                t.Events.ForEach(e =>
                {
                    e.Rectangle.Visibility = Visibility.Hidden;
                    eventPanel.Children.Add(e.Rectangle);
                    e.Rectangle.Height = GetBottomYFromTime(e.EndTime) - GetBottomYFromTime(e.StartTime);
                    e.Rectangle.Width = eventPanel.ActualWidth / 9;
                });
            }
            foreach (var fakeCatch in line.FakeCatch)
            {
                fakeCatch.rectangle.Visibility = Visibility.Hidden;
                notePanel.Children.Add(fakeCatch.rectangle);
                fakeCatch.rectangle.Height = 10;
            }

            foreach (var function in line.Functions)
            {
                function.rectangle.Visibility = Visibility.Hidden;
                functionPanel.Children.Add(function.rectangle);
                function.rectangle.Height = 10;
            }
        }
        SpeedChooseBox.ItemsSource = Settings.currSetting.canSelectedSpeedList;
        LineListView.ItemsSource = Window.track.Lines;
        LineListView.SelectedIndex = 0;
        init = true;
        previewRange = 30.0 / Window.track.Length;
    }
    
    private void Tick(object? sender, EventArgs e)
    {
        if (!double.IsNaN(FPS) && (DateTime.Now - lastRenderTime).TotalMilliseconds < 1000.0 / FPS) return;
        Fps.Text = $"FPS: {(int)(1000.0 / (DateTime.Now - lastRenderTime).TotalMilliseconds)}";
        lastRenderTime = DateTime.Now;
        if (Window.isPlaying) Update();
        //初始化的时候进行第一次绘制
        if (init && notePanel.ActualWidth != 0)
        {
            UpdateNote();
            UpdateEvent();
            UpdateFakeCatch();
            UpdateEventTypeList();
            init = false;
            Window.OperationInfo.Text = "就绪";
        }

        DrawLineAndBeat();
        DrawPreview();
    }
    
    //滚轮滚动
    private void notePanel_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        var num = e.Delta / 60.0;
        //更改fake catch高度
        if (Keyboard.IsKeyDown(Key.LeftAlt))
        {
            catchHeight = Math.Max(0, Math.Min(1, catchHeight + num / 50));
            if (selectedNotes.Count != 0)
            {
                foreach (var note in selectedNotes)
                {
                    if (note is FakeCatch f) f.Height = catchHeight;
                }
            }
        }
        //更改谱面缩放比例
        else if (Keyboard.IsKeyDown(Key.LeftCtrl))
        {
            currDisplayLength = Math.Max(1, Math.Min(30, currDisplayLength + num));
        }
        //更改谱面拍数
        else if (Keyboard.IsKeyDown(Key.LeftShift))
        {
            divideNum = Math.Max(1, divideNum + (int)(num / 2));
        }
        else
        {
            //时间设置
            Window.player.Pause();
            Window.isPlaying = false;
            Window.playerTime += num * WheelSpeed;
        }
        CheckCurrTime();
        //Label设置
        TimeDis.Content = Window.playerTime.ToString("0.00") + " / " + Window.track.Length.ToString("0.00");
        HeightDis.Content = $"Fake Catch高度: {catchHeight:F2}";
        noteChange = true;
        eventChange = true;
        fakeCatchChange = true;
        functionChange = true;
        Update();
    }

    private void CheckCurrTime()
    {
        if (Window.playerTime < 0)
        {
            Window.playerTime = 0;
        }
    }
    
    //绘制辅助线
    private void DrawLineAndBeat()
    {
        //移除线
        for (var i = 0; i < notePanel.Children.Count; i++)
        {
            if (notePanel.Children[i] is not System.Windows.Shapes.Line line) continue;
            line.Stroke = null;
            notePanel.Children.RemoveAt(i);
            i--;
        }

        //移除拍数
        BeatDisplay.Children.Clear();
        //绘制节拍线
        //开始绘制
        //整拍
        //基准线相对坐标计算
        var currTime = Window.playerTime;
        var startMeasure = 0;
        double drawTime = 0;
        for (var i = 0; i < Window.track.LineTimes.Count; i++)
        {
            if (Window.track.LineTimes[i] <= currTime && Window.track.LineTimes[i + 1] > currTime)
            {
                startMeasure = i;
                drawTime = Window.track.LineTimes[i];
                break;
            }
        }

        //绘制
        for (var i = startMeasure; drawTime < currTime + currDisplayLength ; i++)
        {
            var (_, length) = Window.track.GetTimeRange(i);
            var div = length / divideNum;
            //绘制主线
            if (drawTime >= currTime)
            {
                var y = notePanel.ActualHeight - GetBottomYFromTime(drawTime);
                System.Windows.Shapes.Line line = new()
                {
                    X1 = 0,
                    X2 = notePanel.ActualWidth,
                    Y1 = y,
                    Y2 = y,
                    Stroke = new SolidColorBrush(Color.FromRgb(237, 70, 255)),
                    StrokeThickness = 3
                };
                //拍数
                TextBlock textBlock = new()
                {
                    Text = i.ToString(),
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 12
                };
                BeatDisplay.Children.Add(textBlock);
                Canvas.SetTop(textBlock, y);
                notePanel.Children.Add(line); 
            }
            drawTime += div;
            for (var j = 0; j < divideNum - 1; j++)
            {
                if(drawTime < currTime)
                {
                    drawTime += div;
                    continue;
                }
                var y = notePanel.ActualHeight - GetBottomYFromTime(drawTime);
                System.Windows.Shapes.Line line = new()
                {
                    X1 = 0,
                    X2 = notePanel.ActualWidth,
                    Y1 = y,
                    Y2 = y,
                    Stroke = new SolidColorBrush(Color.FromRgb(254, 235, 255)),
                    StrokeThickness = 1
                };
                notePanel.Children.Add(line);
                drawTime += div;
            }
        }
    }

    //绘制谱面预览
    private void DrawPreview()
    {
        var width = trackPreview.ActualWidth;
        var height = trackPreview.ActualHeight;
        foreach (var item in trackPreview.Children)
            switch (item)
            {
                case System.Windows.Shapes.Line line:
                    line.Stroke = null;
                    break;
                case Rectangle rect:
                    rect.Fill = null;
                    break;
            }
        trackPreview.Children.Clear();
        //调整预览范围
        if (Window.playerTime < previewStartTime)
            previewStartTime = Window.playerTime;
        else if (Window.playerTime > previewStartTime + previewRange * Window.track.Length)
            previewStartTime = Window.playerTime - previewRange * Window.track.Length;
        //绘制谱面
        foreach (var note in Window.track.AllLines.SelectMany(line => line.Notes))
            if (note.type == NoteType.Hold)
            {
                var x = note.Rail * width / 9;
                var y = (note.ActualTime - previewStartTime) / (Window.track.Length * previewRange) * height;
                var w = width / 9;
                var h = note.ActualHoldTime / (Window.track.Length * previewRange) * height;
                Rectangle rect = new()
                {
                    Width = w,
                    Height = h,
                    Fill = new SolidColorBrush(EditorColors.holdColor)
                };
                trackPreview.Children.Add(rect);
                Canvas.SetLeft(rect, x);
                Canvas.SetBottom(rect, y);
            }
            else
            {
                var x = note.Rail * width / 9;
                var y = (note.ActualTime - previewStartTime) / (Window.track.Length * previewRange) * height;
                var w = width / 9;
                const double h = 2;
                Rectangle rect = new()
                {
                    Width = w,
                    Height = h,
                    Fill = note.type == NoteType.Tap
                        ? new SolidColorBrush(EditorColors.tapColor)
                        : new SolidColorBrush(EditorColors.catchColor)
                };
                trackPreview.Children.Add(rect);
                Canvas.SetLeft(rect, x);
                Canvas.SetBottom(rect, y);
            }

        //绘制时间线
        var timeLineY = height - (Window.playerTime - previewStartTime) / (Window.track.Length * previewRange) * height;
        System.Windows.Shapes.Line timeLine = new()
        {
            X1 = 0,
            X2 = width,
            Y1 = timeLineY,
            Y2 = timeLineY,
            Stroke = new SolidColorBrush(Colors.Red),
            StrokeThickness = 2,
            IsHitTestVisible = false
        };
        trackPreview.Children.Add(timeLine);
    }

    private void DrawTrackWithEvent()
    {
        //清空画布
        trackPreviewWithEvent.Children.OfType<Rectangle>()
            .ToList()
            .ForEach(t =>
            {
                t.Fill = null;
                trackPreviewWithEvent.Children.Remove(t);
            });
        var mapLength = Settings.currSetting.MapLength;
        var windowHeight = trackPreviewWithEvent.ActualHeight;
        var windowWidth = trackPreviewWithEvent.ActualWidth;
        trackPreviewWithEvent.Children.Clear();
        //绘制note
        //如果谱面正在播放，就直接让note按照对应速度移动
        //如果谱面没有播放，就计算note的位置
        var time = Window.playerTime;
        var noteWidth = windowWidth / 9;
        foreach (var line in nbtTrack.lines)
        {
            if (editingMode == 2) continue;
            foreach (var note in line.notes)
            {
                if (note is NBTHold hold)
                {
                    if (hold.summonTime <= time && time <= hold.judgeTime + hold.holdTime)
                    {
                        Rectangle noteRec = new()
                        {
                            Width = noteWidth,
                            Height = Math.Max(hold.holdLength / mapLength * windowHeight, 0),
                            Fill = new SolidColorBrush(EditorColors.holdColor)
                        };
                        //计算note的位置
                        var i = time;
                        double locate = 0;
                        if (hold.Expression != null)
                        {
                            var exp = hold.Expression;
                            exp.Parameters["t"] = (int)((hold.judgeTime - i) * Settings.currSetting.Tick);
                            exp.Parameters["l"] = mapLength;
                            locate = Convert.ToDouble(exp.Evaluate()!);
                            exp.Parameters["t"] = (int)exp.Parameters["t"]! + 1;
                            var nextLocate = Convert.ToDouble(exp.Evaluate()!);
                            var delta = (i - hold.judgeTime) * Settings.currSetting.Tick - (int)exp.Parameters["t"]! + 1;
                            locate += (nextLocate - locate) * delta;
                        }
                        else
                        {
                            for (;
                                 i < hold.judgeTime + hold.holdTime - 1 / Settings.currSetting.Tick;
                                 i += 1 / Settings.currSetting.Tick)
                                locate += line.line.GetSpeed(i) / Settings.currSetting.Tick;
                            //计算差值保证帧数
                            var delta = hold.judgeTime + hold.holdTime - i;
                            locate += line.line.GetSpeed(i) * delta;
                            locate -= hold.holdLength;
                        }
                        locate = locate / mapLength * windowHeight;
                        trackPreviewWithEvent.Children.Add(noteRec);
                        Canvas.SetBottom(noteRec, locate);
                        Canvas.SetLeft(noteRec, note.Rail * noteWidth);
                    }
                }
                else
                {
                    if (note.summonTime <= time && time <= note.judgeTime)
                    {
                        SolidColorBrush solidColorBrush;
                        if (note is NBTFakeCatch fakeCatch)
                            solidColorBrush = new SolidColorBrush(FakeCatch.GetColor(fakeCatch.height));
                        else
                            solidColorBrush = note.type == (int)NoteType.Tap
                                ? new SolidColorBrush(EditorColors.tapColor)
                                : new SolidColorBrush(EditorColors.catchColor);
                        Rectangle noteRec = new()
                        {
                            Width = noteWidth,
                            Height = 10,
                            Fill = solidColorBrush
                        };
                        //计算note的位置
                        var i = time;
                        double locate = 0;
                        if (note.Expression != null)
                        {
                            var exp = note.Expression;
                            var t = (int)((note.judgeTime - i) * Settings.currSetting.Tick);
                            exp.Parameters["t"] = t;
                            exp.Parameters["l"] = mapLength;
                            locate = Convert.ToDouble(exp.Evaluate()!);
                            exp.Parameters["t"] = t + 1;
                            var nextLocate = Convert.ToDouble(exp.Evaluate()!);
                            var delta = (note.judgeTime - i) * Settings.currSetting.Tick - t;
                            locate += (nextLocate - locate) * delta;
                        }
                        else
                        {
                            for (; i < note.judgeTime - 1 / Settings.currSetting.Tick; i += 1 / Settings.currSetting.Tick)
                                locate += line.line.GetSpeed(i) / Settings.currSetting.Tick;
                            //计算差值保证帧数
                            var delta = note.judgeTime - i;
                            locate += line.line.GetSpeed(i) * delta;
                        }
                        locate = locate / mapLength * windowHeight;
                        trackPreviewWithEvent.Children.Add(noteRec);
                        Canvas.SetBottom(noteRec, locate);
                        Canvas.SetLeft(noteRec, note.Rail * noteWidth);
                    }
                }
            }
        }
        //播放note音效
        //目前显示的时间范围
        var minTime = Window.playerTime;
        foreach (var t in Window.track.Lines)
        {
            foreach (var note in t.Notes)
            {
                if (note.type == NoteType.Hold)
                {
                    if (minTime <= note.ActualTime + note.ActualHoldTime)
                    {
                        //在判定线上方
                        if (minTime <= note.ActualTime)
                        {
                            if (note.hasJudged) note.hasJudged = false;
                        }
                        else
                        {
                            //如果未被判定过且谱面正在被播放
                            if (!note.hasJudged && Window.isPlaying)
                            {
                                Window.tapSoundManager.AppendSoundRequest();
                                note.hasJudged = true;
                            }
                        }
                    }

                    continue;
                }

                //在判定线上方
                if (minTime <= note.ActualTime)
                {
                    if (note.hasJudged) note.hasJudged = false;
                }
                //在判定线下方
                else
                {
                    if (note.hasJudged || !Window.isPlaying) continue;
                    //如果未被判定过且谱面正在被播放
                    if (note.type == NoteType.Tap)
                    {
                        Window.tapSoundManager.AppendSoundRequest();
                    }
                    else
                    {
                        Window.catchSoundManager.AppendSoundRequest();
                    }
                    note.hasJudged = true;
                }
            }
        }
    }

    //更新
    private void Update()
    {
        if (Window.isPlaying)
        {
            Window.playerTime = Window.player.Position.TotalSeconds;
            //更新时间
            TimeDis.Content = Window.playerTime.ToString("0.00") + " / " + Window.track.Length.ToString("0.00");
            HeightDis.Content = $"Fake Catch高度: {catchHeight:F2}";
        }

        if (previewing)
        {
            //更新谱面预览
            DrawTrackWithEvent();
        }
        else
        {
            //更新note和事件
            if (Window.isPlaying || noteChange)
            {
                UpdateNote();
                noteChange = false;
            }

            if (editingMode == EVENT && (Window.isPlaying || eventChange))
            {
                UpdateEvent();
                eventChange = false;
            }

            if (editingMode == FAKE_CATCH && (Window.isPlaying || fakeCatchChange))
            {
                UpdateFakeCatch();
                fakeCatchChange = false;
            }

            if (editingMode == FUNCTION && (Window.isPlaying || functionChange))
            {
                UpdateFunction();
                functionChange = false;
            }
        }

        Window.tapSoundManager.PlaySound();
        Window.catchSoundManager.PlaySound();
    }
    
    public void UpdateNote()
    {
        //目前显示的时间范围
        var minTime = Window.playerTime;
        var maxTime = minTime + currDisplayLength;
        var lines = Window.track.Lines.Concat(new[]{ Window.track.FreeLine}).ToList();
        foreach (var note in lines.SelectMany(t => t.Notes))
        {
            if (note.type == NoteType.Hold)
            {
                if (minTime <= note.ActualTime + note.ActualHoldTime)
                {
                    //在判定线上方
                    if (minTime <= note.ActualTime)
                    {
                        if (note.hasJudged) note.hasJudged = false;
                    }
                    else
                    {
                        //如果未被判定过且谱面正在被播放
                        if (!note.hasJudged && Window.isPlaying)
                        {
                            Window.tapSoundManager.AppendSoundRequest();
                            note.hasJudged = true;
                        }
                    }

                    //在可视区域内
                    if (note.ActualTime < maxTime)
                    {
                        note.rectangle.Visibility = Visibility.Visible;
                        var qwq = Math.Max(minTime, note.ActualTime); //末端
                        var pwp = Math.Min(note.ActualTime + note.ActualHoldTime, maxTime); //上端
                        note.rectangle.Height = GetBottomYFromTime(pwp) - GetBottomYFromTime(qwq);
                        Canvas.SetBottom(note.rectangle, GetBottomYFromTime(qwq));
                        Canvas.SetLeft(note.rectangle, note.Rail * notePanel.ActualWidth / 9);
                    }
                    else
                    {
                        note.rectangle.Visibility = Visibility.Hidden;
                    }
                }
                else
                {
                    note.rectangle.Visibility = Visibility.Hidden;
                }

                continue;
            }

            //在判定线上方
            if (minTime <= note.ActualTime)
            {
                if (note.hasJudged) note.hasJudged = false;
                //在可视区域内
                if (note.ActualTime <= maxTime)
                {
                    note.rectangle.Visibility = Visibility.Visible;
                    Canvas.SetBottom(note.rectangle, GetBottomYFromTime(note.ActualTime));
                    Canvas.SetLeft(note.rectangle, note.Rail * notePanel.ActualWidth / 9);
                }
                else
                {
                    note.rectangle.Visibility = Visibility.Hidden;
                }
            }
            //在判定线下方
            else
            {
                //如果未被判定过且谱面正在被播放
                if (!note.hasJudged && Window.isPlaying)
                {
                    if (note.type == NoteType.Tap)
                        Window.tapSoundManager.AppendSoundRequest();
                    else
                        Window.catchSoundManager.AppendSoundRequest();
                    note.hasJudged = true;
                }

                note.rectangle.Visibility = Visibility.Hidden;
            }
        }
    }

    public void UpdateFakeCatch()
    {
        //目前显示的时间范围
        var minTime = Window.playerTime;
        var maxTime = minTime + currDisplayLength;
        foreach (var fakeCatch in CurrLine.FakeCatch)
        {
            //在判定线上方
            if (minTime <= fakeCatch.ActualTime)
            {
                //在可视区域内
                if (fakeCatch.ActualTime <= maxTime)
                {
                    fakeCatch.rectangle.Visibility = Visibility.Visible;
                    Canvas.SetBottom(fakeCatch.rectangle, GetBottomYFromTime(fakeCatch.ActualTime));
                    Canvas.SetLeft(fakeCatch.rectangle, fakeCatch.Rail * notePanel.ActualWidth / 9);
                }
                else
                {
                    fakeCatch.rectangle.Visibility = Visibility.Hidden;
                }
            }
            //在判定线下方
            else
            {
                fakeCatch.rectangle.Visibility = Visibility.Hidden;
            }
        }
    }


    public void UpdateFunction()
    {
        //目前显示的时间范围
        var minTime = Window.playerTime;
        var maxTime = minTime + currDisplayLength;
        for (var i = 0; i < Window.track.Lines.Count; i++)
            foreach (var function in CurrLine.Functions)
                //在判定线上方
                if (minTime <= function.Time)
                {
                    //在可视区域内
                    if (function.Time <= maxTime)
                    {
                        function.rectangle.Visibility = Visibility.Visible;
                        Canvas.SetBottom(function.rectangle, GetBottomYFromTime(function.Time));
                        Canvas.SetLeft(function.rectangle, function.Rail * functionPanel.ActualWidth / 9);
                    }
                    else
                    {
                        function.rectangle.Visibility = Visibility.Hidden;
                    }
                }
                //在判定线下方
                else
                {
                    function.rectangle.Visibility = Visibility.Hidden;
                }
    }

    public void UpdateEvent()
    {
        //目前显示的时间范围
        var minTime = Window.playerTime;
        var maxTime = minTime + currDisplayLength;
        foreach (var t in Window.track.Lines)
        {
            for (var j = 0; j < t.EventLists.Count; j++)
            {
                foreach (var e in t.EventLists[j].Events)
                {
                    if (minTime <= e.EndTime && e.StartTime < maxTime)
                    {
                        e.Rectangle.Visibility = Visibility.Visible;
                        Canvas.SetBottom(e.Rectangle, GetBottomYFromTime(e.StartTime));
                        Canvas.SetLeft(e.Rectangle, (j - page * 9) * notePanel.ActualWidth / 9);
                    }
                    else
                    {
                        e.Rectangle.Visibility = Visibility.Hidden;
                    }
                }
            }
        }
        UpdateFunctionCurve();
    }

    public void UpdateEventTypeList()
    {
        for (var i = 0; i < 9; i++)
        {
            var textBlock = (ColumnInfo.Children[i] as TextBlock)!;
            string text;
            if (editingMode != 1)
            {
                text = i.ToString();
            }
            else
            {
                var type = CurrLine.GetType(i + page * 9);
                text = Event.TypeString(type);
                if (type == EventType.Unknown)
                    textBlock.Foreground = new SolidColorBrush(Colors.Gray);
                else if (CurrLine.EventLists[i + page * 9].IsMainEvent())
                    textBlock.Foreground = new SolidColorBrush(Colors.Orange);
                else
                    textBlock.Foreground = new SolidColorBrush(Colors.Olive);
            }

            textBlock.Text = text;
        }
    }

    public void UpdateFunctionCurve()
    {
        foreach (var line in Window.track.Lines)
        {
            foreach (var e in line.EventLists.SelectMany(el => el.Events))
            {
                if (e.IsHeaderEvent)
                {
                    var rs = e.EventGroup.Select(item => item.Rectangle).ToList();
                    EventRectangle.DrawFunction(rs);
                    rs.Clear();
                }

                e.Rectangle.UpdateText();
            }
        }
    }

    //放置hold，或者拖动note
    private void notePanel_MouseMove(object sender, MouseEventArgs e)
    {
        //note将要放置，预览位置
        var (_, measure, beat, endPos, _) = GetAlignedPoint(e.GetPosition(notePanel));
        if (!Window.isPlaying && !isSelecting)
        {
            if (startPos != new Point(-1, -1))
            {
                //hold预览
                if (endPos.Y > startPos.Y)
                {
                    notePreview.Height = endPos.Y - startPos.Y;
                    notePreview.Fill = Window.puttingTap ? TapBrush : CatchBrush;
                    //绘制note矩形
                    notePreview.Visibility = Visibility.Visible;
                    Canvas.SetLeft(notePreview, startPos.X);
                    Canvas.SetBottom(notePreview, startPos.Y);
                }
            }
            else
            {
                //tap预览
                notePreview.Height = 10;
                //获取鼠标位置，生成note位置预览
                FlushNotePreview();
                //绘制note矩形
                notePreview.Visibility = Visibility.Visible;
                Canvas.SetLeft(notePreview, endPos.X);
                Canvas.SetBottom(notePreview, endPos.Y);
            }
            BeatDis.Content = $"beat {measure}:{beat}/{divideNum}";
        }
        else
        {
            if (isSelecting || isDragging)
            {
                BeatDis.Content = $"beat {measure}:{beat}/{divideNum}";
                if (isDragging)
                {
                    if (selectedNotes[0].rectangle.IsResizing)
                    {
                        var n = selectedNotes[0];
                        var y = Math.Abs(endPos.Y - GetBottomYFromTime(n.ActualTime));
                        n.rectangle.Height = y;
                    }
                    else
                    {
                        //拖动note
                        foreach (var note in selectedNotes)
                        {
                            //获取鼠标移动的位置
                            var delta = endPos - dragStartPoint;
                            dragStartPoint = endPos;
                            //移动note
                            Canvas.SetLeft(note.rectangle, Canvas.GetLeft(note.rectangle) + delta.X);
                            Canvas.SetBottom(note.rectangle, Canvas.GetBottom(note.rectangle) + delta.Y);
                        }
                    }
                }
            }

            notePreview.Visibility = Visibility.Collapsed;
        }
    }

    public void FlushNotePreview()
    {
        if (editingMode == 2)
            notePreview.Fill = FakeCatchBrush;
        else if (Window.puttingTap)
            notePreview.Fill = TapBrush;
        else
            notePreview.Fill = CatchBrush;
    }

    private void notePanel_MouseLeave(object sender, MouseEventArgs e)
    {
        notePreview.Visibility = Visibility.Collapsed;
    }

    //放置note
    private void notePanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (Window.isPlaying) return;
        if (startPos != new Point(-1, -1)) return;
        (var time ,_ ,_ ,startPos, var rail) = GetAlignedPoint(e.GetPosition(notePanel));
        if (editingMode == 0 && CurrLine.ClickOnNote(time, rail) || editingMode == 2 && CurrLine.ClickOnFakeCatch(time, rail))
        {
            startPos = new Point(-1, -1);
        }
    }

    //放置note，完成拖动
    private void notePanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        //获取鼠标位置
        var (time, _, _, endPos, _) = GetAlignedPoint(e.GetPosition(notePanel));
        //拖动note
        if (isDragging)
        {
            if (selectedNotes[0].rectangle.IsResizing)
            {
                selectedNotes[0].ActualHoldTime = time - selectedNotes[0].ActualTime;
            }
            else
            {
                foreach (var note in selectedNotes)
                {
                    var x = Canvas.GetLeft(note.rectangle);
                    var y = Canvas.GetBottom(note.rectangle);
                    Point point = new(x, notePanel.ActualHeight - y);
                    (var noteTime,_,_, point,var noteRail) = GetAlignedPoint(point);
                    note.Rail = noteRail;
                    if (note.Rail < 0) note.Rail = 0;
                    note.ActualTime = noteTime;
                    Canvas.SetBottom(note.rectangle, point.Y);
                }
            }
            UpdateSelectedNote(selectedNotes);
            isDragging = false;
            return;
        }

        if (Window.isPlaying) return;
        if (startPos == new Point(-1, -1)) return;
        if (selectedNotes.Count != 0)
        {
            UpdateSelectedNote(new List<Note>());
            infoFrame.Content = null;
            startPos = new Point(-1, -1);
            return;
        }

        //获取时间
        var startTime = GetTimeFromBottomY(startPos.Y);
        var endTime = GetTimeFromBottomY(endPos.Y);
        var rail = (int)Math.Round(startPos.X * 9 / notePanel.ActualWidth);
        if (CurrLine.ClickOnEvent(endTime, rail))
        {
            startPos = new Point(-1, -1);
            return;
        }

        if (editingMode == 2)
        {
            willPutNote ??= new FakeCatch(
                rail,
                0,
                startTime,
                catchHeight
            );
            if (CurrLine == Window.track.FreeLine) willPutNote.ExpressionString = "[t]";
            willPutNote.rectangle.Height = 10;
            willPutNote.rectangle.Width = notePreview.Width;
        }
        //放置note
        else if (endTime - startTime > 0)
        {
            willPutNote ??= new Note(
                rail,
                (int)NoteType.Hold,
                0,
                false,
                startTime,
                actualHoldTime: endTime - startTime,
                isCurrentLineNote: true);
            if (CurrLine == Window.track.FreeLine) willPutNote.ExpressionString = "[t]";
            willPutNote.rectangle.Height = endPos.Y - startPos.Y;
            willPutNote.rectangle.Width = notePreview.Width;
        }
        else
        {
            willPutNote ??= new Note(
                rail,
                (int)(Window.puttingTap ? NoteType.Tap : NoteType.Catch),
                0,
                false,
                startTime,
                true);
            if (CurrLine == Window.track.FreeLine) willPutNote.ExpressionString = "[t]";
            willPutNote.rectangle.Height = 10;
            willPutNote.rectangle.Width = notePreview.Width;
        }

        willPutNote.parentLine = CurrLine;
        if (!CurrLine.IsNoteOverLap(willPutNote))
        {
            if (willPutNote is FakeCatch f)
            {
                CurrLine.FakeCatch.Add(f);
                fakeCatchChange = true;
            }
            else
            {
                CurrLine.Notes.Add(willPutNote);
                noteChange = true;
            }

            notePanel.Children.Add(willPutNote.rectangle);
            Canvas.SetLeft(willPutNote.rectangle, willPutNote.Rail * notePanel.ActualWidth / 9);
            Canvas.SetBottom(willPutNote.rectangle, startPos.Y);
            OperationManager.AddOperation(new PutNoteOperation(willPutNote, CurrLine));
        }

        willPutNote = null;
        startPos = new Point(-1, -1);
    }

    //调整note大小
    private void notePanel_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        notePreview.Width = notePanel.ActualWidth / 9;
        foreach (var line in Window.track.AllLines)
        {
            line.Notes.ForEach(note => { note.rectangle.Width = notePanel.ActualWidth / 9; });
            line.FakeCatch.ForEach(f => { f.rectangle.Width = notePanel.ActualWidth / 9; });
        }
    }

    //调整事件大小
    private void eventPanel_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        eventPreview.Width = eventPanel.ActualWidth / 9;
        foreach (var line in Window.track.Lines)
            line.EventLists.ForEach(el =>
            {
                el.Events.ForEach(@event => { @event.Rectangle.Width = eventPanel.ActualWidth / 9; });
            });
    }

    //切换速度
    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        float speed;
        try
        {
            speed = float.Parse((e.AddedItems[0] as string)!);
        }
        catch (Exception)
        {
            speed = 1.0f;
        }

        //速度设置
        Window.player.SpeedRatio = speed;
    }

    //切换速度
    private void speedChooseBox_MouseEnter(object sender, MouseEventArgs e)
    {
        SpeedChooseBox.Focusable = true;
    }

    //切换速度
    private void speedChooseBox_MouseLeave(object sender, MouseEventArgs e)
    {
        SpeedChooseBox.Focusable = false;
    }

    //切换判定线
    private void lineListView_SelectionChanged(object sender, SelectionChangedEventArgs args)
    {
        if (isDeleting)
        {
            isDeleting = false;
            return;
        }
        if(LineListView.SelectedIndex == -1) return;

        if (editingMode == 0)
        {
            foreach (var note in CurrLine.Notes)
                if (note.type == NoteType.Tap)
                    note.Color = EditorColors.tapColorButNotOnThisLine;
                else if (note.type == NoteType.Hold)
                    note.Color = EditorColors.holdColorButNotOnThisLine;
                else
                    note.Color = EditorColors.catchColorButNotOnThisLine;
            LineIndex = LineListView.SelectedIndex;
            selectingFreeLine = false;
            foreach (var note in CurrLine.Notes)
                if (note.type == NoteType.Tap)
                    note.Color = EditorColors.tapColor;
                else if (note.type == NoteType.Hold)
                    note.Color = EditorColors.holdColor;
                else
                    note.Color = EditorColors.catchColor;
        }
        else if (editingMode == 1)
        {
            foreach (var el in CurrLine.EventLists)
                el.Events.ForEach(e => { e.Rectangle.Visibility = Visibility.Hidden; });
            LineIndex = LineListView.SelectedIndex;
            selectingFreeLine = false;
            foreach (var el in CurrLine.EventLists)
                el.Events.ForEach(e => { e.Rectangle.Visibility = Visibility.Visible; });
            UpdateEventTypeList();
        }
        else
        {
            CurrLine.FakeCatch.ForEach(e => { e.rectangle.Visibility = Visibility.Hidden; });
            LineIndex = LineListView.SelectedIndex;
            selectingFreeLine = false;
            CurrLine.FakeCatch.ForEach(e => { e.rectangle.Visibility = Visibility.Visible; });
        }
    }

    //删除
    private void MenuItem_Click(object sender, RoutedEventArgs e)
    {
        isDeleting = true;
        var i = LineIndex - 1;
        if (i < 0) i = 0;
        if (Window.track.Lines.Count == 1) Window.track.Lines.Add(new Line(0));
        LineListView.SelectedIndex = i;
        OperationManager.AddOperation(new RemoveLineOperation(CurrLine));
        Window.track.Lines.RemoveAt(LineIndex);
        LineIndex = i;
    }

    //重命名
    private void MenuItem_Click_1(object sender, RoutedEventArgs e)
    {
        var box = (((sender as MenuItem)!.Parent as ContextMenu)!.PlacementTarget as Grid)!;
        box.Children[0].Visibility = Visibility.Collapsed;
        (box.Children[1] as TextBox)!.Text = CurrLine.Id;
        (box.Children[1] as TextBox)!.SelectAll();
        box.Children[1].Visibility = Visibility.Visible;
        box.Children[1].Focus();
    }

    //属性
    private void MenuItem_Click_2(object sender, RoutedEventArgs e)
    {
        infoFrame.Content = new LineInfoFrame(CurrLine, LineIndex);
    }

    //新建判定线
    private void Button_Click(object sender, RoutedEventArgs e)
    {
        Line qwq = new(0);
        Window.track.Lines.Add(qwq);
        LineIndex = Window.track.Lines.Count - 1;
        LineListView.SelectedIndex = LineIndex;
    }

    //判定线重命名
    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        var box = ((sender as TextBox)!.Parent as Grid)!;
        ChangeLineId((sender as TextBox)!.Text);
        (box.Children[0] as Label)!.Content = CurrLine.Id;
        box.Children[0].Visibility = Visibility.Visible;
        box.Children[1].Visibility = Visibility.Collapsed;
    }

    //判定线重命名
    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var box = ((sender as TextBox)!.Parent as Grid)!;
            ChangeLineId((sender as TextBox)!.Text);
            (box.Children[0] as Label)!.Content = CurrLine.Id;
            box.Children[0].Visibility = Visibility.Visible;
            box.Children[1].Visibility = Visibility.Collapsed;
        }
    }

    //判定线重命名
    private void ChangeLineId(string lineId)
    {
        CurrLine.Id = lineId;
    }

    //放置事件
    private void eventPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (Window.isPlaying) return;
        if (startPos != new Point(-1, -1)) return;
        (var time, _, _, startPos, var rail) = GetAlignedPoint(e.GetPosition(eventPanel));
        if (CurrLine.ClickOnEvent(time, rail))
        {
            startPos = new Point(-1, -1);
        }
    }

    //放置事件
    private void eventPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        //获取鼠标位置
        var (time, _, _, endPos, _) = GetAlignedPoint(e.GetPosition(eventPanel));
        //拖动事件
        if (isDragging)
        {
            if (selectedEvents[0].Rectangle.IsResizing == 1)
            {
                var ev = selectedEvents[0];
                //调整结束时间
                ev.EndTime = time;
            }
            else if (selectedEvents[0].Rectangle.IsResizing == -1)
            {
                var ev = selectedEvents[0];
                //调整开始时间
                ev.StartTime = time;
            }
            else
            {
                foreach (var @event in selectedEvents)
                {
                    var y = Canvas.GetBottom(@event.Rectangle);
                    var i = @event.StartTime;
                    @event.StartTime = GetTimeFromBottomY(y);
                    @event.EndTime += @event.StartTime - i;
                }

                UpdateSelectedEvent(selectedEvents);
            }

            isDragging = false;
            return;
        }

        if (Window.isPlaying) return;
        if (startPos == new Point(-1, -1)) return;
        if (selectedEvents.Count != 0)
        {
            UpdateSelectedEvent(new List<Event>());
            infoFrame.Content = null;
            startPos = new Point(-1, -1);
            return;
        }

        //获取时间
        var startTime = GetTimeFromBottomY(startPos.Y);
        var endTime = GetTimeFromBottomY(endPos.Y);
        //计算轨道
        var rail = (int)Math.Round(startPos.X * 9 / eventPanel.ActualWidth) + page * 9;
        if (CurrLine.ClickOnEvent(endTime, rail))
        {
            startPos = new Point(-1, -1);
            return;
        }

        //最小时间
        if (Math.Abs(endTime - startTime) < 0.0001)
        {
            startPos = new Point(-1, -1);
            return;
        }

        //获取事件类型
        var type = CurrLine.GetType(rail);
        double v;
        if (type == EventType.Unknown)
        {
            type = EventType.Speed;
            v = Event.GetDefaultValue(type);
            CurrLine.SetType(type, rail + page * 9);
        }
        else
        {
            v = CurrLine.GetLastEventValue(rail);
        }

        Event.puttingEvent = type;
        //获取事件默认值
        //放置事件
        willPutEvent ??= new Event(
            startTime,
            endTime,
            "Linear",
            v,
            v
        );
        willPutEvent.Rectangle.Height = endPos.Y - startPos.Y;
        willPutEvent.Rectangle.Width = eventPreview.Width;
        if (!CurrLine.EventLists[rail].Events.Contains(willPutEvent))
        {
            eventPanel.Children.Add(willPutEvent.Rectangle);
            Canvas.SetLeft(willPutEvent.Rectangle, rail * eventPanel.ActualWidth / 9);
            Canvas.SetBottom(willPutEvent.Rectangle, startPos.Y);
            CurrLine.EventLists[rail].Events.Add(willPutEvent);
            willPutEvent.ParentList = CurrLine.EventLists[rail];
            eventChange = true;
            OperationManager.AddOperation(new PutEventOperation(willPutEvent, CurrLine.EventLists[rail]));
            //更新这个轨道的事件组
            CurrLine.EventLists[rail].GroupEvent();
        }

        willPutEvent = null;
        startPos = new Point(-1, -1);
        eventPreview.Visibility = Visibility.Collapsed;
        UpdateEventTypeList();
    }

    //拖动事件或者放置事件，改变事件的时间
    private void eventPanel_MouseMove(object sender, MouseEventArgs e)
    {
        if (!Window.isPlaying && !isSelecting)
        {
            var (_, measure, beat ,endPos, _) = GetAlignedPoint(e.GetPosition(eventPanel));
            if (startPos != new Point(-1, -1))
            {
                if (endPos.Y > startPos.Y)
                {
                    eventPreview.Height = endPos.Y - startPos.Y;
                    //绘制note矩形
                    Canvas.SetLeft(eventPreview, startPos.X);
                    Canvas.SetBottom(eventPreview, startPos.Y);
                    eventPreview.Visibility = Visibility.Visible;
                }
            }
            BeatDis.Content = $"beat {measure}:{beat}/{divideNum}";
        }
        else
        {
            if (isSelecting || isDragging)
            {
                var (_, measure, beat ,endPos, _) = GetAlignedPoint(e.GetPosition(eventPanel));
                BeatDis.Content = $"beat {measure}:{beat}/{divideNum}";
                if (isDragging)
                {
                    if (selectedEvents[0].Rectangle.IsResizing == 1)
                    {
                        var ev = selectedEvents[0];
                        //改结束时间
                        var y = Math.Abs(endPos.Y - GetBottomYFromTime(ev.StartTime));
                        ev.Rectangle.Height = y;
                    }
                    else if (selectedEvents[0].Rectangle.IsResizing == -1)
                    {
                        var ev = selectedEvents[0];
                        //改开始时间
                        var y = Math.Abs(GetBottomYFromTime(ev.EndTime) - endPos.Y);
                        ev.Rectangle.Height = y;
                        Canvas.SetBottom(ev.Rectangle, endPos.Y);
                    }
                    else
                    {
                        endPos.X = dragStartPoint.X; //只能竖着拖
                        //获取鼠标移动的位置
                        var delta = endPos - dragStartPoint;
                        dragStartPoint = endPos;
                        if (selectedEvents.Any(@event => Canvas.GetBottom(@event.Rectangle) + delta.Y < 0))
                        {
                            return;
                        }
                        //拖动事件
                        foreach (var @event in selectedEvents)
                        {
                            //移动事件
                            Canvas.SetLeft(@event.Rectangle, Canvas.GetLeft(@event.Rectangle) + delta.X);
                            Canvas.SetBottom(@event.Rectangle, Canvas.GetBottom(@event.Rectangle) + delta.Y);
                        }
                    }
                }
            }
            else
            {
                notePreview.Visibility = Visibility.Collapsed;
                eventPanel.Visibility = Visibility.Collapsed;
            }
        }
    }


    private void functionPanel_MouseMove(object sender, MouseEventArgs e)
    {
        if (!Window.isPlaying && !isSelecting)
        {
            var (_, measure, beat ,endPos, _) = GetAlignedPoint(e.GetPosition(functionPanel));
            //tap预览
            functionPreview.Height = 10;
            //获取鼠标位置，生成函数位置预览
            functionPreview.Visibility = Visibility.Visible;
            Canvas.SetLeft(functionPreview, endPos.X);
            Canvas.SetBottom(functionPreview, endPos.Y);
            //计算拍数
            BeatDis.Content = $"beat {measure}:{beat}/{divideNum}";
        }
        else
        {
            if (isSelecting || isDragging)
            {
                var (_, measure, beat ,endPos, _) = GetAlignedPoint(e.GetPosition(functionPanel));
                BeatDis.Content = $"beat {measure}:{beat}/{divideNum}";
                if (isDragging)
                {
                    //拖动note
                    foreach (var function in selectedFunctions)
                    {
                        //获取鼠标移动的位置
                        var delta = endPos - dragStartPoint;
                        dragStartPoint = endPos;
                        //移动note
                        Canvas.SetLeft(function.rectangle, Canvas.GetLeft(function.rectangle) + delta.X);
                        Canvas.SetBottom(function.rectangle, Canvas.GetBottom(function.rectangle) + delta.Y);
                    }
                }
            }

            functionPreview.Visibility = Visibility.Collapsed;
        }
    }

    private void functionPanel_MouseLeave(object sender, MouseEventArgs e)
    {
        functionPreview.Visibility = Visibility.Collapsed;
    }

    private void functionPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        //获取鼠标位置
        var (time, _, _, pos, _) = GetAlignedPoint(e.GetPosition(functionPanel));
        //拖动function
        if (isDragging)
        {
            foreach (var @event in selectedEvents)
            {
                var y = Canvas.GetBottom(@event.Rectangle);
                var i = @event.StartTime;
                @event.StartTime = GetTimeFromBottomY(y);
                @event.EndTime += @event.StartTime - i;
            }

            UpdateSelectedEvent(selectedEvents);
            isDragging = false;
            return;
        }

        if (Window.isPlaying) return;
        if (selectedEvents.Count != 0)
        {
            UpdateSelectedFunction(new List<Function>());
            infoFrame.Content = null;
            return;
        }
        //计算轨道
        var rail = (int)Math.Round(pos.X * 9 / eventPanel.ActualWidth) + page * 9;
        if (CurrLine.ClickOnFunction(time, rail)) return;
        //放置
        willPutFunction = new Function(time, rail, Guid.NewGuid().ToString())
        {
            rectangle =
            {
                Height = 10,
                Width = eventPreview.Width
            }
        };
        if (!CurrLine.Functions.Contains(willPutFunction))
        {
            functionPanel.Children.Add(willPutFunction.rectangle);
            Canvas.SetLeft(willPutFunction.rectangle, rail * functionPanel.ActualWidth / 9);
            Canvas.SetBottom(willPutFunction.rectangle, pos.Y);
            CurrLine.Functions.Add(willPutFunction);
            willPutFunction.parentLine = CurrLine;
            willPutFunction.TryLink(EditorWindow.Instance.track, true);
            functionChange = true;
            OperationManager.AddOperation(new PutFunctionOperation(willPutFunction, CurrLine));
        }

        willPutFunction = null;
        functionPreview.Visibility = Visibility.Collapsed;
    }

    private void functionPanel_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        functionPreview.Width = functionPanel.ActualWidth / 9;
        foreach (var line in Window.track.Lines)
            line.Functions.ForEach(f => { f.rectangle.Width = eventPanel.ActualWidth / 9; });
    }

    public void UpdateSelectedNote(List<Note> note)
    {
        if (note.Count == 1) UpdateSelectedNote(note[0]);
        selectedNotes.ForEach(note1 => { note1.rectangle.HighLight = false; });
        selectedNotes = note;
        selectedNotes.ForEach(note1 => { note1.rectangle.HighLight = true; });
        isDragging = isSelecting = selectedNotes.Count != 0;
        dragStartPoint = GetAlignedPoint(Mouse.GetPosition(notePanel)).mousePos;
    }

    public void UpdateSelectedNote(Note note, bool multiSelect = false)
    {
        if (!multiSelect)
        {
            selectedNotes.ForEach(note1 => { note1.rectangle.HighLight = false; });
            selectedNotes.Clear();
        }

        selectedNotes.Add(note);
        note.rectangle.HighLight = true;
        isSelecting = isDragging = true;
        dragStartPoint = GetAlignedPoint(Mouse.GetPosition(notePanel)).mousePos;
    }

    public void UpdateSelectedEvent(List<Event> events)
    {
        if (events.Count == 1) UpdateSelectedEvent(events[0]);
        selectedEvents.ForEach(@event => { @event.Rectangle.HighLight = false; });
        selectedEvents = events;
        selectedEvents.ForEach(@event => { @event.Rectangle.HighLight = true; });
        isSelecting = selectedEvents.Count != 0;
        isDragging = isSelecting;
        dragStartPoint = GetAlignedPoint(Mouse.GetPosition(eventPanel)).mousePos;
    }

    public void UpdateSelectedEvent(Event events, bool multiSelect = false)
    {
        if (!multiSelect)
        {
            selectedEvents.ForEach(@event => { @event.Rectangle.HighLight = false; });
            selectedEvents.Clear();
        }

        selectedEvents.Add(events);
        events.Rectangle.HighLight = true;
        isSelecting = selectedEvents.Count != 0;
        isDragging = isSelecting;
        dragStartPoint = GetAlignedPoint(Mouse.GetPosition(eventPanel)).mousePos;
    }

    public void UpdateSelectedFunction(List<Function> function)
    {
        if (function.Count == 1) UpdateSelectedFunction(function[0]);
        selectedFunctions.ForEach(function1 => { function1.rectangle.HighLight = false; });
        selectedFunctions = function;
        selectedFunctions.ForEach(function1 => { function1.rectangle.HighLight = true; });
        isDragging = isSelecting = selectedFunctions.Count != 0;
        dragStartPoint = GetAlignedPoint(Mouse.GetPosition(functionPanel)).mousePos;
    }

    public void UpdateSelectedFunction(Function function, bool multiSelect = false)
    {
        if (!multiSelect)
        {
            selectedFunctions.ForEach(function1 => { function1.rectangle.HighLight = false; });
            selectedFunctions.Clear();
        }

        selectedFunctions.Add(function);
        function.rectangle.HighLight = true;
        isSelecting = isDragging = true;
        dragStartPoint = GetAlignedPoint(Mouse.GetPosition(functionPanel)).mousePos;
    }


    private void trackPreview_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        //时间设置
        if (Keyboard.IsKeyDown(Key.LeftCtrl))
        {
            previewRange = Math.Min(previewRange + 0.1 * e.Delta / 60.0, 1.0);
            previewRange = Math.Max(10 / Window.track.Length, previewRange);
        }
        else
        {
            Update();
            noteChange = true;
            Window.player.Pause();
            Window.isPlaying = false;
            var t = Window.playerTime + e.Delta / 120.0 * Window.track.Length * previewRange / 10.0;
            t = Math.Max(0, t);
            t = Math.Min(Window.track.Length * previewRange + previewStartTime, t);
            //调整预览窗格
            if (t - previewStartTime > Window.track.Length * previewRange)
                previewStartTime = t - Window.track.Length * previewRange;
            else if (t < previewStartTime) previewStartTime = t;
            Window.playerTime = t;
            Window.player.Position = TimeSpan.FromSeconds(Window.playerTime);
            TimeDis.Content = Window.playerTime.ToString("0.00") + " / " + Window.track.Length.ToString("0.00");
        }
    }

    private void trackPreview_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        //获取鼠标位置
        var pos = Mouse.GetPosition(trackPreview);
        var time =
            (trackPreview.ActualHeight - pos.Y) / trackPreview.ActualHeight * Window.track.Length * previewRange +
            previewStartTime;
        Window.player.Position = TimeSpan.FromSeconds(time);
        Window.playerTime = time;
        //暂停播放器
        Window.player.Pause();
        Window.isPlaying = false;
        //更新时间
        TimeDis.Content = Window.playerTime.ToString("0.00") + " / " + Window.track.Length.ToString("0.00");
        noteChange = true;
        eventChange = true;
        fakeCatchChange = true;
        functionChange = true;
        Update();
        //
        isMouseDown = true;
    }

    private void trackPreview_MouseMove(object sender, MouseEventArgs e)
    {
        if (!isMouseDown) return;
        //获取鼠标位置
        var pos = Mouse.GetPosition(trackPreview);
        var time =
            (trackPreview.ActualHeight - pos.Y) / trackPreview.ActualHeight * Window.track.Length * previewRange +
            previewStartTime;
        Window.player.Position = TimeSpan.FromSeconds(time);
        Window.playerTime = time;
        //更新时间
        TimeDis.Content = Window.playerTime.ToString("0.00") + " / " + Window.track.Length.ToString("0.00");
        noteChange = true;
        eventChange = true;
        fakeCatchChange = true;
        functionChange = true;
        Update();
    }

    private void StopDragTrackPreview()
    {
        isMouseDown = false;
    }

    private void trackPreview_MouseUp(object sender, MouseButtonEventArgs e)
    {
        StopDragTrackPreview();
    }

    private void trackPreview_MouseLeave(object sender, MouseEventArgs e)
    {
        StopDragTrackPreview();
    }

    private void trackPreviewWithEvent_SizeChanged(object sender, SizeChangedEventArgs e)
    {
    }

    private void EditOrPreviewChanged(object sender, EventArgs e)
    {
        if (previewing)
        {
            //切换至谱面编辑
            previewing = false;
            notePanel.Visibility = Visibility.Visible;
            trackPreviewWithEvent.Visibility = Visibility.Hidden;
            ModeSet(editingMode);
            UpdateEventTypeList();
        }
        else
        {
            //切换至预览
            //刷新NBT
            nbtTrack = NBTTrack.FromTrack(Window.track);
            previewing = true;
            notePanel.Visibility = Visibility.Hidden;
            eventPanel.Visibility = Visibility.Hidden;
            trackPreviewWithEvent.Visibility = Visibility.Visible;
            DrawTrackWithEvent();
        }
    }

    private void Note_Event_Catch_Function_ContentChanged(object sender, EventArgs e)
    {
        editingMode++;
        if (previewing) return;
        editingMode = ModeSet(editingMode);
        UpdateEventTypeList();
    }

    private byte ModeSet(byte mode)
    {
        ModeReset();
        mode %= MAX_MODE;
        switch (mode)
        {
            case NOTE:
                ModeToNote();
                break;
            case EVENT:
                ModeToEvent();
                break;
            case FAKE_CATCH:
                ModeToFakeCatch();
                break;
            case FUNCTION:
                ModeToFunction();
                break;
        }

        return mode;
    }

    private void ModeReset()
    {
        eventPanel.Visibility = Visibility.Hidden;
        functionPanel.Visibility = Visibility.Hidden;
        foreach (var line in Window.track.Lines)
        {
            line.FakeCatch.ForEach(e => { e.rectangle.Visibility = Visibility.Hidden; });
            line.Notes.ForEach(note =>
            {
                if (note.type == NoteType.Tap)
                    note.Color = EditorColors.tapColorButNotOnThisLine;
                else if (note.type == NoteType.Hold)
                    note.Color = EditorColors.holdColorButNotOnThisLine;
                else
                    note.Color = EditorColors.catchColorButNotOnThisLine;
            });
        }
    }

    private void ModeToNote()
    {
        foreach (var note in CurrLine.Notes)
            if (note.type == NoteType.Tap)
                note.Color = EditorColors.tapColor;
            else if (note.type == NoteType.Hold)
                note.Color = EditorColors.holdColor;
            else
                note.Color = EditorColors.catchColor;
        UpdateNote();
    }

    private void ModeToEvent()
    {
        eventPanel.Visibility = Visibility.Visible;
        UpdateEvent();
    }

    private void ModeToFakeCatch()
    {
        foreach (var line in Window.track.Lines)
            line.FakeCatch.ForEach(e => { e.rectangle.Visibility = Visibility.Hidden; });
        CurrLine.FakeCatch.ForEach(e => { e.rectangle.Visibility = Visibility.Visible; });
        UpdateFakeCatch();
    }

    private void ModeToFunction()
    {
        functionPanel.Visibility = Visibility.Visible;
        UpdateFunction();
    }

    //进入自由判定线
    private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs args)
    {
        if (selectingFreeLine) return;
        if (editingMode == 0)
        {
            foreach (var note in CurrLine.Notes)
                if (note.type == NoteType.Tap)
                    note.Color = EditorColors.tapColorButNotOnThisLine;
                else if (note.type == NoteType.Hold)
                    note.Color = EditorColors.holdColorButNotOnThisLine;
                else
                    note.Color = EditorColors.catchColorButNotOnThisLine;
            selectingFreeLine = true;
            LineListView.SelectedIndex = -1;
            foreach (var note in CurrLine.Notes)
                if (note.type == NoteType.Tap)
                    note.Color = EditorColors.tapColor;
                else if (note.type == NoteType.Hold)
                    note.Color = EditorColors.holdColor;
                else
                    note.Color = EditorColors.catchColor;
        }
        else if (editingMode == 1)
        {
            foreach (var el in CurrLine.EventLists)
                el.Events.ForEach(e => { e.Rectangle.Visibility = Visibility.Hidden; });
            selectingFreeLine = true;
            LineListView.SelectedIndex = -1;
            foreach (var el in CurrLine.EventLists)
                el.Events.ForEach(e => { e.Rectangle.Visibility = Visibility.Visible; });
            UpdateEventTypeList();
        }
        else
        {
            CurrLine.FakeCatch.ForEach(e => { e.rectangle.Visibility = Visibility.Hidden; });
            selectingFreeLine = true;
            LineListView.SelectedIndex = -1;
            CurrLine.FakeCatch.ForEach(e => { e.rectangle.Visibility = Visibility.Visible; });
        }
    }
}