namespace PMEditor.Operation
{
    public class PutNoteOperation : BaseOperation
    {
        private readonly Line line;
        private readonly Note note;


        public PutNoteOperation(Note note, Line line)
        {
            this.note = note;
            this.line = line;
        }

        public override void Redo()
        {
            line.AddNote(note);
            TrackEditorPage.Instance!.UpdateNote();
        }

        public override void Undo()
        {
            line.RemoveNote(note);
            TrackEditorPage.Instance!.NotePanel.Children.Remove(note.rectangle);
            note.rectangle.Visibility = System.Windows.Visibility.Hidden;
            TrackEditorPage.Instance.UpdateNote();
        }

        public override string GetInfo()
        {
            return "放置" + note;
        }
    }
}
