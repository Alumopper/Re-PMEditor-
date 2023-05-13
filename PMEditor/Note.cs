using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace PMEditor
{
    public partial class Note
    {
        static Rectangle template;

        Rectangle rectangle;

        public NoteCanvas parent;

        public NoteType type;

        public void SetTemplate(Rectangle rectangle)
        {
            Note.template = rectangle;
        }
    }

    public enum NoteType{
        Tap,Drag
    }
}
