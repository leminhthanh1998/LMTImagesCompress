using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LMT_Images_Compress
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string[] args;
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 0)
                args = e.Args;
            if(!File.Exists(Helper.shortcutPath))
                Helper.CreateShortcut();
            if (!File.Exists(Helper.pathOptiPNG))
            {
                MessageBox.Show("Không tìm thấy file optipng.exe, chương trình sẽ thoát!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                System.Windows.Application.Current.Shutdown();
            }
        }
    }
}
