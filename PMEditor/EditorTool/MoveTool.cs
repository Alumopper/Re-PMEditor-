using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using PMEditor.Controls;
using PMEditor.Operation;
using PMEditor.Util;

namespace PMEditor.EditorTool;

public class MoveTool: AbstractTool
{
    public override EditorToolType ToolType => EditorToolType.Move;

    public event Action<List<ObjectRectangle>>? OnMoveEnd;
    
    //选中note。若点击到面板，说明没有点击到任何note，此时清空选中的note
    public override void OnMouseClick(ObjectPanel target, ToolClickArgs e)
    {
        target.UpdateSelectedObj(new List<ObjectRectangle>());
    }

    public override void OnMouseDrag(ObjectPanel target, ToolDragArgs e)
    {
        //拖动note
        foreach (var obj in e.SelectedObjs)
        {
            var x = obj.BeforeMovePos.X + e.AlignedDeltaPos.X;
            var y = obj.BeforeMovePos.Y + e.AlignedDeltaPos.Y;
            if (x < 0) x = 0;
            if (x > target.ActualWidth - obj.Width) x = target.ActualWidth - obj.Width;
            if(obj.StartTime + e.DeltaTime < 0) y = target.GetTopYFromTime(0) - obj.Height;
            //移动note
            Canvas.SetLeft(obj, x);
            Canvas.SetTop(obj, y);
        }
    }

    public override void OnMouseDragEnd(ObjectPanel target, ToolDragArgs e)
    {
        List<(ObjectAdapter, double, int, double, int)> objs = new();
        //确定obj属性
        foreach (var obj in e.SelectedObjs)
        {
            var qwq =(obj.Data, obj.StartTime, obj.Rail, obj.StartTime, obj.Rail);
            obj.Data.StartTime += e.DeltaTime;
            if(obj.Data.StartTime < 0) obj.Data.StartTime = 0;
            obj.Data.Rail += (int)(e.AlignedDeltaPos.X / target.ActualWidth * 9);
            if(obj.Data.Rail < 0) obj.Data.Rail = 0;
            if(obj.Data.Rail > 8) obj.Data.Rail = 8;
            if (obj.StartTime < EditorWindow.Instance.playerTime)
            {
                obj.Data.IsJudged = true;
            }
            qwq.Item4 = obj.Data.StartTime;
            qwq.Item5 = obj.Data.Rail;
            objs.Add(qwq);
            obj.BeforeMovePos = new Point(Canvas.GetLeft(obj), Canvas.GetTop(obj));
        }
        OperationManager.AddOperation(new MoveObjOperation(target, objs));
        OnMoveEnd?.Invoke(e.SelectedObjs);
    }

    public override void OnMouseMove(ObjectPanel target, ToolMoveArgs e) { }

    public override void OnMouseWheel(ObjectPanel target, ToolWheelArgs e) { }
}