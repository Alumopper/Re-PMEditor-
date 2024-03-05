using PMEditor.Operation;
using PMEditor.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

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

        static readonly SolidColorBrush dragBrush = new(
            Color.FromArgb(102, 227, 214, 76)
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

        bool editingNote = true;

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
        int lineIndex = 0;
        public int LineIndex
        {
            get => lineIndex;
        }

        Line CurrLine
        {
            get => window.track.lines[lineIndex];
        }

        bool noteChange = false;

        bool eventChange = true;

        bool init = false;

        bool isSelecting = false;

        bool isDraging = false;

        Point dragStartPoint = new(-1, -1);

        //可能会加入的，事件可以翻页，防止事件太多放不下
        int page = 0;

        private List<Note> selecedNotes = new();
        private List<Event> selectedEvents = new();

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
                    note.parentLine = line;
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
                    line.eventLists[i].parentLine = line;
                    line.eventLists[i].events.ForEach(e =>
                    {
                        e.parentList = line.eventLists[i];
                        line.SetType(e.type, i);
                        e.rectangle.Visibility = Visibility.Hidden;
                        eventPanel.Children.Add(e.rectangle);
                        e.rectangle.Height = GetYFromTime(e.endTime) - GetYFromTime(e.startTime);
                        e.rectangle.Width = eventPanel.ActualWidth / 9; 
                    });
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
                UpdateFunctionCurve();
                UpdateEventTypeList();
                init = false;
                window.operationInfo.Text = "就绪";
            }
            DrawLineAndBeat();
            DrawPreview();
            UpdateFunctionCurve();
        }

        //滚轮滚动
        private void panel_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double num = e.Delta / 60.0;
            double currTime = window.playerTime;
            //更改谱面缩放比例
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
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
            noteChange = true;
            eventChange = true;
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
                            Fill = note.type == NoteType.Tap ? new SolidColorBrush(EditorColors.tapColor) : new SolidColorBrush(EditorColors.dragColor)
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
                foreach(var note in line.notes)
                {
                    if(note is NBTHold hold)
                    {
                        if (hold.summonTime <= time && time <= hold.judgeTime + hold.holdTime)
                        {
                            Rectangle noteRec = new()
                            {
                                Width = noteWidth,
                                Height = hold.holdLength / 10 * trackPreviewWithEvent.ActualHeight,
                                Fill = new SolidColorBrush(EditorColors.holdColor)
                            };
                            //计算note的位置
                            double i = time;
                            double locate = 0;
                            for (; i < hold.judgeTime - 1 / Settings.currSetting.Tick; i += 1 / Settings.currSetting.Tick)
                            {
                                locate += line.line.GetSpeed(i) / Settings.currSetting.Tick;
                            }
                            //计算差值保证帧数
                            double delta = hold.judgeTime - i;
                            locate += line.line.GetSpeed(i) * delta;
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
                            Rectangle noteRec = new()
                            {
                                Width = noteWidth,
                                Height = 10,
                                Fill = note.type == (int)NoteType.Tap ? new SolidColorBrush(EditorColors.tapColor) : new SolidColorBrush(EditorColors.dragColor)
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
                                    note.sound.Position = new TimeSpan(0);
                                }
                            }
                            else
                            {
                                //如果未被判定过且谱面正在被播放
                                if ((!note.hasJudged) && window.isPlaying)
                                {
                                    note.sound.Play();
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
                            note.sound.Position = new TimeSpan(0);
                        }
                    }
                    //在判定线下方
                    else
                    {
                        //如果未被判定过且谱面正在被播放
                        if ((!note.hasJudged) && window.isPlaying)
                        {
                            note.sound.Play();
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
                if (window.isPlaying || eventChange)
                {
                    UpdateEvent();
                    eventChange = false;
                }
            }
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
                                    note.sound.Position = new TimeSpan(0);
                                }
                            }
                            else
                            {
                                //如果未被判定过且谱面正在被播放
                                if ((!note.hasJudged) && window.isPlaying)
                                {
                                    note.sound.Play();
                                    note.hasJudged = true;
                                }
                            }
                            //在可视区域内
                            if (note.actualTime < maxTime)
                            {
                                note.rectangle.Visibility = Visibility.Visible;
                                double qwq = Math.Max(minTime, note.actualTime);
                                double pwp = Math.Min(note.actualTime + note.actualHoldTime, maxTime);
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
                            note.sound.Position = new TimeSpan(0);
                        }
                        //在可视区域内
                        if (note.actualTime <= maxTime)
                        {
                            note.rectangle.Visibility = Visibility.Visible;
                            double qwq = note.actualTime - minTime;
                            Canvas.SetBottom(note.rectangle, qwq / secondsPreBeat * pixelPreBeat);
                            Canvas.SetLeft(note.rectangle, note.rail * notePanel.ActualWidth / 9);
                        }
                    }
                    //在判定线下方
                    else
                    {
                        //如果未被判定过且谱面正在被播放
                        if ((!note.hasJudged) && window.isPlaying)
                        {
                            note.sound.Play();
                            note.hasJudged = true;
                        }
                        note.rectangle.Visibility = Visibility.Hidden;
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
                                double bottom = Math.Max(minTime, e.startTime);
                                double top = Math.Min(e.endTime, maxTime);
                                e.rectangle.Height = GetYFromTime(top) - GetYFromTime(bottom);
                                Canvas.SetBottom(e.rectangle, GetYFromTime(bottom));
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
        }

        public void UpdateEventTypeList()
        {
            for (int i = 0; i < 9; i++)
            {
                var textBlock = columnInfo.Children[i] as TextBlock;
                string text;
                if (editingNote)
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
                            notePreview.Fill = dragBrush;
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
                    if (window.puttingTap)
                    {
                        notePreview.Fill = tapBrush;
                    }
                    else
                    {
                        notePreview.Fill = dragBrush;
                    }

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
                        if (selecedNotes[0].rectangle.IsResizing)
                        {
                            var n = selecedNotes[0];
                            double y = Math.Abs(endPos.Y - GetYFromTime(n.actualTime));
                            n.rectangle.Height = y;
                        }
                        else
                        {
                            //拖动note
                            foreach (var note in selecedNotes)
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
            if (window.puttingTap)
            {
                notePreview.Fill = tapBrush;
            }
            else
            {
                notePreview.Fill = dragBrush;
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
                if (CurrLine.ClickOnNote(GetTimeFromY(startPos.Y), (int)Math.Round(startPos.X * 9 / notePanel.ActualWidth)))
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
                if (selecedNotes[0].rectangle.IsResizing)
                {
                    selecedNotes[0].actualHoldTime = GetTimeFromY(endPos.Y) - selecedNotes[0].actualTime;
                }
                else
                {
                    foreach (var note in selecedNotes)
                    {
                        double x = Canvas.GetLeft(note.rectangle);
                        double y = Canvas.GetBottom(note.rectangle);
                        note.rail = (int)Math.Round(x * 9 / notePanel.ActualWidth);
                        if (note.rail < 0)
                        {
                            note.rail = 0;
                        }
                        note.actualTime = GetTimeFromY(y);
                    }
                }
                UpdateSelectedNote(selecedNotes);
                isDraging = false;
                return;
            }
            if (window.isPlaying) return;
            if (startPos == new Point(-1, -1)) return;
            if (selecedNotes.Count != 0)
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
            //放置note
            if (endTime - startTime >= secondsPreDevideBeat)
            {
                willPutNote ??= new Note(
                    rail: rail,
                    noteType: (int)NoteType.Hold,
                    fallType: 0,
                    isFake: false,
                    actualTime: startTime,
                    actualHoldTime: endTime - startTime,
                    isCurrentLineNote: true);
                willPutNote.rectangle.Height = endPos.Y - startPos.Y;
                willPutNote.rectangle.Width = notePreview.Width;
            }
            else
            {
                willPutNote ??= new Note(
                    rail: rail,
                    noteType: (int)(window.puttingTap ? NoteType.Tap : NoteType.Drag),
                    fallType: 0,
                    isFake: false,
                    actualTime: startTime,
                    isCurrentLineNote: true);
                willPutNote.rectangle.Height = 10;
                willPutNote.rectangle.Width = notePreview.Width;
            }
            willPutNote.parentLine = window.track.lines[lineIndex];
            if (!window.track.lines[lineIndex].IsNoteOverLap(willPutNote))
            {
                window.track.lines[lineIndex].notes.Add(willPutNote);
                notePanel.Children.Add(willPutNote.rectangle);
                Canvas.SetLeft(willPutNote.rectangle, willPutNote.rail * notePanel.ActualWidth / 9);
                Canvas.SetBottom(willPutNote.rectangle, startPos.Y);
                noteChange = true;
                OperationManager.AddOperation(new PutNoteOperation(willPutNote, window.track.lines[lineIndex]));
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
                speed = float.Parse(e.AddedItems[0] as string);
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

        bool isDeleting = false;
        //切换判定线
        private void lineListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isDeleting)
            {
                isDeleting = false;
                return;
            }
            if (editingNote)
            {
                foreach (var note in window.track.lines[lineIndex].notes)
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
                        note.Color = EditorColors.dragColorButNotOnThisLine;
                    }
                }
                lineIndex = lineListView.SelectedIndex;
                foreach (var note in window.track.lines[lineIndex].notes)
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
                        note.Color = EditorColors.dragColor;
                    }
                }
            }
            else
            {
                foreach (var el in window.track.lines[lineIndex].EventLists)
                {
                    el.events.ForEach(e =>
                    {
                        e.rectangle.Visibility = Visibility.Hidden;
                    });
                }
                lineIndex = lineListView.SelectedIndex;
                foreach (var el in window.track.lines[lineIndex].eventLists)
                {
                    el.events.ForEach(e =>
                    {
                        e.rectangle.Visibility = Visibility.Visible;
                    });
                }
                UpdateEventTypeList();
            }
        }

        //删除
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

            //isDeleting = true;
            //int i = lineIndex - 1;
            //if (i < 0)
            //{
            //    i = 0;
            //}
            //if(window.track.lines.Count == 1)
            //{
            //    window.track.lines.Add(new(0));
            //}
            //lineListView.SelectedIndex = i;
            //OperationManager.AddOperation(new RemoveLineOperation(CurrLine));
            window.track.lines.RemoveAt(lineIndex);
        }

        //重命名
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            Grid box = ((sender as MenuItem).Parent as ContextMenu).PlacementTarget as Grid;
            box.Children[0].Visibility = Visibility.Collapsed;
            (box.Children[1] as TextBox).Text = window.track.lines[lineIndex].id;
            (box.Children[1] as TextBox).SelectAll();
            box.Children[1].Visibility = Visibility.Visible;
            box.Children[1].Focus();

        }

        //属性
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            infoFrame.Content = new Pages.LineInfoFrame(window.track.lines[lineIndex], lineIndex);
        }

        //新建判定线
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Line qwq = new(0);
            window.track.lines.Add(qwq);
            lineIndex = window.track.lines.Count - 1;
            lineListView.SelectedIndex = lineIndex;
            /*
            ListViewItem item = lineListView.ItemContainerGenerator.ContainerFromItem(qwq) as ListViewItem; 
            Grid box = item.ContentTemplate.FindName("linebox", item) as Grid;
            box.Children[0].Visibility = Visibility.Collapsed;
            (box.Children[1] as TextBox).Text = window.track.lines[lineIndex].id;
            (box.Children[1] as TextBox).SelectAll();
            box.Children[1].Visibility = Visibility.Visible;
            box.Children[1].Focus();
            */
        }

        //判定线重命名
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Grid box = (sender as TextBox).Parent as Grid;
            ChangeLineId((sender as TextBox).Text);
            (box.Children[0] as Label).Content = window.track.lines[lineIndex].Id;
            box.Children[0].Visibility = Visibility.Visible;
            box.Children[1].Visibility = Visibility.Collapsed;
        }

        //判定线重命名
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Grid box = (sender as TextBox).Parent as Grid;
                ChangeLineId((sender as TextBox).Text);
                (box.Children[0] as Label).Content = window.track.lines[lineIndex].Id;
                box.Children[0].Visibility = Visibility.Visible;
                box.Children[1].Visibility = Visibility.Collapsed;
            }
        }

        //判定线重命名
        private void ChangeLineId(string lineId)
        {
            window.track.lines[lineIndex].Id = lineId;
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
            var endPos = GetAlignedPoint(e.GetPosition(notePanel));
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

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        public void UpdateSelectedNote(List<Note> note)
        {
            if (note.Count == 1) UpdateSelectedNote(note[0]);
            selecedNotes.ForEach(note => { note.rectangle.HighLight = false; });
            selecedNotes = note;
            selecedNotes.ForEach(note => { note.rectangle.HighLight = true; });
            isSelecting = selecedNotes.Count != 0;
            isDraging = isSelecting;
            dragStartPoint = GetAlignedPoint(Mouse.GetPosition(notePanel));
        }

        public void UpdateSelectedNote(Note note, bool multiSelect = false)
        {
            if (!multiSelect)
            {
                selecedNotes.ForEach(note => { note.rectangle.HighLight = false; });
                selecedNotes.Clear();
            }
            selecedNotes.Add(note);
            note.rectangle.HighLight = true;
            isSelecting = selecedNotes.Count != 0;
            isDraging = isSelecting;
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
                if (!editingNote)
                {
                    foreach (var note in window.track.lines[lineIndex].notes)
                    {
                        if (note.type == NoteType.Tap || note.type == NoteType.Hold)
                        {
                            note.Color = EditorColors.tapColorButNotOnThisLine;
                        }
                        else
                        {
                            note.Color = EditorColors.dragColorButNotOnThisLine;
                        }
                    }
                    eventPanel.Visibility = Visibility.Visible;
                    trackPreviewWithEvent.Visibility = Visibility.Hidden;
                }
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

        private void NoteOrEvent_ContentChanged(object sender, EventArgs e)
        {
            if (previewing) return; 
            if (editingNote)
            {
                editingNote = false;
                eventPanel.Visibility = Visibility.Visible;
                foreach (var note in window.track.lines[lineIndex].notes)
                {
                    if (note.type == NoteType.Tap || note.type == NoteType.Hold)
                    {
                        note.Color = EditorColors.tapColorButNotOnThisLine;
                    }
                    else
                    {
                        note.Color = EditorColors.dragColorButNotOnThisLine;
                    }
                }
            }
            else
            {
                editingNote = true;
                eventPanel.Visibility = Visibility.Hidden;
                foreach (var note in window.track.lines[lineIndex].notes)
                {
                    if (note.type == NoteType.Tap || note.type == NoteType.Hold)
                    {
                        note.Color = EditorColors.tapColor;
                    }
                    else
                    {
                        note.Color = EditorColors.dragColor;
                    }
                }
            }
            UpdateEventTypeList();
        }

    }
}
