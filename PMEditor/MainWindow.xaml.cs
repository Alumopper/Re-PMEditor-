using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            CreateTrack createTrack = new CreateTrack();
            createTrack.ShowDialog();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (trackList.SelectedItem is TrackInfo curr)
            {
                Track? track = Track.GetTrack(new FileInfo("./tracks/" + curr.TrackName + "/track.json"));
                if(track != null)
                {
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
            DirectoryInfo directoryInfo = new DirectoryInfo (@"./tracks/");
            foreach (DirectoryInfo track in directoryInfo.GetDirectories())
            {
                foreach(FileInfo file in track.GetFiles())
                {
                    if(file.Name == "info.txt")
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
    }
}
