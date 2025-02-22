using PMEditor.Util;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;

namespace PMEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            DebugWindow.Instance?.Close();
            DebugWindow debugWindow = new();
            debugWindow.Show();
#endif
            DebugWindow.Log("刚刚启动");
            //文件夹修改监视
            using var watcher = new FileSystemWatcher(@"./tracks/");
            watcher.IncludeSubdirectories = true;
            watcher.Filter = "*.txt";
            watcher.Changed += Flush;
            watcher.Renamed += Flush;
            watcher.Deleted += Flush;
            watcher.Created += Flush;
            watcher.EnableRaisingEvents = true;
            Flush(null, null);
            //设置
            //TODO
            Settings.currSetting = SettingManager.Read(SettingManager.SettingPath);
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            DebugWindow.Log("正在创建谱面");
            var createTrack = new CreateTrack();
            if (createTrack.ShowDialog() != true)
            {
                DebugWindow.Log("创建谱面窗口被取消");
                return;
            }
            var editorWindow = new EditorWindow(createTrack.TrackInfo, Track.GetTrack(new FileInfo("./tracks/" + createTrack.TrackInfo.TrackName + "/track.json")));
            editorWindow.Show();
            this.Close();
            DebugWindow.Log("导航窗口关闭");
        }

        private void OpenTrackButtonClick(object? sender, RoutedEventArgs? e)
        {
            if (trackList.SelectedItem is not TrackInfo curr) return;
            DebugWindow.Log("正在读取谱面");
            var track = Track.GetTrack(new FileInfo("./tracks/" + curr.TrackName + "/track.json"));
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            EditorWindow.Instance?.Close();
            var editorWindow = new EditorWindow(curr, track);
            editorWindow.Show();
            this.Close();
            DebugWindow.Log("导航窗口关闭");
        }

        /// <summary>
        /// 刷新文件列表
        /// </summary>
        private void Flush(object? sender, FileSystemEventArgs? e)
        {
            DebugWindow.Log("正在刷新谱面列表");
            var directoryInfo = new DirectoryInfo("./tracks/");
            foreach (var track in directoryInfo.GetDirectories())
            {
                foreach (var file in track.GetFiles())
                {
                    if (file.Name != "info.txt") continue;
                    var streamReader = new StreamReader(file.Open(FileMode.Open));
                    var trackName = streamReader.ReadLine();
                    var musicAuthor = streamReader.ReadLine();
                    var trackAuthor = streamReader.ReadLine();
                    if (trackName != null && trackAuthor != null && musicAuthor != null)
                    {
                        trackList.Items.Add(new TrackInfo(trackName, musicAuthor, trackAuthor));
                    }
                    streamReader.Close();
                }
            }
        }

        private void deleteTrack_Click(object sender, RoutedEventArgs e)
        {
            //删除谱面
            if (trackList.SelectedItem is TrackInfo curr)
            {
                DebugWindow.Log($"删除谱面 {curr.TrackName}");
                Directory.Delete("./tracks/" + curr.TrackName, true);
            }
            trackList.Items.Remove(trackList.SelectedItem);
            Flush(null, null);
        }

        // ReSharper disable once RedundantDefaultMemberInitializer
        private int i = 0;
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }
            i += 1;
            var timer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 0, 100),
            };
            timer.Tick += (_, _) =>
            {
                timer.IsEnabled = false;
                i = 0;
            };
            timer.IsEnabled = true;
            if (i == 2)
            {
                timer.IsEnabled = false;
                i = 0;
                OpenTrackButtonClick(null, null);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            //导出为mcfunction
            if (trackList.SelectedItem is not TrackInfo curr) return;
            DebugWindow.Log($"正在导出谱面 {curr.TrackName} 为数据包");
            var track = Track.GetTrack(new FileInfo("./tracks/" + curr.TrackName + "/track.json"));
            FolderBrowserDialog saveFileDialog = new();
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Task.Run(() => NBTTrack.FromTrack(track).ToFrameFunctions(new DirectoryInfo(saveFileDialog.SelectedPath))).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        System.Windows.MessageBox.Show("成功导出帧序列到 " + saveFileDialog.SelectedPath, "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    });
                });
            }

        }
    }
}
