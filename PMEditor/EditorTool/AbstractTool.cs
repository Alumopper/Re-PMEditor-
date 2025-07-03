using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using PMEditor.Controls;

namespace PMEditor.EditorTool;

public abstract class AbstractTool
{
    public bool Active { get; set; } = true;
    public virtual void OnMouseClick(ObjectPanel target, ToolClickArgs e){}
    public virtual void OnMouseDrag(ObjectPanel target, ToolDragArgs e){}
    public virtual void OnMouseDragEnd(ObjectPanel target, ToolDragArgs e){}
    public virtual void OnMouseMove(ObjectPanel target, ToolMoveArgs e){}
    public virtual void OnMouseWheel(ObjectPanel target, ToolWheelArgs e){}

    public virtual void OnRectangleLeftClick(ObjectRectangle rect)
    {
        TrackEditorPage.Instance!.CurrPanel.UpdateSelectedObj(rect);
    }

    public virtual void OnRectangleDragOver(ObjectRectangle rect) { }
}

public class ToolClickArgs
{
    public MousePosInfo Info { get; }
    
    public ToolClickArgs(MousePosInfo info)
    {
        Info = info;
    }
}

public class ToolDragArgs
{
    public MousePosInfo StartInfo { get; }
    public MousePosInfo EndInfo { get; }
    public Point DeltaPos => new Point(EndInfo.Pos.X - StartInfo.Pos.X, EndInfo.Pos.Y - StartInfo.Pos.Y);
    public double DeltaTime => EndInfo.Time - StartInfo.Time;
    
    public List<ObjectRectangle> SelectedObjs { get; private set; }

    public ToolDragArgs(MousePosInfo startInfo, MousePosInfo endInfo, List<ObjectRectangle> selectedObjs)
    {
        StartInfo = startInfo;
        EndInfo = endInfo;
        SelectedObjs = selectedObjs; 
    }
}

public class ToolMoveArgs
{
    public MousePosInfo Info { get; }
    
    public ToolMoveArgs(MousePosInfo info)
    {
        Info = info;
    }
}

public class ToolWheelArgs
{
    
    public MousePosInfo Info { get; }
    public int Delta { get; }
    public double Num => Delta / 60.0;
    public List<ObjectRectangle> SelectedObjs { get; private set; }

    public ToolWheelArgs(MousePosInfo info, int delta, List<ObjectRectangle> selectedObjs)
    {
        Info = info;
        Delta = delta;
        SelectedObjs = selectedObjs;
    }
}

public record MousePosInfo(Point Pos, double Time, int Measure, int Beat, int Rail);