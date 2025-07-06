using System.Collections.Generic;
using System.Windows;
using PMEditor.Controls;
using PMEditor.Util;

namespace PMEditor.Operation;

public class MoveObjOperation: BaseOperation
{
    private ObjectPanel panel;
    
    private readonly List<(ObjectAdapter obj, double oldTime, int oldRail, double newTime, int newRail)> objs;

    public MoveObjOperation(ObjectPanel objectPanel, List<(ObjectAdapter, double, int, double, int)> objs)
    {
        this.panel = objectPanel;
        this.objs = objs;
    }


    public override void Redo()
    {
        foreach (var (obj, oldTime, oldRail, _, _) in objs)
        {
            obj.StartTime = oldTime;
            obj.Rail = oldRail;
            panel.Update();
        }
    }

    public override void Undo()
    {
        foreach (var (obj, _, _, newTime, newRail) in objs)
        {
            obj.StartTime = newTime;
            obj.Rail = newRail;
            panel.Update();
        }
    }

    public override string GetInfo()
    {
        return "移动" + objs.Count + "个对象";
    }
}