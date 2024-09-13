using System.Windows;
using System.Windows.Controls;
using Expression = NCalc.Expression;

namespace PMEditor.Controls
{
    /// <summary>
    /// FreeFakeCatchPropertyPanel.xaml 的交互逻辑
    /// </summary>
    public partial class FreeFakeCatchPropertyPanel : UserControl
    {

        FreeFakeCatch note;

        public FreeFakeCatchPropertyPanel(FreeFakeCatch note)
        {
            InitializeComponent();
            this.note = note;
            noteType.Text = note.type.ToString();
            startTime.Value = note.actualTime;
            fakeCatchHeight.Value = note.Height;
            expression.Value = note.expr;
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
            note.Height = value;
            (EditorWindow.Instance.page.Content as TrackEditorPage)?.UpdateNote();
        }

        private void expression_PropertyChangeEvent(object sender, RoutedEventArgs e)
        {
            var exprStr = (string)((PropertyChangeEventArgs)e).PropertyValue;
            var expr = new Expression(exprStr);
            if (!expr.HasErrors())
            {
                note.expr = expr;
                (EditorWindow.Instance.page.Content as TrackEditorPage)?.UpdateNote();
            }else
            {
                expression.Value = note.expr.ExpressionString??string.Empty;
            }
        }
    }
}
