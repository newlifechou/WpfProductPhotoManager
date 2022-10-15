using FluentFTP;
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
            LoadFTPSettings();
        }

        private void LoadFTPSettings()
        {
            serverAddress = Properties.Settings.Default.serverAddress;
            username = Properties.Settings.Default.username;
            password = Properties.Settings.Default.password;
            serverFolder = Properties.Settings.Default.serverFolder;
        }

        private string serverAddress;
        private string username;
        private string password;
        private string serverFolder;

        public bool OverrideMode { get; set; }

        public bool CheckState(List<InputFile> inputFiles)
        {
            return inputFiles.Count(i => i.IsUploaded) > 0;
        }

        public void UploadFiles(List<InputFile> inputFiles)
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

            foreach (var item in inputFiles)
            {
                if (string.IsNullOrEmpty(item.NewFileName))
                {
                    item.IsUploaded = false;
                    item.UploadError = "要上传图片不存在，请先做复制操作";
                    continue;
                }

                string remoteFilePath = $"/home/pi/{serverFolder}/{item.NewDisplayFileName}";

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
                

            }
            client.Disconnect();
        }

    }
}
