namespace PMEditor.Operation
{
    internal class RemoveEventOperation : BaseOperation
    {
        Line line;
        Event @event;


        public RemoveEventOperation(Event e, Line line)
        {
            this.@event = e;
            this.line = line;
        }

        public override void Undo()
        {
            line.events.Add(@event);
            OperationManager.editorPage.notePanel.Children.Add(@event.rectangle);
            OperationManager.editorPage.UpdateNote();
        }

        public override void Redo()
        {
            line.events.Remove(@event);
            OperationManager.editorPage.notePanel.Children.Remove(@event.rectangle);
            @event.rectangle.Visibility = System.Windows.Visibility.Hidden;
            OperationManager.editorPage.UpdateNote();
        }

        public override string GetInfo()
        {
            return "移除" + @event.ToString();
        }
    }
}
