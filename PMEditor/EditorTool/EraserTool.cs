using System;
using System.Collections.Generic;
using PMEditor.Controls;
using PMEditor.Operation;
using PMEditor.Util;

namespace PMEditor.EditorTool;

public class EraserTool: AbstractTool
{
    public override EditorToolType ToolType => EditorToolType.Eraser;
    
    private readonly List<ObjectAdapter> deletedObjs = new();
    
    public event Action<ObjectRectangle>? OnErase;
    
    public override void OnRectangleLeftClick(ObjectRectangle rect)
    {
        //删除note
        rect.Data.ParentLine.RemoveObj(rect.Data.Value!);
        rect.ParentPanel.ObjectRectangles.Remove(rect);
        rect.ParentPanel.ObjCanvas.Children.Remove(rect);
        rect.ParentPanel.Objects.Remove(rect.Data);
        OperationManager.AddOperation(new RemoveObjOperation(rect.ParentPanel, rect.Data));
        OnErase?.Invoke(rect);
    }

    public override void OnRectangleDragOver(ObjectRectangle rect)
    {
        //删除note
        rect.Data.ParentLine.RemoveObj(rect.Data.Value!);
        rect.ParentPanel.ObjectRectangles.Remove(rect);
        rect.ParentPanel.ObjCanvas.Children.Remove(rect);
        rect.ParentPanel.Objects.Remove(rect.Data);
        deletedObjs.Add(rect.Data);
        OnErase?.Invoke(rect);
    }

    public override void OnMouseDragEnd(ObjectPanel target, ToolDragArgs e)
    {
        OperationManager.AddOperation(new RemoveObjOperation(target, deletedObjs));
        deletedObjs.Clear();
    }
}