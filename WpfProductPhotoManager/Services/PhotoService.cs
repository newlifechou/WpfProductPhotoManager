using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfProductPhotoManager.Models;
using WpfProductPhotoManager.PMS;

namespace WpfProductPhotoManager.Services
{
    public class PhotoService
    {
        private string outputFolder;
        public string OutputFolder
        {
            get
            {
                return outputFolder;
            }
            set
            {
                outputFolder = value;
                Properties.Settings.Default.outputfolder = value;
                Properties.Settings.Default.Save();
            }
        }

        private string workListFileName;

        private ILogger logger;
        public PhotoService()
        {
            outputFolder = Properties.Settings.Default.outputfolder;
            if (string.IsNullOrEmpty(outputFolder))
            {
                OutputFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            workListFileName = Path.Combine(Environment.CurrentDirectory, "worklist.json");

            logger = LogManager.GetCurrentClassLogger();
        }


        private string outputFilePrefix;

        public string OutputFilePrefix
        {
            get
            {
                return outputFilePrefix;
            }
        }

        public string CurrentProductID { get; set; }
        public string CurrentCategory { get; set; }
        public string CurrentKeyword { get; set; }

        public void SetOutputFilePrefix(string productid, string category, string keyword)
        {
            CurrentProductID = productid;
            CurrentCategory = category;
            CurrentKeyword = keyword ?? "";

            outputFilePrefix = $"{productid}_{category}";
            if (!string.IsNullOrEmpty(keyword))
            {
                outputFilePrefix += "_" + keyword;
            }
        }

        public bool CheckState(List<InputFile> files)
        {
            return files.Count(i => i.IsCopied) > 0;
        }

        public bool ExistSameLocalFiles()
        {
            if (!Directory.Exists(outputFolder))
            {
                return true;
            }

            return Directory.GetFiles(outputFolder, $"{outputFilePrefix}*").Length > 0;
        }

        public void CopyPhoto(List<InputFile> files, IProgress<int> progress, bool addBottomText = false)
        {
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }
            int count = 1;

            int total = files.Count;
            int current = 0;
            foreach (var item in files)
            {
                var outputFileName = "";

                do
                {
                    string filename = $"{outputFilePrefix}_{count}.jpg";
                    outputFileName = Path.Combine(outputFolder, filename);
                    count++;
                } while (File.Exists(outputFileName));

                if (!File.Exists(item.OrignalFileName))
                {
                    item.IsCopied = false;
                    item.CopyError = "文件不存在";
                    continue;
                }

                if (addBottomText)
                {
                    CopyWithBottomMarginContent(item.OrignalFileName, outputFileName, CurrentProductID);
                }
                else
                {
                    File.Copy(item.OrignalFileName, outputFileName, true);
                }

                item.IsCopied = true;
                item.NewFileName = outputFileName;
                item.NewDisplayFileName = Path.GetFileName(outputFileName);
                item.CopyError = "成功";

                item.IsUploaded = false;
                item.UploadError = "";

                logger.Info($"{item.DisplayFileName} 复制处理为 {item.NewDisplayFileName}");

                current++;
                progress.Report(current * 100 / total);
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

        /// <summary>
        /// 添加底部黑色文字部分
        /// </summary>
        /// <param name="inputImage"></param>
        /// <param name="productid"></param>
        private void CopyWithBottomMarginContent(string inputImage, string outputImage, string productid)
        {
            if (File.Exists(inputImage))
            {
                string text = productid;
                using (var s = new RecordTestServiceClient())
                {
                    var test = s.GetRecordTestByProductID(productid).FirstOrDefault();
                    if (test != null)
                    {
                        text += "-" + test.Composition ?? "";
                    }
                }
                Image image = Image.FromFile(inputImage);
                Graphics gp = Graphics.FromImage(image);
                int height = image.Height;
                int width = image.Width;
                double ratio = 1;
                //区分大小分辨率照片
                if (width < 3000)
                {
                    ratio = 1;
                }
                else if (width < 6000)
                {
                    ratio = 2;
                }
                else
                {
                    ratio = 3;
                }
                Font font = new Font("黑体", (float)(6.66 * ratio));
                gp.FillRectangle(Brushes.Black, 0, 0, width, (int)(33.33 * ratio));
                gp.DrawString(text, font, Brushes.White, 5, (int)(10 * ratio));
                image.Save(outputImage);
                image.Dispose();
                gp.Dispose();

            }
        }

        public void SaveWorkList(string json)
        {
            if (string.IsNullOrEmpty(json))
                return;
            File.WriteAllText(workListFileName, json);
        }

        public string LoadWorkList()
        {
            if (!File.Exists(workListFileName))
                return "";
            return File.ReadAllText(workListFileName);
        }
    }
}
