using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using MahApps.Metro.Controls;

namespace NonDarkTheme.Views
{
    public partial class SettingsWindow : MetroWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();

            ShowMinButton = false;
            ShowMaxRestoreButton = false;

            SourceInitialized += Settings_SourceInitialized;
            PreviewMouseDoubleClick += Settings_PreviewMouseDoubleClick;
            Closing += (s, e) =>
            {
                e.Cancel = true;
                Hide();
            };
        }

        private void EditMatrixButton_Click(object sender, RoutedEventArgs e) => tab_ColorMatrix.IsChecked = true;


        // そこまでこだわる必要もないが。気になってしまったので
        #region ダイアログをリサイズ可能かつ最小化最大化しない
        [DllImport("user32")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_STYLE = -16;
        private const int WS_MAXIMIZEBOX = 0x10000;
        private const int WS_MINIMIZEBOX = 0x20000;

        private Thumb windowTitleThumb;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            windowTitleThumb = GetTemplateChild("PART_WindowTitleThumb") as Thumb;
        }

        private void Settings_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(windowTitleThumb == null) return;

            var p = Mouse.GetPosition(windowTitleThumb);
            var isMouseOnTitlebar = p.Y <= TitlebarHeight && TitlebarHeight > 0;
            if(isMouseOnTitlebar) e.Handled = true;
        }

        private void Settings_SourceInitialized(object sender, EventArgs e)
        {
            SourceInitialized -= Settings_SourceInitialized;

            var hwnd = new WindowInteropHelper(this).Handle;
            var style = GetWindowLong(hwnd, GWL_STYLE);
            SetWindowLong(hwnd, GWL_STYLE, style & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX);
        }
        #endregion
    }
}
