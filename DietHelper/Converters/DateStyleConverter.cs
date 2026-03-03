using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DietHelper.Converters
{
    public class DateStyleConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count == 2 && values[0] is DateTime currentDate && values[1] is DateTime selectedDate)
                return currentDate == selectedDate ? "isSelected" : "";
            return "";
        }
    }
}
