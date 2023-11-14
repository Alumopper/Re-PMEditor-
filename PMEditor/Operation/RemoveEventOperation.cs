namespace PMEditor.Operation
{
    internal class RemoveEventOperation : BaseOperation
    {
        EventList list;
        Event @event;


        public RemoveEventOperation(Event e, EventList list)
        {
            this.@event = e;
            this.list = list;
        }

        public override void Undo()
        {
            @event.parentList.GroupEvent();
            list.events.Add(@event);
            OperationManager.editorPage.eventPanel.Children.Add(@event.rectangle);
            OperationManager.editorPage.UpdateEvent();
        }

        public override void Redo()
        {
            @event.parentList.GroupEvent();
            list.events.Remove(@event);
            OperationManager.editorPage.eventPanel.Children.Remove(@event.rectangle);
            @event.rectangle.Visibility = System.Windows.Visibility.Hidden;
            OperationManager.editorPage.UpdateEvent();
        }

        public override string GetInfo()
        {
            return "移除" + @event.ToString();
        }
    }
}
