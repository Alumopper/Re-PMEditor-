using IronPython.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PMEditor
{
    /// <summary>
    /// 一个note
    /// </summary>
    public partial class Note
    {
        static Rectangle template;

        public Rectangle rectangle;

        public NoteType type;

        public bool hasJudged = false;

        public MediaPlayer sound = new MediaPlayer();

        public static void SetTemplate(Rectangle rectangle)
        {
            Note.template = rectangle;
        }

        public void SetNoteType(NoteType noteType)
        {
            this.type = noteType;
            this.noteType = (int)noteType;
        }

        public override bool Equals(object? obj)
        {
            if(obj != null && obj is Note note)
            {
                return this.actualTime == note.actualTime && this.rail == note.rail;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return actualTime.GetHashCode() + rail.GetHashCode() + noteType.GetHashCode();
        }

        public override string ToString()
        {
            return $"{(type == PMEditor.NoteType.Tap?"Tap":"Drag")}[rail={rail},time={actualTime}]";
        }
    }

    public enum NoteType{
        Tap,Drag,Hold
    }
}
