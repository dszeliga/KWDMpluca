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

            patientsListBox.ItemsSource = PatientHelper.GetPatients(Properties.Settings.Default.IP, ushort.Parse(Properties.Settings.Default.Port), Properties.Settings.Default.AET, Properties.Settings.Default.AEC);
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
            //string serwerIP = "127.0.0.0.1";
            //ushort serwerPort = 10010;
            //string klientAET = "KLIENTL";
            //string serwerAET = "ARCHIWUM";
            //bool stan = gdcm.CompositeNetworkFunctions.CEcho(serwerIP, serwerPort, klientAET, serwerAET);
            //string pathToSeries = "C:/Users/chmur/Desktop/BazaKWDM/RIDERLungCT/RIDER-1286684383/02-26-2007-85682/100-45297";

            //// Wczytanie serii obrazow
            //LCheck.Content = "Wczytanie obrazu... czekaj";
            ////... TODO
            //var seriesIDs = sitk.ImageSeriesReader.GetGDCMSeriesIDs(pathToSeries);

            //foreach (var item in seriesIDs)
            //{
            //    Console.WriteLine(item);
            //}
            //sitk.VectorString fnames1 = sitk.ImageSeriesReader.GetGDCMSeriesFileNames(pathToSeries, seriesIDs[0]);

            //LCheck.Content = "Przeszło";
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

        }

        private void BAdd_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
