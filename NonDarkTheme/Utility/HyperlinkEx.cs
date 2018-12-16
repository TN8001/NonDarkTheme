using System;
using System.Diagnostics;
using System.Windows.Documents;

namespace NonDarkTheme.Utility
{
    /// <summary>クリックで規定動作の起動とツールチップ設定</summary>
    public class HyperlinkEx : Hyperlink
    {
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            ToolTip = NavigateUri?.AbsoluteUri;
        }

        protected override void OnClick()
        {
            base.OnClick();

            if(NavigateUri?.AbsoluteUri == null) return;

            try
            {
                Process.Start(NavigateUri.AbsoluteUri);
            }
            catch { /* NOP */ }
        }
    }
}
