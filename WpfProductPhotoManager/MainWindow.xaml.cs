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
            TxtOutputPath.Text = photoService.OutputFolder;
        }



        private List<InputFile> inputFiles = new List<InputFile>();
        private void DataGrid_Drop(object sender, DragEventArgs e)
        {
            inputFiles.Clear();
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var file in files)
            {
                InputFile inputFile = new InputFile();
                inputFile.OrignalFileName = file;
                inputFile.DisplayFileName=System.IO.Path.GetFileName(file);
                inputFile.CreatedTime=File.GetCreationTime(file);

                inputFiles.Add(inputFile);
            }

            DgInputs.ItemsSource = null;
            DgInputs.ItemsSource = inputFiles;
        }
    }
}
