using System;
using System.Windows.Media;
using PMEditor.Operation;
using System.Windows.Input;
using System.Text.Json.Serialization;

namespace PMEditor.Util
{
    public class FakeCatch : Note
    {
        protected double height = 0;  //高度。一个介于0和1之间的数值，0表示最下方，1表示最上方

        public double Height
        {
            get
            {
                return height;
            }
            set
            {
                height = value;
            }
        }

        [JsonConstructor]
        public FakeCatch(double height, int rail, int noteType, int fallType, bool isFake, double actualTime, double actualHoldTime): base(rail, noteType, fallType, isFake, actualTime, actualHoldTime)
        {
            this.height = height;
        }

        public FakeCatch(int rail, int fallType, double actualTime, double height) : base(rail, (int)PMEditor.NoteType.Catch, fallType, true, actualTime, 0)
        {
            this.height = height;
        }

        public static Color GetColor(double height)
        {
            //根据高度返回颜色
            return Color.FromArgb(
                (byte)Math.Clamp((GroundColor.A + (SkyColor.A - GroundColor.A) * height),0,255),
                (byte)Math.Clamp((GroundColor.R + (SkyColor.R - GroundColor.R) * height),0,255), 
                (byte)Math.Clamp((GroundColor.G + (SkyColor.G - GroundColor.G) * height),0,255), 
                (byte)Math.Clamp((GroundColor.B + (SkyColor.B - GroundColor.B) * height),0,255)
                );
        }

        public static Color GroundColor = EditorColors.DefaultGroundCatchColor;
        public static Color SkyColor = EditorColors.DefaultSkyCatchColor;

    }
}
