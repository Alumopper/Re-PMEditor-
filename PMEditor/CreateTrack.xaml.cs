using Microsoft.Win32;
using PMEditor.Util;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace PMEditor
{
    /// <summary>
    /// CreateTrack.xaml 的交互逻辑
    /// </summary>
    public partial class CreateTrack : Window
    {
        TrackInfo trackInfo;
        public TrackInfo TrackInfo
        {
            get
            {
                return trackInfo;
            }
        }

        public CreateTrack()
        {
            InitializeComponent();
        }

        FileInfo chosenFile;

        private void Window_ContentRendered(object sender, EventArgs e)
        {

        }

        private void BPMButton_Click(object sender, RoutedEventArgs e)
        {
            //调用python
            Process p = new();
            var args = @"python ./lib/getbpm.py " + "\"" + chosenFile.FullName + "\"" + "&exit";

            p.StartInfo.FileName = @"cmd.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            p.StandardInput.WriteLine(args);
            p.StandardInput.AutoFlush = true;
            string? output = null;
            while (!p.StandardOutput.EndOfStream)
            {
                output = p.StandardOutput.ReadLine();
            }
            Bpm.Text = output;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //打开文件
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Wav Files(*.wav)|*.wav"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                chosenFile = new FileInfo(openFileDialog.FileName);
                var file = chosenFile.Name;
                FileName.Text = file;
            }
        }

        //创建谱面
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if(FileName.Text == "" || MusicAuthor.Text == "" || TrackAuthor.Text == "" || Bpm.Text == "" || Difficulty.Text == "")
            {
                MessageBox.Show("请填写完整信息", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            //创建文件夹和文件
            string trackName = FileName.Text[..FileName.Text.LastIndexOf(".", StringComparison.Ordinal)];
            string ma = MusicAuthor.Text;
            string ta = TrackAuthor.Text;
            MediaPlayer md = new();
            double time = 0;
            md.Open(new Uri(chosenFile.FullName));
            md.MediaOpened += (ms, mc) =>
            {
                time = md.NaturalDuration.TimeSpan.TotalSeconds;
                md.Close();
                TrackInfo trackInfo = new(trackName, ma, ta);
                this.trackInfo = trackInfo;
                Directory.CreateDirectory("./tracks/" + trackName);
                File.WriteAllText("./tracks/" + trackName + "/info.txt", trackInfo.ToString()); //info
                File.Copy(chosenFile.FullName, "./tracks/" + trackName + "/" + chosenFile.Name, true);  //音频
                File.WriteAllText("./tracks/" + trackName + "/track.json", new Track(trackName, ma, ta, double.Parse(Bpm.Text), time, Difficulty.Text).ToJsonString());
                this.DialogResult = true;
                this.Close();
            };
        }
    }
}
