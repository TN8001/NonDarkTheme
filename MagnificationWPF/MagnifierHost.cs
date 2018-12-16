using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace MagnificationWPF
{
    ///<summary>Magnification API による Magnifierウィンドウをホストします。</summary>
    public class MagnifierHost : HwndHost
    {
        #region NativeMethods
        [DllImport("Magnification")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool MagInitialize();

        [DllImport("Magnification")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool MagSetColorEffect(IntPtr hWnd, ref MagColorEffect pEffect);

        [DllImport("Magnification")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool MagSetWindowSource(IntPtr hWnd, Win32Rect rect);

        [DllImport("Magnification")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool MagSetWindowTransform(IntPtr hWnd, ref Transformation pTransform);

        [DllImport("Magnification")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool MagUninitialize();


        [DllImport("user32", CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateWindowEx(int dwExStyle, string lpClassName, string lpWindowName, WS dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lParam);

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyWindow(IntPtr hwnd);

        [DllImport("user32")]
        private static extern int GetWindowLong(IntPtr hWnd, GWL nIndex);

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hwnd, out Win32Rect lpRect);

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool InvalidateRect(IntPtr hWnd, IntPtr rect, bool erase);

        [DllImport("user32")]
        private static extern IntPtr SetCapture(IntPtr hWnd);

        [DllImport("user32")]
        private static extern int SetWindowLong(IntPtr hWnd, GWL nIndex, int dwLong);

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int flags);

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ReleaseCapture();
        #endregion
        #region NativeStructures 
        private readonly struct Win32Rect
        {
            public readonly int Left;
            public readonly int Top;
            public readonly int Right;
            public readonly int Bottom;

            public int X => Left;
            public int Y => Top;
            public int Width => Right - Left;
            public int Height => Bottom - Top;


            public Win32Rect(int left, int top, int right, int bottom)
                => (Left, Right, Top, Bottom) = (left, right, top, bottom);

            public override string ToString() => $"X:{X} Y:{Y} Width:{Width} Height:{Height}";
        }

        private readonly struct Transformation
        {
            private readonly float m00;
            private readonly float m10;
            private readonly float m20;
            private readonly float m01;
            private readonly float m11;
            private readonly float m21;
            private readonly float m02;
            private readonly float m12;
            private readonly float m22;

            public Transformation(float magnificationFactor) : this()
            {
                m00 = m11 = magnificationFactor;
                m22 = 1.0f;
            }
        }

        [Flags]
        private enum WS : uint
        {
            CHILD = 0x40000000,
            VISIBLE = 0x10000000,
            SHOWMAGNIFIEDCURSOR = 0x0001,
            SS_NOTIFY = 0x0100,
        }

        [Flags]
        private enum WM
        {
            MOUSEMOVE = 0x0200,
            LBUTTONDOWN = 0x0201,
            LBUTTONUP = 0x0202,
            MOUSEWHEEL = 0x020A,
        }

        [Flags]
        private enum MK
        {
            LBUTTON = 0x0001,
        }

        private enum GWL
        {
            STYLE = -16,
            EXSTYLE = -20,
        }
        #endregion

        #region DependencyProperty ColorEffect
        /// <summary>Viewのカラーエフェクトを取得または設定します。これは依存関係プロパティです。</summary>
        public MagColorEffect ColorEffect { get => (MagColorEffect)GetValue(ColorEffectProperty); set => SetValue(ColorEffectProperty, value); }
        public static readonly DependencyProperty ColorEffectProperty
            = DependencyProperty.Register(nameof(ColorEffect), typeof(MagColorEffect), typeof(MagnifierHost),
                new PropertyMetadata(MagColorEffect.Identity, OnColorEffectChanged));
        private static void OnColorEffectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((MagnifierHost)d).OnColorEffectChanged((MagColorEffect)e.NewValue);
        #endregion
        #region DependencyProperty Scale
        /// <summary>Viewの拡大率を取得または設定します。これは依存関係プロパティです。
        /// （10以上 100:等倍 200:倍に拡大 50:半分に縮小）</summary>
        public int Scale { get => (int)GetValue(ScaleProperty); set => SetValue(ScaleProperty, value); }
        public static readonly DependencyProperty ScaleProperty
            = DependencyProperty.Register(nameof(Scale), typeof(int), typeof(MagnifierHost),
                new PropertyMetadata(100, OnScaleChanged), ValidateScale);
        private static void OnScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((MagnifierHost)d).OnScaleChanged((int)e.OldValue, (int)e.NewValue);
        private static bool ValidateScale(object value) => 10 <= (int)value;
        #endregion
        #region DependencyProperty ShowMagnifiedCursor
        /// <summary>キャプチャ内にマウスカーソルを描画するかどうかを取得または設定します。これは依存関係プロパティです。</summary>
        public bool ShowMagnifiedCursor { get => (bool)GetValue(ShowMagnifiedCursorProperty); set => SetValue(ShowMagnifiedCursorProperty, value); }
        public static readonly DependencyProperty ShowMagnifiedCursorProperty
            = DependencyProperty.Register(nameof(ShowMagnifiedCursor), typeof(bool), typeof(MagnifierHost),
                new PropertyMetadata(false, OnShowMagnifiedCursorChanged));
        private static void OnShowMagnifiedCursorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((MagnifierHost)d).OnShowMagnifiedCursorChanged((bool)e.NewValue);
        #endregion
        #region DependencyProperty WindowX
        /// <summary>親ウィンドウの位置Xを取得または設定します。これは依存関係プロパティです。
        /// ウィンドウ移動とキャプチャ位置の同期に使用します。</summary>
        public double WindowX { get => (double)GetValue(WindowXProperty); set => SetValue(WindowXProperty, value); }
        public static readonly DependencyProperty WindowXProperty
            = DependencyProperty.Register(nameof(WindowX), typeof(double), typeof(MagnifierHost),
                new PropertyMetadata(0.0, OnWindowXChanged));
        private static void OnWindowXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((MagnifierHost)d).OnWindowXChanged((double)e.NewValue);
        #endregion
        #region DependencyProperty WindowY
        /// <summary>親ウィンドウの位置Yを取得または設定します。これは依存関係プロパティです。
        /// ウィンドウ移動とキャプチャ位置の同期に使用します。</summary>
        public double WindowY { get => (double)GetValue(WindowYProperty); set => SetValue(WindowYProperty, value); }
        public static readonly DependencyProperty WindowYProperty
            = DependencyProperty.Register(nameof(WindowY), typeof(double), typeof(MagnifierHost),
                new PropertyMetadata(0.0, OnWindowYChanged));
        private static void OnWindowYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((MagnifierHost)d).OnWindowYChanged((double)e.NewValue);
        #endregion
        #region DependencyProperty PinnedView
        /// <summary>WindowX WindowYの反映を止めるかどうかを取得または設定します。これは依存関係プロパティです。</summary>
        public bool PinnedView { get => (bool)GetValue(PinnedViewProperty); set => SetValue(PinnedViewProperty, value); }
        public static readonly DependencyProperty PinnedViewProperty
            = DependencyProperty.Register(nameof(PinnedView), typeof(bool), typeof(MagnifierHost),
                new PropertyMetadata(false));
        #endregion
        #region DependencyProperty IsDisabledGrayed
        /// <summary>無効時にグレイ化するどうかを取得または設定します。これは依存関係プロパティです。</summary>
        public bool IsDisabledGrayed { get => (bool)GetValue(IsDisabledGrayedProperty); set => SetValue(IsDisabledGrayedProperty, value); }
        public static readonly DependencyProperty IsDisabledGrayedProperty
            = DependencyProperty.Register(nameof(IsDisabledGrayed), typeof(bool), typeof(MagnifierHost),
                new PropertyMetadata(true));
        #endregion

        #region readonly DependencyProperty SourceRect
        /// <summary>キャプチャ範囲を取得します。これは依存関係プロパティです。</summary>
        public Rect SourceRect { get => (Rect)GetValue(SourceRectProperty); private set => SetValue(SourceRectPropertyKey, value); }
        private static readonly DependencyPropertyKey SourceRectPropertyKey
            = DependencyProperty.RegisterReadOnly(nameof(SourceRect), typeof(Rect), typeof(MagnifierHost),
                new PropertyMetadata(default(Rect)));
        public static readonly DependencyProperty SourceRectProperty = SourceRectPropertyKey.DependencyProperty;
        #endregion
        #region readonly DependencyProperty Fps
        /// <summary>1秒平均でのリフレッシュレートを取得します。これは依存関係プロパティです。</summary>
        public float Fps { get => (float)GetValue(FpsProperty); private set => SetValue(FpsPropertyKey, value); }
        private static readonly DependencyPropertyKey FpsPropertyKey
            = DependencyProperty.RegisterReadOnly(nameof(Fps), typeof(float), typeof(MagnifierHost),
                new PropertyMetadata(default(float)));
        public static readonly DependencyProperty FpsProperty = FpsPropertyKey.DependencyProperty;
        #endregion
        #region DependencyProperty IsClickThrough
        /// <summary>View内でマウス操作を透過するかどうかを取得または設定します。これは依存関係プロパティです。
        /// <para>【注意】透過するとスクロールやホイールでの操作は出来ません。
        /// ウィンドウにAllowsTransparency=trueが必要になります。</para></summary>
        public bool IsClickThrough { get => (bool)GetValue(IsClickThroughProperty); set => SetValue(IsClickThroughProperty, value); }
        public static readonly DependencyProperty IsClickThroughProperty
            = DependencyProperty.Register(nameof(IsClickThrough), typeof(bool), typeof(MagnifierHost),
                new PropertyMetadata(false, OnIsClickThroughChanged));
        private static void OnIsClickThroughChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((MagnifierHost)d).OnIsClickThroughChanged((bool)e.NewValue);
        #endregion


        ///<summary>Magnifierウィンドウハンドル</summary>
        protected IntPtr MagnifierHandle { get; private set; }

        ///<summary>親ウィンドウ</summary>
        protected Window ParentWindow { get; private set; }


        private static readonly int interval = 1000;    // FPS計測用 平均間隔(ms)

        private Point sourceLocation = new Point(0, 0); // 【スクリーン座標】描画範囲の左上座標
        private Point windowLocation = new Point(0, 0); // 【スクリーン座標】親ウィンドウの左上座標
        private Point scaleOrigin;                      // 【クライアント座標】拡縮時の中心座標
        private Point beforePos;                        // 【クライアント座標】ドラッグ中の前回座標
        private Rect oldBoundingBox;                    // 【親ウィンドウ座標】クライアント 位置サイズ
        private MagColorEffect colorEffectCore;         // 適用中のEffect
        private bool sourceDirty;                       // 描画範囲の更新が必要かどうか
        private bool isDraging;                         // ドラッグ中かどうか
        private int frameCount;                         // FPS計測用 フレーム数
        private int beforeTick;                         // FPS計測用 前回Tick


        static MagnifierHost() => IsEnabledProperty.OverrideMetadata(typeof(MagnifierHost), new UIPropertyMetadata(true, OnIsEnabledChanged));

        ///<summary>Scaleを設定します。</summary>
        /// <param name="scale">拡大率</param>
        /// <param name="origin">中心点</param>
        public void SetScale(int scale, Point origin)
        {
            scaleOrigin = origin;
            Scale = scale;
            scaleOrigin = new Point(0, 0);
        }

        ///<summary>Scaleを大きくし表示範囲を縮小します。</summary>
        /// <param name="delta">拡大率の増分</param>
        /// <param name="origin">中心点</param>
        public void ZoomIn(int delta, Point origin)
        {
            scaleOrigin = origin;
            Scale += delta;
            scaleOrigin = new Point(0, 0);
        }

        ///<summary>Scaleを小さくし表示範囲を拡大します。</summary>
        /// <param name="delta">拡大率の差分</param>
        /// <param name="origin">中心点</param>
        public void ZoomOut(int delta, Point origin)
        {
            if(Scale - delta < 10) return;

            scaleOrigin = origin;
            Scale -= delta;
            scaleOrigin = new Point(0, 0);
        }

        ///<summary>指定方向にスクロールします。</summary>
        /// <param name="offset">オフセット</param>
        public void Offset(Vector offset) => SourceOffset(offset.X, offset.Y);

        ///<summary>指定方向にスクロールします。</summary>
        /// <param name="dx">オフセットX</param>
        /// <param name="dy">オフセットY</param>
        public void Offset(double dx, double dy) => SourceOffset(dx, dy);

        ///<summary>ScaleとOffsetを元に戻します（コントロールの真裏がそのまま表示される状態）</summary>
        public void Reset()
        {
            Scale = 100;
            GetWindowRect(Handle, out var rect);
            sourceLocation = new Point(rect.X, rect.Y);
        }


        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            ParentWindow = Window.GetWindow(this);

            if(IsClickThrough && !ParentWindow.AllowsTransparency)
                throw new InvalidOperationException("ClickThrough require AllowsTransparency.");

            if(!MagInitialize()) throw new InvalidOperationException("MagInitialize fail.");

            // WndProcでマウスイベントを取るため1段重ねる（直接Magnifierだと取れなかった
            var hwndHost = CreateWindowEx(0, "Static", "", WS.CHILD | WS.VISIBLE | WS.SS_NOTIFY,
                 0, 0, 0, 0, hwndParent.Handle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            MagnifierHandle = CreateWindowEx(0, "Magnifier", "", WS.CHILD | WS.VISIBLE,
               0, 0, 0, 0, hwndHost, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            if(MagnifierHandle == IntPtr.Zero)
            {
                MagUninitialize();
                throw new InvalidOperationException("Create MagnifierWindow fail.");
            }

            CompositionTarget.Rendering += CompositionTarget_Rendering;
            beforeTick = Environment.TickCount;

            // DependencyPropertyの反映タイミングが早いのでセットし直し
            OnColorEffectChanged(ColorEffect);
            OnScaleChanged(100, Scale);
            OnShowMagnifiedCursorChanged(ShowMagnifiedCursor);
            OnIsClickThroughChanged(IsClickThrough);

            return new HandleRef(this, hwndHost);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
            DestroyWindow(hwnd.Handle);
            MagUninitialize();
        }

        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if(!IsEnabled) return base.WndProc(hwnd, msg, wParam, lParam, ref handled);

            // マウス操作のみ
            // キー入力も考慮はしたがキーボードフォーカスの良い表現方法がなく
            // 操作可能な時と不可な時で混乱しそう
            switch((WM)msg)
            {
                case WM.MOUSEWHEEL:
                    var delta = HIWORD(wParam);
                    scaleOrigin = GetPos(lParam);

                    if(0 < delta) Scale += 10;
                    else if(20 <= Scale) Scale -= 10;

                    scaleOrigin = new Point(0, 0);
                    break;

                case WM.LBUTTONDOWN:
                    beforePos = GetPos(lParam);
                    isDraging = true;
                    SetCapture(Handle);
                    break;

                case WM.MOUSEMOVE:
                    if(!isDraging) break;
                    if(MK.LBUTTON != (MK)wParam)
                    {
                        isDraging = false;
                        ReleaseCapture();
                        break;
                    }

                    var p = GetPos(lParam);
                    var v = beforePos - p;
                    SourceOffset(v.X, v.Y);
                    beforePos = p;
                    break;

                case WM.LBUTTONUP:
                    isDraging = false;
                    ReleaseCapture();
                    break;

                default:
                    return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
            }

            handled = true;
            return IntPtr.Zero;


            Point GetPos(IntPtr lp) => PointFromScreen(new Point(GET_X_LPARAM(lp), GET_Y_LPARAM(lp)));
            int GET_X_LPARAM(IntPtr lp) => unchecked((short)(long)lp);
            int GET_Y_LPARAM(IntPtr lp) => unchecked((short)((long)lp >> 16));
            int HIWORD(IntPtr wp) => unchecked((short)((long)wp >> 16));
        }

        // Windowの中でhwndHostの位置＆サイズが変化した場合に呼ばれる（Window自体の移動は関知しない）
        // それ以外になぜか１秒ごとに発火してる
        protected override void OnWindowPositionChanged(Rect rcBoundingBox)
        {
            base.OnWindowPositionChanged(rcBoundingBox);

            if(oldBoundingBox != rcBoundingBox)
            {
                SetWindowPos(MagnifierHandle, IntPtr.Zero, 0, 0, (int)rcBoundingBox.Width, (int)rcBoundingBox.Height, 0);

                var x = rcBoundingBox.Left - oldBoundingBox.Left;
                var y = rcBoundingBox.Top - oldBoundingBox.Top;
                SourceOffset(x, y);

                oldBoundingBox = rcBoundingBox;
            }
        }


        private void OnColorEffectChanged(in MagColorEffect effect)
        {
            if(MagnifierHandle == IntPtr.Zero) return;

            // disableがわかりやすい様にグレイスケール化
            if(!IsEnabled && IsDisabledGrayed) colorEffectCore = MagColorEffect.Gray;
            else colorEffectCore = effect;

            MagSetColorEffect(MagnifierHandle, ref colorEffectCore);
        }

        private void OnScaleChanged(int oldValue, int newValue)
        {
            if(MagnifierHandle == IntPtr.Zero) return;
            if(oldValue == newValue) return;

            var oldScale = (float)oldValue / 100;
            var newScale = (float)newValue / 100;

            var n = new Transformation(newScale);
            MagSetWindowTransform(MagnifierHandle, ref n);

            // 基準点はずらさずに拡縮するオフセット
            var vx = scaleOrigin.X * (newScale - oldScale) / (newScale * oldScale);
            var vy = scaleOrigin.Y * (newScale - oldScale) / (newScale * oldScale);
            SourceOffset(vx, vy);
        }

        private void OnShowMagnifiedCursorChanged(bool show)
        {
            if(MagnifierHandle == IntPtr.Zero) return;

            var style = GetWindowLong(MagnifierHandle, GWL.STYLE);
            if(show)
                style |= (int)WS.SHOWMAGNIFIEDCURSOR;
            else
                style &= ~(int)WS.SHOWMAGNIFIEDCURSOR;

            SetWindowLong(MagnifierHandle, GWL.STYLE, style);
        }

        private void OnIsClickThroughChanged(bool through)
        {
            if(ParentWindow == null) return;

            if(through) ParentWindow.Background = null;
            else ParentWindow.Background = Brushes.White;
        }

        private void OnWindowXChanged(double newValue)
        {
            if(double.IsNaN(newValue)) return;

            if(PinnedView) return;

            SourceOffset(newValue - windowLocation.X, 0);
            windowLocation.X = newValue;
        }

        private void OnWindowYChanged(double newValue)
        {
            if(double.IsNaN(newValue)) return;

            if(PinnedView) return;

            SourceOffset(0, newValue - windowLocation.Y);
            windowLocation.Y = newValue;
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((MagnifierHost)d).OnIsEnabledChanged((bool)e.NewValue);
        private void OnIsEnabledChanged(bool newValue)
        {
            // disableはユーザー操作は不可 プロパティ操作は可 
            // disable状態はグレイスケール化（が良いのかは要検討）

            // グレイスケール状態を反映or復帰
            OnColorEffectChanged(ColorEffect);
            InvalidateRect();
        }

        // 基本的には60fpsで呼ばれるようだが処理が混むとかなり落ち込む
        // あまり安定感はない が録画とかではないので気にしない
        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            InvalidateRect();

            frameCount++;
            var now = Environment.TickCount;
            var delta = now - beforeTick;
            if(delta < interval) return;

            Fps = frameCount / (float)delta * interval;
            frameCount = 0;
            beforeTick = now;
        }

        private void SourceOffset(double vx, double vy)
        {
            sourceLocation.Offset(vx, vy);
            sourceDirty = true;

            var w = (int)(ActualWidth / ((float)Scale / 100));
            var h = (int)(ActualHeight / ((float)Scale / 100));
            var rr = new Rect((int)sourceLocation.X, (int)sourceLocation.Y, w, h);
            SourceRect = rr;
        }

        private void InvalidateRect()
        {
            // 描画範囲の更新が多いとちらついて目障りなので 描画時に反映する
            if(sourceDirty)
            {
                sourceDirty = false;

                // w hは入れなくてもいいみたい
                var r = new Win32Rect((int)sourceLocation.X, (int)sourceLocation.Y, 0, 0);
                MagSetWindowSource(MagnifierHandle, r);
            }

            InvalidateRect(MagnifierHandle, IntPtr.Zero, true);
        }
    }
}
