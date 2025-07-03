using System.Windows.Media;

namespace PMEditor.Util
{
    public class EditorColors
    {
        public readonly static Color tapColor = Color.FromArgb(255, 91, 175, 179);
        public readonly static Color tapHighlightColor = Color.FromArgb(255, 44, 85, 87);
        public readonly static Color catchColor = Color.FromArgb(255, 201, 190, 67);
        public readonly static Color catchHighlightColor = Color.FromArgb(255, 107, 101, 36);
        public readonly static Color holdColor = Color.FromArgb(255, 37, 122, 181);
        public readonly static Color holdHighlightColor = Color.FromArgb(128, 25,82,122);
        public readonly static Color DefaultGroundCatchColor = Color.FromArgb(255, 56, 164, 255);
        public readonly static Color DefaultSkyCatchColor = Color.FromArgb(255, 255, 96, 60);

        public readonly static Color YEventColor = Color.FromArgb(255, 80, 242, 150);
        public readonly static Color YHighlightColor = Color.FromArgb(128, 80, 242, 150);
        public readonly static Color speedEventColor = Color.FromArgb(255, 172, 100, 255);
        public readonly static Color speedHighlightColor = Color.FromArgb(128, 172, 100, 255);
    
        public readonly static Color functionColor = Color.FromArgb(255, 255, 119, 10);
        public readonly static Color functionHighlightColor = Color.FromArgb(255, 255, 190, 19);

        public static Color GetEventColor(EventType eventType)
        {
            return eventType switch
            {
                EventType.Speed => speedEventColor,
                EventType.YPosition => YEventColor,
                _ => Colors.White,
            };
        }

        public static Color GetEventHighlightColor(EventType eventType)
        {
            return eventType switch
            {
                EventType.Speed => speedHighlightColor,
                EventType.YPosition => YHighlightColor,
                _ => Colors.White,
            };
        }

    }
}
