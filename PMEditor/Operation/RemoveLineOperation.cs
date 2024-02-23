namespace PMEditor.Operation
{
    public class RemoveLineOperation : BaseOperation
    {
        Line line;

        public RemoveLineOperation(Line line)
        {
            this.line = line;
        }

        public override string GetInfo()
        {
            return "移除判定线：" + line.id;
        }

        public override void Redo()
        {
            EditorWindow.Instance.track.lines.Remove(line);
            TrackEditorPage.Instance.UpdateNote();
        }

        public override void Undo()
        {
            EditorWindow.Instance.track.lines.Add(line);
            TrackEditorPage.Instance.UpdateNote();
        }
    }
}
