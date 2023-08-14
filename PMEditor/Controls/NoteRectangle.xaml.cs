using System.Windows.Controls;
using System.Windows.Media;

namespace PMEditor
{
    /// <summary>
    /// NoteRectangle.xaml 的交互逻辑
    /// </summary>
    public partial class NoteRectangle : UserControl
    {
        /// <summary>
        /// 这个矩形对应的note
        /// </summary>
        public Note note;

        public NoteRectangle(Note note)
        {
            InitializeComponent();
            this.note = note;
        }

        public Brush Fill
        {
            get => rect.Fill; set => rect.Fill = value;
        }
    }
}
