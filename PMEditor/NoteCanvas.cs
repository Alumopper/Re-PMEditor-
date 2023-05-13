using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PMEditor
{
    public class NoteCanvas
    {
        private Canvas canvas;
        public Canvas Canvas
        {
            get { return canvas; }
        }

        public List<Note> notes = new List<Note>();

        public NoteCanvas(Canvas canvas)
        {
            this.canvas = canvas;
        }
        
        public void AddNote(Note note)
        {
            this.notes.Add(note);
            note.parent = this;
        }
    }
}
