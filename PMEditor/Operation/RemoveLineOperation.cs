namespace PMEditor.Operation;

public class RemoveLineOperation : BaseOperation
{
    Line line;

    public RemoveLineOperation(Line line)
    {
        this.line = line;
    }

    public override string GetInfo()
    {
        return "移除判定线：" + line.Id;
    }

    public override void Redo()
    {
        EditorWindow.Instance.track.Lines.Remove(line);
        TrackEditorPage.Instance!.UpdateNote();
    }

    public override void Undo()
    {
        EditorWindow.Instance.track.Lines.Add(line);
        TrackEditorPage.Instance!.UpdateNote();
    }
}