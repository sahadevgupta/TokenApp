using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TokenApp.Controls.Enums;

namespace TokenApp.Converters
{
    public class IconTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            return value is CustomIconType type &&
                   type != CustomIconType.None;
        }

        public object ConvertBack(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
