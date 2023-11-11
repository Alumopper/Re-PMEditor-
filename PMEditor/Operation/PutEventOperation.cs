namespace PMEditor.Operation
{
    public class PutEventOperation : BaseOperation
    {
        Line line;
        Event @event;


        public PutEventOperation(Event @event, Line line)
        {
            this.@event = @event;
            this.line = line;
        }

        public override void Redo()
        {
            line.events.Add(@event);
            OperationManager.editorPage.UpdateNote();
        }

        public override void Undo()
        {
            line.events.Remove(@event);
            OperationManager.editorPage.notePanel.Children.Remove(@event.rectangle);
            @event.rectangle.Visibility = System.Windows.Visibility.Hidden;
            OperationManager.editorPage.UpdateNote();
        }

        public override string GetInfo()
        {
            return "放置" + @event.ToString();
        }
    }
}
