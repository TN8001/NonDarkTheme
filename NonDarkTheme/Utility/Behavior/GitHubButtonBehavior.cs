using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace NonDarkTheme.Utility
{
    // xamlがごちゃつくので添付ビヘイビアのほうがまだましかな？と
    ///<summary>雑 GitHubへのリンク＆バージョンアップ確認</summary>
    public class GitHubButtonBehavior
    {
        #region Attached DependencyProperty Url
        public static string GetUrl(DependencyObject d) => (string)d.GetValue(UrlProperty);
        public static void SetUrl(DependencyObject d, string value) => d.SetValue(UrlProperty, value);
        public static readonly DependencyProperty UrlProperty
            = DependencyProperty.RegisterAttached("Url", typeof(string), typeof(GitHubButtonBehavior),
                new PropertyMetadata(null, OnUrlChanged));
        private static void OnUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is Button button)
            {
                if(e.NewValue != null)
                {
                    button.Click += Button_Click;

                    ToolTipService.SetPlacement(button, PlacementMode.Mouse);
                    ToolTipService.SetShowDuration(button, 30000);
                }
                else
                {
                    button.Click -= Button_Click;
                }
            }
        }
        #endregion
        #region Attached DependencyProperty UpdateCheck
        public static bool GetUpdateCheck(DependencyObject d) => (bool)d.GetValue(UpdateCheckProperty);
        public static void SetUpdateCheck(DependencyObject d, bool value) => d.SetValue(UpdateCheckProperty, value);
        public static readonly DependencyProperty UpdateCheckProperty
            = DependencyProperty.RegisterAttached("UpdateCheck", typeof(bool), typeof(GitHubButtonBehavior),
                new PropertyMetadata(false, OnUpdateCheckChanged));
        private static void OnUpdateCheckChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is Button button)
            {
                if((bool)e.NewValue)
                {
                    button.Initialized += Button_Initialized;
                }
                else
                {
                    button.Initialized -= Button_Initialized;
                }
            }
        }
        #endregion

        private static void Button_Click(object sender, RoutedEventArgs e)
        {
            if(sender is Button button)
            {
                var url = GetUrl(button);
                if(!string.IsNullOrEmpty(button.ToolTip?.ToString()))
                    url += "/releases";

                CommandHelper.OpenBrowserCommand.Execute(url);
            }
        }

        private static void Button_Initialized(object sender, EventArgs e)
        {
            if(sender is Button button)
            {
                var url = GetUrl(button);
                if(url == null) return;

                var text = UpdateChecker.GetNewVersionString(url);

                if(text != "")
                {
                    button.ToolTip = "新しいバージョンがあります\n\n" + text;
                    var c = (Color)ColorConverter.ConvertFromString("#FFFFCC00");
                    button.Background = new SolidColorBrush(c);
                    button.Foreground = Brushes.Black;
                }
            }
        }
    }
}
