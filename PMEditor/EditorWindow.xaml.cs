using PMEditor.Operation;
using PMEditor.Pages;
using PMEditor.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;

namespace PMEditor
{
    /// <summary>
    /// EditorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class EditorWindow : Window
    {
        public static EditorWindow Instance { get; private set; }

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

            Instance = this;
            if (track.lines.Count == 0)
            {
                track.lines.Add(new Line());
            }
            this.info = info;
            this.track = track;
            this.Title = "Re:PMEditor - " + info.TrackName;
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1.0 / 60)
            };
            timer.Start();
            player = new MediaPlayer();
            player.Open(new Uri("./tracks/" + track.TrackName + "/" + info.trackName + ".wav", UriKind.Relative));
            player.Play();
            player.Pause();
            player.MediaEnded += (object? sender, EventArgs e) =>
            {
                isPlaying = false;
            };
            pages = new List<Page>() { new TrackEditorPage(), new CodeViewer(), new TODOPage(), new SettingPage()};
            SetCurrPage(0);
            UpdateStatusBar();
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
            SetCurrPage(2);
        }

        private void settingButton_Click(object sender, RoutedEventArgs e)
        {
            SetCurrPage(3);
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
                var result = System.Windows.MessageBox.Show("是否保存对谱面的修改", "Re:PMEditor", MessageBoxButton.YesNoCancel);
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

        //导出为snbt
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new()
            {
                Filter = "SNBT Files(*.txt)|*.txt"
            };
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog.FileName, NBTTrack.FromTrack(track).ToNBTTag().Stringify());
            }
            operationInfo.Text = "成功导出谱面SNBT到 " + saveFileDialog.FileName;
        }

        //导出为mcfunction
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog saveFileDialog = new();
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                NBTTrack.FromTrack(track).ToFrameFunctions(new(saveFileDialog.SelectedPath));
            }
            operationInfo.Text = "成功导出帧序列到 " + saveFileDialog.SelectedPath;
        }

        private void infoButton_Click(object sender, RoutedEventArgs e)
        {

        }

        public void UpdateStatusBar()
        {
            allNotesCount.Text = "谱面物量: " + track.notesNumber;
        }
    }

}
