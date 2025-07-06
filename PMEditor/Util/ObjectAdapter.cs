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
    
    public bool IsInViewRange => Rect != null;

    public Line ParentLine
    {
        get
        {
            return note?.ParentLine??
                   @event?.ParentList.ParentLine??
                   fakeCatch?.ParentLine??
                   throw new NullReferenceException("没有值");
        }
    }

    public ObjectAdapter(){}
    
    public ObjectAdapter(object obj)
    {
        Value = obj;
    }

    public ObjectRectangle EnterViewRange(ObjectPanel panel)
    {
        Rect = ObjectRectangle.ObjectRectanglePool.Get();
        Rect.Data = this;
        Rect.SetFromObject(Value!, panel);
        return Rect;
    }

    public ObjectRectangle? ExitViewRange()
    {
        if (Rect == null) return null;
        ObjectRectangle.ObjectRectanglePool.Release(Rect);
        var re = Rect;
        Rect = null;
        return re;
    }

    public override bool Equals(object? obj)
    {
        return obj is ObjectAdapter adapter && adapter.Value == Value;
    }

    public override int GetHashCode()
    {
        return Value?.GetHashCode() ?? 0;
    }

    public override string ToString()
    {
        return Value?.ToString() ?? "null";
    }

    public ObjectAdapter Clone()
    {
        return Value switch
        {
            FakeCatch fakeCatch1 => new ObjectAdapter(fakeCatch1.Clone()),
            Note note1 => new ObjectAdapter(note1.Clone()),
            Event event1 => new ObjectAdapter(event1.Clone()),
            _ => throw new NullReferenceException("没有值")
        };
    }

    public bool IsOverlap(ObjectAdapter adapter)
    {
        if (this.GetType() != adapter.GetType()) return false;
        return this.Value switch
        {
            FakeCatch fakeCatch1 => fakeCatch1.IsOverlap((FakeCatch)adapter.Value!),
            Note note1 => note1.IsOverlap((Note)adapter.Value!),
            Event event1 => event1.IsOverlap((Event)adapter.Value!),
            _ => throw new NullReferenceException("没有值")
        };
    }
}