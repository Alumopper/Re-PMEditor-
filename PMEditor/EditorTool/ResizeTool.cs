using System;
using System.Collections.Generic;
using System.Linq;
using PMEditor.Controls;

namespace PMEditor.EditorTool;

public class ResizeTool: AbstractTool
{
    public event Action<List<ObjectRectangle>>? OnResizeEnd; 
    
    public override void OnMouseClick(ObjectPanel target, ToolClickArgs e) { }

    public override void OnMouseDrag(ObjectPanel target, ToolDragArgs e)
    {
        //拖动note
        foreach (var obj in e.SelectedObjs.Where(note => note.IsResizable))
        {                   
            //更改note大小
            var y = Math.Abs(e.DeltaPos.Y);
            obj.Height = Math.Max(5, obj.Height + y);
        }
    }

    public override void OnMouseDragEnd(ObjectPanel target, ToolDragArgs e)
    {
        foreach (var obj in e.SelectedObjs.Where(note => note.IsResizable))
        {                   
            //更改note大小
            obj.Data.LengthTime = Math.Max(0, obj.LengthTime + e.DeltaTime);
        }
        OnResizeEnd?.Invoke(e.SelectedObjs);
    }

    public override void OnMouseMove(ObjectPanel target, ToolMoveArgs e) { }

    public override void OnMouseWheel(ObjectPanel target, ToolWheelArgs e) { }
}