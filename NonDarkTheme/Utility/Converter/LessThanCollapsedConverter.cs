using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NonDarkTheme.Utility
{
    ///<summary>指定幅以下で非表示化 GitHubボタンで使用</summary>
    public class LessThanCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var v = System.Convert.ToDouble(value);
            var p = double.Parse(parameter as string);

            return v > p ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
