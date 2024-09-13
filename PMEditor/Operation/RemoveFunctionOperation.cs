namespace PMEditor.Operation
{
    public class RemoveFunctionOperation : BaseOperation
    {
        Line line;
        Function function;


        public RemoveFunctionOperation(Function function, Line line)
        {
            this.function = function;
            this.line = line;
        }

        public override void Undo()
        {
            line.functions.Add(function);
            TrackEditorPage.Instance.functionPanel.Children.Add(function.rectangle);
            TrackEditorPage.Instance.UpdateNote();
        }

        public override void Redo()
        {
            line.functions.Remove(function);
            TrackEditorPage.Instance.functionPanel.Children.Remove(function.rectangle);
            function.rectangle.Visibility = System.Windows.Visibility.Hidden;
            TrackEditorPage.Instance.UpdateNote();
        }

        public override string GetInfo()
        {
            return "移除" + function.ToString();
        }
    }
}
