using System;
using System.Threading.Tasks;
using System.Windows;
using PMEditor.Util;

namespace PMEditor;

public partial class ExportWindows : Window
{
    Track track => EditorWindow.Instance.track;
    
    public ExportWindows()
    {
        InitializeComponent();
        Path.Text = track.ExportPath;
    }

    private void SelectExportPath(object sender, RoutedEventArgs e)
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog();
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            track.ExportPath = dialog.SelectedPath + "\\" + track.TrackName;
            Path.Text = track.ExportPath;
        }
    }

    private void Export(object sender, RoutedEventArgs e)
    {
        if (CheckBox20.IsChecked == true)
        {
            Settings.currSetting.Tick = 20;
            EditorWindow.Instance.Export().ContinueWith(_ =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (CheckBox60.IsChecked == true)
                    {
                        Settings.currSetting.Tick = 60;
                        EditorWindow.Instance.Export();
                    }
                });
            });
        }
        Close();
    }
}