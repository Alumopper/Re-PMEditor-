using System.Windows.Controls;

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
            tapCount.Text = line.notes.FindAll(e => e.type == NoteType.Tap).Count.ToString();
            catchCount.Text = line.notes.FindAll(e => e.type == NoteType.Catch).Count.ToString();
        }
    }
}
