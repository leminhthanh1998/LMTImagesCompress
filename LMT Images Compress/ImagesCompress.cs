using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMT_Images_Compress
{
    class ImagesCompress
    {
        #region Compress Images
        public static void CompressImage(string filePath, string savePath, int quality, int size, SupportedMimeType type)
        {
            Bitmap img = new Bitmap(filePath);
            CompressImage(
                filePath,
                img,
                savePath,
                quality,
                new Size((int)(img.Width * size / 100), (int)(img.Height * size / 100)),
                type
            );
            img.Dispose();
        }

        private static void CompressImage(string filePath, Bitmap img, string savePath, int quality, Size size,
            SupportedMimeType type, bool isOnlyForDisplay = true)
        {
            ImageCodecInfo imageCodecInfo;

            if (type == SupportedMimeType.JPEG)
                imageCodecInfo = Helper.GetEncoderInfo("image/jpeg");
            else if (type == SupportedMimeType.PNG)
                imageCodecInfo = Helper.GetEncoderInfo("image/png");
            else
                imageCodecInfo = Helper.GetEncoderInfoFromOriginalFile(filePath);

            EncoderParameters encoderParameters;

            if (img.Size.Height < size.Height || img.Size.Width < size.Width)
                size = img.Size;

            Bitmap imgCompressed = new Bitmap(img, size);



            foreach (var id in img.PropertyIdList)
            {
                imgCompressed.SetPropertyItem(img.GetPropertyItem(id));
            }


            SetImageComments(imgCompressed);
            img.Dispose();

            encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] =
                new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);


            imgCompressed.Save(Helper.FolderSaveFile(filePath, savePath), imageCodecInfo, encoderParameters);
            imgCompressed.Dispose();
        }
        #endregion


        /// <summary>
        /// Cmt images
        /// </summary>
        /// <param name="bmp"></param>
        public static void SetImageComments(Bitmap bmp)
        {
            string newVal = "Compressed by LMT Images Compress!";
            try
            {
                PropertyItem propItem;

                if (bmp.PropertyIdList.Contains(0x9286))
                    propItem = bmp.GetPropertyItem(0x9286);
                else
                {
                    propItem =
                        (PropertyItem)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(
                            typeof(PropertyItem));
                    propItem.Id = 0x9286;
                }


                propItem.Len = newVal.Length + 1;

                byte[] newValb = System.Text.Encoding.UTF8.GetBytes(newVal + "\0");

               

                propItem.Value = newValb;
                propItem.Type = 2;
                bmp.SetPropertyItem(propItem);
            }
            catch (Exception)
            {
            }
        }
    }
}
