using PMEditor.Util;
using System;
using System.Collections.Generic;
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
            SettingManager.Write(Settings.currSetting);
        }
    }
}
