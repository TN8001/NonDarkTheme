using System.Linq;
using MagnificationWPF;

namespace NonDarkTheme.Models
{
    // https://docs.microsoft.com/ja-jp/dotnet/framework/winforms/advanced/recoloring-images
    // この辺りを参考に 同一クラスではないが仕組みは同じ

    /// <summary>シリアライズ用マトリックスデータ</summary>
    public class ColorMatrix
    {
        // 例: 単純な反転
        //             R   G   B  A  ダミー
        //        R { -1,  0,  0, 0, 0},
        //        G {  0, -1,  0, 0, 0},
        //        B {  0,  0, -1, 0, 0},
        //        A {  0,  0,  0, 1, 0},
        // 平行移動 {  1,  1,  1, 1, 1}


        // シリアライズ用（ダミー列含まず）
        public string Row1 { get; set; }
        public string Row2 { get; set; }
        public string Row3 { get; set; }
        public string Row4 { get; set; }
        public string Row5 { get; set; }


        /// <summary>XmlSerializer用コンストラクタ</summary>
        public ColorMatrix() : this(MagColorEffect.Invert3.GetMatrix()) { }

        /// <summary>マトリックスデータで初期化 float[25]</summary>
        public ColorMatrix(float[] matrix)
        {
            Row1 = string.Join(",", matrix.Take(4));
            Row2 = string.Join(",", matrix.Skip(5).Take(4));
            Row3 = string.Join(",", matrix.Skip(10).Take(4));
            Row4 = string.Join(",", matrix.Skip(15).Take(4));
            Row5 = string.Join(",", matrix.Skip(20).Take(4));
        }

        /// <summary>マトリックスデータを取得 float[25]</summary>
        /// <returns></returns>
        public float[] GetMatrix()
        {
            var m = new float[25];
            m[24] = 1;

            try
            {
                var f = Row1.Split(',').Take(4).ToArray();
                for(var i = 0; i < f.Length; i++) m[i] = float.Parse(f[i]);

                f = Row2.Split(',').Take(4).ToArray();
                for(var i = 0; i < f.Length; i++) m[i + 5] = float.Parse(f[i]);

                f = Row3.Split(',').Take(4).ToArray();
                for(var i = 0; i < f.Length; i++) m[i + 10] = float.Parse(f[i]);

                f = Row4.Split(',').Take(4).ToArray();
                for(var i = 0; i < f.Length; i++) m[i + 15] = float.Parse(f[i]);

                f = Row5.Split(',').Take(4).ToArray();
                for(var i = 0; i < f.Length; i++) m[i + 20] = float.Parse(f[i]);
            }
            catch
            {
                // 設定ファイルに不正データがあった場合 反転なし状態で初期化
                // （パース失敗がわかりやすいので）
                m = new float[25];
                m[0] = 1;
                m[6] = 1;
                m[12] = 1;
                m[18] = 1;
                m[24] = 1;
            }

            return m;
        }
    }
}
