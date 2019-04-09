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
using System.Drawing.Drawing2D;

namespace KWDMpluca
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> bitmapList = new List<string>();
        Point currentPoint = new Point();

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

            #region Załadowanie obrazu
            //gdcm.KeyValuePairArrayType keys_new = new gdcm.KeyValuePairArrayType();
            //keys_new.Add(new gdcm.KeyValuePairType(new gdcm.Tag(0x0010, 0x0010), L_SelectedName.Content.ToString()));

            //gdcm.BaseRootQuery query_new = gdcm.CompositeNetworkFunctions.ConstructQuery(type, level, keys_new, true);

            //String received = System.IO.Path.Combine(".", "odebrane");

            //if (!System.IO.Directory.Exists(received))
            //    System.IO.Directory.CreateDirectory(received);

            //String data = System.IO.Path.Combine(received, System.IO.Path.GetRandomFileName());
            //System.IO.Directory.CreateDirectory(data);

            //status = gdcm.CompositeNetworkFunctions.CMove(Properties.Settings.Default.IP, ushort.Parse(Properties.Settings.Default.Port), query_new, 10104, Properties.Settings.Default.AET, Properties.Settings.Default.AEC, data);

            //if (!status)
            //{
            //    MessageBox.Show("Pobieranie obrazów nie powodło się");
            //    return;
            //}

            //List<string> files = new List<string>(System.IO.Directory.EnumerateFiles(data));
            //foreach (String file in files)
            //{
            //    gdcm.PixmapReader reader = new gdcm.PixmapReader();
            //    reader.SetFileName(file);
            //    if (!reader.Read())
            //    {
            //        MessageBox.Show("Opuszczam plik {0}", file);
            //        continue;
            //    }

            //    gdcm.Bitmap bmjpeg2000 = BitmapHelper.pxmap2jpeg2000(reader.GetPixmap());
            //    System.Drawing.Bitmap[] X = BitmapHelper.gdcmBitmap2Bitmap(bmjpeg2000);

            //    for (int j = 0; j < X.Length; j++)
            //    {
            //        String name = String.Format("{0}_warstwa{1}.jpg", file, j);
            //        X[j].Save(name);
            //    }
            //}

            //bitmapList.AddRange(System.IO.Directory.EnumerateFiles(data, "*.jpg"));

            //ImageDicom.Source = BitmapHelper.LoadBitmapImage(0, bitmapList);
            #endregion

            FileStream file = new FileStream("lena.bmp", FileMode.Open);
            //ImageDicom.Source = BitmapFrame.Create(file, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            //ImageBrush ib = new ImageBrush();
            //ib.ImageSource = new BitmapImage(new Uri(, UriKind.RelativeOrAbsolute));
            Image MyImg = new Image();
            MyImg.Source = BitmapFrame.Create(file, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

            MyImg.Width = 370;
            MyImg.Height = 370;

            canvas.Children.Add(MyImg);
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //ImageDicom.Source = BitmapHelper.LoadBitmapImage(Convert.ToInt32(e.NewValue), bitmapList);
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                currentPoint = e.GetPosition(this);
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Line line = new Line();

                line.Stroke = SystemColors.WindowFrameBrush;
                line.X1 = currentPoint.X;
                line.Y1 = currentPoint.Y;
                line.X2 = e.GetPosition(this).X;
                line.Y2 = e.GetPosition(this).Y;

                currentPoint = e.GetPosition(this);

                canvas.Children.Add(line);
            }
        }

        //GraphicsPath GP = null;
        //List<Point> points = new List<Point>();

        //private void ImageDicom_MouseDown(object sender, MouseEventArgs e)
        //{
        //    points.Clear();
        ////    points.Add(e.Lo);
        ////}

        //private void ImageDicom_MouseUp(object sender, MouseEventArgs e)
        //{
        //    GP = new GraphicsPath();
        //    //GP.AddClosedCurve(points.ToArray());
        //}

        //private void ImageDicom_MouseMove(object sender, MouseEventArgs e)
        //{
        //    points.Add(e.Location);
        //}
    }
}
