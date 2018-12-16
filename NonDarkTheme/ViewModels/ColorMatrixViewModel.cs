using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NonDarkTheme.Models;

namespace NonDarkTheme.ViewModels
{
    // 雑 ColorMatrixを編集しやすくViewに公開
    public class ColorMatrixViewModel : IEnumerable<MatrixValueViewModel>
    {
        private List<MatrixValueViewModel> matrix = new List<MatrixValueViewModel>();

        public ColorMatrixViewModel(ColorMatrix colorMatrix)
            : this(colorMatrix?.GetMatrix() ?? throw new ArgumentNullException(nameof(colorMatrix))) { }
        private ColorMatrixViewModel(float[] matrix)
        {
            for(var i = 0; i < 25; i++)
            {
                var f = new MatrixValueViewModel
                {
                    Value = matrix[i],
                    IsEditable = true,
                    Text = i + "",
                };

                if(i % 5 == 4)
                {
                    f.IsEditable = false;
                    f.Text = i + " fixed";
                }

                this.matrix.Add(f);
            }
        }

        public float[] GetMatrix() => matrix.Select(x => x.Value).ToArray();

        public IEnumerator<MatrixValueViewModel> GetEnumerator() => matrix.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
