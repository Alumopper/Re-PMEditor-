using PMEditor.Controls;
using PMEditor.Operation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        public bool IsResizing { get; set; } = false;

        //右键删除此note
        private void Rectangle_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            note.parentLine.notes.Remove(note);
            TrackEditorPage.Instance.notePanel.Children.Remove(this);
            OperationManager.AddOperation(new RemoveNoteOperation(note, note.parentLine));
        }

        //左键选中此note
        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (note is FreeNote freeNote)
            {
                TrackEditorPage.Instance.infoFrame.Content = new FreeNotePropertyPanel(freeNote);
            }
            else if(note is FreeFakeCatch freeFakeCatch)
            {
                TrackEditorPage.Instance.infoFrame.Content = new FreeFakeCatch(freeFakeCatch);
            }
            else
            {
                TrackEditorPage.Instance.infoFrame.Content = new NotePropertyPanel(note);
            }
            TrackEditorPage.Instance.UpdateSelectedNote(note); 
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                Point currentPosition = e.GetPosition(rect);
                if (currentPosition.Y < 20)
                {
                    IsResizing = true;
                    return;
                }
            }
            IsResizing = false;
        }
        
        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                Point currentPosition = e.GetPosition(rect);
                if (currentPosition.Y < 20)
                {
                    rect.Cursor = Cursors.SizeNS;
                    return;
                }
            }
            rect.Cursor = Cursors.Arrow;
        }

    }
}
