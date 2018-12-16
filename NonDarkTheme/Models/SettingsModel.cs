using System;
using System.Xml.Serialization;
using MagnificationWPF;
using NonDarkTheme.Utility;

namespace NonDarkTheme.Models
{
    ///<summary>反転タイプ</summary>
    public enum InvertType
    {
        ///<summary>何もしない</summary>
        Identity,
        /// <summary>ネガポジ反転</summary>
        Invert,
        /// <summary>色は変えずに白黒反転</summary>
        Invert2,
        /// <summary>ユーザー設定</summary>
        Custom,
    }

    ///<summary>その他オプションフラグ</summary>
    [Flags]
    public enum WindowOptions
    {
        ///<summary>無し</summary>
        None,
        ///<summary>マウス操作を透過</summary>
        Penetration = 1,
        ///<summary>カーソルを表示</summary>
        ShowMagnifiedCursor = 1 << 1,
        ///<summary>移動を同期しない</summary>
        PinnedView = 1 << 2,
    }

    ///<summary>ユーザー設定</summary>
    [XmlRoot("Settings")]
    public class SettingsModel : Observable
    {
        ///<summary>ウィンドウ設定</summary>
        public WindowModel Window { get; set; }

        ///<summary>カラーマトリクス</summary>
        public ColorMatrix Matrix { get; set; }

        ///<summary>最前面制御</summary>
        public ZOrder ZOrder { get => _ZOrder; set => Set(ref _ZOrder, value); }
        private ZOrder _ZOrder;

        ///<summary>反転タイプ</summary>
        public InvertType InvertType { get => _InvertType; set => Set(ref _InvertType, value); }
        private InvertType _InvertType;

        ///<summary>前面化するウィンドウタイトル</summary>
        public string Titles { get => _Titles; set => Set(ref _Titles, value); }
        private string _Titles;

        ///<summary>その他オプションフラグ</summary>
        public WindowOptions WindowOptions { get => _WindowOptions; set => Set(ref _WindowOptions, value); }
        private WindowOptions _WindowOptions;

        ///<summary>ツールバーを表示する</summary>
        public bool ShowToolBar { get => _ShowToolBar; set => Set(ref _ShowToolBar, value); }
        private bool _ShowToolBar;

        ///<summary>新しいバージョンがリリースされているかを確認する</summary>
        public bool UpdateCheck { get => _UpdateCheck; set => Set(ref _UpdateCheck, value); }
        private bool _UpdateCheck;


        public SettingsModel()
        {
            Window = new WindowModel();
            Matrix = new ColorMatrix();

            InvertType = InvertType.Identity;
            ZOrder = ZOrder.Nomal;
            WindowOptions = WindowOptions.None;
            Titles = "ニコニコ生放送";
            UpdateCheck = true;
            ShowToolBar = true;
        }
    }
}
