using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ScreenToGif.Util.Converters
{
    /// <summary>
    /// Converts the System.Drawing.Color to a WPF Brush.
    /// </summary>
    public class ColorToBrush : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = value as Color?;

            if (!color.HasValue)
                return DependencyProperty.UnsetValue;

            var brush = new SolidColorBrush(color.Value);

            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var brush = value as SolidColorBrush;

            if (brush == null)
                return DependencyProperty.UnsetValue;

            return brush.Color;
        }
    }
}
