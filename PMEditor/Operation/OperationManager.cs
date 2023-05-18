using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMEditor.Operation
{
    public class OperationManager
    {
        //列表迭代指针（指向下一次撤销的操作。若重做则是index+1）
        static int index = -1;

        public static TrackEditorPage editorPage;

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

        public static void ReDo()
        {
            if(index < ops.Count-1)
            {
                index++;
                ops[index].ReDo();
            }
        }

        public static void Undo()
        {
            if(index >= 0)
            {
                ops[index].Undo();
                index--;
            }
        }

        public static void AddOperation(BaseOperation op)
        {
            //记录截取
            if(index != ops.Count)
            {
                ops = ops.Take(index + 1).ToList();
            }
            ops.Add(op);
            index++;
        }
    }
}
