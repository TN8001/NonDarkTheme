using System.Globalization;
using System.Windows.Controls;

namespace NonDarkTheme.Utility
{
    // ColorMatrixの入力値に使用
    /// <summary>Floatに制限するValidationRule</summary>
    public class FloatValueValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if(float.TryParse(value.ToString(), out var f))
                return ValidationResult.ValidResult;

            return new ValidationResult(false, "数値のみ");
        }
    }
}
