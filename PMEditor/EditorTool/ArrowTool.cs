using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using PMEditor.Controls;
using PMEditor.Util;

namespace PMEditor.EditorTool;

public class ArrowTool: AbstractTool
{
    //选中note。若点击到面板，说明没有点击到任何note，此时清空选中的note
    public override void OnMouseClick(ObjectPanel target, ToolClickArgs e)
    {
        target.UpdateSelectedObj(new List<ObjectRectangle>());
    }

    //选择一个区域
    public override void OnMouseDrag(ObjectPanel target, ToolDragArgs e)
    {
        //更新选择框预览
        var left = Math.Min(e.StartInfo.Pos.X, e.EndInfo.Pos.X);
        var right = Math.Max(e.StartInfo.Pos.X, e.EndInfo.Pos.X);
        var bottom = Math.Max(e.StartInfo.Pos.Y, e.EndInfo.Pos.Y);
        var top = Math.Min(e.StartInfo.Pos.Y, e.EndInfo.Pos.Y);
        var width = right - left;
        var height = bottom - top;
        target.UpdateSelectingBorder(left, top, width, height);
    }

    //选中所有在这个区域内的note
    public override void OnMouseDragEnd(ObjectPanel target, ToolDragArgs e)
    {
        //框选note
        var notes = target.ObjectRectangles.Where(note =>
            note.Rail.IsBetween(e.StartInfo.Rail, e.EndInfo.Rail) &&
            Utils.HasOverlap(note.StartTime, note.StartTime + note.LengthTime, e.StartInfo.Time, e.EndInfo.Time))
            .ToList();
        target.UpdateSelectedObj(notes);
        target.UpdateSelectingBorder(false);
    }

    public override void OnMouseMove(ObjectPanel target, ToolMoveArgs e)
    {
        TrackEditorPage.Instance!.BeatDis.Content = $"拍数: {e.Info.Measure}:{e.Info.Beat}/{TrackEditorPage.Instance.DivideNum}";
    }

    public override void OnMouseWheel(ObjectPanel target, ToolWheelArgs e)
    {
        //更改fake catch高度
        if (Keyboard.IsKeyDown(Key.LeftAlt))
        {
            foreach (var fakeCatch in e.SelectedObjs.Where(note => note.Data.Value is FakeCatch)
                         .Select(note => (FakeCatch)note.Data.Value!))
            {
                fakeCatch.Height = Math.Clamp(fakeCatch.Height + e.Num / 50, 0, 1);
            }
            return;
        }
        TrackEditorPage.Instance!.BeatDis.Content = $"拍数: {e.Info.Measure}:{e.Info.Beat}/{TrackEditorPage.Instance.DivideNum}";
    }
}