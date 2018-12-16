using NonDarkTheme.Utility;

namespace NonDarkTheme.ViewModels
{
    ///<summary>Matrixの１セル分のデータ 変更通知と拡張情報</summary>
    public class MatrixValueViewModel : Observable
    {
        ///<summary>セルの値</summary>
        public float Value { get => _Value; set => Set(ref _Value, value); }
        private float _Value;

        // xamlを楽にするため逆も用意
        ///<summary>編集禁止かどうか</summary>
        public bool IsReadOnly { get => !_IsIsEditable; set { Set(ref _IsIsEditable, !value); OnPropertyChanged(nameof(IsEditable)); } }
        ///<summary>編集可能かどうか</summary>
        public bool IsEditable { get => _IsIsEditable; set { Set(ref _IsIsEditable, value); OnPropertyChanged(nameof(IsReadOnly)); } }
        private bool _IsIsEditable;

        ///<summary>セルの説明</summary>
        public string Text { get => _Text; set => Set(ref _Text, value); }
        private string _Text;

        public override string ToString() => $"{Value} ({Text})";
    }
}
