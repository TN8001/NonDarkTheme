using System;
using System.Runtime.InteropServices;

namespace MagnificationWPF
{
    // https://docs.microsoft.com/ja-jp/dotnet/api/system.drawing.imaging.colormatrix
    // この辺を参考に指定してください

    /// <summary>MagSetColorEffectに渡す構造体（取扱注意）</summary>
    public readonly struct MagColorEffect
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 25)]
        private readonly float[] transform;

        /// <summary>MagSetColorEffectに渡す構造体を初期化します。</summary>
        /// <param name="matrix">５x５の変換行列を1次元配列で設定</param>
        public MagColorEffect(float[] matrix) : this()
        {
            transform = new float[25];
            Array.Copy(matrix, 0, transform, 0, Math.Min(25, matrix.Length));
        }


        /// <summary>５x５の変換行列を1次元配列で取得します。</summary>
        public float[] GetMatrix() => (float[])transform.Clone();


        /// <summary>何もしないEffectを取得します。</summary>
        public static readonly MagColorEffect Identity = new MagColorEffect(new[]
        {
            1.0f, 0.0f, 0.0f, 0.0f, 0.0f,
            0.0f, 1.0f, 0.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 1.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 1.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 0.0f, 1.0f,
        });

        /// <summary>グレイスケールに変換するEffectを取得します。</summary>
        public static readonly MagColorEffect Gray = new MagColorEffect(new[]
        {
            0.3f, 0.3f, 0.3f, 0.0f, 0.0f,
            0.6f, 0.6f, 0.6f, 0.0f, 0.0f,
            0.1f, 0.1f, 0.1f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 1.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 0.0f, 1.0f,
        });

        /// <summary>ネガポジ反転するEffectを取得します。</summary>
        public static readonly MagColorEffect Invert = new MagColorEffect(new[]
        {
           -1.0f, 0.0f, 0.0f, 0.0f, 0.0f,
            0.0f,-1.0f, 0.0f, 0.0f, 0.0f,
            0.0f, 0.0f,-1.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 1.0f, 0.0f,
            1.0f, 1.0f, 1.0f, 0.0f, 1.0f,
        });

        /// <summary>色は変えずに白黒反転するEffectを取得します。</summary>
        public static readonly MagColorEffect Invert2 = new MagColorEffect(new[]
        {
            0.0f, -0.5f, -0.5f, 0.0f, 0.0f,
           -0.5f,  0.0f, -0.5f, 0.0f, 0.0f,
           -0.5f, -0.5f,  0.0f, 0.0f, 0.0f,
            0.0f,  0.0f,  0.0f, 1.0f, 0.0f,
            1.0f,  1.0f,  1.0f, 0.0f, 1.0f,
        });

        // 白飛びしすぎる感じ もうちょっとましな数値が欲しい
        /// <summary>vsをわりと白っぽくするEffectを取得します。</summary>
        public static readonly MagColorEffect Invert3 = new MagColorEffect(new[]
        {
            0.0f, -1.2f, -1.2f, 0.0f, 0.0f,
           -1.2f,  0.0f, -1.2f, 0.0f, 0.0f,
           -1.2f, -1.2f,  0.0f, 0.0f, 0.0f,
            0.0f,  0.0f,  0.0f, 0.0f, 0.0f,
            1.5f,  1.5f,  1.5f, 1.0f, 1.0f,
        });
    }
}
