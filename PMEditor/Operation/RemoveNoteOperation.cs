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
            OperationManager.editorPage.notePanel.Children.Add(note.rectangle);
            OperationManager.editorPage.UpdateNote();
        }

        public override void Redo()
        {
            line.notes.Remove(note);
            OperationManager.editorPage.notePanel.Children.Remove(note.rectangle);
            note.rectangle.Visibility = System.Windows.Visibility.Hidden;
            OperationManager.editorPage.UpdateNote();
        }

        public override string GetInfo()
        {
            return "移除" + note.ToString();
        }
    }
}
