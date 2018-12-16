using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Input;

namespace NonDarkTheme.Utility
{
    ///<summary>開くフォルダ種別</summary>
    public enum FolderType
    {
        ///<summary>インストールフォルダ</summary>
        Assembly,
        ///<summary>設定ファイルフォルダ</summary>
        Settings,
    }

    ///<summary>共通で使うコマンド</summary>
    public static class CommandHelper
    {
        ///<summary>ブラウザで開くコマンド</summary>
        public static ICommand OpenBrowserCommand { get; }

        ///<summary>規定ファイラで開くコマンド</summary>
        public static ICommand OpenFolderCommand { get; }

        static CommandHelper()
        {
            OpenFolderCommand = new RelayCommand<FolderType>((f) => OpenFolderExecute(f));
            OpenBrowserCommand = new RelayCommand<string>((s) => OpenBrowserExecute(s), (s) => OpenBrowserCanExecute(s));

        }

        private static bool OpenBrowserCanExecute(string s)
            => !string.IsNullOrEmpty(s) || s.StartsWith("http://") || s.StartsWith("https://");

        private static void OpenBrowserExecute(string s)
        {
            if(!OpenBrowserCanExecute(s)) return;

            try
            {
                Process.Start(s);
            }
            catch { Debug.WriteLine($"OpenBrowserCommand error"); }
        }

        private static void OpenFolderExecute(FolderType type)
        {
            try
            {
                string path;
                switch(type)
                {
                    case FolderType.Assembly:
                        path = AppDomain.CurrentDomain.BaseDirectory;
                        break;

                    case FolderType.Settings:
                        var p = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                        var assembly = Assembly.GetExecutingAssembly();
                        var name = assembly.GetName().Name;
                        path = Path.Combine(p, name);
                        break;

                    default:
                        return;
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true,
                    Verb = "open",
                });
            }
            catch { Debug.WriteLine($"OpenFolderCommand error"); }
        }
    }
}
