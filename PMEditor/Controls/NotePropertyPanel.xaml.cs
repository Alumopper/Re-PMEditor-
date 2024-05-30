using PMEditor.Util;
using System.Management.Automation;
using System.Windows;
using System.Windows.Controls;

namespace PMEditor.Controls
{
    /// <summary>
    /// NotePropertyPanel.xaml 的交互逻辑
    /// </summary>
    public partial class NotePropertyPanel : UserControl
    {
        Note note;

        public NotePropertyPanel(Note note)
        {
            InitializeComponent();
            this.note = note;
            noteType.Text = note.type.ToString();
            startTime.Value = note.actualTime;
            if(note is FakeCatch fakeCatch)
            {
                fakeCatchHeight.Value = fakeCatch.Height;
            }
            else
            {
                fakeCatchHeightLable.Visibility = Visibility.Collapsed;
                fakeCatchHeight.Visibility = Visibility.Collapsed;
            }
            if (note.type != NoteType.Hold)
            {
                endTime.Visibility = Visibility.Collapsed;
                endTimeLable.Visibility = Visibility.Collapsed;
            }
            else
            {
                endTime.Value = note.actualTime + note.actualHoldTime;
            }
        }

        private void startTime_PropertyChangeEvent(object sender, RoutedEventArgs e)
        {
            var value = (double)((PropertyChangeEventArgs)e).PropertyValue;
            note.actualTime = value;
            (EditorWindow.Instance.page.Content as TrackEditorPage)?.UpdateNote();
        }

        private void endTime_PropertyChangeEvent(object sender, RoutedEventArgs e)
        {
            var value = (double)((PropertyChangeEventArgs)e).PropertyValue;
            note.actualHoldTime = value - note.actualTime;
            (EditorWindow.Instance.page.Content as TrackEditorPage)?.UpdateNote();
        }

        private void fakeCatchHeight_PropertyChangeEvent(object sender, RoutedEventArgs e)
        {
            var value = (double)((PropertyChangeEventArgs)e).PropertyValue;
            (note as FakeCatch)!.Height = value;
            (EditorWindow.Instance.page.Content as TrackEditorPage)?.UpdateNote();
        }

        private void noteType_PropertyChangeEvent(object sender, RoutedEventArgs e)
        {

        }
    }
}
