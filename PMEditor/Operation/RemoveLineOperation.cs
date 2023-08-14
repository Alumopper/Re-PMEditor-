using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMEditor.Operation
{
    public class RemoveLineOperation : BaseOperation
    {
        Line line;

        public RemoveLineOperation(Line line)
        {
            this.line = line;
        }

        public override string GetInfo()
        {
            return "移除判定线：" + line.id;
        }

        public override void Redo()
        {
            OperationManager.editorWindow.track.lines.Remove(line);
            OperationManager.editorPage.UpdateNote();
        }

        public override void Undo()
        {
            OperationManager.editorWindow.track.lines.Add(line);
            OperationManager.editorPage.UpdateNote();
        }
    }
}
