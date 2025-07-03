using System;
using PMEditor.Controls;

namespace PMEditor.Util;

public class ObjectAdapter
{
    private Note? note;
    private Event? @event;
    private FakeCatch? fakeCatch;

    public object? Value
    {
        get => (object?)note ?? (object?)@event ?? fakeCatch;
        set
        {
            note = null;
            @event = null;
            fakeCatch = null;
            switch (value)
            {
                case FakeCatch fakeCatch1:
                    this.fakeCatch = fakeCatch1;
                    OnJudged = null;
                    break;
                case Note note1: 
                    this.note = note1;
                    OnJudged = ObjectPanel.OnNoteJudged;
                    break;
                case Event event1:
                    this.@event = event1;
                    OnJudged = null;
                    break;
                default:
                    throw new ArgumentException("不支持的类型: " + value);
            }
        }
    }

    public double StartTime
    {
        get
        {
            return note?.ActualTime ??
                   @event?.StartTime ?? 
                   fakeCatch?.ActualTime ?? 
                   throw new NullReferenceException("没有值");
        }
        set
        {
            if (note != null)
            {
                note.ActualTime = value;
            }
            else if (@event != null)
            {
                @event.StartTime = value;
            }
            else if (fakeCatch != null)
            {
                fakeCatch.ActualTime = value;
            }
            else
            {
                throw new NullReferenceException("没有值");
            }
        }
    }
    
    public double LengthTime
    {
        get
        {
            return note?.ActualHoldTime??
                   @event?.EndTime - @event?.StartTime?? 
                   fakeCatch?.ActualHoldTime?? 
                   throw new NullReferenceException("没有值");
        }
        set
        {
            
            if (note != null)
            {
                note.ActualHoldTime = value;
            }
            else if (@event != null)
            {
                @event.EndTime = @event.StartTime + value;
            }
            else if (fakeCatch != null) {}
            else
            {
                throw new NullReferenceException("没有值");
            }
        }
    }
    
    public double EndTime
    {
        get
        {
            return note?.ActualTime + note?.ActualHoldTime ??
                   @event?.EndTime ?? 
                   fakeCatch?.ActualTime + fakeCatch?.ActualHoldTime ??
                   throw new NullReferenceException("没有值");
        }
        set
        {
            if (note != null)
            {
                note.ActualHoldTime = value - note.ActualTime;
            }
            else if (@event != null)
            {
                @event.EndTime = value;
            }
            else if (fakeCatch != null) {}
            else
            {
                throw new NullReferenceException("没有值");
            }
        }
    }

    public int Rail
    {
        get
        {
            return note?.Rail??
                   @event?.Rail?? 
                   fakeCatch?.Rail?? 
                   throw new NullReferenceException("没有值");
        }
        set
        {
            
            if (note != null)
            {
                note.Rail = value;
            }
            else if (@event != null)
            {
                @event.Rail = value;
            }
            else if (fakeCatch != null)
            {
                fakeCatch.Rail = value;
            }
            else
            {
                throw new NullReferenceException("没有值");
            }
        }
    }

    public Action<Note>? OnJudged { get; set; }

    public bool IsJudged;

    public ObjectRectangle? Rect;
    
    public bool IsInViewRange;

    public ObjectAdapter(){}
    
    public ObjectAdapter(object obj)
    {
        Value = obj;
    }

    public ObjectRectangle EnterViewRange()
    {
        IsInViewRange = true;
        Rect = ObjectRectangle.ObjectRectanglePool.Get();
        Rect.SetFromObject(Value!);
        return Rect;
    }

    public ObjectRectangle? ExitViewRange()
    {
        IsInViewRange = false;
        if (Rect == null) return null;
        ObjectRectangle.ObjectRectanglePool.Release(Rect);
        var re = Rect;
        Rect = null;
        return re;
    }
    
}