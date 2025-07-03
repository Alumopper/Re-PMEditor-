using System;
using System.Collections.Generic;
using System.Windows.Controls;
using PMEditor.Controls;

namespace PMEditor.EditorTool;

public class MoveTool: AbstractTool
{

    public event Action<List<ObjectRectangle>>? OnMoveEnd;
    
    //选中note。若点击到面板，说明没有点击到任何note，此时清空选中的note
    public override void OnMouseClick(ObjectPanel target, ToolClickArgs e) { }

    public override void OnMouseDrag(ObjectPanel target, ToolDragArgs e)
    {
        //拖动note
        foreach (var obj in e.SelectedObjs)
        {                   
            //移动note
            Canvas.SetLeft(obj, Canvas.GetLeft(obj) + e.DeltaPos.X);
            Canvas.SetTop(obj, Canvas.GetTop(obj) + e.DeltaPos.Y);
        }
    }

    public override void OnMouseDragEnd(ObjectPanel target, ToolDragArgs e)
    {
        //确定obj属性
        foreach (var obj in e.SelectedObjs)
        {
            obj.Data.StartTime = e.StartInfo.Time;
            obj.Data.Rail = (int)(e.DeltaPos.X / target.ActualWidth * 9);
            if (obj.StartTime < EditorWindow.Instance.playerTime)
            {
                obj.Data.IsJudged = true;
            }
        }
        OnMoveEnd?.Invoke(e.SelectedObjs);
    }

    public override void OnMouseMove(ObjectPanel target, ToolMoveArgs e) { }

    public override void OnMouseWheel(ObjectPanel target, ToolWheelArgs e) { }
}