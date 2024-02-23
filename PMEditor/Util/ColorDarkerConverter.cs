using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace PMEditor.Util
{
    public class BrushDarkerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is SolidColorBrush brush)
            {
                var color = brush.Color;
                float factor = 0.2f; // Change this to make the color lighter or darker
                float red = (color.R / 255f) - factor;
                float green = (color.G / 255f) - factor;
                float blue = (color.B / 255f) - factor;

                // Ensure the values stay within the valid range
                red = Math.Min(1f, Math.Max(0f, red));
                green = Math.Min(1f, Math.Max(0f, green));
                blue = Math.Min(1f, Math.Max(0f, blue));

                return new SolidColorBrush(Color.FromScRgb(color.A / 255f, red, green, blue));
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
