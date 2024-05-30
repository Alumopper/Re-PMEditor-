using System.Collections.Generic;
using System.Linq;

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

        //进行过的操作的列表
        static List<BaseOperation> ops = new();

        private OperationManager() { }

        public static bool CanRedo
        {
            get => index < ops.Count - 1 && index >= -1;
        }

        public static bool CanUndo
        {
            get => index < ops.Count && index >= 0;
        }

        public static bool HasSaved
        {
            get => savedOperationIndex == index;
        }

        public static BaseOperation? Redo()
        {
            if (index < ops.Count - 1)
            {
                index++;
                ops[index].Redo();
                return ops[index];
            }
            return null;
        }

        public static BaseOperation? Undo()
        {
            if (index >= 0)
            {
                ops[index].Undo();
                index--;
                return ops[index + 1];
            }
            return null;
        }

        public static void AddOperation(BaseOperation op)
        {
            EditorWindow.Instance.UpdateStatusBar();
            //记录截取
            if (index != ops.Count)
            {
                ops = ops.Take(index + 1).ToList();
            }
            ops.Add(op);
            //更改标题
            EditorWindow.Instance.Title = "Re:PMEditor - " + EditorWindow.Instance.info.TrackName + " *";
            index++;
        }
    }
}
