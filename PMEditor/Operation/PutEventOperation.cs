namespace PMEditor.Operation;

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
        @event.ParentList.GroupEvent();
        list.Events.Add(@event);
        TrackEditorPage.Instance.UpdateEvent();
    }

    public override void Undo()
    {
        @event.ParentList.GroupEvent();
        list.Events.Remove(@event);
        //TrackEditorPage.Instance.EventPanel.Children.Remove(@event.Rectangle);
        TrackEditorPage.Instance.UpdateEvent();
    }

    public override string GetInfo()
    {
        return "放置" + @event.ToString();
    }
}