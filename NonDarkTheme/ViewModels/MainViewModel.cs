using System;
using System.ComponentModel;
using MagnificationWPF;
using NonDarkTheme.Models;
using NonDarkTheme.Utility;

namespace NonDarkTheme.ViewModels
{
    ///<summary>メインVM</summary>
    public class MainViewModel : Observable
    {
        ///<summary>保存する設定データ</summary>
        public SettingsModel Settings { get; }

        ///<summary>ユーザー定義カラーマトリクス</summary>
        public ColorMatrixViewModel Matrix { get; }

        ///<summary>アプリ情報</summary>
        public ProductInfo ProductInfo { get; } = new ProductInfo();

        ///<summary>選択中のカラーマトリクス</summary>
        public MagColorEffect Effect { get => _Effect; set => Set(ref _Effect, value); }
        private MagColorEffect _Effect;

        public string Message { get => _Message; set => Set(ref _Message, value); }
        private string _Message;

        ///<summary>設定の保存</summary>
        public RelayCommand SaveCommand { get; }


        public MainViewModel()
        {
            SaveCommand = new RelayCommand(() => Save());

            Settings = SettingsHelper.LoadOrDefault<SettingsModel>();
            Settings.PropertyChanged += Settings_PropertyChanged;

            Matrix = new ColorMatrixViewModel(Settings.Matrix);
            foreach(var cell in Matrix)
                cell.PropertyChanged += Cell_PropertyChanged;

            SetEffect();
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(Settings.InvertType)) SetEffect();
        }

        private void Cell_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(Settings.InvertType != InvertType.Custom) return;

            SetEffect();
        }

        private void SetEffect()
        {
            switch(Settings.InvertType)
            {
                case InvertType.Identity:
                    Effect = MagColorEffect.Identity;
                    break;
                case InvertType.Invert:
                    Effect = MagColorEffect.Invert;
                    break;
                case InvertType.Invert2:
                    Effect = MagColorEffect.Invert2;
                    break;
                case InvertType.Custom:
                    Effect = new MagColorEffect(Matrix.GetMatrix());
                    break;
                default:
                    break;
            }
        }

        private void Save()
        {
            Settings.Matrix = new ColorMatrix(Matrix.GetMatrix());
            SettingsHelper.Save(Settings);
        }
    }
}
