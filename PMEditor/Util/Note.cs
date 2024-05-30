using PMEditor.Controls;
using PMEditor.Operation;
using System;
using System.Windows.Input;
using System.Windows.Media;

namespace PMEditor
{
    /// <summary>
    /// 一个note
    /// </summary>
    public partial class Note
    {
        public NoteRectangle rectangle;

        public NoteType type;

        public bool hasJudged = false;

        public Line parentLine;

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
            double t = actualHoldTime + 0.25;
            int count = (int)(t / 0.5);
            if (t % 0.5 >= 0.25)
            {
                count++;
            }
            return count;
        }

        public void SetNoteType(NoteType noteType)
        {
            this.type = noteType;
            this.noteType = (int)noteType;
        }

        public override bool Equals(object? obj)
        {
            if (obj != null && obj is Note note)
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
                PMEditor.NoteType.Tap => $"Tap[line={parentLine.notes.IndexOf(this)},rail={rail},time={actualTime}]",
                PMEditor.NoteType.Catch => $"Catch[line={parentLine.notes.IndexOf(this)},rail={rail},time={actualTime}]",
                PMEditor.NoteType.Hold => $"Hold[line={parentLine.notes.IndexOf(this)},rail={rail},time={actualTime}]",
                _ => "Unknown",
            };
        }

        public bool IsOverlap(Note note)
        {
            if(!(note.rail == rail && note.parentLine == parentLine)) return false;
            if(type == PMEditor.NoteType.Hold)
            {
                if(note.type == PMEditor.NoteType.Hold)
                {
                    return actualTime <= note.actualTime && note.actualTime <= actualTime + actualHoldTime || actualTime <= note.actualTime + note.actualHoldTime && note.actualTime + note.actualHoldTime <= actualTime + actualHoldTime;
                }
                else
                {
                    return actualTime <= note.actualTime && note.actualTime <= actualTime + actualHoldTime;
                }
            }
            else
            {
                if(note.type == PMEditor.NoteType.Hold)
                {
                    return note.IsOverlap(this);
                }
                else
                {
                    return note.actualTime == actualTime;
                }
            }
        }
    }

    public enum NoteType
    {
        Tap, Catch, Hold
    }
}
