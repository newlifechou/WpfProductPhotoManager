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
            ftpService = new FTPService();
            pmsService = new PMSService();
            Initialize();


            //加载已保存文件
            LoadWorkList();
        }

        private PhotoService photoService;
        private FTPService ftpService;
        private PMSService pmsService;
        private void Initialize()
        {
            BtnOutputPath.Content = photoService.OutputFolder;


            productIds.Add($"{DateTime.Now.ToString("yyMMdd")}-A-1");
            productIds.Add($"{DateTime.Now.ToString("yyMMdd")}-A-2");
            productIds.Add($"{DateTime.Now.ToString("yyMMdd")}-A-3");
            productIds.Add($"{DateTime.Now.ToString("yyMMdd")}-A-4");
            productIdsFilter = productIds;
            SetLstProducts();

            categories.Add("Test");
            categories.Add("Bonding");
            categories.Add("SecondMachine");
            categories.Add("BeforeDelivery");

            CboCategory.ItemsSource = null;
            CboCategory.ItemsSource = categories;
            CboCategory.SelectedIndex = 0;


            keywords.Add("");
            keywords.Add("TCB");
            keywords.Add("BRC");
            keywords.Add("SZJMK");
            keywords.Add("DJY");

            CboKeyword.ItemsSource = null;
            CboKeyword.ItemsSource = keywords;
            CboKeyword.SelectedIndex = 0;

            SetCurrentOutputFileNamePrefix();
        }

        private void SetCurrentOutputFileNamePrefix()
        {
            if (LstProductIds.SelectedItem == null || string.IsNullOrEmpty(LstProductIds.SelectedItem.ToString()))
            {
                return;
            }
            photoService.SetOutputFilePrefix(LstProductIds.SelectedItem.ToString(), CboCategory.Text, CboKeyword.Text);
            TxtCurrentOutputFileNamePrefix.Text = photoService.OutputFilePrefix;
        }

        private List<InputFile> inputFiles = new List<InputFile>();

        private List<string> productIds = new List<string>();
        private List<string> productIdsFilter = new List<string>();
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

            inputFiles = inputFiles.OrderBy(i => i.DisplayFileName).ToList();
            DgInputs.ItemsSource = null;
            DgInputs.ItemsSource = inputFiles;
        }

        private void BtnScan_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ScanWindow();
            if (dialog.ShowDialog() == true)
            {
                TxtIDFilter.Text = "";
                productIds.Clear();
                productIds.AddRange(dialog.ProductIDList);
                productIdsFilter = productIds;
                SetLstProducts();
            }
        }
        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            TxtIDFilter.Text = "";
            productIds.Clear();
            productIds = pmsService.GetProductIds().OrderByDescending(i => i).ToList();
            productIdsFilter = productIds;
            SetLstProducts();
        }

        private void TxtIDFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtIDFilter.Text))
            {
                productIdsFilter = productIds;
            }
            else
            {
                productIdsFilter = productIds.Where(i => i.Contains(TxtIDFilter.Text)).ToList();
            }
            SetLstProducts();
        }

        private void SetLstProducts()
        {
            productIdsFilter = productIdsFilter.OrderByDescending(i => i).ToList();
            LstProductIds.ItemsSource = null;
            LstProductIds.ItemsSource = productIdsFilter;
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
            if (photoService.CheckState(inputFiles))
            {
                if (MessageBox.Show("工作列表中有文件已被处理,是否再次处理?", "请问", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    return;
                }
            }
            photoService.CopyPhoto(inputFiles);
            SetDgInputs();
            MessageBox.Show("复制和规范重命名成功");
            SaveWorkList();
        }

        private void BtnUpload_Click(object sender, RoutedEventArgs e)
        {
            if (inputFiles.Count == 0)
            {
                MessageBox.Show("工作列表没有文件");
                return;
            }

            if (ftpService.CheckState(inputFiles))
            {
                if (MessageBox.Show("工作列表中有文件已被上传,是否再次上传?", "请问", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    return;
                }
            }

            ftpService.OverrideMode = (bool)ChkUploadMode.IsChecked;
            ftpService.UploadFiles(inputFiles);
            SetDgInputs();
            MessageBox.Show("文件上传成功");
            SaveWorkList();
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
            SaveWorkList();
            MessageBox.Show("保存成功");
        }

        private void BtnResetList_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in inputFiles)
            {
                item.IsCopied = false;
                item.CopyError = "";
                item.IsUploaded = false;
                item.UploadError = "";
            }
            SetDgInputs();
            SaveWorkList();
        }
        private void SaveWorkList()
        {
            string json = JsonConvert.SerializeObject(inputFiles);
            photoService.SaveWorkList(json);
        }

        private void BtnLoadWorkList_Click(object sender, RoutedEventArgs e)
        {
            LoadWorkList();
        }

        private void LoadWorkList()
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
