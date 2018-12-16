using System.Globalization;
using System.Windows.Controls;

namespace NonDarkTheme.Utility
{
    // 拡大率の入力値に使用
    /// <summary>自然数に制限するValidationRule</summary>
    public class NaturalNumberValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var s = value as string;
            if(string.IsNullOrWhiteSpace(s))
                return new ValidationResult(false, "１以上の整数");

            s = s.Replace("%", "");
            if(!int.TryParse(s, out var i))
                return new ValidationResult(false, "１以上の整数");

            if(i <= 0)
                return new ValidationResult(false, "１以上の整数");

            return ValidationResult.ValidResult;
        }
    }
}
