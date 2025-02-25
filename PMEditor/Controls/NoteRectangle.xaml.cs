using System.Collections.Generic;
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
                highLightBorder.BorderThickness = highLight ? new Thickness(2) : new Thickness(0);
            }
        }

        public bool IsResizing { get; set; }

        //左键选中此note
        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
            TrackEditorPage.Instance!.InfoFrame.Content = new NotePropertyPanel(note);
            TrackEditorPage.Instance.UpdateSelectedNote(note); 
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                var currentPosition = e.GetPosition(rect);
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
                var currentPosition = e.GetPosition(rect);
                if (currentPosition.Y < 20)
                {
                    rect.Cursor = Cursors.SizeNS;
                    return;
                }
            }
            rect.Cursor = Cursors.Arrow;
        }

        private void UIElement_OnKeyDown(object sender, KeyEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void CopyClick(object sender, RoutedEventArgs e)
        {
            TrackEditorPage.Instance!.UpdateSelectedNote(note);
            TrackEditorPage.Instance.CopyNote(new List<Note>{note});
        }

        private void CutClick(object sender, RoutedEventArgs e)
        {
            TrackEditorPage.Instance!.UpdateSelectedNote(new List<Note>()); 
            TrackEditorPage.Instance.CutNote(new List<Note>{note});
        }

        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            TrackEditorPage.Instance!.UpdateSelectedNote(new List<Note>()); 
            TrackEditorPage.Instance.DeleteNote(new List<Note>{note});
        }
    }
}
