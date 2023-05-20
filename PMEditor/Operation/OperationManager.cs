using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PMEditor.Operation
{
    public class OperationManager
    {
        //列表迭代指针（指向下一次撤销的操作。若重做则是index+1）
        static int index = -1;
        public static int Index
        {
            get { return index; }
        }

        public static int savedOperationIndex = index;

        public static TrackEditorPage editorPage;

        public static EditorWindow editorWindow;

        //进行过的操作的列表
        static List<BaseOperation> ops = new();

        private OperationManager(){}

        public static bool CanRedo
        {
            get => index < ops.Count-1 && index >= -1;
        }

        public static bool CanUndo
        {
            get => index < ops.Count && index >= 0;
        }

        public static BaseOperation? ReDo()
        {
            if(index < ops.Count-1)
            {
                index++;
                ops[index].ReDo();
                return ops[index];
            }
            return null;
        }

        public static BaseOperation? Undo()
        {
            if(index >= 0)
            {
                ops[index].Undo();
                index--;
                return ops[index+1];
            }
            return null;
        }

        public static void AddOperation(BaseOperation op)
        {
            //记录截取
            if(index != ops.Count)
            {
                ops = ops.Take(index + 1).ToList();
            }
            ops.Add(op);
            //更改标题
            editorWindow.Title = "Re:PMEditor - " + editorWindow.info.TrackName + " *";
            index++;
        }
    }
}
