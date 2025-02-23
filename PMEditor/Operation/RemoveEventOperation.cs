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
            @event.ParentList.GroupEvent();
            list.Events.Add(@event);
            TrackEditorPage.Instance.EventPanel.Children.Add(@event.Rectangle);
            TrackEditorPage.Instance.UpdateEvent();
        }

        public override void Redo()
        {
            @event.ParentList.GroupEvent();
            list.Events.Remove(@event);
            TrackEditorPage.Instance.EventPanel.Children.Remove(@event.Rectangle);
            @event.Rectangle.Visibility = System.Windows.Visibility.Hidden;
            TrackEditorPage.Instance.UpdateEvent();
        }

        public override string GetInfo()
        {
            return "移除" + @event.ToString();
        }
    }
}
