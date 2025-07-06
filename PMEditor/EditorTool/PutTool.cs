using System;
using System.Windows.Media;
using PMEditor.Controls;
using PMEditor.Operation;
using PMEditor.Util;

namespace PMEditor.EditorTool;

public class PutTool: AbstractTool
{
    public override EditorToolType ToolType => EditorToolType.Put;
    public record PutArgs(double StartTime, double LengthTime, int Rail);
    
    public event Func<PutArgs, ObjectAdapter?>? OnPut;
    
    //放置一个物件，长度为0

    public override void OnMouseClick(ObjectPanel target, ToolClickArgs e)
    {
        //手动计算放置的位置
        var rail = (int) (e.Info.OrgPos.X / target.ActualWidth * 9);
        var re = OnPut?.Invoke(new PutArgs(e.Info.Time, 0, rail));
        if (re != null)
        {
            target.AddObj(re);
            target.Update();
        }
    }

    public override void OnMouseDrag(ObjectPanel target, ToolDragArgs e)
    {
        var height = e.StartInfo.AlignedPos.Y - e.EndInfo.AlignedPos.Y;
        var color = EditorColors.holdColor;
        color.A = 100;
        TrackEditorPage.Instance!.ObjPreview.Fill = new SolidColorBrush(color);
        TrackEditorPage.Instance!.UpdateObjPreview(e.StartInfo.AlignedPos.X, e.EndInfo.AlignedPos.Y,target.ActualWidth / 9, height > 0 ? height : 10);
    }

    public override void OnMouseDragEnd(ObjectPanel target, ToolDragArgs e)
    {
        //手动计算放置的位置
        var rail = (int) (e.StartInfo.OrgPos.X / target.ActualWidth * 9);
        //放置一个长条
        var re = OnPut?.Invoke(new PutArgs(e.StartInfo.Time, e.DeltaTime, rail));
        if (re != null)
        {
            target.AddObj(re);
            target.Update();
            OperationManager.AddOperation(new PutObjOperation(target, re));
        }
    }

    public override void OnMouseMove(ObjectPanel target, ToolMoveArgs e)
    {
        var rail = (int) (e.Info.OrgPos.X / target.ActualWidth * 9);
        TrackEditorPage.Instance!.UpdateObjPreview(rail * target.ActualWidth / 9.0, e.Info.AlignedPos.Y - 10,
            target.ActualWidth / 9, 10);
    }

    public override void OnMouseWheel(ObjectPanel target, ToolWheelArgs e) { }
}