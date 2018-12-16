using System;
using System.Windows;
using MahApps.Metro.Controls;
using NonDarkTheme.ViewModels;

namespace NonDarkTheme.Views
{
    public partial class MainWindow : MetroWindow
    {
        // MVVM原理主義者ではありません^^;
        private MainViewModel vm => DataContext as MainViewModel;
        private SettingsWindow settings;

        public MainWindow()
        {
            InitializeComponent();

            Closing += (s, e) => vm.SaveCommand.Execute();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            settings = new SettingsWindow { DataContext = vm, Owner = this, };
        }


        private void ShowSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            settings.Show();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            invertView.Offset(new Vector(10, 5));
        }
    }
}
