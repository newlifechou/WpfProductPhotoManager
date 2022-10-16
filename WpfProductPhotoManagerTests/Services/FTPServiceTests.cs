using Microsoft.VisualStudio.TestTools.UnitTesting;
using WpfProductPhotoManager.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfProductPhotoManager.Services.Tests
{
    [TestClass()]
    public class FTPServiceTests
    {
        [TestMethod()]
        public void ListFilesTest()
        {
            var ftp = new FTPService();
            var fileList = ftp.ListFiles("221015-D-1_Test");
            Assert.IsNotNull(fileList);
        }
    }
}