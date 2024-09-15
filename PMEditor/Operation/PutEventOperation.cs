namespace PMEditor.Operation
{
    public class PutEventOperation : BaseOperation
    {
        EventList list;
        Event @event;


        public PutEventOperation(Event @event, EventList list)
        {
            this.@event = @event;
            this.list = list;
        }

        public override void Redo()
        {
            @event.parentList.GroupEvent();
            list.Events.Add(@event);
            TrackEditorPage.Instance.UpdateEvent();
        }

        public override void Undo()
        {
            @event.parentList.GroupEvent();
            list.Events.Remove(@event);
            TrackEditorPage.Instance.eventPanel.Children.Remove(@event.rectangle);
            @event.rectangle.Visibility = System.Windows.Visibility.Hidden;
            TrackEditorPage.Instance.UpdateEvent();
        }

        public override string GetInfo()
        {
            return "放置" + @event.ToString();
        }
    }
}
