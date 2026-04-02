using Avalonia.Data.Converters;
using DietHelper.Common.Models.MealEntries;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DietHelper.Converters
{
    public class MealTypeToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is MealType mealType)
            {
                return mealType switch
                {
                    MealType.Breakfast => "Завтрак",
                    MealType.Lunch => "Обед",
                    MealType.Dinner => "Ужин",
                    MealType.Snack => "Перекус",
                    _ => "Другое"
                };
            }
            return string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
