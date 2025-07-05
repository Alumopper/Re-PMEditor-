using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using PMEditor.Controls;

namespace PMEditor.EditorTool;

public class ResizeTool: AbstractTool
{
    public override EditorToolType ToolType => EditorToolType.Resize;
    public event Action<List<ObjectRectangle>>? OnResizeEnd; 
    
    public override void OnMouseClick(ObjectPanel target, ToolClickArgs e) {
        target.UpdateSelectedObj(new List<ObjectRectangle>());
    }

    public override void OnMouseDrag(ObjectPanel target, ToolDragArgs e)
    {
        //拖动note
        foreach (var obj in e.SelectedObjs.Where(note => note.IsResizable))
        {                   
            //更改note大小
            var h = obj.BeforeResizeSize.Y - e.AlignedDeltaPos.Y;
            if (h < 0)
            {
                obj.Height = 5;
                Canvas.SetTop(obj, obj.BeforeMovePos.Y + obj.BeforeResizeSize.Y - 5);
            }
            else
            {
                obj.Height = Math.Max(5, h);  
                if (obj.LengthTime == 0)
                {
                    obj.Height -= 5;
                    Canvas.SetTop(obj, obj.BeforeMovePos.Y + e.AlignedDeltaPos.Y + 5);
                }
                else
                {
                    Canvas.SetTop(obj, obj.BeforeMovePos.Y + e.AlignedDeltaPos.Y);
                }
            }
        }
    }

    public override void OnMouseDragEnd(ObjectPanel target, ToolDragArgs e)
    {
        foreach (var obj in e.SelectedObjs.Where(note => note.IsResizable))
        {                   
            //更改note大小
            obj.Data.LengthTime = Math.Max(0, obj.LengthTime + e.DeltaTime);
            obj.BeforeMovePos = new Point(Canvas.GetLeft(obj), Canvas.GetTop(obj));
            obj.BeforeResizeSize = new Point(obj.Width, obj.Height);
        }
        OnResizeEnd?.Invoke(e.SelectedObjs);
    }

    public override void OnMouseMove(ObjectPanel target, ToolMoveArgs e) { }

    public override void OnMouseWheel(ObjectPanel target, ToolWheelArgs e) { }
}