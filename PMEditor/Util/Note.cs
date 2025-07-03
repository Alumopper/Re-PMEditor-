using System;
using PMEditor.Util;
using System.Text.Json.Serialization;
using NCalc;
using PMEditor.Controls;

namespace PMEditor;

/// <summary>
/// 一个note
/// </summary>
public partial class Note
{
    [JsonIgnore]
    public readonly NoteType type;

    [JsonIgnore]
    public bool HasJudged = false;

    [JsonIgnore]
    public Line ParentLine;

    [JsonIgnore]
    public Expression? Expression;
    
    [JsonIgnore]
    public ObjectRectangle? ObjectRectangle;

    public int GetCount()
    {
        if(isFake)
        {
            return 0;
        }
        if(type != PMEditor.NoteType.Hold)
        {
            return 1;
        }
        double t = actualHoldTime;
        int count = (int)(t * Settings.currSetting.Tick) / 10 + 1;
        return count;
    }

    public override bool Equals(object? obj)
    {
        if (obj is Note note)
        {
            return this.actualTime == note.actualTime && this.rail == note.rail;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return actualTime.GetHashCode() + rail.GetHashCode() + noteType.GetHashCode() + IsFake.GetHashCode();
    }

    public override string ToString()
    {
        return type switch
        {
            PMEditor.NoteType.Tap => $"Tap[line={ParentLine.Notes.IndexOf(this)},rail={rail},time={actualTime}]",
            PMEditor.NoteType.Catch => $"Catch[line={ParentLine.Notes.IndexOf(this)},rail={rail},time={actualTime}]",
            PMEditor.NoteType.Hold => $"Hold[line={ParentLine.Notes.IndexOf(this)},rail={rail},time={actualTime}]",
            _ => "Unknown",
        };
    }

    public bool IsOverlap(Note note)
    {
        if(!(note.rail == rail && note.ParentLine == ParentLine)) return false;
        if(type == PMEditor.NoteType.Hold)
        {
            if(note.type == PMEditor.NoteType.Hold)
            {
                return actualTime <= note.actualTime && note.actualTime <= actualTime + actualHoldTime || actualTime <= note.actualTime + note.actualHoldTime && note.actualTime + note.actualHoldTime <= actualTime + actualHoldTime;
            }
            return actualTime <= note.actualTime && note.actualTime <= actualTime + actualHoldTime;
        }
        if(note.type == PMEditor.NoteType.Hold)
        {
            return note.IsOverlap(this);
        }
        return note.actualTime == actualTime;
    }

    public Note Clone()
    {
        return new Note(rail, noteType, fallType, isFake, actualTime, actualHoldTime, ExpressionString);
    }

}

public enum NoteType
{
    Tap, Catch, Hold
}