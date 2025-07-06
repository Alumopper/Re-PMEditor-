using System.Collections.Generic;
using System.Linq;
using PMEditor.Controls;
using PMEditor.Util;

namespace PMEditor.Operation;

public class PutObjOperation : BaseOperation
{
    private readonly ObjectPanel panel;
    private readonly List<ObjectAdapter> objs;

    public PutObjOperation(ObjectPanel panel, List<ObjectAdapter> objs)
    {
        this.panel = panel;
        this.objs = objs;
    }
    
    public PutObjOperation(ObjectPanel panel, ObjectAdapter obj)
    {
        this.panel = panel;
        this.objs = new List<ObjectAdapter> {obj};
    }

    public override void Redo()
    {
        foreach (var obj in objs)
        {
            obj.ParentLine.RemoveObj(obj);
            panel.RemoveObj(obj);
            panel.Update();
        }
    }

    public override void Undo()
    {
        foreach (var obj in objs)
        {
            obj.ParentLine.AddObj(obj);
            panel.AddObj(obj);
            panel.Update();
        }
    }

    public override string GetInfo()
    {
        if (objs.Count <= 3)
        {
            return "放置" + string.Join(",", objs.Select(obj => obj.Value));
        }
        else
        {
            return "放置" + objs.Count + "个对象";
        }
    }
}