using PMEditor.Operation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PMEditor.Controls
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class FunctionRectangle : UserControl
    {
        /// <summary>
        /// 这个矩形对应的note
        /// </summary>
        public Function function;

        public FunctionRectangle(Function function)
        {
            InitializeComponent();
            this.function = function;
        }

        public Brush Fill
        {
            get => rect.Fill; set => rect.Fill = value;
        }

        public Brush HighLightBorderBrush
        {
            get => highLightBorder.BorderBrush; set => highLightBorder.BorderBrush = value;
        }

        private bool highLight;
        public bool HighLight
        {
            get => highLight;
            set
            {
                highLight = value;
                //高亮
                if (highLight)
                {
                    highLightBorder.BorderThickness = new(2);
                }
                else
                {
                    highLightBorder.BorderThickness = new(0);
                }
            }
        }

        //右键删除此note
        private void Rectangle_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            function.parentLine.functions.Remove(function);
            TrackEditorPage.Instance.functionPanel.Children.Remove(this);
            OperationManager.AddOperation(new RemoveFunctionOperation(function, function.parentLine));
        }

        //左键选中此note
        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TrackEditorPage.Instance.infoFrame.Content = new FunctionPropertyPanel(function);
            TrackEditorPage.Instance.UpdateSelectedFunction(function);
        }
    }
}
