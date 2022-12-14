using FluentFTP;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfProductPhotoManager.Models;

namespace WpfProductPhotoManager.Services
{
    public class FTPService
    {
        /*
            上传思路
        下载服务器文件夹列表缓存
        对比当前工作列表和服务器列表缓存，
        如果是跳过模式，则上传时候相同的文件跳过
        如果是覆盖模式，则上传的时候覆盖
         */


        public FTPService()
        {
            OverrideMode = false;
            serverAddress = Properties.Settings.Default.serverAddress;
            username = Properties.Settings.Default.username;
            password = Properties.Settings.Default.password;
            serverFolder = Properties.Settings.Default.serverFolder;
            outputFolder = System.IO.Path.Combine(Properties.Settings.Default.outputfolder, "download");
            logger = LogManager.GetCurrentClassLogger();
        }

        private ILogger logger;

        private string serverAddress;
        private string username;
        private string password;
        private string serverFolder;
        private string outputFolder;

        public string SeverFolder
        {
            get { return serverFolder; }
            set { serverFolder = value; }
        }


        public bool OverrideMode { get; set; }

        public bool CheckState(List<InputFile> inputFiles)
        {
            return inputFiles.Count(i => i.IsUploaded) > 0;
        }

        public void UploadFiles(List<InputFile> inputFiles, IProgress<int> progress)
        {
            if (inputFiles == null || inputFiles.Count == 0)
                return;
            var client = new FtpClient(serverAddress, username, password);
            client.Encoding = Encoding.Default;
            client.AutoConnect();

            if (!client.DirectoryExists(serverFolder))
            {
                client.CreateDirectory(serverFolder);
            }
            int total = inputFiles.Count;
            int current = 0;
            foreach (var item in inputFiles)
            {

                if (string.IsNullOrEmpty(item.NewFileName))
                {
                    item.IsUploaded = false;
                    item.UploadError = "要上传图片不存在，请先做复制操作";
                    continue;
                }

                string remoteFilePath = $"{serverFolder}/{item.NewDisplayFileName}";

                FtpRemoteExists remoteMode = OverrideMode ? FtpRemoteExists.Overwrite : FtpRemoteExists.Skip;
                string msg = "上传成功";
                if (client.FileExists(remoteFilePath))
                {
                    if (OverrideMode)
                    {
                        msg = "文件已存在，已覆盖";
                    }
                    else
                    {
                        msg = "文件已存在，已跳过";
                    }
                }
                client.UploadFile(item.NewFileName, remoteFilePath, remoteMode);
                item.IsUploaded = true;
                item.UploadError = msg;

                logger.Info($"上传了 {item.NewDisplayFileName}");

                current++;
                progress.Report(current * 100 / total);
            }
            client.Disconnect();
        }


        public List<string> ListFiles(string search_prefix)
        {
            if (string.IsNullOrEmpty(search_prefix))
                return null;

            var client = new FtpClient(serverAddress, username, password);
            client.Encoding = Encoding.Default;
            client.AutoConnect();
            if (!client.DirectoryExists(serverFolder))
            {
                return null;
            }

            string remoteFilePath = $"{serverFolder}";
            var queryResult = client.GetNameListing(remoteFilePath);
            string searchString = search_prefix;
            var result = queryResult.Where(i => i.StartsWith(searchString))
                .Select(i => System.IO.Path.GetFileName(i))
                .OrderBy(i => i)
                .ToList();
            client.Disconnect();
            return result;
        }

        public void DownloadAllFiles(List<string> filenames, IProgress<int> progress)
        {
            if (filenames == null || filenames.Count == 0)
                return;
            var client = new FtpClient(serverAddress, username, password);
            client.Encoding = Encoding.Default;
            client.AutoConnect();
            if (!client.DirectoryExists(serverFolder))
            {
                return;
            }

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            int total = filenames.Count;
            int current = 0;
            foreach (var item in filenames)
            {
                string remoteFilePath = $"{serverFolder}/{item}";
                string localFilePath = $"{outputFolder}\\{item}";
                client.DownloadFile(localFilePath, remoteFilePath, FtpLocalExists.Overwrite);

                logger.Info($"下载了 {item}");

                current++;
                progress.Report(current * 100 / total);
            }

            client.Disconnect();
        }

    }
}
