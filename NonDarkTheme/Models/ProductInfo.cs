using System.Collections.Generic;
using NonDarkTheme.Utility;

namespace NonDarkTheme.Models
{
    public class ProductInfo : ProductInfoBase
    {
        public ProductInfo()
        {
            DescriptionJp =
@"拡大鏡APIを使用してウィンドウの背面を色の反転や拡大縮小します。
常に最前面だけでなく、タイトルに特定文字を含むウィンドウのみに前面化することもできます。";

//作成目的はニコニコ生放送のプログラミング放送にて、ダークテーマが見づらく反転させて見たかったためですが、逆にライトテーマからダークテーマ風にすることも可能です。
//カスタムカラーマトリクスを使ってほかの目的に使えるかもしれません。";

            Url = "https://github.com/TN8001/NonDarkTheme/";
            UseLibs = new List<ProductUseLib>
            {
                new ProductUseLib
                {
                    Name ="MahApps.Metro",
                    Purpose ="見た目のカスタマイズ",
                    Url ="https://mahapps.com/",
                    Copyright ="Copyright (c) 2018 MahApps",
                },
                new ProductUseLib
                {
                    Name ="Material Design In XAML Toolkit",
                    Purpose ="見た目のカスタマイズと、アプリアイコン以外のアイコンはすべて",
                    Url ="http://materialdesigninxaml.net/",
                    Copyright ="Copyright (c) 2015 James Willock,  Mulholland Software and Contributors",
                },
            };
        }
    }
}
