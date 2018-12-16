using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace MagnificationWPF
{
    /// <summary>ウィンドウのZ-Order制御方法</summary>
    public enum ZOrder
    {
        /// <summary>何もしない</summary>
        Nomal,
        /// <summary>常に最前面</summary>
        Topmost,
        /// <summary>特定ウィンドウタイトルのみ前面</summary>
        SpecificTitle,
    }


    public class AutoZOrderMagView : MagnifierHost
    {
        #region NativeMethods
        [DllImport("user32")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32")]
        private static extern int GetWindowLong(IntPtr hWnd, GWL nIndex);

        [DllImport("user32")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, SWP flags);
        #endregion
        #region NativeStructures
        [Flags]
        private enum GWL
        {
            EXSTYLE = -20,
        }

        [Flags]
        private enum WSEX
        {
            TOPMOST = 8,
        }

        [Flags]
        private enum SWP
        {
            NOSIZE = 0x0001,
            NOMOVE = 0x0002,
            NOACTIVATE = 0x0010,
        }

        private static class HWND
        {
            public static readonly IntPtr NoTopMost = new IntPtr(-2);
            public static readonly IntPtr TopMost = new IntPtr(-1);
        }
        #endregion

        #region DependencyProperty ZOrder
        /// <summary>ウィンドウのZ-Order制御方法を取得または設定します。これは依存関係プロパティです。</summary>
        public ZOrder ZOrder { get => (ZOrder)GetValue(ZOrderProperty); set => SetValue(ZOrderProperty, value); }
        public static readonly DependencyProperty ZOrderProperty
            = DependencyProperty.Register(nameof(ZOrder), typeof(ZOrder), typeof(AutoZOrderMagView),
                new FrameworkPropertyMetadata(ZOrder.Nomal, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnZOrderChanged));
        private static void OnZOrderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((AutoZOrderMagView)d).OnZOrderChanged((ZOrder)e.NewValue);
        #endregion
        #region DependencyProperty Titles
        /// <summary>ZOrder.SpecificTitleの際のウィンドウタイトルに含まれる文字列を取得または設定します。これは依存関係プロパティです。
        /// カンマ区切りで複数指定可</summary>
        public string Titles { get => (string)GetValue(TitlesProperty); set => SetValue(TitlesProperty, value); }
        public static readonly DependencyProperty TitlesProperty
            = DependencyProperty.Register(nameof(Titles), typeof(string), typeof(AutoZOrderMagView),
                new PropertyMetadata(null));
        #endregion


        private DispatcherTimer timer; // タイトルチェックタイマ 100ms
        private IntPtr hwndParent;     // 親ウィンドウハンドル
        private IntPtr hwndForeground; // フォアグラウンドウィンドウハンドル
        private string oldTitle;       // 前回取得時のフォアグラウンドウィンドウタイトル
        private Window window;

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            var h = base.BuildWindowCore(hwndParent);
            window = Window.GetWindow(this);
            this.hwndParent = hwndParent.Handle;
            timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            timer.Tick += Timer_Tick;

            OnZOrderChanged(ZOrder);

            return h;
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            timer.Tick -= Timer_Tick;

            base.DestroyWindowCore(hwnd);
        }


        private void Timer_Tick(object sender, EventArgs e)
        {
            if(string.IsNullOrWhiteSpace(Titles)) return;

            // InvertViewを含むWindow（自分自身）を除外（自分の裏に回ろうとしたりしないように）
            // 子Window（ダイアログ等）は親の上になるのが通常動作なので 特に判定の邪魔にはならないはず
            // Ownerを設定しないような使い方をしたい場合は干渉するだろうがそういうケースは想定しない
            var hWnd = GetForegroundWindow();
            if(hWnd == hwndParent) return;

            var title = GetWindowText(hWnd);
            if(title == "") return;
            if(hwndForeground == hWnd && oldTitle == title) return;

            hwndForeground = hWnd;
            oldTitle = title;

            var contains = Titles.Split(',').Any(title.Contains);
            SetTopMost(contains);
            Debug.WriteLine($"タイトル:" + title);
        }

        private void OnZOrderChanged(ZOrder zOrder)
        {
            if(timer == null) return;

            timer.Stop();
            switch(zOrder)
            {
                case ZOrder.Topmost:
                    SetTopMost(true);
                    break;

                case ZOrder.SpecificTitle:
                    Timer_Tick(null, null);
                    timer.Start();
                    break;

                default:
                    NoTopMost();
                    break;
            }
        }

        private void SetTopMost(bool topMost)
        {
            if(topMost == IsTopMost()) return;

            if(topMost) TopMost();
            else InsertAfter();
        }

        private void TopMost() => SetWindowPos(HWND.TopMost);

        private void NoTopMost() => SetWindowPos(HWND.NoTopMost);

        private void InsertAfter()
        {
            SetWindowPos(HWND.NoTopMost);
            SetWindowPos(hwndForeground);
        }

        private void SetWindowPos(IntPtr hWndInsertAfter)
        {
            // TopMostにするにはActivateが必要
            if(hWndInsertAfter == HWND.TopMost) ParentWindow.Activate();

            var flags = SWP.NOMOVE | SWP.NOSIZE | SWP.NOACTIVATE;
            var b = SetWindowPos(hwndParent, hWndInsertAfter, 0, 0, 0, 0, flags);
            // 失敗する条件がわかりにくい
            if(!b) Debug.WriteLine($"SetWindowPos:fail {hWndInsertAfter}");

            // Activateを戻す
            if(hWndInsertAfter == HWND.TopMost) SetForegroundWindow(hwndForeground);
        }

        private bool IsTopMost()
        {
            var s = GetWindowLong(hwndParent, GWL.EXSTYLE);
            return (s & (int)WSEX.TOPMOST) != 0;
        }

        private string GetWindowText(IntPtr hWnd)
        {
            var len = GetWindowTextLength(hWnd);
            if(len <= 0) return "";

            var sb = new StringBuilder(len + 1);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }
    }
}
