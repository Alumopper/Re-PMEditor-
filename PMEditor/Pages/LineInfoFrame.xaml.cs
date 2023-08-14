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
    /// LineInfoFrame.xaml 的交互逻辑
    /// </summary>
    public partial class LineInfoFrame : Page
    {
        public Line line;

        public LineInfoFrame(Line line, int index)
        {
            InitializeComponent();
            this.line = line;

            //信息加载
            lineIndex.Text = index.ToString();
            lineID.Text = line.id.ToString();
            generalCount.Text = line.notes.Count.ToString();
            tapCount.Text = line.notes.FindAll( e => e.type == NoteType.Tap).Count.ToString();
            dragCount.Text = line.notes.FindAll(e => e.type == NoteType.Drag).Count.ToString();
        }
    }
}
