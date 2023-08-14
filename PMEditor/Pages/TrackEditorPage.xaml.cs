using PMEditor.Operation;
using PMEditor.Util;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            foreach(var line in window.track.lines)
            {
                line.notes.ForEach(note =>
                {
                    note.rectangle.Visibility = Visibility.Hidden;
                    notePanel.Children.Add(note.rectangle);
                    note.rectangle.Height = 10;
                    note.rectangle.Width = notePanel.ActualWidth / 9;
                });
            }
            init = true;
            speedChooseBox.ItemsSource = Settings.currSetting.canSelectedSpeedList;
            lineListView.ItemsSource = window.track.lines;
            lineListView.SelectedIndex = 0;
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
            double currTime = window.playerTime;
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
            window.playerTime = currTime;
            Update();
        }


        bool isPlayerChange = false;
        private void playerControler_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isPlayerChange)
            {
                window.player.Pause();
                window.isPlaying = false;
                window.playerTime = TimeSpan.FromSeconds(playerControler.Value / 100).TotalSeconds;
                window.player.Position = TimeSpan.FromSeconds(playerControler.Value / 100);
                
                timeDis.Content = window.playerTime.ToString("0.00") + " / " + window.track.Length.ToString("0.00");
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
            if (window.playerTime / secondsPreBeat % 1 > 0.001)
            {
                fix = window.playerTime / secondsPreBeat % 1 * pixelPreBeat;
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
                window.playerTime = window.player.Position.TotalSeconds;
                //更新进度条
                playerControler.Value = window.playerTime * 100;
                //更新时间
                timeDis.Content = window.playerTime.ToString("0.00") + " / " + window.track.Length.ToString("0.00");
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
            double minTime = window.playerTime;
            double maxTime = minTime + notePanel.ActualHeight / pixelPreBeat * secondsPreBeat;
            for(int i = 0; i < window.track.lines.Count; i++)
            {
                foreach (var note in window.track.lines[i].notes)
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

                //计算拍数
                double mouseTime = mousePos.Y / pixelPreBeat * secondsPreBeat + window.playerTime;
                int beat = (int)(mouseTime / secondsPreBeat);
                int divBeat = (int)((mouseTime % secondsPreBeat)/secondsPreDevideBeat);
                beatDis.Content = $"beat {beat}:{divBeat}/{divideNum}";
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
            //放置note
            willPut ??= new Note(
                    rail: (int)(mousePos.X * 9 / notePanel.ActualWidth),
                    noteType: (int)(window.puttingTap ? NoteType.Tap : NoteType.Drag),
                    fallType: 0,
                    isFake: false,
                    actualTime: noteTime,
                    generTime: 0,
                    isCurrentLineNote: true);
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
            foreach(var line in window.track.lines)
            {
                line.notes.ForEach(e =>
                {
                    e.rectangle.Width = notePanel.ActualWidth / 9;
                });
            }
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

        private void lineListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var note in window.track.lines[lineIndex].notes)
            {
                if (note.type == NoteType.Tap)
                {
                    note.Color = Note.tapColorButNotOnThisLine;
                }
                else
                {
                    note.Color = Note.dragColorButNotOnThisLine;
                }
            }
            lineIndex = lineListView.SelectedIndex;
            foreach (var note in window.track.lines[lineIndex].notes)
            {
                if (note.type == NoteType.Tap)
                {
                    note.Color = Note.tapColor;
                }
                else
                {
                    note.Color = Note.dragColor;
                }
            }

        }

        //删除
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            window.track.lines.RemoveAt(lineIndex);
        }

        /// <summary>
        /// 重命名
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            infoFrame.Content = new Pages.LineInfoFrame(window.track.lines[lineIndex],lineIndex);
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

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Grid box = (sender as TextBox).Parent as Grid;
            ChangeLineId((sender as TextBox).Text);
            (box.Children[0] as Label).Content = window.track.lines[lineIndex].Id;
            box.Children[0].Visibility = Visibility.Visible;
            box.Children[1].Visibility = Visibility.Collapsed;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                Grid box = (sender as TextBox).Parent as Grid;
                ChangeLineId((sender as TextBox).Text);
                (box.Children[0] as Label).Content = window.track.lines[lineIndex].Id;
                box.Children[0].Visibility = Visibility.Visible;
                box.Children[1].Visibility = Visibility.Collapsed;
            }
        }

        private void ChangeLineId(string lineId)
        {
            window.track.lines[lineIndex].Id = lineId;
        }
    }
}
