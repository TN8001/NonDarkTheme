using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NonDarkTheme.Utility
{
    ///<summary>EnumとRadioButtonのバインド用</summary>
    public class Enum2BooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => parameter.Equals(value);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => (bool)value ? parameter : DependencyProperty.UnsetValue;
    }
}
