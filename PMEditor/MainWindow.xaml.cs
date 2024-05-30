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
            //文件夹修改监视
            FileSystemWatcher watcher = new FileSystemWatcher(@"./tracks/")
            {
                IncludeSubdirectories = true,
                Filter = "*.txt"
            };
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
            CreateTrack createTrack = new CreateTrack();
            if (createTrack.ShowDialog() == true)
            {
                EditorWindow editorWindow = new EditorWindow(createTrack.TrackInfo, Track.GetTrack(new FileInfo("./tracks/" + createTrack.TrackInfo.TrackName + "/track.json")));
                editorWindow.Show();
                this.Close();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (trackList.SelectedItem is TrackInfo curr)
            {
                Track? track = Track.GetTrack(new FileInfo("./tracks/" + curr.TrackName + "/track.json"));
                if (track != null)
                {
                    EditorWindow.Instance?.Close();
                    EditorWindow editorWindow = new EditorWindow(curr, track);
                    editorWindow.Show();
                    this.Close();
                }
            }
        }

        /// <summary>
        /// 刷新文件列表
        /// </summary>
        private void Flush(object sender, FileSystemEventArgs e)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(@"./tracks/");
            foreach (DirectoryInfo track in directoryInfo.GetDirectories())
            {
                foreach (FileInfo file in track.GetFiles())
                {
                    if (file.Name == "info.txt")
                    {
                        StreamReader streamReader = new StreamReader(file.Open(FileMode.Open));
                        string? trackName = streamReader.ReadLine();
                        string? musicAuthor = streamReader.ReadLine();
                        string? trackAuthor = streamReader.ReadLine();
                        if (trackName != null && trackAuthor != null && musicAuthor != null)
                        {
                            trackList.Items.Add(new TrackInfo(trackName, musicAuthor, trackAuthor));
                        }
                        streamReader.Close();
                    }
                }
            }
        }

        private void deleteTrack_Click(object sender, RoutedEventArgs e)
        {
            //删除谱面
            if (trackList.SelectedItem is TrackInfo curr)
            {
                Directory.Delete("./tracks/" + curr.TrackName, true);
            }
            trackList.Items.Remove(trackList.SelectedItem);
            Flush(null, null);
        }

        int i = 0;
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }
            i += 1;
            DispatcherTimer timer = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 0, 0, 100),
            };
            timer.Tick += (sender, e) =>
            {
                timer.IsEnabled = false;
                i = 0;
            };
            timer.IsEnabled = true;
            if (i == 2)
            {
                timer.IsEnabled = false;
                i = 0;
                Button_Click_1(null, null);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            //导出为mcfunction
            if (trackList.SelectedItem is TrackInfo curr)
            {
                Track? track = Track.GetTrack(new FileInfo("./tracks/" + curr.TrackName + "/track.json"));
                if (track == null) return;
                FolderBrowserDialog saveFileDialog = new();
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Task.Run(() => NBTTrack.FromTrack(track).ToFrameFunctions(new(saveFileDialog.SelectedPath))).ContinueWith((t) =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            var re = System.Windows.MessageBox.Show("成功导出帧序列到 " + saveFileDialog.SelectedPath, "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                        });
                    });

                }
            }

        }
    }
}
