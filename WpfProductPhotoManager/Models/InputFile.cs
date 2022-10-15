using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfProductPhotoManager.Models
{
    public class InputFile
    {
        public InputFile()
        {
            IsCopied = false;
            IsUploaded = false;
        }
        public string OrignalFileName { get; set; }

        public string DisplayFileName { get; set; }
        public DateTime CreatedTime { get; set; }
        public bool IsCopied { get; set; }
        public string CopyError { get; set; }
        public string NewFileName { get; set; }
        public string NewDisplayFileName { get; set; }

        public bool IsUploaded { get; set; }
        public string UploadError { get; set; }
    }
}
