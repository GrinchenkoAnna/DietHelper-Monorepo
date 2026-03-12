using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DietHelper.Converters
{
    public class DataRangeConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count == 2 && values[0] is DateTime start && values[1] is DateTime end)
                return start.Date == end.Date
                    ? start.ToString("ddd dd.MM")
                    : $"{start.ToString("dd.MM")} - {end.ToString("dd.MM")}";
            return string.Empty;
        }
    }
}
