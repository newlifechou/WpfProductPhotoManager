﻿using System;
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

        public PhotoService()
        {
            outputFolder = Properties.Settings.Default.outputfolder;
            if (string.IsNullOrEmpty(outputFolder))
            {
                OutputFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            workListFileName = Path.Combine(Environment.CurrentDirectory, "worklist.json");
        }


        private string outputFilePrefix;

        public string OutputFilePrefix
        {
            get
            {
                return outputFilePrefix;
            }
        }
        public void SetOutputFilePrefix(string productid, string category, string keyword)
        {
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

        public void CopyPhoto(List<InputFile> files, IProgress<int> progress)
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

                File.Copy(item.OrignalFileName, outputFileName, true);
                item.IsCopied = true;
                item.NewFileName = outputFileName;
                item.NewDisplayFileName = Path.GetFileName(outputFileName);
                item.CopyError = "成功";

                item.IsUploaded = false;
                item.UploadError = "";

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
