namespace PMEditor.Operation
{
    public class PutNoteOperation : BaseOperation
    {
        Line line;
        Note note;


        public PutNoteOperation(Note note, Line line)
        {
            this.note = note;
            this.line = line;
        }

        public override void Redo()
        {
            line.notes.Add(note);
            OperationManager.editorPage.UpdateNote();
        }

        public override void Undo()
        {
            line.notes.Remove(note);
            OperationManager.editorPage.notePanel.Children.Remove(note.rectangle);
            note.rectangle.Visibility = System.Windows.Visibility.Hidden;
            OperationManager.editorPage.UpdateNote();
        }

        public override string GetInfo()
        {
            return "放置" + note.ToString();
        }
    }
}
