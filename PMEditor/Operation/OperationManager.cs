using System.Collections.Generic;
using System.Linq;

namespace PMEditor.Operation;

public static class OperationManager
{
    //列表迭代指针（指向下一次撤销的操作。若重做则是index+1）
    public static int Index { get; private set; } = -1;

    public static int savedOperationIndex = Index;

    //进行过的操作的列表
    private static List<BaseOperation> _ops = new();

    public static bool CanRedo
    {
        get => Index < _ops.Count - 1 && Index >= -1;
    }

    public static bool CanUndo
    {
        get => Index < _ops.Count && Index >= 0;
    }

    public static bool HasSaved
    {
        get => savedOperationIndex == Index;
    }

    public static BaseOperation? Redo()
    {
        if (Index < _ops.Count - 1)
        {
            Index++;
            _ops[Index].Redo();
            return _ops[Index];
        }
        return null;
    }

    public static BaseOperation? Undo()
    {
        if (Index >= 0)
        {
            _ops[Index].Undo();
            Index--;
            return _ops[Index + 1];
        }
        return null;
    }

    public static void AddOperation(BaseOperation op)
    {
        EditorWindow.Instance.UpdateStatusBar();
        //记录截取
        if (Index != _ops.Count)
        {
            _ops = _ops.Take(Index + 1).ToList();
        }
        _ops.Add(op);
        //更改标题
        EditorWindow.Instance.Title = "Re:PMEditor - " + EditorWindow.Instance.info.TrackName + " *";
        Index++;
    }
}