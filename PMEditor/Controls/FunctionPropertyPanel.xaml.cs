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
using System.Xml.Serialization;

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
            this.function = function;
        }

        private void functionName_PropertyChangeEvent(object sender, RoutedEventArgs e)
        {
            var value = (string)((PropertyChangeEventArgs)e).PropertyValue;
            function.FunctionName = value;
        }

        private void script_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //打开vscode
            if (function.linkedFile != null)
            {
                System.Diagnostics.Process.Start("code", function.linkedFile.FullName);
            }
        }

        //选择文件
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "脚本文件|*.mcfunction"
            };
            if (dialog.ShowDialog() == true)
            {
                function.linkedFile = new System.IO.FileInfo(dialog.FileName);
                script.Text = dialog.FileName;
            }
        }
    }
}
