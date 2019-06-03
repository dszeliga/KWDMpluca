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
using System.IO;
using Microsoft.Win32;

namespace KWDMpluca
{
    /// <summary>
    /// Interaction logic for AddPatient.xaml
    /// </summary>
    public partial class AddPatient : Window
    {
        public AddPatient()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            // Wyświetlenie obrazka o określonej ścieżce na określonym przycisku
            InsertImage("image/AddAndSave.png", B_AddAndSave);
            InsertImage("image/SelectFolder.png", B_AddFolder);           
            InsertImage("image/SendToPACS.png", B_ToPACS);
            InsertImage("image/Close.png", B_Close);
        }

        public void InsertImage(string path, Button buttonName)
        {
            // Wyświetlenie obrazka o określonej ścieżce na określonym przycisku
            BitmapImage bitimg = new BitmapImage();
            bitimg.BeginInit();
            bitimg.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
            bitimg.EndInit();
            Image img = new Image();
            img.Stretch = Stretch.Uniform;
            img.Source = bitimg;
            buttonName.Content = img;
        }

        private void B_AddImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "DICOM files (*.dcm)|*.dcm|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
                L_Path.Content = openFileDialog.FileName;
        }

        private void B_ToPACS_Click(object sender, RoutedEventArgs e)
        {
            string path = L_Path.Content.ToString();

            gdcm.Reader reader = new gdcm.Reader();
            reader.SetFileName(path);
            gdcm.File file = reader.GetFile();
            gdcm.Directory dir = new gdcm.Directory();
            dir.Load(path, true);
            gdcm.FilenamesType filenamesType = dir.GetFilenames();

            bool statusStore = gdcm.CompositeNetworkFunctions.CStore(Properties.Settings.Default.IP, ushort.Parse(Properties.Settings.Default.Port), new gdcm.FilenamesType(filenamesType), Properties.Settings.Default.AET, Properties.Settings.Default.AEC);
            if (statusStore)
                L_Info.Content = "Zdjęcia zostały wysłane do bazy PACS.";
            else
                L_Info.Content = "Wystąpił problem podczas wysyłania zdjęć. Sprawdź połączenie z serwerem PACS.";
        }

        private void B_AddAndSave_Click(object sender, RoutedEventArgs e)
        {
            string path = L_Path.Content.ToString();

            //gdcm.Reader reader = new gdcm.Reader();
            //reader.SetFileName(path);
            //gdcm.File file = reader.GetFile();
            gdcm.Directory dir = new gdcm.Directory();
            dir.Load(path, true);
            gdcm.FilenamesType filenamesType = dir.GetFilenames();

            bool statusStore = gdcm.CompositeNetworkFunctions.CStore(Properties.Settings.Default.IP, ushort.Parse(Properties.Settings.Default.Port), new gdcm.FilenamesType(filenamesType), Properties.Settings.Default.AET, Properties.Settings.Default.AEC);

            if (statusStore)
                L_Info.Content = "Zdjęcia zostały wysłane do bazy PACS.";
            else
                L_Info.Content = "Wystąpił problem podczas wysyłania zdjęć. Sprawdź połączenie z serwerem PACS.";


            Properties.Settings.Default.SelectedPatientID = L_SelectedID.Content.ToString();
            Properties.Settings.Default.Save();
        }

        private void B_AddFolder_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "DICOM files (*.dcm)|*.dcm|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
                L_Path.Content = openFileDialog.FileName;
            string path_all = L_Path.Content.ToString();
            int indexOfLastSlash = path_all.LastIndexOf("\\");
            string path = path_all.Remove(indexOfLastSlash);
            L_Path.Content = path;
            ReadPatientInfo(path_all);
            L_Info.Content = "Folder wybrany został prawidłowo.";
        }

        public void ReadPatientInfo(string path)
        {
            gdcm.Reader reader = new gdcm.Reader();
            reader.SetFileName(path);
            bool ret = reader.Read();
            gdcm.File file = reader.GetFile();

            gdcm.StringFilter filter = new gdcm.StringFilter();
            filter.SetFile(file);
            string patientName = filter.ToString(new gdcm.Tag(0x0010, 0x0010));
            string patientID = filter.ToString(new gdcm.Tag(0x0010, 0x0020));
            string patientBDate = filter.ToString(new gdcm.Tag(0x0010, 0x0030));
            L_SelectedID.Content = patientID;
            L_SelectedName.Content = patientName;
            L_SelectedDateB.Content = patientBDate;
        }

        private void B_CloseAdd_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
