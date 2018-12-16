using System;
using System.Collections.Generic;
using System.Reflection;

namespace NonDarkTheme.Utility
{
    ///<summary>使用ライブラリ</summary>
    public class ProductUseLib
    {
        ///<summary>名前</summary>
        public string Name { get; set; }
        ///<summary>使用目的</summary>
        public string Purpose { get; set; }
        ///<summary>プロジェクトページ</summary>
        public string Url { get; set; }
        ///<summary>著作権情報</summary>
        public string Copyright { get; set; }
    }

    ///<summary>アプリ情報</summary>
    public class ProductInfoBase
    {
        ///<summary>アセンブリ名</summary>
        public string Name { get; }
        ///<summary>アセンブリバージョン</summary>
        public Version Version { get; }
        ///<summary>アセンブリ説明</summary>
        public string Description { get; }
        ///<summary>アセンブリ著作権</summary>
        public string Copyright { get; }

        ///<summary>使用ライブラリ</summary>
        public List<ProductUseLib> UseLibs { get; set; }
        ///<summary>簡単な説明</summary>
        public string DescriptionJp { get; set; }
        ///<summary>プロジェクトページ</summary>
        public string Url { get; set; }

        public ProductInfoBase()
        {
            var assembly = Assembly.GetExecutingAssembly();
            Name = assembly.GetName().Name;
            var version = assembly.GetName().Version;
            Version = new Version(version.Major, version.Minor, version.Build);
            Description = Get<AssemblyDescriptionAttribute>(assembly).Description;
            Copyright = Get<AssemblyCopyrightAttribute>(assembly).Copyright;

            T Get<T>(Assembly a) where T : Attribute
                => (T)Attribute.GetCustomAttribute(a, typeof(T));
        }
    }
}
