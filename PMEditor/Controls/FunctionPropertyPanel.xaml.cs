using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PMEditor.Controls
{
    /// <summary>
    /// FunctionPropertyPanel.xaml 的交互逻辑
    /// </summary>
    public partial class FunctionPropertyPanel : UserControl
    {

        Function function;

        public FunctionPropertyPanel(Function function)
        {
            InitializeComponent();
            functionName.Type = PropertySetBox.PropertyType.String;
            functionName.Value = function.functionName;
            time.Type = PropertySetBox.PropertyType.Double;
            time.Value = function.time;
            linkFile.Text = function.linkedFile?.FullName;
            this.function = function;
        }

        private void functionName_PropertyChangeEvent(object sender, RoutedEventArgs e)
        {
            var value = (string)((PropertyChangeEventArgs)e).PropertyValue;
            function.Rename(value);
        }

        private void script_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //打开vscode
            if (function.linkedFile != null)
            {
                if(EditorWindow.Instance.vscodePath != null)
                {
                    Process.Start(EditorWindow.Instance.vscodePath, EditorWindow.Instance.track.datapack.target.FullName);
                    Process.Start(EditorWindow.Instance.vscodePath, function.linkedFile.FullName);
                }
                else
                {
                    Process.Start(function.linkedFile.FullName);
                }
            }
        }

        private void time_PropertyChangeEvent(object sender, RoutedEventArgs e)
        {
            var value = (double)((PropertyChangeEventArgs)e).PropertyValue;
            function.time = value;
            (EditorWindow.Instance.page.Content as TrackEditorPage)?.UpdateFunction();
        }
    }
}
