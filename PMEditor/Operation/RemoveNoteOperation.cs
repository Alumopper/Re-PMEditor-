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
            line.notes.Add(note);
            TrackEditorPage.Instance.notePanel.Children.Add(note.rectangle);
            TrackEditorPage.Instance.UpdateNote();
        }

        public override void Redo()
        {
            line.notes.Remove(note);
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
