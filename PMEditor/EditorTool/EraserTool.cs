using System;
using PMEditor.Controls;

namespace PMEditor.EditorTool;

public class EraserTool: AbstractTool
{
    
    public event Action<ObjectRectangle>? OnErase;
    
    public override void OnRectangleLeftClick(ObjectRectangle rect)
    {
        //删除note
        rect.ParentLine.RemoveObj(rect);
        rect.ParentPanel.ObjectRectangles.Remove(rect);
        rect.ParentPanel.ObjCanvas.Children.Remove(rect);
        OnErase?.Invoke(rect);
    }

    public override void OnRectangleDragOver(ObjectRectangle rect)
    {
        //删除note
        rect.ParentLine.RemoveObj(rect);
        rect.ParentPanel.ObjectRectangles.Remove(rect);
        rect.ParentPanel.ObjCanvas.Children.Remove(rect);
        OnErase?.Invoke(rect);
    }
}