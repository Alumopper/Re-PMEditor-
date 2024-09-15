namespace PMEditor.Operation
{
    public class PutFunctionOperation : BaseOperation
    {
        Line line;
        Function function;


        public PutFunctionOperation(Function function, Line line)
        {
            this.function = function;
            this.line = line;
        }

        public override void Redo()
        {
            line.Functions.Add(function);
            TrackEditorPage.Instance.UpdateNote();
        }

        public override void Undo()
        {
            line.Functions.Remove(function);
            TrackEditorPage.Instance.functionPanel.Children.Remove(function.rectangle);
            function.rectangle.Visibility = System.Windows.Visibility.Hidden;
            TrackEditorPage.Instance.UpdateNote();
        }

        public override string GetInfo()
        {
            return "放置" + function.ToString();
        }
    }
}
