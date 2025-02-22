using PMEditor.Util;
using System.Windows;
using System.Windows.Controls;
using Expression = NCalc.Expression;

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
            startTime.Value = note.ActualTime;
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
                endTime.Value = note.ActualTime + note.ActualHoldTime;
            }
            if(note.Expression != null)
            {
                expression.Value = note.Expression.ExpressionString!;
            }else
            {
                expressionLable.Visibility = Visibility.Collapsed;
                expression.Visibility = Visibility.Collapsed;
            }
        }

        private void startTime_PropertyChangeEvent(object sender, RoutedEventArgs e)
        {
            var value = (double)((PropertyChangeEventArgs)e).PropertyValue;
            note.ActualTime = value;
            (EditorWindow.Instance.Page.Content as TrackEditorPage)?.UpdateNote();
        }

        private void endTime_PropertyChangeEvent(object sender, RoutedEventArgs e)
        {
            var value = (double)((PropertyChangeEventArgs)e).PropertyValue;
            note.ActualHoldTime = value - note.ActualTime;
            (EditorWindow.Instance.Page.Content as TrackEditorPage)?.UpdateNote();
        }

        private void fakeCatchHeight_PropertyChangeEvent(object sender, RoutedEventArgs e)
        {
            var value = (double)((PropertyChangeEventArgs)e).PropertyValue;
            (note as FakeCatch)!.Height = value;
            (EditorWindow.Instance.Page.Content as TrackEditorPage)?.UpdateNote();
        }

        private void noteType_PropertyChangeEvent(object sender, RoutedEventArgs e)
        {

        }
        
        
        private void expression_PropertyChangeEvent(object sender, RoutedEventArgs e)
        {
            var exprStr = (string)((PropertyChangeEventArgs)e).PropertyValue;
            var expr = new Expression(exprStr);
            if (!expr.HasErrors())
            {
                note.Expression = expr;
                (EditorWindow.Instance.Page.Content as TrackEditorPage)?.UpdateNote();
            }
            else
            {
                expression.Value = note.Expression?.ExpressionString??string.Empty;
            }
        }
    }
}
