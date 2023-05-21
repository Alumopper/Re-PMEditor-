using PMEditor.Operation;
using System;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using static System.Windows.Forms.LinkLabel;

namespace PMEditor
{
    /// <summary>
    /// 一个note
    /// </summary>
    public partial class Note
    {
        public NoteRectangle rectangle;

        public NoteType type;

        public bool hasJudged = false;

        public MediaPlayer sound = new MediaPlayer();

        public void SetNoteType(NoteType noteType)
        {
            this.type = noteType;
            this.noteType = (int)noteType;
        }

        public override bool Equals(object? obj)
        {
            if (obj != null && obj is Note note)
            {
                return this.actualTime == note.actualTime && this.rail == note.rail;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return actualTime.GetHashCode() + rail.GetHashCode() + noteType.GetHashCode();
        }

        public override string ToString()
        {
            return $"{(type == PMEditor.NoteType.Tap ? "Tap" : "Drag")}[rail={rail},time={actualTime}]";
        }

        //右键删除此note
        private void Rectangle_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            OperationManager.editorWindow.track.lines[OperationManager.editorPage.LineIndex].notes.Remove(this);
            OperationManager.editorPage.notePanel.Children.Remove(rectangle);
            OperationManager.AddOperation(
                new RemoveNoteOperation(
                    this,
                    OperationManager.editorWindow.track.lines[OperationManager.editorPage.LineIndex]
                    )
                );
        }

        private void Sound_MediaEnded(object? sender, EventArgs e)
        {
            (sender as MediaPlayer).Stop();
            (sender as MediaPlayer).Position = new TimeSpan(0);
        }
    }

    public enum NoteType
    {
        Tap, Drag, Hold
    }
}
