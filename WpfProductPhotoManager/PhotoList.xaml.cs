using FluentFTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfProductPhotoManager
{
    /// <summary>
    /// PhotoList.xaml 的交互逻辑
    /// </summary>
    public partial class PhotoList : Window
    {
        public PhotoList()
        {
            InitializeComponent();
        }

        public void SetPhotoList(string productid,List<string> list)
        {
            TxtProductID.Text = productid;
            LstPhotoList.ItemsSource = null;
            LstPhotoList.ItemsSource = list;
        }

        public event EventHandler DownloadAllFiles;
        private void BtnDownloadAllFiles_Click(object sender, RoutedEventArgs e)
        {
            if (DownloadAllFiles != null)
            {
                DownloadAllFiles.Invoke(this, null);
                this.Close();
            }
        }
    }
}
