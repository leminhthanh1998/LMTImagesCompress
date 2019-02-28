using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IWshRuntimeLibrary;

namespace LMT_Images_Compress
{
    class Helper
    {

        public static string exePath = System.AppDomain.CurrentDomain.BaseDirectory + "\\" + "LMT Images Compress.exe";
        public static string pathOptiPNG = AppDomain.CurrentDomain.BaseDirectory + "\\" + "optipng.exe";
        public static string shortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.SendTo) + "//LMT Images Compress.lnk";
        public static string FolderSaveFile(string filePath, string outPut)
        {
            return outPut+"\\"+Path.GetFileNameWithoutExtension(filePath)+"-compressed.jpg";
        }
        

        public static ImageCodecInfo GetEncoderInfoFromOriginalFile(String filePath)
        {
            string inputfileExt = "*" + Path.GetExtension(filePath).ToUpper();

            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].FilenameExtension.Contains(inputfileExt))
                    return encoders[j];
            }

            return null;
        }

        public static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        public static void CreateShortcut()
        {

            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);

            shortcut.Description = "Compress images";
            shortcut.IconLocation = System.AppDomain.CurrentDomain.BaseDirectory + "\\" + "Icon.ico";
            shortcut.TargetPath = exePath;
            shortcut.Save();
        }
    }
}
