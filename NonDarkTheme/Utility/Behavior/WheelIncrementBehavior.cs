using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NonDarkTheme.Utility
{
    ///<summary>超雑 ホイールで値の増減 Matrix TextBox用 float決め打ち</summary>
    public class WheelIncrementBehavior
    {
        #region Attached DependencyProperty Use
        public static bool GetUse(DependencyObject d) => (bool)d.GetValue(UseProperty);
        public static void SetUse(DependencyObject d, bool value) => d.SetValue(UseProperty, value);
        public static readonly DependencyProperty UseProperty
            = DependencyProperty.RegisterAttached("Use", typeof(bool), typeof(WheelIncrementBehavior),
                new PropertyMetadata(false, OnUseChanged));
        private static void OnUseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is TextBox textBox)
            {
                if((bool)e.NewValue)
                    textBox.MouseWheel += Element_MouseWheel;
                else
                    textBox.MouseWheel -= Element_MouseWheel;
            }
        }
        #endregion

        private static void Element_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if(sender is TextBox textBox)
            {
                if(float.TryParse(textBox.Text, out var f))
                {
                    if(e.Delta < 0)
                        textBox.Text = (f + 0.1f).ToString();
                    else
                        textBox.Text = (f - 0.1f).ToString();

                    // バリデーションと両立が難しい
                    // UpdateSourceTriggerはLostFocusだがホイールだけ利便性のため強制評価
                    var expression = textBox.GetBindingExpression(TextBox.TextProperty);
                    expression.UpdateSource();
                }
            }
        }
    }
}
