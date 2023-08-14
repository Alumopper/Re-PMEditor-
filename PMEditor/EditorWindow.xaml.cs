using PMEditor.Operation;
using PMEditor.Util;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace PMEditor
{
    /// <summary>
    /// EditorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class EditorWindow : Window
    {
        public TrackInfo info;
        public Track track;
        public List<Page> pages;
        public MediaPlayer player;
        public DispatcherTimer timer;

        public bool isPlaying = false;
        public bool puttingTap = true;

        public double playerTime = 0;

        int currPageIndex;

        public EditorWindow(TrackInfo info, Track track)
        {
            InitializeComponent();

            if (track.lines.Count == 0)
            {
                track.lines.Add(new Line());
            }
            this.info = info;
            this.track = track;
            this.Title = "Re:PMEditor - " + info.TrackName;
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1.0 / 120)
            };
            timer.Start();
            pages = new List<Page>() { new TrackEditorPage(this), new CodeViewer(this) };
            SetCurrPage(0);
            player = new MediaPlayer();
            player.Open(new Uri("./tracks/" + track.TrackName + "/" + info.trackName + ".wav", UriKind.Relative));
            player.Play();
            player.Pause();
            player.MediaEnded += (object? sender, EventArgs e) =>
            {
                isPlaying = false;
            };
            OperationManager.editorWindow = this;
        }

        private void editorButton_Click(object sender, RoutedEventArgs e)
        {
            SetCurrPage(0);
        }

        private void codeButton_Click(object sender, RoutedEventArgs e)
        {
            SetCurrPage(1);
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void settingButton_Click(object sender, RoutedEventArgs e)
        {

        }

        public void SetCurrPage(int index)
        {
            currPageIndex = index;
            //设置页面
            page.Content = pages[index];
            //刷新json
            (pages[1] as CodeViewer).jsonViewer.Text = track.ToJsonString();
            //设置按钮状态
            UIElementCollection uIElements = buttonPanel.Children;
            for (int i = 0; i < uIElements.Count; i++)
            {
                if (i == index * 2 + 1 || (i != index * 2 && i % 2 == 0))
                {
                    uIElements[i].Visibility = Visibility.Collapsed;
                }
                else
                {
                    uIElements[i].Visibility = Visibility.Visible;
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            player.Stop();
            isPlaying = false;
            if (!OperationManager.HasSaved)
            {
                var result = MessageBox.Show("是否保存对谱面的修改", "Re:PMEditor", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Yes)
                {
                    CommandBinding_Executed_4(sender, null);
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }
    }

}
