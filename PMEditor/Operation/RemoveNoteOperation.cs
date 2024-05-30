using PMEditor.Util;

namespace PMEditor.Operation
{
    public class RemoveNoteOperation : BaseOperation
    {
        Line line;
        Note note;


        public RemoveNoteOperation(Note note, Line line)
        {
            this.note = note;
            this.line = line;
        }

        public override void Undo()
        {
            if(note is FakeCatch f)
            {
                line.fakeCatch.Add(f);
            }
            else
            {
                line.notes.Add(note);
            }
            TrackEditorPage.Instance.notePanel.Children.Add(note.rectangle);
            TrackEditorPage.Instance.UpdateNote();
        }

        public override void Redo()
        {
            if(note is FakeCatch f)
            {
                line.FakeCatch.Remove(f);
            }
            else
            {
                line.notes.Remove(note);
            }
            TrackEditorPage.Instance.notePanel.Children.Remove(note.rectangle);
            note.rectangle.Visibility = System.Windows.Visibility.Hidden;
            TrackEditorPage.Instance.UpdateNote();
        }

        public override string GetInfo()
        {
            return "移除" + note.ToString();
        }
    }
}
