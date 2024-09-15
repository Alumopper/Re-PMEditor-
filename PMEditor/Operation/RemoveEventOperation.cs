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
            list.Events.Add(@event);
            TrackEditorPage.Instance.eventPanel.Children.Add(@event.rectangle);
            TrackEditorPage.Instance.UpdateEvent();
        }

        public override void Redo()
        {
            @event.parentList.GroupEvent();
            list.Events.Remove(@event);
            TrackEditorPage.Instance.eventPanel.Children.Remove(@event.rectangle);
            @event.rectangle.Visibility = System.Windows.Visibility.Hidden;
            TrackEditorPage.Instance.UpdateEvent();
        }

        public override string GetInfo()
        {
            return "移除" + @event.ToString();
        }
    }
}
