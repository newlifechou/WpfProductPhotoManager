using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Drawing;
using WpfProductPhotoManager.Models;

namespace WpfProductPhotoManager.Services
{
    public static class ImageService
    {

        public static BitmapImage GetThumbnail(string filePath)
        {
            Image img = Image.FromFile(filePath);
            Image thumbnail = img.GetThumbnailImage(100, 80, () => true, IntPtr.Zero);
            MemoryStream ms = new MemoryStream();
            thumbnail.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            ms.Position = 0;
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.StreamSource = ms;
            bmp.EndInit();
            return bmp;
        }


        public static void GenerateThumbnails(List<InputFile> files)
        {
            string cacheFolder = System.IO.Path.Combine(Environment.CurrentDirectory, "caches");
            if(!Directory.Exists(cacheFolder))
            {
                Directory.CreateDirectory(cacheFolder);
            }

            foreach (var item in files)
            {
                string saveFileName = System.IO.Path.Combine(cacheFolder, item.DisplayFileName);
                item.CacheImageFile = saveFileName;
                if (File.Exists(saveFileName))
                {
                    continue;
                }
                Image img = Image.FromFile(item.OrignalFileName);
                Image thumbnail = img.GetThumbnailImage(100, 80, () => true, IntPtr.Zero);
                thumbnail.Save(saveFileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                img.Dispose();
                thumbnail.Dispose();
            }
        }

    }
}
