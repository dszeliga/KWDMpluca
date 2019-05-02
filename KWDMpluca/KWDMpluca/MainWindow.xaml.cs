using KWDMpluca.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KWDMpluca
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> bitmapList = new List<string>();
        Point currentPoint = new Point();
        Image MyImg = new Image();
        Point startPoint = new Point();
        List<Point> points = new List<Point>();
        gdcm.DataSetArrayType dataArray;

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
            CreateSaveBitmap(canvas, "image.bmp");

            ImageSource dcm = new BitmapImage(new Uri("image.bmp", UriKind.Relative));
            CreatePDF winPDF = new CreatePDF(dcm);
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

            dataArray = new gdcm.DataSetArrayType();

            bool status = gdcm.CompositeNetworkFunctions.CFind(Properties.Settings.Default.IP, ushort.Parse(Properties.Settings.Default.Port), query, dataArray, Properties.Settings.Default.AET, Properties.Settings.Default.AEC);
            int i = 0;
            foreach (gdcm.DataSet x in dataArray)
            {
                L_SelectedID.Content = x.GetDataElement(new gdcm.Tag(0x0010, 0x0020)).GetValue().toString();
                L_SelectedName.Content = x.GetDataElement(new gdcm.Tag(0x0010, 0x0010)).GetValue().toString();
                L_SelectedDateB.Content = x.GetDataElement(new gdcm.Tag(0x0010, 0x0030)).GetValue().toString();

                if (x.FindDataElement(new gdcm.Tag(0x0010, 0x0030))) //0008, 103E - opis 
                    T_Description.Text = x.GetDataElement(new gdcm.Tag(0x0010, 0x0030)).GetValue().toString();
                else
                    T_Description.Text = "";
            }

            #region Załadowanie obrazu
            //gdcm.KeyValuePairArrayType keys_new = new gdcm.KeyValuePairArrayType();
            //keys_new.Add(new gdcm.KeyValuePairType(new gdcm.Tag(0x0010, 0x0010), L_SelectedName.Content.ToString()));

            gdcm.BaseRootQuery query_new = gdcm.CompositeNetworkFunctions.ConstructQuery(type, level, keys, true);

            String received = System.IO.Path.Combine(".", "odebrane");

            if (!System.IO.Directory.Exists(received))
                System.IO.Directory.CreateDirectory(received);

            String data = System.IO.Path.Combine(received, System.IO.Path.GetRandomFileName());
            System.IO.Directory.CreateDirectory(data);

            status = gdcm.CompositeNetworkFunctions.CMove(Properties.Settings.Default.IP, ushort.Parse(Properties.Settings.Default.Port), query_new, 10104, Properties.Settings.Default.AET, Properties.Settings.Default.AEC, data);

            if (!status)
            {
                MessageBox.Show("Pobieranie obrazów nie powodło się");
                return;
            }

            List<string> files = new List<string>(System.IO.Directory.EnumerateFiles(data));
            foreach (String fileDcm in files)
            {
                gdcm.PixmapReader reader = new gdcm.PixmapReader();
                reader.SetFileName(fileDcm);
                if (!reader.Read())
                {
                    MessageBox.Show("Opuszczam plik {0}", fileDcm);
                    continue;
                }

                gdcm.Bitmap bmjpeg2000 = BitmapHelper.pxmap2jpeg2000(reader.GetPixmap());
                System.Drawing.Bitmap[] X = BitmapHelper.gdcmBitmap2Bitmap(bmjpeg2000);

                for (int j = 0; j < X.Length; j++)
                {
                    String name = String.Format("{0}_warstwa{1}.jpg", fileDcm, j);
                    X[j].Save(name);
                }
            }

            bitmapList.AddRange(System.IO.Directory.EnumerateFiles(data, "*.jpg"));

            MyImg.Source = BitmapHelper.LoadBitmapImage(0, bitmapList);
            #endregion

            //FileStream file = new FileStream("lena.bmp", FileMode.Open);
            //ImageDicom.Source = BitmapFrame.Create(file, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            //ImageBrush ib = new ImageBrush();
            //ib.ImageSource = new BitmapImage(new Uri(, UriKind.RelativeOrAbsolute));

            //MyImg.Source = BitmapFrame.Create(file, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

            MyImg.Width = 370;
            MyImg.Height = 370;

            canvas.Children.Add(MyImg);
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MyImg.Source = BitmapHelper.LoadBitmapImage(Convert.ToInt32(e.NewValue), bitmapList);
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                startPoint = e.GetPosition(canvas);
                currentPoint = startPoint;
                points.Add(currentPoint);
            }
            //currentPoint = e.GetPosition(this);
            //currentPoint = e.GetPosition(canvas);
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Line line = new Line();

                line.Stroke = SystemColors.WindowFrameBrush;
                line.X1 = currentPoint.X;
                line.Y1 = currentPoint.Y;
                line.X2 = e.GetPosition(canvas).X;
                line.Y2 = e.GetPosition(canvas).Y;

                SolidColorBrush redBrush = new SolidColorBrush();
                redBrush.Color = Colors.Red;

                line.Stroke = redBrush;

                currentPoint = e.GetPosition(canvas);

                points.Add(currentPoint);

                canvas.Children.Add(line);
            }
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                if (currentPoint != startPoint)
                {
                    Line line = new Line();

                    line.Stroke = SystemColors.WindowFrameBrush;
                    line.X1 = currentPoint.X;
                    line.Y1 = currentPoint.Y;
                    line.X2 = startPoint.X;
                    line.Y2 = startPoint.Y;

                    canvas.Children.Add(line);
                }

                var area = Math.Abs(points.Take(points.Count - 1).Select((p, i) => (points[i + 1].X - p.X) * (points[i + 1].Y + p.Y)).Sum() / 2);

            }
        }

        private void CreateSaveBitmap(Canvas canvas, string filename)
        {
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
             (int)MyImg.Width, (int)MyImg.Height,
             96d, 96d, PixelFormats.Pbgra32);
            // needed otherwise the image output is black
            canvas.Measure(new Size((int)MyImg.Width, (int)MyImg.Height));
            canvas.Arrange(new Rect(new Size((int)MyImg.Width, (int)MyImg.Height)));

            renderBitmap.Render(canvas);

            //JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            using (FileStream file = File.Create(filename))
            {
                encoder.Save(file);
            }
        }

        private void BEditDescription_Click(object sender, RoutedEventArgs e)
        {
            T_Description.IsReadOnly = false;
            BEditDescription.Visibility = Visibility.Hidden;
            BDescriptionAnuluj.Visibility = Visibility.Visible;
            BDescriptionOK.Visibility = Visibility.Visible;
        }

        private void BDescriptionOK_Click(object sender, RoutedEventArgs e)
        {
            T_Description.IsReadOnly = true;
            BEditDescription.Visibility = Visibility.Visible;
            BDescriptionAnuluj.Visibility = Visibility.Hidden;
            BDescriptionOK.Visibility = Visibility.Hidden;

            // byte[] decription = Encoding.ASCII.GetBytes(T_Description.Text);
            //tu skopiowac to cos z gory z foreachem
            // x.GetDataElement(new gdcm.Tag(0x0008, 0x103E)).SetByteValue(decription, new gdcm.VL((uint)decription.Length));
            // x.GetDataElement(new gdcm.Tag(0x0008, 0x103E)).SetTag(new gdcm.Tag(0x0008, 0x103E).SetElementTag(0x0008, 0x103E));
        }

        private void BDescriptionAnuluj_Click(object sender, RoutedEventArgs e)
        {
            T_Description.IsReadOnly = true;
            BEditDescription.Visibility = Visibility.Visible;
            BDescriptionAnuluj.Visibility = Visibility.Hidden;
            BDescriptionOK.Visibility = Visibility.Hidden;
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
