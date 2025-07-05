using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using PMEditor.Controls;
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

    private enum EditorToolType
    {
        Arrow, Resize, Move, Put, Eraser
    }
    
    private EditorToolType currToolType = EditorToolType.Arrow;
    
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
    private const byte MAX_MODE = 3;

    //可能会加入的，事件可以翻页，防止事件太多放不下
    public readonly int EventPageIndex = 0;

    //当前能看到的总谱面时长
    public double CurrDisplayLength = 4;

    public double CatchHeight = 1.0;

    //节拍分割
    public int DivideNum = 4;

    /// <summary>
    ///     0-note 1-event 2-catch
    /// </summary>
    private byte editingMode;

    private bool init;

    private bool isDeleting;

    private bool isMouseDown;

    //渲染主循环
    private DateTime lastRenderTime = DateTime.MinValue;

    private NBTTrack nbtTrack;

    private bool previewing;

    //可预览的范围
    private double previewRange;

    //可预览的起始时间点
    private double previewStartTime;

    private bool selectingFreeLine;
    
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

    public Line CurrLine
    {
        get
        {
            return selectingFreeLine ? Window.track.FreeLine : Window.track.Lines[LineIndex];
        }
    }

    private FastDrawing? fastDrawing;

    private WatchDog watchDog;

    private readonly List<ObjectPanel> notePanels = new();
    private readonly List<ObjectPanel> eventPanels = new();
    private readonly List<ObjectPanel> fakeCatchPanels = new();

    private ObjectPanel currPanel;
    public ObjectPanel CurrPanel
    {
        get => currPanel;
        private set
        {
            if(currPanel != null) Panel.SetZIndex(currPanel, 0);
            currPanel = value;
            Panel.SetZIndex(currPanel, 1);
            UpdateToolBarStatus(currToolType);
        }
    }
    
#pragma warning disable CS8618
    public TrackEditorPage()
    {
        InitializeComponent();
        Instance = this;
        TimeDis.Content = "0.00" + " / " + Window.track.Length.ToString("0.00");
        CompositionTarget.Rendering += Tick;
        //添加note渲染和事件渲染
        foreach (var line in Window.track.AllLines)
        {
            line.NotePanel = ObjectPanel.NewNotePanel(line);
            line.EventPanel = ObjectPanel.NewEventPanel(line);
            line.FakeCatchPanel = ObjectPanel.NewFakeCatchPanel(line);
            line.NotePanel.SetVisible(ObjectVisible.Translucent);
            line.EventPanel.SetVisible(ObjectVisible.Hidden);
            line.FakeCatchPanel.SetVisible(ObjectVisible.Hidden);
            ObjectPanels.Children.Add(line.NotePanel);
            ObjectPanels.Children.Add(line.EventPanel);
            ObjectPanels.Children.Add(line.FakeCatchPanel);
            notePanels.Add(line.NotePanel);
            eventPanels.Add(line.EventPanel);
            fakeCatchPanels.Add(line.FakeCatchPanel);
        }
        notePanels.First().SetVisible(ObjectVisible.Visible);
        CurrPanel = notePanels.First();
        SpeedChooseBox.ItemsSource = Settings.currSetting.canSelectedSpeedList;
        LineListView.ItemsSource = Window.track.Lines;
        LineListView.SelectedIndex = 0;
        var defaultPreviewColor = EditorColors.tapColor;
        defaultPreviewColor.A = 100;
        ObjPreview.Fill = new SolidColorBrush(defaultPreviewColor);
        init = true;
        previewRange = 30.0 / Window.track.Length;
        watchDog = new WatchDog(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10));
    }

    private List<double> fpss = new();
    private void Tick(object? sender, EventArgs e)
    {
        if (!double.IsNaN(FPS) && (DateTime.Now - lastRenderTime).TotalMilliseconds < 1000.0 / FPS) return;
        watchDog.ReportActivity();
        if(fpss.Count > 100) fpss.RemoveAt(0);
        fpss.Add(1000.0 / (DateTime.Now - lastRenderTime).TotalMilliseconds);
        Fps.Text = $"FPS: {(int)fpss.Average()}";
        lastRenderTime = DateTime.Now;
        if (Window.isPlaying) Update();
        //初始化的时候进行第一次绘制
        if (init && ObjectPanels.ActualWidth != 0)
        {
            UpdateNote();
            UpdateEvent();
            UpdateFakeCatch();
            UpdateEventTypeList();
            DrawLineAndBeat();
            DrawPreview();
            init = false;
            Window.OperationInfo.Text = "就绪";
        }
    }
    
    //滚轮滚动
    private void ObjPanelMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var num = e.Delta / 60.0;
        if (Keyboard.IsKeyDown(Key.LeftCtrl))
        {
            CurrDisplayLength = Math.Max(1, Math.Min(30, CurrDisplayLength + num));
        }
        //更改谱面拍数
        else if (Keyboard.IsKeyDown(Key.LeftShift))
        {
            DivideNum = Math.Max(1, DivideNum + (int)(num / 2));
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
        HeightDis.Content = $"Fake Catch高度: {CatchHeight:F2}";
        //事件
        
        Update();
    }

    private static void CheckCurrTime()
    {
        if (Window.playerTime < 0)
        {
            Window.playerTime = 0;
        }
    }
    
    //绘制辅助线
    public void DrawLineAndBeat()
    {
        //移除线
        for (var i = 0; i < ObjectPanels.Children.Count; i++)
        {
            if (ObjectPanels.Children[i] is not System.Windows.Shapes.Line line) continue;
            line.Stroke = null;
            ObjectPanels.Children.RemoveAt(i);
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
        for (var i = startMeasure; drawTime < currTime + CurrDisplayLength ; i++)
        {
            var (_, length) = Window.track.GetTimeRange(i);
            var div = length / DivideNum;
            //绘制主线
            if (drawTime >= currTime)
            {
                var y = CurrPanel.GetTopYFromTime(drawTime);
                System.Windows.Shapes.Line line = new()
                {
                    X1 = 0,
                    X2 = ObjectPanels.ActualWidth,
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
                Canvas.SetTop(textBlock, y - 9);    //对齐。这个9是一个像素一个像素调的
                ObjectPanels.Children.Add(line); 
            }
            drawTime += div;
            for (var j = 0; j < DivideNum - 1; j++)
            {
                if(drawTime < currTime)
                {
                    drawTime += div;
                    continue;
                }
                var y = CurrPanel.GetTopYFromTime(drawTime);
                System.Windows.Shapes.Line line = new()
                {
                    X1 = 0,
                    X2 = ObjectPanels.ActualWidth,
                    Y1 = y,
                    Y2 = y,
                    Stroke = new SolidColorBrush(Color.FromRgb(254, 235, 255)),
                    StrokeThickness = 1
                };
                ObjectPanels.Children.Add(line);
                drawTime += div;
            }
        }
    }

    //绘制谱面预览
    private void DrawPreview()
    {
        if (fastDrawing == null)
        {
            if ((int)TrackPreviewCanvas.ActualWidth == 0 && (int)TrackPreviewCanvas.ActualHeight == 0)
            {
                return;
            }
            fastDrawing = new FastDrawing((int)TrackPreviewCanvas.ActualWidth, (int)TrackPreviewCanvas.ActualHeight);
            TrackPreview.Source = fastDrawing.Bitmap;
        }
        var width = fastDrawing!.Width;
        var height = fastDrawing.Height;
        fastDrawing.Clear(Colors.Black);
        //调整预览范围
        if (Window.playerTime < previewStartTime)
        {
            previewStartTime = Window.playerTime;
        }
        else if (Window.playerTime > previewStartTime + previewRange * Window.track.Length)
        {
            previewStartTime = Window.playerTime - previewRange * Window.track.Length;
        }
        //绘制谱面
        foreach (var note in Window.track.AllLines.SelectMany(line => line.Notes).Where(note => note.ActualTime >= previewStartTime && note.ActualTime < previewStartTime + previewRange * Window.track.Length))
        {
            if (note.type == NoteType.Hold)
            {
                var x = note.Rail * width / 9d;
                var y = fastDrawing.Height - (note.ActualTime - previewStartTime) / (Window.track.Length * previewRange) * height;
                var w = width / 9d;
                var h = note.ActualHoldTime / (Window.track.Length * previewRange) * height;
                fastDrawing.DrawRectangle((int)x, (int)(y - h), (int)w, (int)h, EditorColors.holdColor, 0, true);
            }
            else
            {
                var x = note.Rail * width / 9d;
                var y = fastDrawing.Height - (note.ActualTime - previewStartTime) / (Window.track.Length * previewRange) * height;
                var w = width / 9d;
                var color = note.type == NoteType.Catch ? EditorColors.catchColor : EditorColors.tapColor;
                fastDrawing.DrawLine((int)x, (int)y, (int)x + (int)w,  (int)y, color, 2);
            }
        }

        //绘制时间线
        var timeLineY = height - (Window.playerTime - previewStartTime) / (Window.track.Length * previewRange) * height;
        //绘制时间线
        fastDrawing.DrawLine(0, (int)timeLineY,  width, (int)timeLineY, Colors.Red, 2);
        fastDrawing.Update();
    }

    private void DrawTrackWithEvent()
    {
        //清空画布
        TrackPreviewWithEvent.Children.OfType<Rectangle>()
            .ToList()
            .ForEach(t =>
            {
                t.Fill = null;
                TrackPreviewWithEvent.Children.Remove(t);
            });
        var mapLength = Settings.currSetting.MapLength;
        var windowHeight = TrackPreviewWithEvent.ActualHeight;
        var windowWidth = TrackPreviewWithEvent.ActualWidth;
        TrackPreviewWithEvent.Children.Clear();
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
                            var delta = (i - hold.judgeTime) * Settings.currSetting.Tick - (int)exp.Parameters["t"]! +
                                        1;
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
                        TrackPreviewWithEvent.Children.Add(noteRec);
                        Canvas.SetBottom(noteRec, locate);
                        Canvas.SetLeft(noteRec, note.Rail * noteWidth);
                    }
                }
                else if (note.summonTime <= time && time <= note.judgeTime)
                {
                    SolidColorBrush solidColorBrush;
                    if (note is NBTFakeCatch fakeCatch)
                    {
                        solidColorBrush = new SolidColorBrush(FakeCatch.GetColor(fakeCatch.height));
                    }
                    else
                    {
                        solidColorBrush = note.type == (int)NoteType.Tap
                            ? new SolidColorBrush(EditorColors.tapColor)
                            : new SolidColorBrush(EditorColors.catchColor);
                    }
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
                        {
                            locate += line.line.GetSpeed(i) / Settings.currSetting.Tick;
                        }
                        //计算差值保证帧数
                        var delta = note.judgeTime - i;
                        locate += line.line.GetSpeed(i) * delta;
                    }

                    locate = locate / mapLength * windowHeight;
                    TrackPreviewWithEvent.Children.Add(noteRec);
                    Canvas.SetBottom(noteRec, locate);
                    Canvas.SetLeft(noteRec, note.Rail * noteWidth);
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
                            if (note.HasJudged) note.HasJudged = false;
                        }
                        else
                        {
                            //如果未被判定过且谱面正在被播放
                            if (!note.HasJudged && Window.isPlaying)
                            {
                                Window.tapSoundManager.AppendSoundRequest();
                                note.HasJudged = true;
                            }
                        }
                    }

                    continue;
                }

                //在判定线上方
                if (minTime <= note.ActualTime)
                {
                    if (note.HasJudged) note.HasJudged = false;
                }
                //在判定线下方
                else
                {
                    if (note.HasJudged || !Window.isPlaying) continue;
                    //如果未被判定过且谱面正在被播放
                    if (note.type == NoteType.Tap)
                    {
                        Window.tapSoundManager.AppendSoundRequest();
                    }
                    else
                    {
                        Window.catchSoundManager.AppendSoundRequest();
                    }
                    note.HasJudged = true;
                }
            }
        }
    }

    //更新
    private void Update()
    {
        DrawLineAndBeat();
        DrawPreview();
        if (Window.isPlaying)
        {
            Window.playerTime = Window.player.Position.TotalSeconds;
            //更新时间
            TimeDis.Content = Window.playerTime.ToString("0.00") + " / " + Window.track.Length.ToString("0.00");
            HeightDis.Content = $"Fake Catch高度: {CatchHeight:F2}";
        }

        notePanels.ForEach(notePanel => notePanel.Update());
        if(editingMode != 0) currPanel.Update();
        
        if (previewing)
        {
            //更新谱面预览
            DrawTrackWithEvent();
        }
        Window.tapSoundManager.PlaySound();
        Window.catchSoundManager.PlaySound();
    }
    
    public void UpdateNote()
    {
        foreach (var notePanel in notePanels)
        {
            notePanel.Update();
        }
    }

    public void UpdateFakeCatch()
    {
        foreach (var fakeCatchPanel in fakeCatchPanels)
        {
            fakeCatchPanel.Update();
        }
    }

    public void UpdateEvent()
    {
        foreach (var eventPanel in eventPanels)
        {
            eventPanel.Update();
        }
        //UpdateFunctionCurve();
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
                var type = CurrLine.GetType(i + EventPageIndex * 9);
                text = Event.TypeString(type);
                if (type == EventType.Unknown)
                    textBlock.Foreground = new SolidColorBrush(Colors.Gray);
                else if (CurrLine.EventLists[i + EventPageIndex * 9].IsMainEvent())
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
                    //var rs = e.EventGroup.Select(item => item.Rectangle).ToList();
                    //EventRectangle.DrawFunction(rs);
                    //rs.Clear();
                }

                //e.Rectangle.UpdateText();
            }
        }
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

        switch (editingMode)
        {
            case 0:
                CurrLine.NotePanel.SetVisible(ObjectVisible.Translucent);
                LineIndex = LineListView.SelectedIndex;
                selectingFreeLine = false;
                CurrLine.NotePanel.SetVisible(ObjectVisible.Visible);
                break;
            case 1:
            {
                CurrLine.EventPanel.SetVisible(ObjectVisible.Translucent);
                LineIndex = LineListView.SelectedIndex;
                selectingFreeLine = false;
                CurrLine.EventPanel.SetVisible(ObjectVisible.Visible);
                break;
            }
            default:
                CurrLine.FakeCatchPanel.SetVisible(ObjectVisible.Translucent);
                LineIndex = LineListView.SelectedIndex;
                selectingFreeLine = false;
                CurrLine.FakeCatchPanel.SetVisible(ObjectVisible.Visible);
                break;
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
        InfoFrame.Content = new LineInfoFrame(CurrLine, LineIndex);
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

    private void trackPreview_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        //时间设置
        if (Keyboard.IsKeyDown(Key.LeftCtrl))
        {
            previewRange = Math.Min(previewRange + 0.1 * e.Delta / 60.0, 1.0);
            previewRange = Math.Max(10 / Window.track.Length, previewRange);
            DrawPreview();
        }
        else
        {
            Update();
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
        var pos = Mouse.GetPosition(TrackPreview);
        var time =
            (TrackPreview.ActualHeight - pos.Y) / TrackPreview.ActualHeight * Window.track.Length * previewRange +
            previewStartTime;
        Window.player.Position = TimeSpan.FromSeconds(time);
        Window.playerTime = time;
        //暂停播放器
        Window.player.Pause();
        Window.isPlaying = false;
        //更新时间
        TimeDis.Content = Window.playerTime.ToString("0.00") + " / " + Window.track.Length.ToString("0.00");
        Update();
        //
        isMouseDown = true;
    }

    private void trackPreview_MouseMove(object sender, MouseEventArgs e)
    {
        if (!isMouseDown) return;
        //获取鼠标位置
        var pos = Mouse.GetPosition(TrackPreview);
        var time =
            (TrackPreview.ActualHeight - pos.Y) / TrackPreview.ActualHeight * Window.track.Length * previewRange +
            previewStartTime;
        Window.player.Position = TimeSpan.FromSeconds(time);
        Window.playerTime = time;
        //更新时间
        TimeDis.Content = Window.playerTime.ToString("0.00") + " / " + Window.track.Length.ToString("0.00");
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
            ObjectPanels.Visibility = Visibility.Visible;
            TrackPreviewWithEvent.Visibility = Visibility.Hidden;
            ModeSet(editingMode);
            UpdateEventTypeList();
        }
        else
        {
            //切换至预览
            //刷新NBT
            nbtTrack = NBTTrack.FromTrack(Window.track);
            previewing = true;
            ObjectPanels.Visibility = Visibility.Hidden;
            TrackPreviewWithEvent.Visibility = Visibility.Visible;
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
        }

        return mode;
    }

    private void ModeReset()
    {
        foreach (var eventPanel in eventPanels)
        {
            eventPanel.SetVisible(ObjectVisible.Hidden);
        }
        foreach (var fakeCatchPanel in fakeCatchPanels)
        {
            fakeCatchPanel.SetVisible(ObjectVisible.Hidden);
        }
        foreach (var notePanel in notePanels)
        {
            notePanel.SetVisible(ObjectVisible.Translucent);
        }
        CurrLine.NotePanel.SetVisible(ObjectVisible.Visible);
        CurrPanel = CurrLine.NotePanel;
    }

    private void ModeToNote()
    {
        UpdateNote();
    }

    private void ModeToEvent()
    {
        CurrLine.EventPanel.SetVisible(ObjectVisible.Visible);
        CurrLine.NotePanel.SetVisible(ObjectVisible.Translucent);
        CurrPanel = CurrLine.EventPanel;
        UpdateEvent();
    }

    private void ModeToFakeCatch()
    {
        CurrLine.FakeCatchPanel.SetVisible(ObjectVisible.Visible);
        CurrLine.NotePanel.SetVisible(ObjectVisible.Translucent);
        CurrPanel = CurrLine.FakeCatchPanel;
        UpdateFakeCatch();
    }

    //进入自由判定线
    private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs args)
    {
        if (selectingFreeLine) return;
        switch (editingMode)
        {
            case 0:
                CurrLine.NotePanel.SetVisible(ObjectVisible.Translucent);
                selectingFreeLine = true;
                LineListView.SelectedIndex = -1;
                CurrLine.NotePanel.SetVisible(ObjectVisible.Visible);
                CurrPanel = CurrLine.NotePanel;
                break;
            case 1:
                CurrLine.EventPanel.SetVisible(ObjectVisible.Hidden);
                selectingFreeLine = true;
                LineListView.SelectedIndex = -1;
                CurrLine.EventPanel.SetVisible(ObjectVisible.Visible);
                CurrPanel = CurrLine.EventPanel;
                UpdateEventTypeList();
                break;
            default:
                CurrLine.FakeCatchPanel.SetVisible(ObjectVisible.Hidden);
                selectingFreeLine = true;
                LineListView.SelectedIndex = -1;
                CurrLine.FakeCatchPanel.SetVisible(ObjectVisible.Visible);
                CurrPanel = CurrLine.FakeCatchPanel;
                break;
        }
    }

    private void TrackEditorPage_OnUnloaded(object sender, RoutedEventArgs e)
    {
        watchDog.Dispose();
    }

    private void ToolButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        currToolType = button.Name switch
        {
            "ArrowButton" => EditorToolType.Arrow,
            "ResizeButton" => EditorToolType.Resize,
            "MoveButton" => EditorToolType.Move,
            "PutButton" => EditorToolType.Put,
            "EraserButton" => EditorToolType.Eraser,
            _ => currToolType
        };
        UpdateToolBarStatus(currToolType);
    }


    private void UpdateToolBarStatus(EditorToolType type)
    {
        foreach (var button in ToolButtons.Children.OfType<Button>())
        {
            button.IsEnabled = true;
        }

        switch (type)
        {
            case EditorToolType.Arrow:
                ArrowButton.IsEnabled = false;
                CurrPanel.CurrTool = CurrPanel.Arrow;
                ObjectPanels.Cursor = Cursors.Arrow;
                break;
            case EditorToolType.Resize:
                ResizeButton.IsEnabled = false;
                CurrPanel.CurrTool = CurrPanel.Resize;
                ObjectPanels.Cursor = Cursors.SizeNS;
                break;
            case EditorToolType.Move:
                MoveButton.IsEnabled = false;
                CurrPanel.CurrTool = CurrPanel.Move;
                ObjectPanels.Cursor = Cursors.SizeAll;
                break;
            case EditorToolType.Put:
                PutButton.IsEnabled = false;
                CurrPanel.CurrTool = CurrPanel.Put;
                ObjectPanels.Cursor = Cursors.Pen;
                break;
            case EditorToolType.Eraser:
                EraserButton.IsEnabled = false;
                CurrPanel.CurrTool = CurrPanel.Eraser;
                ObjectPanels.Cursor = Cursors.Cross;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        CurrPanel.OnMouseLeftButtonDown(sender, e);
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        CurrPanel.OnMouseLeftButtonUp(sender, e);
    }
    
    private void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        CurrPanel.OnMouseWheel(sender, e);
    }
    
    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        CurrPanel.OnMouseMove(sender, e);
    }
    
    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        CurrPanel.OnMouseLeave(sender, e);
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        ObjPreview.Width = ActualWidth / 9;
        CurrPanel.OnSizeChanged(sender, e);
    }
    
    public void UpdateObjPreview(double left, double top, double width, double height)
    {
        ObjPreview.Visibility = Visibility.Visible;
        ObjPreview.Width = width;
        ObjPreview.Height = height;
        Canvas.SetLeft(ObjPreview, left);
        Canvas.SetTop(ObjPreview, top);
    }

    public void UpdateObjPreview(Visibility visibility)
    {
        ObjPreview.Visibility = visibility;
    }

    public void FlushObjPreviewFill()
    {
        Color color;
        switch (editingMode)
        {
            case 0:
                //note
                color = Window.puttingTap ? EditorColors.tapColor : EditorColors.catchColor;
                break;
            case 1: 
                return;
            case 2:
                color = FakeCatch.GetColor(CatchHeight);
                break;
            default:
                return;
        }

        color.A = 100;
        ObjPreview.Fill = new SolidColorBrush(color);
    }
}