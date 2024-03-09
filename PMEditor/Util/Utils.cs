using System;
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
            Point mousePos = p;
            double width = notePanel.ActualWidth / 9;
            //x坐标对齐
            int rail = (int)(mousePos.X / width);
            //如果是Tap，额外需要对齐主线
            if (window.puttingTap)
            {
                if (rail == 0) { rail = 1; }
                if (rail == 2) { rail = 3; }
                if (rail == 4) { rail = 5; }
                if (rail == 6) { rail = 7; }
                if (rail == 8) { rail = 7; }
            }
            mousePos.X = rail * width;
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

        public double GetYFromTime(double time)
        {
            return (time - window.player.Position.TotalSeconds) / secondsPreDevideBeat * pixelPreDividedBeat;
        }
    }

    public class Utils
    {
        public static string ToNBTUUID(Guid uuid)
        {
            byte[] bytes = uuid.ToByteArray();
            int[] array = new int[4];
            array[0] = BitConverter.ToInt32(bytes, 0);
            array[1] = BitConverter.ToInt32(bytes, 4);
            array[2] = BitConverter.ToInt32(bytes, 8);
            array[3] = BitConverter.ToInt32(bytes, 12);

            return $"[I;{array[0]},{array[1]},{array[2]},{array[3]}]";
        }
    }

}