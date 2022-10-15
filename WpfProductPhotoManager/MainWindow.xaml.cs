using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfProductPhotoManager.Models;
using WpfProductPhotoManager.Services;

namespace WpfProductPhotoManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            photoService = new PhotoService();
            Initialize();
        }

        private PhotoService photoService;

        private void Initialize()
        {
            BtnOutputPath.Content = photoService.OutputFolder;


            productIds.Add($"{DateTime.Now.ToString("yyMMdd")}-A-1");
            productIds.Add($"{DateTime.Now.ToString("yyMMdd")}-A-2");
            productIds.Add($"{DateTime.Now.ToString("yyMMdd")}-A-3");
            productIds.Add($"{DateTime.Now.ToString("yyMMdd")}-A-4");

            SetLstProducts();

            categories.Add("测试前");
            categories.Add("绑定后");
            categories.Add("二次加工后");
            categories.Add("发货前");

            CboCategory.ItemsSource = null;
            CboCategory.ItemsSource = categories;
            CboCategory.SelectedIndex = 0;


            keywords.Add("");
            keywords.Add("TCB");
            keywords.Add("BRC");
            keywords.Add("苏州精美科");
            keywords.Add("都江堰");

            CboKeyword.ItemsSource = null;
            CboKeyword.ItemsSource = keywords;
            CboKeyword.SelectedIndex = 0;

            SetCurrentOutputFileNamePrefix();
        }

        private void SetCurrentOutputFileNamePrefix()
        {
            photoService.SetOutputFilePrefix(LstProductIds.SelectedItem.ToString(), CboCategory.Text, CboKeyword.Text);
            TxtCurrentOutputFileNamePrefix.Text = photoService.OutputFilePrefix;
        }

        private List<InputFile> inputFiles = new List<InputFile>();

        private List<string> productIds = new List<string>();

        private List<string> categories = new List<string>();
        private List<string> keywords = new List<string>();

        private void DataGrid_Drop(object sender, DragEventArgs e)
        {
            inputFiles.Clear();
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var file in files)
            {
                InputFile inputFile = new InputFile();
                inputFile.OrignalFileName = file;
                inputFile.DisplayFileName = System.IO.Path.GetFileName(file);
                inputFile.CreatedTime = File.GetCreationTime(file);

                inputFiles.Add(inputFile);
            }

            SetDgInputs();
        }

        private void SetDgInputs()
        {
            DgInputs.ItemsSource = null;
            DgInputs.ItemsSource = inputFiles;
        }

        private void BtnScan_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ScanWindow();
            if (dialog.ShowDialog() == true)
            {
                productIds.Clear();
                productIds.AddRange(dialog.ProductIDList);

                SetLstProducts();
            }
        }

        private void SetLstProducts()
        {
            LstProductIds.ItemsSource = null;
            LstProductIds.ItemsSource = productIds;
            LstProductIds.SelectedIndex = 0;
        }

        private void LstProductIds_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetCurrentOutputFileNamePrefix();
        }

        private void CboCategory_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetCurrentOutputFileNamePrefix();

        }

        private void CboKeyword_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetCurrentOutputFileNamePrefix();

        }

        private void BtnCopyAndReName_Click(object sender, RoutedEventArgs e)
        {
            if (inputFiles.Count == 0)
            {
                MessageBox.Show("工作列表没有文件");
                return;
            }
            photoService.CopyPhoto(inputFiles);
            SetDgInputs();
            MessageBox.Show("复制和规范重命名成功");
        }

        private void BtnUpload_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnOutputFolderSet_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.RootFolder = Environment.SpecialFolder.Desktop;
            dialog.Description = "请选一个用来存储输出图片的文件夹";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                photoService.OutputFolder = dialog.SelectedPath;
                BtnOutputPath.Content = photoService.OutputFolder;
            }
        }

        private void BtnSaveWorkList_Click(object sender, RoutedEventArgs e)
        {
            string json = JsonConvert.SerializeObject(inputFiles);
            photoService.SaveWorkList(json);
            MessageBox.Show("保存成功");
        }

        private void BtnLoadWorkList_Click(object sender, RoutedEventArgs e)
        {
            string json = photoService.LoadWorkList();
            inputFiles = JsonConvert.DeserializeObject<List<InputFile>>(json);
            SetDgInputs();
        }

        private void BtnClearList_Click(object sender, RoutedEventArgs e)
        {
            inputFiles.Clear();
            SetDgInputs();
        }

        private void BtnOutputPath_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string folder = photoService.OutputFolder;
                System.Diagnostics.Process.Start(folder);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
