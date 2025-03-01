﻿using System.Globalization;
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
            MapAuthor.Text = EditorWindow.Instance.track.TrackAuthor;
            MusicAuthor.Text = EditorWindow.Instance.track.MusicAuthor;
            BPM.Text = EditorWindow.Instance.track.BaseBpm.ToString(CultureInfo.InvariantCulture);
            MapLevel.Text = EditorWindow.Instance.track.Difficulty;
            MapLength.Text = Settings.currSetting.MapLength.ToString(CultureInfo.InvariantCulture);
            
            TickValue.Text = Settings.currSetting.Tick.ToString(CultureInfo.InvariantCulture);

            WarnMultiEventType.IsChecked = Settings.currSetting.WarnEventTypeChange;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            EditorWindow.Instance.track.TrackName = MapName.Text;
            EditorWindow.Instance.track.TrackAuthor = MapAuthor.Text;
            EditorWindow.Instance.track.MusicAuthor = MusicAuthor.Text;
            EditorWindow.Instance.track.BaseBpm = double.Parse(BPM.Text);
            EditorWindow.Instance.track.UpdateLineTimes();
            EditorWindow.Instance.track.Difficulty = MapLevel.Text;
            Settings.currSetting.MapLength = double.Parse(MapLength.Text);
            Settings.currSetting.Tick = double.Parse(TickValue.Text);
            Settings.currSetting.WarnEventTypeChange = WarnMultiEventType.IsChecked == true;
            SettingManager.Write(Settings.currSetting);
        }
    }
}
