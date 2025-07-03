using PMEditor.Util;

namespace PMEditor.Operation;

public class RemoveNoteOperation : BaseOperation
{
    private readonly Line line;
    private readonly Note note;


    public RemoveNoteOperation(Note note, Line line)
    {
        this.note = note;
        this.line = line;
    }

    public override void Undo()
    {
        if(note is FakeCatch f)
        {
            line.FakeCatch.Add(f);
        }
        else
        {
            line.AddNote(note);
        }
        //TrackEditorPage.Instance!.NotePanel.Children.Add(note.rectangle);
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
            line.RemoveNote(note);
        }
        //TrackEditorPage.Instance!.NotePanel.Children.Remove(note.rectangle);
        //note.rectangle.Visibility = System.Windows.Visibility.Hidden;
        TrackEditorPage.Instance.UpdateNote();
    }

    public override string GetInfo()
    {
        return "移除" + note.ToString();
    }
}