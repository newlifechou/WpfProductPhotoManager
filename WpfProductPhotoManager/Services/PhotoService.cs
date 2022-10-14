using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfProductPhotoManager.Models;

namespace WpfProductPhotoManager.Services
{
    public class PhotoService
    {
        private string outputFolder;
        public PhotoService()
        {
            outputFolder = Properties.Settings.Default.outputfolder;
            if (string.IsNullOrEmpty(outputFolder))
            {
                outputFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }

        }


        public string outputFilePrefix;

        public void SetOutputFilePrefix(string productid, string category, string keyword)
        {
            outputFilePrefix = $"{productid}_{category}";
            if (!string.IsNullOrEmpty(keyword))
            {
                outputFilePrefix += "_" + keyword;
            }
        }

        public void CopyPhoto(List<InputFile> files)
        {
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }
            int count = 1;
            foreach (var item in files)
            {
                var outputFileName = "";
                do
                {
                    string filename= $"{outputFilePrefix}_{count}.jpg";
                    outputFileName = Path.Combine(outputFolder,filename);
                    count++;
                } while (File.Exists(outputFileName));

                File.Copy(item.OrignalFileName, outputFileName, true);
            }
        }

        public string[] GetRelatedPhotos(string productid)
        {
            if (!Directory.Exists(outputFolder))
            {
                throw new DirectoryNotFoundException("目标文件夹不存在");
            }
            return Directory.GetFiles(outputFolder, $"{productid}*");
        }

    }
}
