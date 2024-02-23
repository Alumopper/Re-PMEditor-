using PMEditor.Util;
using SharpNBT;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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

namespace PMEditor.Pages
{
    /// <summary>
    /// TrackPreview2D.xaml 的交互逻辑
    /// </summary>
    public partial class TrackPreview2D : Page
    {
        Track track;
        NBTTrack nbtTrack;
        EditorWindow window { get => EditorWindow.Instance; }

        double width { get => previewPanel.ActualWidth; }
        double height { get => previewPanel.ActualHeight; }
        double noteWidth
        {
            get => previewPanel.ActualWidth / 9 * 0.75;
        }

        public TrackPreview2D(Track track)
        {
            InitializeComponent();
            this.track = track;
            this.nbtTrack = NBTTrack.FromTrack(track);
            CompositionTarget.Rendering += DrawTrackPreview;
        }

        private void DrawTrackPreview(object? sender, EventArgs e)
        {
            //清空画布
            var controlsToRemove = 
                previewPanel.Children.OfType<UIElement>()
                .Where(c => c is FrameworkElement element && element.Tag?.ToString() == "note")
                .ToList();
            if (window.isPlaying)
            {
                isPlayerChange = true;
                window.playerTime = window.player.Position.TotalSeconds;
                //更新进度条
                progressSlider.Value = window.playerTime / track.length * 100;
                //更新时间
                //timeDis.Content = window.playerTime.ToString("0.00") + " / " + window.track.Length.ToString("0.00");
            }
            foreach (var control in controlsToRemove)
            {
                previewPanel.Children.Remove(control);
            }
            double time = window.playerTime;
            //绘制note
            foreach(var line in nbtTrack.lines)
            {
                for (int index = 0; index < line.notes.Count; index++)
                {
                    NBTNote note = line.notes[index];
                    //渲染note
                    if (note is NBTHold hold)
                    {
                        if (hold.summonTime <= time && time <= hold.judgeTime + hold.holdTime)
                        {
                            Rectangle holdrec = new()
                            {
                                Width = width / 9,
                                Height = hold.holdLength,
                                Fill = new SolidColorBrush(EditorColors.holdColor),
                            };
                            previewPanel.Children.Add(holdrec);
                            if (time > hold.judgeTime)
                            {
                                //获取终点位置
                                for (double i = hold.judgeTime + hold.holdTime; i > time; i -= Settings.currSetting.Tick)
                                {
                                    holdrec.Height += 1 / Settings.currSetting.Tick * line.line.GetSpeed(i);
                                }
                                holdrec.Height = (1 - (time - hold.judgeTime) / hold.holdTime) * hold.holdLength;
                                Canvas.SetBottom(holdrec, 50);
                            }
                            else
                            {
                                Canvas.SetBottom(holdrec, 50 + hold.holdLength);
                            }
                            holdrec.Tag = "note";
                            Canvas.SetLeft(holdrec, width / 9 * hold.rail);
                        }
                    }
                    else
                    {
                        //tap或者drag
                        if (note.summonTime <= time && time <= note.judgeTime)
                        {
                            Rectangle noteRec = new()
                            {
                                Width = noteWidth,
                                Height = 10,
                                Fill = note.type == (int)NoteType.Tap ? new SolidColorBrush(EditorColors.tapColor) : new SolidColorBrush(EditorColors.dragColor)
                            };
                            //计算note的位置

                            previewPanel.Children.Add(noteRec);
                            Canvas.SetBottom(noteRec, 50 + note.summonPos);
                        }
                    }
                }
            }
        }

        bool isPlayerChange = false;    //是否是因为播放器播放导致的进度条移动
        private void progressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isPlayerChange)
            {
                window.player.Pause();
                window.isPlaying = false;
                window.playerTime = TimeSpan.FromSeconds(progressSlider.Value / 100).TotalSeconds;
                window.player.Position = TimeSpan.FromSeconds(progressSlider.Value / 100);
                //timeDis.Content = window.playerTime.ToString("0.00") + " / " + window.track.Length.ToString("0.00");
            }
            isPlayerChange = false;
        }

        public double GetY(double y)
        {
            return y / Settings.currSetting.MapLength * (height - 50) + 50;
        }

    }
}
