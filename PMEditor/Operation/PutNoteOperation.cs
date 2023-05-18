using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMEditor.Operation
{
    public class PutNoteOperation : BaseOperation
    {
        Line line;
        Note note;


        public PutNoteOperation(Note note, Line line)
        {
            this.note = note;
            this.line = line;
        }

        public override void ReDo()
        {
            line.notes.Add(note);
            OperationManager.editorPage.UpdateNote();
        }

        public override void Undo()
        {
            line.notes.Remove(note);
            note.rectangle.Visibility = System.Windows.Visibility.Hidden;
        }
    }
}
