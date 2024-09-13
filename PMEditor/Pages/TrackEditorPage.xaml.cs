using PMEditor.Operation;
using PMEditor.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PMEditor
{
    /// <summary>
    /// TrackEditorPage.xaml 的交互逻辑
    /// </summary>
    public partial class TrackEditorPage : Page
    {

        static readonly SolidColorBrush tapBrush = new(
            Color.FromArgb(102, 109, 209, 213)
            );
        static readonly SolidColorBrush catchBrush = new(
            Color.FromArgb(102, 227, 214, 76)
            );
        static readonly SolidColorBrush fakeCatchBrush = new(
            Color.FromArgb(102, 255, 255, 255)
            );

        public static TrackEditorPage Instance { get; private set; }

        public const double FPS = double.NaN;

        public EditorWindow window { get => EditorWindow.Instance; }

        double secondsPreBeat
        {
            get => 60.0 / window.track.bpm;
        }

        double secondsPreDevideBeat
        {
            get => secondsPreBeat / divideNum;
        }

        /// <summary>
        /// 0-note 1-event 2-catch
        /// </summary>
        byte editingMode = 0;
        const byte NOTE = 0;
        const byte EVENT = 1;
        const byte FAKE_CATCH = 2;
        const byte FUNCTION = 3;
        const byte MAX_MODE = 4;

        bool previewing = false;

        NBTTrack nbtTrack;

        //节拍分割
        int divideNum = 4;

        //节拍线显示间隔
        double pixelPreDividedBeat = 30;
        private double pixelPreBeat
        {
            get { return pixelPreDividedBeat * divideNum; }
        }

        Note? willPutNote;
        Event? willPutEvent;
        Function? willPutFunction;
        int lineIndex = 0;
        public int LineIndex
        {
            get => lineIndex;
        }

        bool selectingFreeLine = false;
        Line CurrLine
        {
            get {
                if (selectingFreeLine)
                {
                    return window.track.FreeLine;
                }
                else
                {
                    return window.track.Lines[lineIndex];
                }
            }
        }

        bool noteChange = false;
        bool eventChange = true;
        bool fakeCatchChange = false;
        bool functionChange = false;

        bool init = false;

        bool isSelecting = false;

        bool isDraging = false;

        double catchHeight = 1.0;

        Point dragStartPoint = new(-1, -1);

        //可能会加入的，事件可以翻页，防止事件太多放不下
        int page = 0;

        private List<Note> selectedNotes = new();
        private List<Event> selectedEvents = new();
        private List<Function> selectedFunctions = new();

        //可预览的范围
        double previewRange;

        //可预览的起始时间点
        double previewStartTime = 0;

        public TrackEditorPage()
        {
            InitializeComponent();
            Instance = this;
            timeDis.Content = "0.00" + " / " + window.track.Length.ToString("0.00");
            CompositionTarget.Rendering += Tick;
            //添加note渲染和事件渲染
            foreach (var line in window.track.lines)
            {
                line.notes.ForEach(note =>
                {
                    note.rectangle.Visibility = Visibility.Hidden;
                    notePanel.Children.Add(note.rectangle);
                    if (note.actualHoldTime != 0)
                    {
                        note.rectangle.Height = GetYFromTime(note.actualTime + note.actualHoldTime) - GetYFromTime(note.actualTime);
                    }
                    else
                    {
                        note.rectangle.Height = 10;
                    }
                    note.rectangle.Width = notePanel.ActualWidth / 9;
                });
                for(int i = 0; i < line.eventLists.Count; i++)
                {
                    line.eventLists[i].events.ForEach(e =>
                    {
                        e.rectangle.Visibility = Visibility.Hidden;
                        eventPanel.Children.Add(e.rectangle);
                        e.rectangle.Height = GetYFromTime(e.endTime) - GetYFromTime(e.startTime);
                        e.rectangle.Width = eventPanel.ActualWidth / 9; 
                    });
                }
                foreach (FakeCatch fakeCatch in line.fakeCatch)
                {
                    fakeCatch.rectangle.Visibility = Visibility.Hidden;
                    notePanel.Children.Add(fakeCatch.rectangle);
                    fakeCatch.rectangle.Height = 10;
                }
                foreach(Function function in line.functions)
                {
                    function.rectangle.Visibility = Visibility.Hidden;
                    functionPanel.Children.Add(function.rectangle);
                    function.rectangle.Height = 10;
                }
            }
            speedChooseBox.ItemsSource = Settings.currSetting.canSelectedSpeedList;
            lineListView.ItemsSource = window.track.lines;
            lineListView.SelectedIndex = 0;
            init = true;
            previewRange = 30.0 / window.track.length;
        }

        //渲染主循环
        private DateTime lastRenderTime = DateTime.MinValue;
        private void Tick(object? sender, EventArgs e)
        {
            if (!double.IsNaN(FPS) && (DateTime.Now - lastRenderTime).TotalMilliseconds < 1000.0 / FPS) return;
            lastRenderTime = DateTime.Now;
            if (window.isPlaying)
            {
                Update();
            }
            //初始化的时候进行第一次绘制
            if (init && notePanel.ActualWidth != 0)
            {
                UpdateNote();
                UpdateEvent();
                UpdateFakeCatch();
                UpdateFunctionCurve();
                UpdateEventTypeList();
                init = false;
                window.operationInfo.Text = "就绪";
            }
            DrawLineAndBeat();
            DrawPreview();
        }

        //滚轮滚动
        private void notePanel_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double num = e.Delta / 60.0;
            double currTime = window.playerTime;
            //更改fake catch高度
            if(Keyboard.IsKeyDown(Key.LeftAlt))
            {
                catchHeight = Math.Max(0, Math.Min(1, catchHeight + num / 50));
                if(selectedNotes.Count != 0)
                {
                    foreach(var note in selectedNotes)
                    {
                        if(note is FakeCatch f)
                        {
                            f.Height = catchHeight;
                        }
                    }
                }
            }
            //更改谱面缩放比例
            else if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
            }
            //更改谱面拍数
            else if(Keyboard.IsKeyDown(Key.LeftShift))
            {
                divideNum = Math.Max(1, divideNum + (int)(num/2));
            }
            else
            {
                //时间设置
                window.player.Pause();
                window.isPlaying = false;
                //横线自动对齐
                if (currTime / secondsPreDevideBeat % 1 > 0.001 && currTime / secondsPreDevideBeat % 1 < 0.999)
                {
                    currTime = (int)(currTime / secondsPreDevideBeat) * secondsPreDevideBeat;
                    if (num < 0)
                    {
                        num = 0;
                    }
                }
                currTime += num * secondsPreDevideBeat;
                if (currTime < 0)
                {
                    currTime = 0;
                }
                else if (currTime > window.track.Length)
                {
                    currTime = window.track.Length;
                }
                window.player.Position = TimeSpan.FromSeconds(currTime);
            }
            //Lable设置
            timeDis.Content = currTime.ToString("0.00") + " / " + window.track.Length.ToString("0.00");
            heightDis.Content = $"Fake Catch高度: {catchHeight:F2}";
            noteChange = true;
            eventChange = true;
            fakeCatchChange = true;
            functionChange = true;
            window.playerTime = currTime;
            Update();
        }

        //绘制辅助线
        public void DrawLineAndBeat()
        {
            //移除线
            for (int i = 0; i < notePanel.Children.Count; i++)
            {
                if (notePanel.Children[i] is System.Windows.Shapes.Line line)
                {
                    line.Stroke = null;
                    notePanel.Children.RemoveAt(i);
                    i--;
                }
            }
            //移除拍数
            beatDisplay.Children.Clear();
            //绘制节拍线
            //开始绘制
            //整拍
            //基准线相对坐标计算
            double fix = 0;
            if (window.playerTime / secondsPreBeat % 1 > 0.001)
            {
                fix = window.playerTime / secondsPreBeat % 1 * pixelPreBeat;
            }
            for (double i = notePanel.ActualHeight; i + fix > 0; i -= pixelPreDividedBeat)
            {
                double actualY = i + fix;
                System.Windows.Shapes.Line line = new()
                {
                    X1 = 0,
                    X2 = notePanel.ActualWidth,
                    Y1 = actualY,
                    Y2 = actualY
                };
                if ((notePanel.ActualHeight - i) % pixelPreBeat < 0.001 || (notePanel.ActualHeight - i) % pixelPreBeat > 0.999 && (notePanel.ActualHeight - i) % pixelPreBeat < 1)
                {
                    //是整拍
                    line.Stroke = new SolidColorBrush(Color.FromRgb(237, 70, 255));
                    line.StrokeThickness = 3;
                    //获取当前拍数
                    int beats = (int)Math.Round(GetTimeFromY(notePanel.ActualHeight - actualY) / secondsPreBeat);
                    //拍数
                    TextBlock textBlock = new()
                    {
                        Text = beats.ToString(),
                        Foreground = new SolidColorBrush(Colors.White),
                        FontSize = 12
                    };
                    beatDisplay.Children.Add(textBlock);
                    Canvas.SetBottom(textBlock, notePanel.ActualHeight - actualY);
                }
                else
                {
                    //是分拍
                    line.Stroke = new SolidColorBrush(Color.FromRgb(254, 235, 255));
                    line.StrokeThickness = 1;
                }
                notePanel.Children.Add(line);
            }
        }

        //绘制谱面预览
        public void DrawPreview()
        {
            double width = trackPreview.ActualWidth;
            double height = trackPreview.ActualHeight;
            foreach (var item in trackPreview.Children)
            {
                if (item is System.Windows.Shapes.Line line)
                {
                    line.Stroke = null;
                }
                else if(item is Rectangle rect)
                {
                    rect.Fill = null;
                }
            }
            trackPreview.Children.Clear();
            //调整预览范围
            if(window.playerTime < previewStartTime)
            {
                previewStartTime = window.playerTime;
            }
            else if(window.playerTime > previewStartTime + previewRange * window.track.length)
            {
                previewStartTime = window.playerTime - previewRange * window.track.length;
            }
            //绘制谱面
            foreach (var line in window.track.lines)
            {
                foreach (var note in line.notes)
                {
                    if (note.type == NoteType.Hold)
                    {
                        double x = note.rail * width / 9;
                        double y = (note.actualTime - previewStartTime) / (window.track.Length * previewRange) * height;
                        double w = width / 9;
                        double h = note.actualHoldTime / (window.track.Length * previewRange) * height;
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
                        double x = note.rail * width / 9;
                        double y = (note.actualTime - previewStartTime) / (window.track.Length * previewRange) * height;
                        double w = width / 9;
                        double h = 2;
                        Rectangle rect = new()
                        {
                            Width = w,
                            Height = h,
                            Fill = note.type == NoteType.Tap ? new SolidColorBrush(EditorColors.tapColor) : new SolidColorBrush(EditorColors.catchColor)
                        };
                        trackPreview.Children.Add(rect);
                        Canvas.SetLeft(rect, x);
                        Canvas.SetBottom(rect, y);
                    }
                }
            }
            //绘制时间线
            double timeLineY = height - (window.playerTime - previewStartTime) / (window.track.Length * previewRange) * height;
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

        public void DrawTrackWithEvent()
        {
            //清空画布
            trackPreviewWithEvent.Children.OfType<Rectangle>()
                .ToList()
                .ForEach(t =>
                {
                    t.Fill = null;
                    trackPreviewWithEvent.Children.Remove(t);
                });
            double height = trackPreviewWithEvent.ActualHeight;
            double width = trackPreviewWithEvent.ActualWidth;
            trackPreviewWithEvent.Children.Clear();
            //绘制note
            //如果谱面正在播放，就直接让note按照对应速度移动
            //如果谱面没有播放，就计算note的位置
            double time = window.playerTime;
            double noteWidth = trackPreviewWithEvent.ActualWidth / 9;
            foreach(var line in nbtTrack.lines)
            {
                if(editingMode == 2)
                {
                    continue;
                }
                foreach(var note in line.notes)
                {
                    if(note is NBTHold hold)
                    {
                        if (hold.summonTime <= time && time <= hold.judgeTime + hold.holdTime)
                        {
                            Rectangle noteRec = new()
                            {
                                Width = noteWidth,
                                Height = Math.Max(hold.holdLength / 10 * trackPreviewWithEvent.ActualHeight,0),
                                Fill = new SolidColorBrush(EditorColors.holdColor)
                            };
                            //计算note的位置
                            double i = time;
                            double locate = 0;
                            for (; i < hold.judgeTime + hold.holdTime - 1 / Settings.currSetting.Tick; i += 1 / Settings.currSetting.Tick)
                            {
                                locate += line.line.GetSpeed(i) / Settings.currSetting.Tick;
                            }
                            //计算差值保证帧数
                            double delta = hold.judgeTime + hold.holdTime - i;
                            locate += line.line.GetSpeed(i) * delta;
                            locate -= hold.holdLength;
                            locate = locate / 10 * trackPreviewWithEvent.ActualHeight;
                            trackPreviewWithEvent.Children.Add(noteRec);
                            Canvas.SetBottom(noteRec, locate);
                            Canvas.SetLeft(noteRec, note.rail * noteWidth);
                        }
                    }
                    else 
                    {
                        if (note.summonTime <= time && time <= note.judgeTime)
                        {
                            SolidColorBrush solidColorBrush;
                            if(note is NBTFakeCatch fakeCatch)
                            {
                                solidColorBrush = new(FakeCatch.GetColor(fakeCatch.height));
                            }
                            else
                            {
                                solidColorBrush = note.type == (int)NoteType.Tap ? new SolidColorBrush(EditorColors.tapColor) : new SolidColorBrush(EditorColors.catchColor);
                            }
                            Rectangle noteRec = new()
                            {
                                Width = noteWidth,
                                Height = 10,
                                Fill = solidColorBrush
                            };
                            //计算note的位置
                            double i = time;
                            double locate = 0;
                            for (; i < note.judgeTime - 1 / Settings.currSetting.Tick; i += 1 / Settings.currSetting.Tick)
                            {
                                locate += line.line.GetSpeed(i) / Settings.currSetting.Tick;
                            }
                            //计算差值保证帧数
                            double delta = note.judgeTime - i;
                            locate += line.line.GetSpeed(i) * delta;
                            locate = locate / 10 * trackPreviewWithEvent.ActualHeight;
                            trackPreviewWithEvent.Children.Add(noteRec);
                            Canvas.SetBottom(noteRec, locate);
                            Canvas.SetLeft(noteRec, note.rail * noteWidth);
                        }
                    }
                }
            }
            //播放note音效
            //目前显示的时间范围
            double minTime = window.playerTime;
            double maxTime = minTime + notePanel.ActualHeight / pixelPreBeat * secondsPreBeat;
            for (int i = 0; i < window.track.lines.Count; i++)
            {
                foreach (var note in window.track.lines[i].notes)
                {
                    if (note.type == NoteType.Hold)
                    {
                        if (minTime <= note.actualTime + note.actualHoldTime)
                        {
                            //在判定线上方
                            if (minTime <= note.actualTime)
                            {
                                if (note.hasJudged)
                                {
                                    note.hasJudged = false;
                                }
                            }
                            else
                            {
                                //如果未被判定过且谱面正在被播放
                                if ((!note.hasJudged) && window.isPlaying)
                                {
                                    window.tapSoundManager.AppendSoundRequest();
                                    note.hasJudged = true;
                                }
                            }
                        }
                        continue;
                    }
                    //在判定线上方
                    if (minTime <= note.actualTime)
                    {
                        if (note.hasJudged)
                        {
                            note.hasJudged = false;
                        }
                    }
                    //在判定线下方
                    else
                    {
                        //如果未被判定过且谱面正在被播放
                        if ((!note.hasJudged) && window.isPlaying)
                        {
                            if(note.type == NoteType.Tap)
                            {
                                window.tapSoundManager.AppendSoundRequest();
                            }
                            else
                            {
                                window.catchSoundManager.AppendSoundRequest();
                            }
                            note.hasJudged = true;
                        }
                    }
                }
            }
        }

        //更新
        private void Update()
        {
            if (window.isPlaying)
            {
                window.playerTime = window.player.Position.TotalSeconds;
                //更新时间
                timeDis.Content = window.playerTime.ToString("0.00") + " / " + window.track.Length.ToString("0.00");
                heightDis.Content = $"Fake Catch高度: {catchHeight:F2}";
            }
            if (previewing)
            {
                //更新谱面预览
                DrawTrackWithEvent();
            }
            else
            {
                //更新note和事件
                if (window.isPlaying || noteChange)
                {
                    UpdateNote();
                    noteChange = false;
                }
                if (editingMode == EVENT && (window.isPlaying || eventChange))
                {
                    UpdateEvent();
                    eventChange = false;
                }
                if(editingMode == FAKE_CATCH && (window.isPlaying || fakeCatchChange))
                {
                    UpdateFakeCatch();
                    fakeCatchChange = false;
                }
                if(editingMode == FUNCTION && (window.isPlaying || functionChange))
                {
                    UpdateFunction();
                    functionChange = false;
                }
            }
            window.tapSoundManager.PlaySound();
            window.catchSoundManager.PlaySound();
        }

        public void UpdateNote()
        {
            //目前显示的时间范围
            double minTime = window.playerTime;
            double maxTime = minTime + notePanel.ActualHeight / pixelPreBeat * secondsPreBeat;
            for (int i = 0; i < window.track.lines.Count; i++)
            {
                foreach (var note in window.track.lines[i].notes)
                {
                    if (note.type == NoteType.Hold)
                    {
                        if (minTime <= note.actualTime + note.actualHoldTime)
                        {
                            //在判定线上方
                            if (minTime <= note.actualTime)
                            {
                                if (note.hasJudged)
                                {
                                    note.hasJudged = false;
                                }
                            }
                            else
                            {
                                //如果未被判定过且谱面正在被播放
                                if ((!note.hasJudged) && window.isPlaying)
                                {
                                    window.tapSoundManager.AppendSoundRequest();
                                    note.hasJudged = true;
                                }
                            }
                            //在可视区域内
                            if (note.actualTime < maxTime)
                            {
                                note.rectangle.Visibility = Visibility.Visible;
                                double qwq = Math.Max(minTime, note.actualTime);    //末端
                                double pwp = Math.Min(note.actualTime + note.actualHoldTime, maxTime);  //上端
                                note.rectangle.Height = GetYFromTime(pwp) - GetYFromTime(qwq);
                                Canvas.SetBottom(note.rectangle, GetYFromTime(qwq));
                                Canvas.SetLeft(note.rectangle, note.rail * notePanel.ActualWidth / 9);
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
                    if (minTime <= note.actualTime)
                    {
                        if (note.hasJudged)
                        {
                            note.hasJudged = false;
                        }
                        //在可视区域内
                        if (note.actualTime <= maxTime)
                        {
                            note.rectangle.Visibility = Visibility.Visible;
                            double qwq = note.actualTime - minTime;
                            Canvas.SetBottom(note.rectangle, qwq / secondsPreBeat * pixelPreBeat);
                            Canvas.SetLeft(note.rectangle, note.rail * notePanel.ActualWidth / 9);
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
                        if ((!note.hasJudged) && window.isPlaying)
                        {
                            if(note.type == NoteType.Tap)
                            {
                                window.tapSoundManager.AppendSoundRequest();
                            }
                            else
                            {
                                window.catchSoundManager.AppendSoundRequest();
                            }
                            note.hasJudged = true;
                        }
                        note.rectangle.Visibility = Visibility.Hidden;
                    }
                }
            }
        }

        public void UpdateFakeCatch()
        {
            //目前显示的时间范围
            double minTime = window.playerTime;
            double maxTime = minTime + notePanel.ActualHeight / pixelPreBeat * secondsPreBeat;
            for (int i = 0; i < window.track.lines.Count; i++)
            {
                foreach (var fakeCatch in CurrLine.fakeCatch)
                {
                    //在判定线上方
                    if (minTime <= fakeCatch.actualTime)
                    {
                        //在可视区域内
                        if (fakeCatch.actualTime <= maxTime)
                        {
                            fakeCatch.rectangle.Visibility = Visibility.Visible;
                            double qwq = fakeCatch.actualTime - minTime;
                            Canvas.SetBottom(fakeCatch.rectangle, qwq / secondsPreBeat * pixelPreBeat);
                            Canvas.SetLeft(fakeCatch.rectangle, fakeCatch.rail * notePanel.ActualWidth / 9);
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
        }


        public void UpdateFunction()
        {
            //目前显示的时间范围
            double minTime = window.playerTime;
            double maxTime = minTime + functionPanel.ActualHeight / pixelPreBeat * secondsPreBeat;
            for (int i = 0; i < window.track.lines.Count; i++)
            {
                foreach (var function in CurrLine.functions)
                {
                    //在判定线上方
                    if (minTime <= function.time)
                    {
                        //在可视区域内
                        if (function.time <= maxTime)
                        {
                            function.rectangle.Visibility = Visibility.Visible;
                            double qwq = function.time - minTime;
                            Canvas.SetBottom(function.rectangle, qwq / secondsPreBeat * pixelPreBeat);
                            Canvas.SetLeft(function.rectangle, function.rail * functionPanel.ActualWidth / 9);
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
            }
        }

        public void UpdateEvent()
        {
            //目前显示的时间范围
            double minTime = window.playerTime;
            double maxTime = minTime + eventPanel.ActualHeight / pixelPreBeat * secondsPreBeat;
            for (int i = 0; i < window.track.lines.Count; i++)
            {
                for(int j = 0; j < window.track.lines[i].eventLists.Count; j++)
                {
                    foreach (var e in window.track.lines[i].eventLists[j].events)
                    {
                        if (minTime <= e.endTime)
                        {
                            //在可视区域内
                            if (e.startTime < maxTime)
                            {
                                e.rectangle.Visibility = Visibility.Visible;
                                Canvas.SetBottom(e.rectangle, GetYFromTime(e.startTime));
                                Canvas.SetLeft(e.rectangle, (j - page * 9) * notePanel.ActualWidth / 9);
                            }
                            else
                            {
                                e.rectangle.Visibility = Visibility.Hidden;
                            }
                        }
                        else
                        {
                            e.rectangle.Visibility = Visibility.Hidden;
                        }
                        continue;
                    }
                }
            }
            UpdateFunctionCurve();
        }

        public void UpdateEventTypeList()
        {
            for (int i = 0; i < 9; i++)
            {
                var textBlock = (columnInfo.Children[i] as TextBlock)!;
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
                    {
                        textBlock.Foreground = new SolidColorBrush(Colors.Gray);
                    }
                    else if (CurrLine.eventLists[i + page*9].IsMainEvent())
                    {
                        textBlock.Foreground = new SolidColorBrush(Colors.Orange);
                    }
                    else
                    {
                        textBlock.Foreground = new SolidColorBrush(Colors.Olive);
                    }
                }
                textBlock.Text = text;

            }
        }

        public void UpdateFunctionCurve()
        {
            foreach (var line in window.track.lines)
            {
                foreach(var el in line.eventLists)
                {
                    foreach(var e in el.events)
                    {
                        if (e.isHeaderEvent)
                        {
                            List<EventRectangle> rs = new();
                            foreach(var item in e.EventGroup)
                            {
                                rs.Add(item.rectangle);
                            }
                            EventRectangle.DrawFunction(rs);
                            rs.Clear();
                        }
                        e.rectangle.UpdateText();
                    }
                }
            }
        }

        //放置hold，或者拖动note
        private void notePanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (!window.isPlaying && !isSelecting)
            {
                var endPos = GetAlignedPoint(e.GetPosition(notePanel));
                if (startPos != new Point(-1, -1))
                {
                    //hold预览
                    if (endPos.Y > startPos.Y)
                    {
                        notePreview.Height = endPos.Y - startPos.Y;
                        if (window.puttingTap)
                        {
                            notePreview.Fill = tapBrush;
                        }
                        else
                        {
                            notePreview.Fill = catchBrush;
                        }
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
                //计算拍数
                double mouseTime = endPos.Y / pixelPreBeat * secondsPreBeat + window.playerTime;
                int beat = (int)(mouseTime / secondsPreBeat);
                int divBeat = (int)((mouseTime % secondsPreBeat) / secondsPreDevideBeat);
                beatDis.Content = $"beat {beat}:{divBeat}/{divideNum}";
            }
            else
            {
                if (isSelecting || isDraging)
                {
                    var endPos = GetAlignedPoint(e.GetPosition(notePanel)); 
                    //计算拍数
                    double mouseTime = endPos.Y / pixelPreBeat * secondsPreBeat + window.playerTime;
                    int beat = (int)(mouseTime / secondsPreBeat);
                    int divBeat = (int)((mouseTime % secondsPreBeat) / secondsPreDevideBeat);
                    beatDis.Content = $"beat {beat}:{divBeat}/{divideNum}";
                    if (isDraging)
                    {
                        if (selectedNotes[0].rectangle.IsResizing)
                        {
                            var n = selectedNotes[0];
                            double y = Math.Abs(endPos.Y - GetYFromTime(n.actualTime));
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
            if(editingMode == 2)
            {
                notePreview.Fill = fakeCatchBrush;
            }
            else if (window.puttingTap)
            {
                notePreview.Fill = tapBrush;
            }
            else
            {
                notePreview.Fill = catchBrush;
            }
        }

        private void notePanel_MouseLeave(object sender, MouseEventArgs e)
        {
            notePreview.Visibility = Visibility.Collapsed;
        }

        Point startPos = new(-1, -1);

        //放置note
        private void notePanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (window.isPlaying) return;
            if (startPos == new Point(-1, -1))
            {
                startPos = GetAlignedPoint(e.GetPosition(notePanel));
                if (editingMode == 0 && CurrLine.ClickOnNote(GetTimeFromY(startPos.Y), (int)Math.Round(startPos.X * 9 / notePanel.ActualWidth))
                || editingMode == 2 && CurrLine.ClickOnFakeCatch(GetTimeFromY(startPos.Y), (int)Math.Round(startPos.X * 9 / notePanel.ActualWidth)))
                {
                    startPos = new Point(-1, -1);
                }
            }
        }

        //放置note，完成拖动
        private void notePanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //获取鼠标位置
            var endPos = GetAlignedPoint(e.GetPosition(notePanel));
            //拖动note
            if (isDraging)
            {
                if (selectedNotes[0].rectangle.IsResizing)
                {
                    selectedNotes[0].actualHoldTime = GetTimeFromY(endPos.Y) - selectedNotes[0].actualTime;
                }
                else
                {
                    foreach (var note in selectedNotes)
                    {
                        double x = Canvas.GetLeft(note.rectangle);
                        double y = Canvas.GetBottom(note.rectangle);
                        Point point = new(x, notePanel.ActualHeight - y);
                        point = GetAlignedPoint(point);
                        note.rail = (int)Math.Round(x * 9 / notePanel.ActualWidth);
                        if (note.rail < 0)
                        {
                            note.rail = 0;
                        }
                        note.actualTime = GetTimeFromY(point.Y);
                        Canvas.SetBottom(note.rectangle, point.Y);
                    }
                }
                UpdateSelectedNote(selectedNotes);
                isDraging = false;
                return;
            }
            if (window.isPlaying) return;
            if (startPos == new Point(-1, -1)) return;
            if (selectedNotes.Count != 0)
            {
                UpdateSelectedNote(new());
                infoFrame.Content = null;
                startPos = new(-1,-1);
                return;
            }
            //获取时间
            var startTime = GetTimeFromY(startPos.Y);
            var endTime = GetTimeFromY(endPos.Y);
            int rail = (int)Math.Round(startPos.X * 9 / notePanel.ActualWidth);
            if (CurrLine.ClickOnEvent(endTime, rail))
            {
                startPos = new Point(-1, -1);
                return;
            }
            if(editingMode == 2)
            {
                willPutNote ??= new FakeCatch(
                    rail: rail, 
                    fallType: 0,
                    actualTime: startTime, 
                    catchHeight
                    );
                if (CurrLine == window.track.FreeLine)
                {
                    willPutNote = new FreeFakeCatch(willPutNote as FakeCatch);
                }
                willPutNote.rectangle.Height = 10;
                willPutNote.rectangle.Width = notePreview.Width;
            }
            //放置note
            else if (endTime - startTime >= secondsPreDevideBeat)
            {
                willPutNote ??= new Note(
                    rail: rail,
                    noteType: (int)NoteType.Hold,
                    fallType: 0,
                    isFake: false,
                    actualTime: startTime,
                    actualHoldTime: endTime - startTime,
                    isCurrentLineNote: true);
                if (CurrLine == window.track.FreeLine)
                {
                    willPutNote = new FreeNote(willPutNote);
                }
                willPutNote.rectangle.Height = endPos.Y - startPos.Y;
                willPutNote.rectangle.Width = notePreview.Width;
            }
            else
            {
                willPutNote ??= new Note(
                    rail: rail,
                    noteType: (int)(window.puttingTap ? NoteType.Tap : NoteType.Catch),
                    fallType: 0,
                    isFake: false,
                    actualTime: startTime,
                    isCurrentLineNote: true);
                if (CurrLine == window.track.FreeLine)
                {
                    willPutNote = new FreeNote(willPutNote);
                }
                willPutNote.rectangle.Height = 10;
                willPutNote.rectangle.Width = notePreview.Width;
            }
            willPutNote.parentLine = CurrLine;
            if (!CurrLine.IsNoteOverLap(willPutNote))
            {
                if(willPutNote is FakeCatch f)
                {
                    CurrLine.fakeCatch.Add(f);
                    fakeCatchChange = true;
                }
                else
                {
                    CurrLine.notes.Add(willPutNote);
                    noteChange = true;
                }
                notePanel.Children.Add(willPutNote.rectangle);
                Canvas.SetLeft(willPutNote.rectangle, willPutNote.rail * notePanel.ActualWidth / 9);
                Canvas.SetBottom(willPutNote.rectangle, startPos.Y);
                OperationManager.AddOperation(new PutNoteOperation(willPutNote, CurrLine));
            }
            willPutNote = null;
            startPos = new(-1, -1);
        }

        //调整note大小
        private void notePanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            notePreview.Width = notePanel.ActualWidth / 9;
            foreach (var line in window.track.lines)
            {
                line.notes.ForEach(e =>
                {
                    e.rectangle.Width = notePanel.ActualWidth / 9;
                });
                line.fakeCatch.ForEach(f =>
                {
                    f.rectangle.Width = notePanel.ActualWidth / 9;
                });
            }
        }

        //调整事件大小
        private void eventPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            eventPreview.Width = eventPanel.ActualWidth / 9;
            foreach (var line in window.track.lines)
            {
                line.eventLists.ForEach(el =>
                {
                    el.events.ForEach(e =>
                    {
                        e.rectangle.Width = eventPanel.ActualWidth / 9;
                    });
                });
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
            if (window != null && window.player != null)
            {
                window.player.SpeedRatio = speed;
            }
        }

        //切换速度
        private void speedChooseBox_MouseEnter(object sender, MouseEventArgs e)
        {
            speedChooseBox.Focusable = true;
        }

        //切换速度
        private void speedChooseBox_MouseLeave(object sender, MouseEventArgs e)
        {
            speedChooseBox.Focusable = false;
        }

        private bool isDeleting = false;
        //切换判定线
        private void lineListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isDeleting)
            {
                isDeleting = false;
                return;
            }
            if (editingMode == 0)
            {
                foreach (var note in CurrLine.notes)
                {
                    if (note.type == NoteType.Tap)
                    {
                        note.Color = EditorColors.tapColorButNotOnThisLine;
                    }
                    else if(note.type == NoteType.Hold)
                    {
                        note.Color = EditorColors.holdColorButNotOnThisLine;
                    }
                    else
                    {
                        note.Color = EditorColors.catchColorButNotOnThisLine;
                    }
                }
                lineIndex = lineListView.SelectedIndex;
                selectingFreeLine = false;
                foreach (var note in CurrLine.notes)
                {
                    if (note.type == NoteType.Tap)
                    {
                        note.Color = EditorColors.tapColor;
                    }
                    else if (note.type == NoteType.Hold)
                    {
                        note.Color = EditorColors.holdColor;
                    }
                    else
                    {
                        note.Color = EditorColors.catchColor;
                    }
                }
            }
            else if(editingMode == 1)
            {
                foreach (var el in CurrLine.EventLists)
                {
                    el.events.ForEach(e =>
                    {
                        e.rectangle.Visibility = Visibility.Hidden;
                    });
                }
                lineIndex = lineListView.SelectedIndex;
                selectingFreeLine = false;
                foreach (var el in CurrLine.eventLists)
                {
                    el.events.ForEach(e =>
                    {
                        e.rectangle.Visibility = Visibility.Visible;
                    });
                }
                UpdateEventTypeList();
            }
            else
            {
                CurrLine.fakeCatch.ForEach(e =>
                {
                    e.rectangle.Visibility = Visibility.Hidden;
                });
                lineIndex = lineListView.SelectedIndex;
                selectingFreeLine = false;
                CurrLine.fakeCatch.ForEach(e =>
                {
                    e.rectangle.Visibility = Visibility.Visible;
                });
            }
        }

        //删除
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

            isDeleting = true;
            int i = lineIndex - 1;
            if (i < 0)
            {
                i = 0;
            }
            if(window.track.lines.Count == 1)
            {
                window.track.lines.Add(new(0));
            }
            lineListView.SelectedIndex = i;
            OperationManager.AddOperation(new RemoveLineOperation(CurrLine));
            window.track.lines.RemoveAt(lineIndex);
        }

        //重命名
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            Grid box = (((sender as MenuItem)!.Parent as ContextMenu)!.PlacementTarget as Grid)!;
            box.Children[0].Visibility = Visibility.Collapsed;
            (box.Children[1] as TextBox)!.Text = CurrLine.id;
            (box.Children[1] as TextBox)!.SelectAll();
            box.Children[1].Visibility = Visibility.Visible;
            box.Children[1].Focus();

        }

        //属性
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            infoFrame.Content = new Pages.LineInfoFrame(CurrLine, lineIndex);
        }

        //新建判定线
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Line qwq = new(0);
            window.track.lines.Add(qwq);
            lineIndex = window.track.lines.Count - 1;
            lineListView.SelectedIndex = lineIndex;
        }

        //判定线重命名
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Grid box = ((sender as TextBox)!.Parent as Grid)!;
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
                Grid box = ((sender as TextBox)!.Parent as Grid)!;
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
            if (window.isPlaying) return;
            if (startPos == new Point(-1, -1))
            {
                startPos = GetAlignedPoint(e.GetPosition(eventPanel));
                if (CurrLine.ClickOnEvent(GetTimeFromY(startPos.Y), (int)Math.Round(startPos.X * 9 / notePanel.ActualWidth)))
                {
                    startPos = new Point(-1, -1);
                }
            }
        }

        //放置事件
        private void eventPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //获取鼠标位置
            var endPos = GetAlignedPoint(e.GetPosition(eventPanel));
            //拖动事件
            if (isDraging)
            {
                if (selectedEvents[0].rectangle.IsResizing == 1)
                {
                    var ev = selectedEvents[0];
                    //调整结束时间
                    ev.endTime = GetTimeFromY(endPos.Y);
                }
                else if(selectedEvents[0].rectangle.IsResizing == -1)
                {
                    var ev = selectedEvents[0];
                    //调整开始时间
                    ev.startTime = GetTimeFromY(endPos.Y);
                }
                else
                {
                    foreach (var @event in selectedEvents)
                    {
                        double y = Canvas.GetBottom(@event.rectangle);
                        double i = @event.startTime;
                        @event.startTime = GetTimeFromY(y);
                        @event.endTime += @event.startTime - i;
                    }
                    UpdateSelectedEvent(selectedEvents);
                }
                isDraging = false;
                return;
            }
            if (window.isPlaying) return;
            if (startPos == new Point(-1, -1)) return;
            if (selectedEvents.Count != 0)
            {
                UpdateSelectedEvent(new());
                infoFrame.Content = null;
                startPos = new Point(-1, -1);
                return;
            }
            //获取时间
            var startTime = GetTimeFromY(startPos.Y);
            var endTime = GetTimeFromY(endPos.Y);
            //计算轨道
            int rail = (int)Math.Round(startPos.X * 9 / eventPanel.ActualWidth) + page * 9;
            if (CurrLine.ClickOnEvent(endTime, rail))
            {
                startPos = new Point(-1, -1);
                return;
            }
            //最小时间
            if (endTime - startTime <= secondsPreDevideBeat)
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
                startTime: startTime,
                endTime: endTime,
                easeFunction: "Linear",
                startValue: v,
                endValue: v
                );
            willPutEvent.rectangle.Height = endPos.Y - startPos.Y;
            willPutEvent.rectangle.Width = eventPreview.Width;
            if (!CurrLine.eventLists[rail].events.Contains(willPutEvent))
            {
                eventPanel.Children.Add(willPutEvent.rectangle);
                Canvas.SetLeft(willPutEvent.rectangle, rail * eventPanel.ActualWidth / 9);
                Canvas.SetBottom(willPutEvent.rectangle, startPos.Y);
                CurrLine.eventLists[rail].events.Add(willPutEvent);
                willPutEvent.parentList = CurrLine.eventLists[rail];
                eventChange = true;
                OperationManager.AddOperation(new PutEventOperation(willPutEvent, CurrLine.eventLists[rail]));
                //更新这个轨道的事件组
                CurrLine.eventLists[rail].GroupEvent();
            }
            willPutEvent = null;
            startPos = new(-1, -1);
            eventPreview.Visibility = Visibility.Collapsed;
            UpdateEventTypeList();
        }

        //拖动事件或者放置事件，改变事件的时间
        private void eventPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (!window.isPlaying && !isSelecting)
            {
                var endPos = GetAlignedPoint(e.GetPosition(eventPanel));
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
                //计算拍数
                double mouseTime = endPos.Y / pixelPreBeat * secondsPreBeat + window.playerTime;
                int beat = (int)(mouseTime / secondsPreBeat);
                int divBeat = (int)((mouseTime % secondsPreBeat) / secondsPreDevideBeat);
                beatDis.Content = $"beat {beat}:{divBeat}/{divideNum}";
            }
            else
            {
                if (isSelecting || isDraging)
                {
                    var endPos = GetAlignedPoint(e.GetPosition(eventPanel));
                    //计算拍数
                    double mouseTime = endPos.Y / pixelPreBeat * secondsPreBeat + window.playerTime;
                    int beat = (int)(mouseTime / secondsPreBeat);
                    int divBeat = (int)((mouseTime % secondsPreBeat) / secondsPreDevideBeat);
                    beatDis.Content = $"beat {beat}:{divBeat}/{divideNum}";
                    if (isDraging)
                    {
                        if (selectedEvents[0].rectangle.IsResizing == 1)
                        {
                            var ev = selectedEvents[0];
                            //改结束时间
                            double t = GetTimeFromY(endPos.Y);
                            double y = Math.Abs(GetYFromTime(t) - GetYFromTime(ev.startTime));
                            ev.rectangle.Height = y;
                        }
                        else if(selectedEvents[0].rectangle.IsResizing == -1)
                        {
                            var ev = selectedEvents[0];
                            //改开始时间
                            double t = GetTimeFromY(endPos.Y);
                            double y = Math.Abs(GetYFromTime(ev.endTime) - GetYFromTime(t));
                            ev.rectangle.Height = y;
                            Canvas.SetBottom(ev.rectangle, GetYFromTime(t));
                        }
                        else
                        {
                            endPos.X = dragStartPoint.X;    //只能竖着拖
                            //获取鼠标移动的位置
                            var delta = endPos - dragStartPoint;
                            dragStartPoint = endPos;
                            foreach (var @event in selectedEvents)
                            {
                                if (Canvas.GetBottom(@event.rectangle) + delta.Y < 0) return;
                            }
                            //拖动事件
                            foreach (var @event in selectedEvents)
                            {
                                //移动事件
                                Canvas.SetLeft(@event.rectangle, Canvas.GetLeft(@event.rectangle) + delta.X);
                                Canvas.SetBottom(@event.rectangle, Canvas.GetBottom(@event.rectangle) + delta.Y);
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
            if (!window.isPlaying && !isSelecting)
            {
                var endPos = GetAlignedPoint(e.GetPosition(notePanel));
                //tap预览
                functionPreview.Height = 10;
                //获取鼠标位置，生成函数位置预览
                functionPreview.Visibility = Visibility.Visible;
                Canvas.SetLeft(functionPreview, endPos.X);
                Canvas.SetBottom(functionPreview, endPos.Y);
                //计算拍数
                double mouseTime = endPos.Y / pixelPreBeat * secondsPreBeat + window.playerTime;
                int beat = (int)(mouseTime / secondsPreBeat);
                int divBeat = (int)((mouseTime % secondsPreBeat) / secondsPreDevideBeat);
                beatDis.Content = $"beat {beat}:{divBeat}/{divideNum}";
            }
            else
            {
                if (isSelecting || isDraging)
                {
                    var endPos = GetAlignedPoint(e.GetPosition(notePanel));
                    //计算拍数
                    double mouseTime = endPos.Y / pixelPreBeat * secondsPreBeat + window.playerTime;
                    int beat = (int)(mouseTime / secondsPreBeat);
                    int divBeat = (int)((mouseTime % secondsPreBeat) / secondsPreDevideBeat);
                    beatDis.Content = $"beat {beat}:{divBeat}/{divideNum}";
                    if (isDraging)
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
            var pos = GetAlignedPoint(e.GetPosition(functionPanel));
            //拖动function
            if (isDraging)
            {
                foreach (var @event in selectedEvents)
                {
                    double y = Canvas.GetBottom(@event.rectangle);
                    double i = @event.startTime;
                    @event.startTime = GetTimeFromY(y);
                    @event.endTime += @event.startTime - i;
                }
                UpdateSelectedEvent(selectedEvents);
                isDraging = false;
                return;
            }
            if (window.isPlaying) return;
            if (selectedEvents.Count != 0)
            {
                UpdateSelectedFunction(new());
                infoFrame.Content = null;
                return;
            }
            //获取时间
            var time = GetTimeFromY(pos.Y);
            //计算轨道
            int rail = (int)Math.Round(pos.X * 9 / eventPanel.ActualWidth) + page * 9;
            if (CurrLine.ClickOnFunction(time, rail)) return;
            //放置
            willPutFunction = new Function(
                time: time,
                rail: rail,
                functionName: Guid.NewGuid().ToString()
                );
            willPutFunction.rectangle.Height = 10;
            willPutFunction.rectangle.Width = eventPreview.Width;
            if (!CurrLine.functions.Contains(willPutFunction))
            {
                functionPanel.Children.Add(willPutFunction.rectangle);
                Canvas.SetLeft(willPutFunction.rectangle, rail * functionPanel.ActualWidth / 9);
                Canvas.SetBottom(willPutFunction.rectangle, pos.Y);
                CurrLine.functions.Add(willPutFunction);
                willPutFunction.parentLine = CurrLine;
                willPutFunction.TryLink(EditorWindow.Instance.track ,true);
                functionChange = true;
                OperationManager.AddOperation(new PutFunctionOperation(willPutFunction, CurrLine));
            }
            willPutFunction = null;
            functionPreview.Visibility = Visibility.Collapsed;
        }

        private void functionPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            functionPreview.Width = functionPanel.ActualWidth / 9;
            foreach (var line in window.track.lines)
            {
                line.functions.ForEach(f =>
                {
                    f.rectangle.Width = eventPanel.ActualWidth / 9;
                });
            }
        }

        public void UpdateSelectedNote(List<Note> note)
        {
            if (note.Count == 1) UpdateSelectedNote(note[0]);
            selectedNotes.ForEach(note => { note.rectangle.HighLight = false; });
            selectedNotes = note;
            selectedNotes.ForEach(note => { note.rectangle.HighLight = true; });
            isDraging = isSelecting = selectedNotes.Count != 0;
            dragStartPoint = GetAlignedPoint(Mouse.GetPosition(notePanel));
        }

        public void UpdateSelectedNote(Note note, bool multiSelect = false)
        {
            if (!multiSelect)
            {
                selectedNotes.ForEach(note => { note.rectangle.HighLight = false; });
                selectedNotes.Clear();
            }
            selectedNotes.Add(note);
            note.rectangle.HighLight = true;
            isSelecting = isDraging = true;
            dragStartPoint = GetAlignedPoint(Mouse.GetPosition(notePanel));
        }

        public void UpdateSelectedEvent(List<Event> @events)
        {
            if(@events.Count == 1) UpdateSelectedEvent(@events[0]);
            selectedEvents.ForEach(@event => { @event.rectangle.HighLight = false; });
            selectedEvents = @events;
            selectedEvents.ForEach(@event => { @event.rectangle.HighLight = true; });
            isSelecting = selectedEvents.Count != 0;
            isDraging = isSelecting;
            dragStartPoint = GetAlignedPoint(Mouse.GetPosition(eventPanel));
        }

        public void UpdateSelectedEvent(Event @events, bool multiSelect = false)
        {
            if (!multiSelect)
            {
                selectedEvents.ForEach(@event => { @event.rectangle.HighLight = false; });
                selectedEvents.Clear();
            }
            selectedEvents.Add(@events);
            @events.rectangle.HighLight = true;
            isSelecting = selectedEvents.Count != 0;
            isDraging = isSelecting;
            dragStartPoint = GetAlignedPoint(Mouse.GetPosition(eventPanel));
        }

        public void UpdateSelectedFunction(List<Function> function)
        {
            if (function.Count == 1) UpdateSelectedFunction(function[0]);
            selectedFunctions.ForEach(function => { function.rectangle.HighLight = false; });
            selectedFunctions = function;
            selectedFunctions.ForEach(function => { function.rectangle.HighLight = true; });
            isDraging = isSelecting = selectedFunctions.Count != 0;
            dragStartPoint = GetAlignedPoint(Mouse.GetPosition(functionPanel));
        }

        public void UpdateSelectedFunction(Function function, bool multiSelect = false)
        {
            if (!multiSelect)
            {
                selectedFunctions.ForEach(function => { function.rectangle.HighLight = false; });
                selectedFunctions.Clear();
            }
            selectedFunctions.Add(function);
            function.rectangle.HighLight = true;
            isSelecting = isDraging = true;
            dragStartPoint = GetAlignedPoint(Mouse.GetPosition(functionPanel));
        }


        private void trackPreview_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //时间设置
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                previewRange = Math.Min(previewRange + 0.1 * e.Delta / 60.0, 1.0);
                previewRange = Math.Max(10 / window.track.Length, previewRange);
            }
            else
            {
                Update();
                noteChange = true;
                window.player.Pause();
                window.isPlaying = false;
                double t = window.playerTime + e.Delta / 120.0 * window.track.Length * previewRange / 10.0;
                t = Math.Max(0, t);
                t = Math.Min(window.track.Length * previewRange + previewStartTime, t);
                //调整预览窗格
                if(t - previewStartTime > window.track.Length * previewRange)
                {
                    previewStartTime = t - window.track.Length * previewRange;
                }
                else if (t < previewStartTime)
                {
                    previewStartTime = t;
                }
                window.playerTime = t;
                window.player.Position = TimeSpan.FromSeconds(window.playerTime);
                timeDis.Content = window.playerTime.ToString("0.00") + " / " + window.track.Length.ToString("0.00");
            }
        }

        bool isMouseDown = false;
        private void trackPreview_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //获取鼠标位置
            var pos = Mouse.GetPosition(trackPreview);
            double time = (trackPreview.ActualHeight - pos.Y) / trackPreview.ActualHeight * window.track.Length * previewRange + previewStartTime;
            window.player.Position = TimeSpan.FromSeconds(time);
            window.playerTime = time;
            //暂停播放器
            window.player.Pause();
            window.isPlaying = false;
            //更新时间
            timeDis.Content = window.playerTime.ToString("0.00") + " / " + window.track.Length.ToString("0.00");
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
            double time = (trackPreview.ActualHeight - pos.Y) / trackPreview.ActualHeight * window.track.Length * previewRange + previewStartTime;
            window.player.Position = TimeSpan.FromSeconds(time);
            window.playerTime = time;
            //更新时间
            timeDis.Content = window.playerTime.ToString("0.00") + " / " + window.track.Length.ToString("0.00");
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
                nbtTrack = NBTTrack.FromTrack(window.track);
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

        byte ModeSet(byte mode)
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

        void ModeReset()
        {
            eventPanel.Visibility = Visibility.Hidden;
            functionPanel.Visibility = Visibility.Hidden;
            foreach (var line in window.track.lines)
            {
                line.fakeCatch.ForEach(e =>
                {
                    e.rectangle.Visibility = Visibility.Hidden;
                });
                line.notes.ForEach(note =>
                {
                    if (note.type == NoteType.Tap)
                    {
                        note.Color = EditorColors.tapColorButNotOnThisLine;
                    }
                    else if (note.type == NoteType.Hold)
                    {
                        note.Color = EditorColors.holdColorButNotOnThisLine;
                    }
                    else
                    {
                        note.Color = EditorColors.catchColorButNotOnThisLine;
                    }
                });
            }
        }

        void ModeToNote()
        {
            foreach (var note in CurrLine.notes)
            {
                if (note.type == NoteType.Tap)
                {
                    note.Color = EditorColors.tapColor;
                }
                else if (note.type == NoteType.Hold)
                {
                    note.Color = EditorColors.holdColor;
                }
                else
                {
                    note.Color = EditorColors.catchColor;
                }
            }
            UpdateNote();
        }

        void ModeToEvent()
        {
            eventPanel.Visibility = Visibility.Visible;
            UpdateEvent();
        }

        void ModeToFakeCatch()
        {
            foreach (var line in window.track.lines)
            {
                line.fakeCatch.ForEach(e =>
                {
                    e.rectangle.Visibility = Visibility.Hidden;
                });
            }
            CurrLine.fakeCatch.ForEach(e =>
            {
                e.rectangle.Visibility = Visibility.Visible;
            });
            UpdateFakeCatch();
        }

        void ModeToFunction()
        {
            functionPanel.Visibility = Visibility.Visible;
            UpdateFunction();
        }

        //进入自由判定线
        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (selectingFreeLine) return;
            if (editingMode == 0)
            {
                foreach (var note in CurrLine.notes)
                {
                    if (note.type == NoteType.Tap)
                    {
                        note.Color = EditorColors.tapColorButNotOnThisLine;
                    }
                    else if (note.type == NoteType.Hold)
                    {
                        note.Color = EditorColors.holdColorButNotOnThisLine;
                    }
                    else
                    {
                        note.Color = EditorColors.catchColorButNotOnThisLine;
                    }
                }
                lineListView.SelectedIndex = -1;
                selectingFreeLine = true;
                foreach (var note in CurrLine.notes)
                {
                    if (note.type == NoteType.Tap)
                    {
                        note.Color = EditorColors.tapColor;
                    }
                    else if (note.type == NoteType.Hold)
                    {
                        note.Color = EditorColors.holdColor;
                    }
                    else
                    {
                        note.Color = EditorColors.catchColor;
                    }
                }
            }
            else if (editingMode == 1)
            {
                foreach (var el in CurrLine.EventLists)
                {
                    el.events.ForEach(e =>
                    {
                        e.rectangle.Visibility = Visibility.Hidden;
                    });
                }
                lineListView.SelectedIndex = -1;
                selectingFreeLine = true;
                foreach (var el in CurrLine.eventLists)
                {
                    el.events.ForEach(e =>
                    {
                        e.rectangle.Visibility = Visibility.Visible;
                    });
                }
                UpdateEventTypeList();
            }
            else
            {
                CurrLine.fakeCatch.ForEach(e =>
                {
                    e.rectangle.Visibility = Visibility.Hidden;
                });
                lineListView.SelectedIndex = -1;
                selectingFreeLine = true;
                CurrLine.fakeCatch.ForEach(e =>
                {
                    e.rectangle.Visibility = Visibility.Visible;
                });
            }
        }
    }
}
