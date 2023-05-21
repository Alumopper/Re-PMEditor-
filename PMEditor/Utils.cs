using System.Windows;

namespace PMEditor
{
    public partial class TrackEditorPage
    {
        //坐标对齐转换
        public Point GetAlignedPoint(Point p)
        {
            //获取时间
            //获取鼠标位置，生成note位置预览
            var mousePos = p;
            //x坐标对齐
            mousePos.X = (int)(mousePos.X / (notePanel.ActualWidth / 9)) * (notePanel.ActualWidth / 9);
            //y坐标对齐
            //基准线相对坐标计算
            double fix = 0;
            if (window.player.Position.TotalSeconds / secondsPreDevideBeat % 1 > 0.001)
            {
                fix = window.player.Position.TotalSeconds / secondsPreDevideBeat % 1 * pixelPreDividedBeat;
            }
            mousePos.Y = ((int)((notePanel.ActualHeight - mousePos.Y + fix) / pixelPreDividedBeat)) * pixelPreDividedBeat - fix;
            return mousePos;
        }

        /// <summary>
        /// 坐标时间转换
        /// </summary>
        /// <param name="y">相对于底部的长度</param>
        /// <returns></returns>
        public double GetTimeFromY(double y)
        {
            return y / pixelPreDividedBeat * secondsPreDevideBeat + window.player.Position.TotalSeconds;
        }
    }
}
