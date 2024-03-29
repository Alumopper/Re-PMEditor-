﻿namespace PMEditor.Operation
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
            TrackEditorPage.Instance.UpdateNote();
        }

        public override void Undo()
        {
            line.notes.Remove(note);
            TrackEditorPage.Instance.notePanel.Children.Remove(note.rectangle);
            note.rectangle.Visibility = System.Windows.Visibility.Hidden;
            TrackEditorPage.Instance.UpdateNote();
        }

        public override string GetInfo()
        {
            return "放置" + note.ToString();
        }
    }
}
