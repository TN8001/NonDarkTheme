using System;
using System.Globalization;
using System.Windows.Data;

namespace NonDarkTheme.Utility
{
    ///<summary>FlagsEnumのvalueにparameterが含まれるかどうか</summary>
    public class FlagsEnumValueConverter : IValueConverter
    {
        private int targetValue;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mask = (int)parameter;
            targetValue = (int)value;

            return (mask & targetValue) != 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            targetValue ^= (int)parameter;
            return Enum.Parse(targetType, targetValue.ToString());
        }
    }
}
