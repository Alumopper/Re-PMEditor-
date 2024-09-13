using PMEditor.Util;
using System.Windows;
using System.Windows.Controls;

namespace PMEditor.Pages
{
    /// <summary>
    /// SettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingPage : Page
    {
        public SettingPage()
        {
            InitializeComponent();
            //获取设置
            MapName.Text = EditorWindow.Instance.track.TrackName;
            MapAuthor.Text = EditorWindow.Instance.track.trackAuthor;
            MusicAuthor.Text = EditorWindow.Instance.track.musicAuthor;
            BPM.Text = EditorWindow.Instance.track.bpm.ToString();
            MapLevel.Text = EditorWindow.Instance.track.difficulty;
            MapLength.Text = Settings.currSetting.MapLength.ToString();
            
            TickValue.Text = Settings.currSetting.Tick.ToString();

            WarnMultiEventType.IsChecked = Settings.currSetting.WarnEventTypeChange;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            EditorWindow.Instance.track.TrackName = MapName.Text;
            EditorWindow.Instance.track.trackAuthor = MapAuthor.Text;
            EditorWindow.Instance.track.musicAuthor = MusicAuthor.Text;
            EditorWindow.Instance.track.bpm = double.Parse(BPM.Text);
            EditorWindow.Instance.track.difficulty = MapLevel.Text;
            Settings.currSetting.MapLength = double.Parse(MapLength.Text);
            Settings.currSetting.Tick = double.Parse(TickValue.Text);
            Settings.currSetting.WarnEventTypeChange = WarnMultiEventType.IsChecked == true;
            SettingManager.Write(Settings.currSetting);
        }
    }
}
