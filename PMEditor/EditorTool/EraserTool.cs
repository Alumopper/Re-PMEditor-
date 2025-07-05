using System;
using PMEditor.Controls;

namespace PMEditor.EditorTool;

public class EraserTool: AbstractTool
{
    
    public event Action<ObjectRectangle>? OnErase;
    
    public override void OnRectangleLeftClick(ObjectRectangle rect)
    {
        //删除note
        rect.Data.ParentLine.RemoveObj(rect.Data.Value!);
        rect.ParentPanel.ObjectRectangles.Remove(rect);
        rect.ParentPanel.ObjCanvas.Children.Remove(rect);
        rect.ParentPanel.Objects.Remove(rect.Data);
        OnErase?.Invoke(rect);
    }

    public override void OnRectangleDragOver(ObjectRectangle rect)
    {
        //删除note
        rect.Data.ParentLine.RemoveObj(rect.Data.Value!);
        rect.ParentPanel.ObjectRectangles.Remove(rect);
        rect.ParentPanel.ObjCanvas.Children.Remove(rect);
        rect.ParentPanel.Objects.Remove(rect.Data);
        OnErase?.Invoke(rect);
    }
}