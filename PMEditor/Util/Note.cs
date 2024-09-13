using PMEditor.Util;
using System.Text.Json.Serialization;
using NCalc;

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

    interface IFreeNote
    {
        /// <param name="t">还有多少tick这个note被判定</param>
        /// <param name="l">地图的长度是多少</param>
        public Expression Expr { get; set; }
    }

    public class FreeNote: Note, IFreeNote
    {

        public Expression expr;

        public Expression Expr
        {
            get => expr; set => expr = value;
        }

        [JsonConstructor]
        public FreeNote(int rail, int noteType, int fallType, bool isFake, double actualTime, double actualHoldTime = 0, string expr = "time")
            : base(rail, noteType, fallType, isFake, actualTime, false, actualHoldTime) 
        {
            this.expr = new Expression(expr);
        }


        public FreeNote(int rail, int noteType, int fallType, bool isFake, double actualTime, bool isCurrentLineNote, double actualHoldTime = 0, string expr = "time")
            : base(rail, noteType, fallType, isFake, actualTime, isCurrentLineNote, actualHoldTime)
        {
            this.expr = new Expression(expr);
        }

        public FreeNote(Note note, string expr = "time"): base(note.rail, note.noteType, note.fallType, note.isFake, note.actualTime, note.actualHoldTime)
        {
            this.expr = new Expression(expr);
        }

    }

    public class FreeFakeCatch : FakeCatch, IFreeNote
    {
        public Expression expr;

        public Expression Expr
        {
            get => expr; set => expr = value;
        }

        [JsonConstructor]
        public FreeFakeCatch(double height, int rail, int noteType, int fallType, bool isFake, double actualTime, double actualHoldTime, string expr = "time") : base(height, rail, noteType, fallType, isFake, actualHoldTime, actualHoldTime)
        {
            this.expr = new Expression(expr);
        }

        public FreeFakeCatch(int rail, int fallType, double actualTime, double height, string expr = "time") : base(rail, fallType, actualTime, height)
        {
            this.expr = new Expression(expr);
        }

        public FreeFakeCatch(int rail, int fallType, double actualTime, double height, bool isCurrentLineNote, string expr = "time") : base(rail, fallType, actualTime, height, isCurrentLineNote)
        {
            this.expr = new Expression(expr);
        }

        public FreeFakeCatch(FakeCatch fakeCatch,string expr = "time"): this(fakeCatch.Height, fakeCatch.rail, fakeCatch.noteType, fakeCatch.fallType, fakeCatch.isFake, fakeCatch.actualTime, fakeCatch.actualHoldTime)
        {
            this.expr = new Expression(expr);
        }
    }
}
