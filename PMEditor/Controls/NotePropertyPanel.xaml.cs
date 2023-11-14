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
            startTime.Text = note.actualTime.ToString();
            if (note.type != NoteType.Hold)
            {
                endTime.Visibility = Visibility.Collapsed;
                endTimeLable.Visibility = Visibility.Collapsed;
            }
            else
            {
                endTime.Text = (note.actualTime + note.actualHoldTime).ToString();
            }
        }

        private void eventType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void startTime_LostFocus(object sender, RoutedEventArgs e)
        {
            if (startTime.IsReadOnly) return;
            var qwq = double.TryParse(startTime.Text, out double value);
            if (qwq)
            {
                note.actualTime = value;
            }
            else
            {
                startTime.Text = note.actualTime.ToString();
            }
        }

        private void endTime_LostFocus(object sender, RoutedEventArgs e)
        {
            if (endTime.IsReadOnly) return;
            var qwq = double.TryParse(endTime.Text, out double value);
            if (qwq)
            {
                note.actualHoldTime = value - note.actualTime;
            }
            else
            {
                endTime.Text = (note.actualTime + note.actualHoldTime).ToString();
            }
        }
    }
}
