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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Windows.Threading;
using sitk = itk.simple;
using System.IO;
using KWDMpluca.Helpers;

namespace KWDMpluca
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Wyświetlenie aplikacji na środku ekranu
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            // Wyświetlenie obrazków na przyciskach
            InsertImage("image/Add.png", BAdd);
            InsertImage("image/Search.png", BSearch);
            InsertImage("image/Settings.png", BSettings);
            InsertImage("image/print.png", BPrint);
            InsertImage("image/RefreshIcon.png", BReload);
            //patientsListBox.ItemsSource = PatientHelper.GetPatients(Properties.Settings.Default.IP, ushort.Parse(Properties.Settings.Default.Port), Properties.Settings.Default.AET, Properties.Settings.Default.AEC);
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

        private void BSettings_Click(object sender, RoutedEventArgs e)
        {
            Settings win2 = new Settings();
            win2.Show();
        }

        private void BSearch_Click(object sender, RoutedEventArgs e)
        {
            FindPatient win2 = new FindPatient();
            win2.Show();
        }

        private void BPrint_Click(object sender, RoutedEventArgs e)
        {
            CreatePDF winPDF = new CreatePDF();
            winPDF.Show();
        }

        private void BAdd_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BReload_Click(object sender, RoutedEventArgs e)
        {
            //LCheck.Content = Properties.Settings.Default.SelectedPatientID;
            List<string> patientList = new List<string>();
            gdcm.ERootType type = gdcm.ERootType.ePatientRootType;

            gdcm.EQueryLevel level = gdcm.EQueryLevel.ePatient;

            gdcm.KeyValuePairArrayType keys = new gdcm.KeyValuePairArrayType();
            gdcm.KeyValuePairType key = new gdcm.KeyValuePairType(new gdcm.Tag(0x0010, 0x0010), "");
            keys.Add(key);
            keys.Add(new gdcm.KeyValuePairType(new gdcm.Tag(0x0010, 0x0020), Properties.Settings.Default.SelectedPatientID));
            keys.Add(new gdcm.KeyValuePairType(new gdcm.Tag(0x0010, 0x0030), ""));
            keys.Add(new gdcm.KeyValuePairType(new gdcm.Tag(0x0010, 0x0040), ""));

            gdcm.BaseRootQuery query = gdcm.CompositeNetworkFunctions.ConstructQuery(type, level, keys);

            gdcm.DataSetArrayType dataArray = new gdcm.DataSetArrayType();

            bool status = gdcm.CompositeNetworkFunctions.CFind(Properties.Settings.Default.IP, ushort.Parse(Properties.Settings.Default.Port), query, dataArray, Properties.Settings.Default.AET, Properties.Settings.Default.AEC);
            int i = 0;
            foreach (gdcm.DataSet x in dataArray)
            {
                L_SelectedID.Content = x.GetDataElement(new gdcm.Tag(0x0010, 0x0020)).GetValue().toString();
                L_SelectedName.Content = x.GetDataElement(new gdcm.Tag(0x0010, 0x0010)).GetValue().toString();
                L_SelectedDateB.Content = x.GetDataElement(new gdcm.Tag(0x0010, 0x0030)).GetValue().toString();
            }
        }
    }
}
