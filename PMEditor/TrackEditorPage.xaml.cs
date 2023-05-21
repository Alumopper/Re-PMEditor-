using PMEditor.Operation;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PMEditor
{
    /// <summary>
    /// TrackEditorPage.xaml 的交互逻辑
    /// </summary>
    public partial class TrackEditorPage : Page
    {
        public EditorWindow window;

        double secondsPreBeat;

        double secondsPreDevideBeat;

        //节拍分割
        int divideNum = 4;

        //节拍线显示间隔
        double pixelPreDividedBeat = 30;
        private double pixelPreBeat
        {
            get { return pixelPreDividedBeat * divideNum; }
        }

        readonly SolidColorBrush tapBrush = new (
            Color.FromArgb(102, 109, 209, 213)
            );

        readonly SolidColorBrush dragBrush = new (
            Color.FromArgb(102, 227, 214, 76)
            );

        Note? willPut;
        int lineIndex = 0;
        public int LineIndex
        {
            get => lineIndex;
        }

        bool noteChange = false;

        bool init = false;

        public TrackEditorPage(EditorWindow window)
        {
            InitializeComponent();
            this.window = window;
            playerControler.Maximum = window.track.Length * 100;
            timeDis.Content = "0.00" + " / " + window.track.Length.ToString("0.00");
            secondsPreBeat = 60.0 / window.track.bpm;
            secondsPreDevideBeat = secondsPreBeat / divideNum;
            window.timer.Tick += Timer_Tick;
            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(10);
                    if (window.isPlaying)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            Update();
                        });
                    }
                }
            });
            OperationManager.editorPage = this;
            //添加note渲染
            window.track.lines.ForEach(line =>
            {
                line.notes.ForEach(note =>
                {
                    note.rectangle.Visibility = Visibility.Hidden;
                    notePanel.Children.Add(note.rectangle);
                    note.rectangle.Height = 10;
                    note.rectangle.Width = notePanel.ActualWidth / 9;
                });
            });
            init = true;
            speedChooseBox.ItemsSource = Settings.currSetting.canSelectedSpeedList;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (init)
            {
                UpdateNote();
                init = false;
                window.operationInfo.Text = "就绪";
            }
            Draw();
        }

        private void notePanel_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //时间设置
            window.player.Pause();
            double currTime = window.player.Position.TotalSeconds;
            double num = e.Delta / 60.0;
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
            //进度条设置
            playerControler.Value = currTime * 100;
            //Lable设置
            timeDis.Content = currTime.ToString("0.00") + " / " + window.track.Length.ToString("0.00");
            noteChange = true;
            Update();
        }


        bool isPlayerChange = false;
        private void playerControler_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isPlayerChange)
            {
                window.player.Pause();
                window.isPlaying = false;
                window.player.Position = TimeSpan.FromSeconds(playerControler.Value / 100);
                timeDis.Content = window.player.Position.TotalSeconds.ToString("0.00") + " / " + window.track.Length.ToString("0.00");
            }
            isPlayerChange = false;
        }

        //绘图
        public void Draw()
        {
            //移除线
            for (int i = 0; i < notePanel.Children.Count; i++)
            {
                if (notePanel.Children[i] is System.Windows.Shapes.Line)
                {
                    notePanel.Children.RemoveAt(i);
                    i--;
                }
            }
            VisualBrush notePanelBrush = new VisualBrush();
            notePanelBrush.Visual = notePanel;
            //绘制节拍线
            //开始绘制
            //整拍
            //基准线相对坐标计算
            double fix = 0;
            if (window.player.Position.TotalSeconds / secondsPreBeat % 1 > 0.001)
            {
                fix = window.player.Position.TotalSeconds / secondsPreBeat % 1 * pixelPreBeat;
            }
            for (double i = notePanel.ActualHeight; i + fix > 0; i -= pixelPreDividedBeat)
            {
                double actualY = i + fix;
                System.Windows.Shapes.Line line = new System.Windows.Shapes.Line()
                {
                    X1 = 0,
                    X2 = notePanel.ActualWidth,
                    Y1 = actualY,
                    Y2 = actualY
                };
                if ((notePanel.ActualHeight - i) % pixelPreBeat < 0.001 || (notePanel.ActualHeight - i) % pixelPreBeat > 0.999 && (notePanel.ActualHeight - i) % pixelPreBeat < 1)
                {
                    //是整拍
                    line.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(237, 70, 255));
                    line.StrokeThickness = 3;
                }
                else
                {
                    //是分拍
                    line.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(254, 235, 255));
                    line.StrokeThickness = 1;
                }
                notePanel.Children.Add(line);
            }
        }

        private void Update()
        {
            if (window.isPlaying)
            {
                isPlayerChange = true;
                //更新进度条
                playerControler.Value = window.player.Position.TotalSeconds * 100;
                //更新时间
                timeDis.Content = window.player.Position.TotalSeconds.ToString("0.00") + " / " + window.track.Length.ToString("0.00");
            }
            //更新note
            if (window.isPlaying || noteChange)
            {
                UpdateNote();
                noteChange = false;
            }
        }

        public void UpdateNote()
        {
            //目前显示的时间范围
            double minTime = window.player.Position.TotalSeconds;
            double maxTime = minTime + notePanel.ActualHeight / pixelPreBeat * secondsPreBeat;
            foreach (var line in window.track.lines)
            {
                foreach (var note in line.notes)
                {
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

        private void notePanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (!window.isPlaying)
            {
                //获取鼠标位置，生成note位置预览
                var mousePos = GetAlignedPoint(e.GetPosition(notePanel));
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
                Canvas.SetLeft(notePreview, mousePos.X);
                Canvas.SetBottom(notePreview, mousePos.Y);
            }
            else
            {
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


        MouseButtonEventArgs startState;

        private void notePanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (window.isPlaying) return;
            startState = e;
        }

        private void notePanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (window.isPlaying) return;
            if (startState == null) return;
            //获取时间
            //获取鼠标位置，生成note位置预览
            var mousePos = GetAlignedPoint(startState.GetPosition(notePanel));
            var noteTime = GetTimeFromY(mousePos.Y);
            //放置note(
            willPut ??= new Note(
                    rail: (int)(mousePos.X * 9 / notePanel.ActualWidth),
                    noteType: (int)(window.puttingTap ? NoteType.Tap : NoteType.Drag),
                    fallType: 0,
                    isFake: false,
                    actualTime: noteTime,
                    generTime: 0);
            willPut.rectangle.Height = 10;
            willPut.rectangle.Width = notePreview.Width;
            notePanel.Children.Add(willPut.rectangle);
            Canvas.SetLeft(willPut.rectangle, willPut.rail * notePanel.ActualWidth / 9);
            Canvas.SetBottom(willPut.rectangle, mousePos.Y);
            if (!window.track.lines[lineIndex].notes.Contains(willPut))
            {
                window.track.lines[lineIndex].notes.Add(willPut);
            }
            noteChange = true;
            OperationManager.AddOperation(new PutNoteOperation(willPut, window.track.lines[lineIndex]));
            willPut = null;
        }

        //调整note大小
        private void notePanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            notePreview.Width = notePanel.ActualWidth / 9;
            window.track.lines.ForEach(e =>
            {
                e.notes.ForEach(e =>
                {
                    e.rectangle.Width = notePanel.ActualWidth / 9;
                });
            });
        }

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

        private void speedChooseBox_MouseEnter(object sender, MouseEventArgs e)
        {
            speedChooseBox.Focusable = true;
        }

        private void speedChooseBox_MouseLeave(object sender, MouseEventArgs e)
        {
            speedChooseBox.Focusable = false;
        }
    }
}
