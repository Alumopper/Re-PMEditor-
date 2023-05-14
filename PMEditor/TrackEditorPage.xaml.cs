using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static IronPython.Modules._ast;

namespace PMEditor
{
    /// <summary>
    /// TrackEditorPage.xaml 的交互逻辑
    /// </summary>
    public partial class TrackEditorPage : Page
    {
        EditorWindow window;
        public EditorWindow Window
        {
            get { return window; }
            set { window = value; }
        }

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
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
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
        private void Draw()
        {
            //移除除了第一个元素以外的元素（保留Grid）
            notePanel.Children.RemoveRange(1, notePanel.Children.Count - 1);
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
                if((notePanel.ActualHeight - i) % pixelPreBeat < 0.001 || (notePanel.ActualHeight - i) % pixelPreBeat > 0.999 && (notePanel.ActualHeight - i) % pixelPreBeat < 1)
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
        }
    }
}
