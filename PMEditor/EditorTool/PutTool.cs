using System;
using PMEditor.Controls;
using PMEditor.Util;

namespace PMEditor.EditorTool;

public class PutTool: AbstractTool
{
    public record PutArgs(double StartTime, double LengthTime, int Rail);
    
    public event Func<PutArgs, ObjectAdapter?>? OnPut;
    
    //放置一个物件，长度为0

    public override void OnMouseClick(ObjectPanel target, ToolClickArgs e)
    {
        var rail = (int)Math.Round(e.Info.Pos.X * 9 / target.ActualWidth);
        var re = OnPut?.Invoke(new PutArgs(e.Info.Time, 0, rail));
        if (re != null)
        {
            target.AddObj(re);
        }
    }

    public override void OnMouseDrag(ObjectPanel target, ToolDragArgs e)
    {
        target.UpdateObjPreview(e.StartInfo.Pos.X, e.EndInfo.Pos.Y,target.ActualWidth / 9, e.StartInfo.Pos.Y - e.EndInfo.Pos.Y);
    }

    public override void OnMouseDragEnd(ObjectPanel target, ToolDragArgs e)
    {
        //放置一个长条
        var re = OnPut?.Invoke(new PutArgs(e.StartInfo.Time, e.DeltaTime, e.StartInfo.Rail));
        if (re != null)
        {
            target.AddObj(re);
        }
    }

    public override void OnMouseMove(ObjectPanel target, ToolMoveArgs e)
    {
        target.UpdateObjPreview(e.Info.Pos.X, e.Info.Pos.Y - 10,target.ActualWidth / 9, 10);
    }

    public override void OnMouseWheel(ObjectPanel target, ToolWheelArgs e) { }
}