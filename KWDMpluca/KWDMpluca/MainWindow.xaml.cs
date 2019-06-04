using KWDMpluca.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
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
        List<string> masksNames = new List<string>();
        Point currentPoint = new Point();
        Image MyImg = new Image();
        Point startPoint = new Point();
        List<Point> points = new List<Point>();
        gdcm.DataSetArrayType dataArray;
        String data;
        ScaleTransform st = new ScaleTransform();
        List<string> files;
        List<string> filesNew = new List<string>();
        double pixelSpacing;
        bool bPrintClicked = false;
        bool bDistanceClicked = false;
        bool bZoomClicked = false;
        bool bCorrectAreaClicked = false;
        bool maskReverted = false;
        double[] pointsDistance = new double[4];
        double area, distance;
        int ind = 0;
        int numberOfImage = 0;
        int numberOfDicomInstance = 0;
        string pathEmpty = ".\\tlo.bmp";
        string[] Poukladanesciezki;
        Polygon polygon = new Polygon();
        PointCollection pc = new PointCollection();
        SolidColorBrush yellowGreenBrush = new SolidColorBrush();
        Image MyImg2 = new Image();
        Image MyImg1 = new Image();
        Image MyImg3 = new Image();
        Image MaskImg = new Image();
        SolidColorBrush redBrush = new SolidColorBrush();
        string[] AllMasks;
        BitmapImage ImageBitmap;
        string globalpath;
        List<string> dicomList = new List<string>();
        List<int> globalNumbersOfInstance = new List<int>();
        List<int> maskNumbersOfInstance = new List<int>();

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
            InsertImage("image/NextImage.png", BNextImage);
            InsertImage("image/PreviousImage.png", BPreviousImage);
            BSaveMask.IsEnabled = false;
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
            bPrintClicked = true;
            CreateSaveBitmap(canvasSegm, "image.bmp");

            ImageSource dcm = new BitmapImage(new Uri("image.bmp", UriKind.Relative));
            CreatePDF winPDF = new CreatePDF(dcm, T_Description.Text, area, distance);
            winPDF.Show();
        }

        private void BAdd_Click(object sender, RoutedEventArgs e)
        {
            AddPatient win2 = new AddPatient();
            win2.Show();
        }

        private void BReload_Click(object sender, RoutedEventArgs e)
        {
            ScrollViewer scrollViewer = new ScrollViewer();
            scrollViewer.ScrollToVerticalOffset(100);
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
            keys.Add(new gdcm.KeyValuePairType(new gdcm.Tag(0x0008, 0x103E), ""));
            keys.Add(new gdcm.KeyValuePairType(new gdcm.Tag(0x0028, 0x0030), ""));
            keys.Add(new gdcm.KeyValuePairType(new gdcm.Tag(0x0008, 0x0020), ""));
            keys.Add(new gdcm.KeyValuePairType(new gdcm.Tag(0x0020, 0x0013), "")); // NUMER W SERII

            gdcm.BaseRootQuery query = gdcm.CompositeNetworkFunctions.ConstructQuery(type, level, keys);

            dataArray = new gdcm.DataSetArrayType();

            bool status = gdcm.CompositeNetworkFunctions.CFind(Properties.Settings.Default.IP, ushort.Parse(Properties.Settings.Default.Port), query, dataArray, Properties.Settings.Default.AET, Properties.Settings.Default.AEC);

            #region Załadowanie obrazu

            gdcm.BaseRootQuery query_new = gdcm.CompositeNetworkFunctions.ConstructQuery(type, level, keys, true);

            string received = System.IO.Path.Combine(".", "odebrane");

            if (!System.IO.Directory.Exists(received))
                System.IO.Directory.CreateDirectory(received);

            data = System.IO.Path.Combine(received, System.IO.Path.GetRandomFileName());
            globalpath = data;
            System.IO.Directory.CreateDirectory(data);

            status = gdcm.CompositeNetworkFunctions.CMove(Properties.Settings.Default.IP, ushort.Parse(Properties.Settings.Default.Port), query_new, 10104, Properties.Settings.Default.AET, Properties.Settings.Default.AEC, data);

            if (!status)
            {
                MessageBox.Show("Pobieranie obrazów nie powodło się");
                return;
            }

            files = new List<string>(System.IO.Directory.EnumerateFiles(data));

            int i = 0;
            foreach (String fileDcm in files)
            {
                gdcm.Reader reader = new gdcm.Reader();
                reader.SetFileName(fileDcm);
                bool ret = reader.Read();
                gdcm.File file = reader.GetFile();

                gdcm.StringFilter filter = new gdcm.StringFilter();
                filter.SetFile(file);
                string numberInSeries = filter.ToString(new gdcm.Tag(0x0020, 0x0013));
                string description = filter.ToString(new gdcm.Tag(0x0008, 0x103E));

                if (!string.IsNullOrEmpty(numberInSeries) && description != "mask")
                {
                    int numer = int.Parse(numberInSeries);
                    if (numer > i)
                        i = numer;
                }
            }
            Poukladanesciezki = new string[i + 1];
            AllMasks = new string[i + 1]; // ------------------------------------------------ TUTAJ

            foreach (String fileDcm in files)
            {
                // fileDcm to ścieżka do pliku zapisanego i ściągnętego z bazy
                gdcm.Reader reader = new gdcm.Reader();
                reader.SetFileName(fileDcm);
                bool ret = reader.Read();
                gdcm.File file = reader.GetFile();

                gdcm.StringFilter filter = new gdcm.StringFilter();
                filter.SetFile(file);
                string numberInSeries = filter.ToString(new gdcm.Tag(0x0020, 0x0013));
                string description = filter.ToString(new gdcm.Tag(0x0008, 0x103E));

                if (!string.IsNullOrEmpty(numberInSeries) && description != "mask")
                {
                    int numer = int.Parse(numberInSeries);
                    globalNumbersOfInstance.Add(numer);
                    Poukladanesciezki[numer] = fileDcm;
                    //dicomList.Add(fileDcm);

                }
            }
            int j = 1000;
            foreach (String fileDcm in Poukladanesciezki)
            {
                if (!string.IsNullOrEmpty(fileDcm))
                {
                    dicomList.Add(fileDcm);
                    gdcm.PixmapReader reader = new gdcm.PixmapReader();
                    reader.SetFileName(fileDcm);
                    if (!reader.Read())
                    {
                        MessageBox.Show("Opuszczam plik {0}", fileDcm);
                        continue;
                    }

                    gdcm.Bitmap bmjpeg2000 = BitmapHelper.pxmap2jpeg2000(reader.GetPixmap());
                    System.Drawing.Bitmap[] X = BitmapHelper.gdcmBitmap2Bitmap(bmjpeg2000);

                    String name = String.Format("{0}\\{1}.jpg", data, j);
                    X[0].Save(name);
                    j++;
                }
            }

            if (bitmapList.Count != 0)
                bitmapList.RemoveRange(0, bitmapList.Count);

            bitmapList.AddRange(System.IO.Directory.EnumerateFiles(data, "*.jpg"));

            //dicomList.AddRange(System.IO.Directory.EnumerateFiles(data, "*.dcm"));

            ImageBitmap = BitmapHelper.LoadBitmapImage(numberOfImage, bitmapList);
            MyImg.Source = ImageBitmap;
            MyImg3.Source = BitmapHelper.LoadBitmapImage(numberOfImage, bitmapList);

            #endregion

            MyImg.Width = 280;
            MyImg.Height = 280;
            MyImg3.Width = 280;
            MyImg3.Height = 280;

            if (canvas.Children.Count >= 1)
            {
                canvas.Children.RemoveRange(0, canvas.Children.Count);
                canvasSegm.Children.RemoveRange(0, canvasSegm.Children.Count);
                canvasSegm.Children.Add(MyImg3);
                canvas.Children.Add(MyImg);
            }
            else
            {
                canvas.Children.Add(MyImg);
                canvasSegm.Children.Add(MyImg3);
            }

            INext.Source = BitmapHelper.LoadBitmapImage(pathEmpty);
            IPrevious.Source = BitmapHelper.LoadBitmapImage(pathEmpty);

            foreach (gdcm.DataSet x in dataArray)
            {
                L_SelectedID.Content = x.GetDataElement(new gdcm.Tag(0x0010, 0x0020)).GetValue().toString();
                L_SelectedName.Content = x.GetDataElement(new gdcm.Tag(0x0010, 0x0010)).GetValue().toString();
                //L_SelectedDateB.Content = x.GetDataElement(new gdcm.Tag(0x0010, 0x0030)).GetValue().toString();

                foreach (var path in files)
                {
                    gdcm.ImageReader reader = new gdcm.ImageReader();
                    gdcm.ImageWriter writer = new gdcm.ImageWriter();
                    gdcm.File file;
                    gdcm.Image image;

                    reader.SetFileName(path);

                    if (!reader.Read())
                    {
                        T_Description.Text = "Nie wczytano pliku";
                    }

                    file = reader.GetFile();
                    image = reader.GetImage();
                    gdcm.DataSet ds = file.GetDataSet();
                    gdcm.StringFilter sf = new gdcm.StringFilter();
                    sf.SetFile(file);

                    if (ds.FindDataElement(new gdcm.Tag(0x0008, 0x103E))) //0008, 103E - opis 
                    {
                        var description = sf.ToStringPair(new gdcm.Tag(0x0008, 0x103E));

                        T_Description.Text = description.second;
                    }
                    else
                        T_Description.Text = "";

                    if (ds.FindDataElement(new gdcm.Tag(0x0028, 0x0030)))
                    {
                        var pixelData = sf.ToStringPair(new gdcm.Tag(0x0028, 0x0030));
                        var index = pixelData.second.IndexOf('\\');

                        string pixel = pixelData.second.Substring(0, index).Replace('.', ',');
                        pixelSpacing = Convert.ToDouble(pixel);
                    }
                    else
                        pixelSpacing = 1;
                }
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //MyImg.Source = BitmapHelper.LoadBitmapImage(Convert.ToInt32(e.NewValue), bitmapList);
            if (numberOfImage < bitmapList.Count)
                numberOfImage = Convert.ToInt32(e.NewValue);

            var slider = sender as Slider;
            slider.TickFrequency = 1;
            slider.Minimum = 0;
            slider.Maximum = bitmapList.Count - 1;

            if (numberOfImage == 0)
            {
                ImageBitmap = BitmapHelper.LoadBitmapImage(numberOfImage, bitmapList);
                MyImg.Source = ImageBitmap;
                MyImg3.Source = BitmapHelper.LoadBitmapImage(numberOfImage, bitmapList);
                ShowMask();
                Segmentation(MyImg3.Source);
                string path = ".\\tlo.bmp";
                IPrevious.Source = BitmapHelper.LoadBitmapImage(path);
                INext.Source = BitmapHelper.LoadBitmapImage(numberOfImage + 1, bitmapList);
            }
            else if (numberOfImage < bitmapList.Count && e.OldValue > e.NewValue)
            {
                ImageBitmap = BitmapHelper.LoadBitmapImage(numberOfImage, bitmapList);
                MyImg.Source = ImageBitmap;
                MyImg3.Source = BitmapHelper.LoadBitmapImage(numberOfImage, bitmapList);
                ShowMask();
                Segmentation(MyImg3.Source);
                IPrevious.Source = BitmapHelper.LoadBitmapImage(numberOfImage - 1, bitmapList);
                if (numberOfImage >= bitmapList.Count - 1)
                {
                    INext.Source = BitmapHelper.LoadBitmapImage(pathEmpty);//pusty                   
                }
                else
                {
                    INext.Source = BitmapHelper.LoadBitmapImage(numberOfImage + 1, bitmapList);
                }

            }
            else if (numberOfImage > 0 && e.OldValue < e.NewValue)
            {
                ImageBitmap = BitmapHelper.LoadBitmapImage(numberOfImage, bitmapList);
                MyImg.Source = ImageBitmap;
                MyImg3.Source = BitmapHelper.LoadBitmapImage(numberOfImage, bitmapList);
                ShowMask();
                Segmentation(MyImg3.Source);
                IPrevious.Source = BitmapHelper.LoadBitmapImage(numberOfImage - 1, bitmapList);
                if (numberOfImage >= bitmapList.Count - 1)
                {
                    INext.Source = BitmapHelper.LoadBitmapImage(pathEmpty);//pusty                   
                }
                else
                {
                    INext.Source = BitmapHelper.LoadBitmapImage(numberOfImage + 1, bitmapList);
                }
            }
            else if (numberOfImage >= bitmapList.Count - 1)
            {
                ImageBitmap = BitmapHelper.LoadBitmapImage(numberOfImage - 1, bitmapList);
                MyImg.Source = ImageBitmap;
                MyImg3.Source = BitmapHelper.LoadBitmapImage(numberOfImage - 1, bitmapList);
                ShowMask();
                Segmentation(MyImg3.Source);
                IPrevious.Source = BitmapHelper.LoadBitmapImage(numberOfImage - 2, bitmapList);

                INext.Source = BitmapHelper.LoadBitmapImage(pathEmpty); //pusty

            }
        }

        private void ShowMask()
        {
            if (cbMask.IsChecked == true)
            {
                if (globalNumbersOfInstance.ElementAt(numberOfImage) == maskNumbersOfInstance.ElementAt(numberOfImage))
                {
                    if (canvasSegm.Children.Contains(MyImg1))
                        canvasSegm.Children.Remove(MyImg1);

                    if (masksNames[numberOfImage] != "")
                        MyImg1.Source = BitmapHelper.LoadBitmapImage(numberOfImage, masksNames);
                    else
                    {
                        if (canvasSegm.Children.Contains(MyImg2))
                            canvasSegm.Children.Remove(MyImg2);
                    }
                    canvasSegm.Children.Add(MyImg1);
                }

                if (maskNumbersOfInstance.ElementAt(numberOfImage) == -1)
                {
                    canvasSegm.Children.Remove(MyImg1);
                    if (canvasSegm.Children.Contains(MyImg2))
                        canvasSegm.Children.Remove(MyImg2);

                    canvasSegm.Children.Add(MyImg2);
                }
            }
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                startPoint = e.GetPosition(canvas);
                currentPoint = startPoint;
                points.Add(currentPoint);
            }

            if (bDistanceClicked)
            {
                rbSegmentation.IsChecked = false;
                if (ind < 4)
                {

                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        Ellipse ellipse = new Ellipse();

                        ellipse.Stroke = SystemColors.WindowFrameBrush;
                        ellipse.Height = 5;
                        ellipse.Width = 5;
                        SolidColorBrush greenBrush = new SolidColorBrush();
                        greenBrush.Color = Colors.Red;
                        ellipse.Fill = greenBrush;
                        ellipse.Margin = new Thickness(currentPoint.X, currentPoint.Y, 0, 0);
                        pointsDistance[ind] = currentPoint.X;
                        pointsDistance[ind + 1] = currentPoint.Y;
                        canvas.Children.Add(ellipse);
                        ind += 2;
                        
                    }

                    if (ind == 4)
                    {
                        distance = (Math.Sqrt((Math.Pow(pointsDistance[0] - pointsDistance[2], 2) + Math.Pow(pointsDistance[1] - pointsDistance[3], 2)))) * pixelSpacing;
                        L_Distance.Content = "Długość: " + Math.Round(distance, 2) + "mm";
                    }

                }
                else
                {
                    var length = canvas.Children.Count;
                    for (int i = length - 1; i >= length - 2 && i > 0; i--)
                    {
                        canvas.Children.RemoveAt(i);
                    }

                    ind = 0;

                }
            }

            if (rbSegmentation.IsChecked == true)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    if (canvas.Children.Count > 1)
                    {
                        canvas.Children.RemoveRange(0, canvas.Children.Count);
                        canvas.Children.Add(MyImg);
                    }

                    Ellipse ellipse = new Ellipse();

                    ellipse.Stroke = SystemColors.WindowFrameBrush;
                    ellipse.Height = 5;
                    ellipse.Width = 5;
                    SolidColorBrush greenBrush = new SolidColorBrush();
                    greenBrush.Color = Colors.Red;
                    ellipse.Fill = greenBrush;
                    ellipse.Margin = new Thickness(currentPoint.X, currentPoint.Y, 0, 0);
                    //pointsDistance[ind] = currentPoint.X;
                    //pointsDistance[ind + 1] = currentPoint.Y;
                    canvas.Children.Add(ellipse);
                    //ind += 2;
                }
                }
            }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (rbDrawing.IsChecked == true)
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

        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (rbDrawing.IsChecked == true)
            {
                if (e.LeftButton == MouseButtonState.Released)
                {
                    if (currentPoint != startPoint)
                    {
                        yellowGreenBrush.Color = Colors.YellowGreen;

                        if (canvasSegm.Children.Contains(polygon))
                        {
                            canvasSegm.Children.Remove(polygon);
                            polygon.Points.Clear();
                            pc.Clear();
                        }

                        foreach (var p in points)
                        {
                            pc.Add(p);
                        }

                        polygon.Points = pc;
                        polygon.Stroke = yellowGreenBrush;
                        polygon.Fill = yellowGreenBrush;

                        if (canvasSegm.Children.Contains(MyImg2))
                            canvasSegm.Children.Remove(MyImg2);

                        canvasSegm.Children.Add(MyImg2);
                        canvasSegm.Children.Add(polygon);
                        string path = SimpleITKHelper.GetFolderName(MyImg.Source) + "markedMask" + SimpleITKHelper.GetDicomFileName(MyImg.Source) + ".bmp";

                        CreateSaveBitmap(canvasSegm, path);

                        area = (Math.Abs(points.Take(points.Count - 1).Select((p, i) => (points[i + 1].X - p.X)
                          * (points[i + 1].Y + p.Y)).Sum() / 2)) * Math.Pow(pixelSpacing, 2);
                        var xx = points.ToString();
                        L_Area.Content = "Pole: " + Math.Round(area, 2) + "mm^2";

                        points.Clear();
                    }
                }

                else if (rbSegmentation.IsChecked == true)
                {
                    if (e.LeftButton == MouseButtonState.Released)
                    {
                        Ellipse ellipse = new Ellipse();

                        ellipse.Stroke = SystemColors.WindowFrameBrush;
                        ellipse.Height = 5;
                        ellipse.Width = 5;
                        SolidColorBrush greenBrush = new SolidColorBrush();
                        greenBrush.Color = Colors.Green;
                        ellipse.Fill = greenBrush;
                        ellipse.Margin = new Thickness(currentPoint.X, currentPoint.Y, 0, 0);

                        canvas.Children.Add(ellipse);
                    }
                }

            }
        }

        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (bZoomClicked)
            {
                double scaleRate = 1.2;

                if (e.Delta > 0)
                {
                    st.ScaleX *= scaleRate;
                    st.ScaleY *= scaleRate;
                }
                else
                {
                    st.ScaleX /= scaleRate;
                    st.ScaleY /= scaleRate;
                }

                canvas.LayoutTransform = st;
                canvasSegm.LayoutTransform = st;
            }
        }

        private void CanvasSegm_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //if (e.ButtonState == MouseButtonState.Pressed)
            //{
            //    startPoint = e.GetPosition(canvasSegm);
            //    currentPoint = startPoint;
            //    points.Add(currentPoint);
            //}

            //if (bDistanceClicked)
            //{
            //    if (ind < 4)
            //    {

            //        if (e.LeftButton == MouseButtonState.Pressed)
            //        {
            //            Ellipse ellipse = new Ellipse();

            //            ellipse.Stroke = SystemColors.WindowFrameBrush;
            //            ellipse.Height = 5;
            //            ellipse.Width = 5;
            //            SolidColorBrush greenBrush = new SolidColorBrush();
            //            greenBrush.Color = Colors.Red;
            //            ellipse.Fill = greenBrush;
            //            ellipse.Margin = new Thickness(currentPoint.X, currentPoint.Y, 0, 0);
            //            pointsDistance[ind] = currentPoint.X;
            //            pointsDistance[ind + 1] = currentPoint.Y;
            //            canvasSegm.Children.Add(ellipse);
            //            ind += 2;
            //        }

            //        if (ind == 4)
            //        {
            //            distance = (Math.Sqrt((Math.Pow(pointsDistance[0] - pointsDistance[2], 2) + Math.Pow(pointsDistance[1] - pointsDistance[3], 2)))) * pixelSpacing;
            //            L_Distance.Content = "Długość: " + Math.Round(distance, 2) + "mm";
            //        }

            //    }
            //    else
            //    {
            //        var length = canvasSegm.Children.Count;
            //        for (int i = length - 1; i >= length - 2; i--)
            //        {
            //            canvasSegm.Children.RemoveAt(i);
            //        }

            //        ind = 0;

            //    }
            //}
        }

        private void CanvasSegm_MouseMove(object sender, MouseEventArgs e)
        {
            if (bCorrectAreaClicked && canvasSegm.Children.Contains(polygon))
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {

                    Line line = new Line();
                    line.Stroke = SystemColors.WindowFrameBrush;
                    line.X1 = currentPoint.X;
                    line.Y1 = currentPoint.Y;
                    line.X2 = e.GetPosition(canvasSegm).X;
                    line.Y2 = e.GetPosition(canvasSegm).Y;

                    redBrush.Color = Colors.Red;

                    line.Stroke = redBrush;

                    currentPoint = e.GetPosition(canvasSegm);

                    points.Add(currentPoint);
                    canvasSegm.Children.Add(line);
                }
            }

        }

        private void CanvasSegm_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (bCorrectAreaClicked && canvasSegm.Children.Contains(polygon))
            {
                if (e.LeftButton == MouseButtonState.Released)
                {
                    if (currentPoint != startPoint)
                    {

                        yellowGreenBrush.Color = Colors.YellowGreen;

                        if (canvasSegm.Children.Contains(polygon))
                        {
                            canvasSegm.Children.Remove(polygon);
                            polygon.Points.Clear();
                            pc.Clear();
                        }

                        foreach (var p in points)
                        {
                            pc.Add(p);
                        }

                        polygon.Points = pc;
                        polygon.Stroke = yellowGreenBrush;
                        polygon.Fill = yellowGreenBrush;

                        canvasSegm.Children.Remove(MyImg1);
                        canvasSegm.Children.Add(polygon);

                        string pathBmp = SimpleITKHelper.GetFolderName(MyImg.Source) + "markedMask" + SimpleITKHelper.GetDicomFileName(MyImg.Source) + ".bmp";
                        string pathDicom = SimpleITKHelper.GetFolderName(MyImg.Source) + "markedMask" + SimpleITKHelper.GetDicomFileName(MyImg.Source) + ".dcm";

                        SimpleITKHelper.ConvertToDicom(pathDicom, MyImg.Source, L_SelectedID.Content.ToString(), L_SelectedName.Content.ToString());

                        CreateSaveBitmap(canvasSegm, pathBmp);

                        BSaveMask.IsEnabled = true;

                        area = (Math.Abs(points.Take(points.Count - 1).Select((p, i) => (points[i + 1].X - p.X)
                          * (points[i + 1].Y + p.Y)).Sum() / 2)) * Math.Pow(pixelSpacing, 2);
                        var xx = points.ToString();
                        L_Area.Content = "Pole: " + Math.Round(area, 2) + "mm^2";

                        points.Clear();

                    }


                }
            }

            else if (rbSegmentation.IsChecked == true)
            {
                if (e.LeftButton == MouseButtonState.Released)
                {
                    if (canvasSegm.Children.Count > 1)
                    {
                        canvasSegm.Children.RemoveRange(0, canvasSegm.Children.Count);
                        canvasSegm.Children.Add(MyImg3);
                    }

                    Ellipse ellipse = new Ellipse();

                    ellipse.Stroke = SystemColors.WindowFrameBrush;
                    ellipse.Height = 5;
                    ellipse.Width = 5;
                    SolidColorBrush greenBrush = new SolidColorBrush();
                    greenBrush.Color = Colors.Green;
                    ellipse.Fill = greenBrush;
                    ellipse.Margin = new Thickness(currentPoint.X, currentPoint.Y, 0, 0);

                    canvasSegm.Children.Add(ellipse);
                }
            }

        }
        private void CanvasSegm_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (bZoomClicked)
            {
                double scaleRate = 1.2;

                if (e.Delta > 0)
                {
                    st.ScaleX *= scaleRate;
                    st.ScaleY *= scaleRate;
                }
                else
                {
                    st.ScaleX /= scaleRate;
                    st.ScaleY /= scaleRate;
                }

                canvas.LayoutTransform = st;
                canvasSegm.LayoutTransform = st;
            }
        }
        private void CreateSaveBitmap(Canvas canvas, string filename)
        {
            int height = (int)MyImg.Width;
            int width = (int)MyImg.Height;
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(width, height, 96d, 96d, PixelFormats.Pbgra32);

            canvas.Measure(new Size(width, height));
            canvas.Arrange(new Rect(new Size(width, height)));
            var index = canvas.Children.IndexOf(polygon);
            var mask = canvas.Children[index];

            if (bPrintClicked)
            {
                renderBitmap.Render(canvas);
            }
            else
            {
                renderBitmap.Render(mask);
            }

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

            List<string> patientList = new List<string>();
            gdcm.ERootType type = gdcm.ERootType.ePatientRootType;

            gdcm.EQueryLevel level = gdcm.EQueryLevel.ePatient;

            gdcm.KeyValuePairArrayType keys = new gdcm.KeyValuePairArrayType();
            gdcm.KeyValuePairType key = new gdcm.KeyValuePairType(new gdcm.Tag(0x0010, 0x0010), "");
            keys.Add(key);
            keys.Add(new gdcm.KeyValuePairType(new gdcm.Tag(0x0010, 0x0020), Properties.Settings.Default.SelectedPatientID));
            keys.Add(new gdcm.KeyValuePairType(new gdcm.Tag(0x0010, 0x0030), ""));
            keys.Add(new gdcm.KeyValuePairType(new gdcm.Tag(0x0010, 0x0040), ""));
            keys.Add(new gdcm.KeyValuePairType(new gdcm.Tag(0x0008, 0x103E), ""));

            gdcm.BaseRootQuery query = gdcm.CompositeNetworkFunctions.ConstructQuery(type, level, keys);
            dataArray = new gdcm.DataSetArrayType();

            bool status = gdcm.CompositeNetworkFunctions.CFind(Properties.Settings.Default.IP, ushort.Parse(Properties.Settings.Default.Port), query, dataArray, Properties.Settings.Default.AET, Properties.Settings.Default.AEC);

            int i = 0;

            foreach (var path in files)
            {
                gdcm.ImageReader reader = new gdcm.ImageReader();
                gdcm.ImageWriter writer = new gdcm.ImageWriter();
                gdcm.File file;
                gdcm.Image image;

                reader.SetFileName(path);

                if (!reader.Read())
                {
                    T_Description.Text = "Nie wczytano pliku";
                }

                file = reader.GetFile();
                image = reader.GetImage();
                gdcm.DataSet ds = file.GetDataSet();

                byte[] descriptionTxt = Encoding.ASCII.GetBytes(T_Description.Text);
                gdcm.DataElement de = ds.GetDataElement(new gdcm.Tag(0x0008, 0x103E));
                de.SetTag(new gdcm.Tag(0x0008, 0x103E));
                de.SetByteValue(descriptionTxt, new gdcm.VL((uint)descriptionTxt.Length));
                ds.Insert(de);

                writer.CheckFileMetaInformationOn();
                writer.SetFileName(path);
                writer.SetFile(file);
                writer.SetImage(image);
                writer.Write();

            }

            bool statusStore = gdcm.CompositeNetworkFunctions.CStore(Properties.Settings.Default.IP, ushort.Parse(Properties.Settings.Default.Port), new gdcm.FilenamesType(files), Properties.Settings.Default.AET, Properties.Settings.Default.AEC);

        }

        private void BDescriptionAnuluj_Click(object sender, RoutedEventArgs e)
        {
            T_Description.IsReadOnly = true;
            BEditDescription.Visibility = Visibility.Visible;
            BDescriptionAnuluj.Visibility = Visibility.Hidden;
            BDescriptionOK.Visibility = Visibility.Hidden;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }

        // String name = String.Format("{0}_segmented.bmp", imagePath);
        private void RbSegmentation_Checked(object sender, RoutedEventArgs e)
        {
            rbDrawing.IsChecked = false;
        }

        private void RbDrawing_Checked(object sender, RoutedEventArgs e)
        {
            rbSegmentation.IsChecked = false;
        }

        private void BSegmentation_Click(object sender, RoutedEventArgs e)
        {
            Segmentation(MyImg3.Source);
        }

        private void Segmentation(ImageSource image)
        {
            if (rbSegmentation.IsChecked == true)
            {
                string imagePath = SimpleITKHelper.GetFolderName(image) + "segmentedMask" + SimpleITKHelper.GetDicomFileName(image) + ".dcm";
                var dicom = dicomList.ElementAt(numberOfImage);
                string numberOfInstance = SimpleITKHelper.GetInstanceNumber(dicom);

                area = SimpleITKHelper.SegmentArea(currentPoint, image, numberOfInstance, L_SelectedID.Content.ToString(), L_SelectedName.Content.ToString());
                BSaveMask.IsEnabled = true;
                area = area * Math.Pow(pixelSpacing, 2);
                L_Area.Content = "Pole: " + Math.Round(area) + "mm^2";

                gdcm.PixmapReader reader = new gdcm.PixmapReader();
                reader.SetFileName(imagePath);
                if (!reader.Read())
                {
                    MessageBox.Show("Opuszczam plik {0}", imagePath);
                }

                String name = String.Format("{0}_segmented.jpg", imagePath);

                gdcm.Bitmap bmjpeg2000 = BitmapHelper.pxmap2jpeg2000(reader.GetPixmap());
                System.Drawing.Bitmap[] X = BitmapHelper.gdcmBitmap2Bitmap(bmjpeg2000);


                BitmapData bmd = X[0].LockBits(new System.Drawing.Rectangle(0, 0, (int)X[0].Height, (int)X[0].Width),
                                                     ImageLockMode.ReadOnly, X[0].PixelFormat);
                X[0].UnlockBits(bmd);

                for (int i = 0; i < X[0].Height; i++)
                {
                    for (int j = 0; j < X[0].Width; j++)
                    {
                        var color = X[0].GetPixel(i, j);
                        var nazwa = X[0].GetPixel(i, j).Name;
                        if (X[0].GetPixel(i, j).Name != "ffcbcbcb")
                        {
                            X[0].SetPixel(i, j, System.Drawing.Color.YellowGreen);
                        }
                        else
                        {
                            X[0].SetPixel(i, j, System.Drawing.Color.Transparent);
                        }
                    }
                }

                //if (SimpleITKHelper.sizeOfMask > 2000)
                //{
                //    for (int i = 0; i < X[0].Height; i++)
                //    {
                //        for (int j = 0; j < X[0].Width; j++)
                //        {
                //            X[0].SetPixel(i, j, System.Drawing.Color.Transparent);
                //        }
                //    }
                //}

                MyImg2.Width = 280;
                MyImg2.Height = 280;
                MyImg2.Source = BitmapHelper.LoadBitmapImage(numberOfImage, bitmapList);

                if (canvasSegm.Children.Contains(MyImg2))
                    canvasSegm.Children.Remove(MyImg2);

                canvasSegm.Children.Add(MyImg2);

                X[0].Save(name);

                MyImg1.Width = 280;
                MyImg1.Height = 280;

                MyImg1.Source = BitmapHelper.LoadBitmapImage(name);

                if (canvasSegm.Children.Contains(MyImg1))
                    canvasSegm.Children.Remove(MyImg1);

                AllMasks[numberOfImage] = name;

                canvasSegm.Children.Add(MyImg1);
            }
        }


        private void BDistance_Click(object sender, RoutedEventArgs e)
        {
            bDistanceClicked = true;
        }

        private void BPrevious_Click(object sender, RoutedEventArgs e)
        {
            numberOfImage -= 1;
            if (numberOfImage > 0)
            {
                ImageBitmap = BitmapHelper.LoadBitmapImage(numberOfImage, bitmapList);
                MyImg.Source = ImageBitmap;
                if (SContrast.Value != 0)
                    ChangeContrast();
                if (SBrightness.Value != 0)
                    ChangeBrightness();
                MyImg3.Source = BitmapHelper.LoadBitmapImage(numberOfImage, bitmapList);
                MyImg1.Source = BitmapHelper.LoadBitmapImage(numberOfImage, bitmapList);
                ShowMask();
                Segmentation(MyImg3.Source);
                IPrevious.Source = BitmapHelper.LoadBitmapImage(numberOfImage - 1, bitmapList);
                INext.Source = BitmapHelper.LoadBitmapImage(numberOfImage + 1, bitmapList);
            }
            else
            {
                ImageBitmap = BitmapHelper.LoadBitmapImage(numberOfImage + 1, bitmapList);
                MyImg.Source = ImageBitmap;
                if (SContrast.Value != 0)
                    ChangeContrast();
                if (SBrightness.Value != 0)
                    ChangeBrightness();
                MyImg3.Source = BitmapHelper.LoadBitmapImage(numberOfImage + 1, bitmapList);
                MyImg1.Source = BitmapHelper.LoadBitmapImage(numberOfImage + 1, bitmapList);

                if (numberOfImage != -1)
                {
                    ShowMask();

                    Segmentation(MyImg3.Source);
                }

                string path = ".\\tlo.bmp";
                IPrevious.Source = BitmapHelper.LoadBitmapImage(path);
                INext.Source = BitmapHelper.LoadBitmapImage(numberOfImage + 2, bitmapList);
                numberOfImage = 0;
            }
        }

        private void BZoom_Click(object sender, RoutedEventArgs e)
        {
            bZoomClicked = true;
            sv.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            sv2.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            sv2.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
        }

        private void BCorrectArea_Click(object sender, RoutedEventArgs e)
        {
            bCorrectAreaClicked = true;
        }
        double changeValue = 0;
        private void SBrightness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ChangeBrightness();
        }

        public BitmapImage ConvertWriteableBitmapToBitmapImage(WriteableBitmap wbm)
        {
            BitmapImage bmImage = new BitmapImage();
            using (MemoryStream stream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(wbm));
                encoder.Save(stream);
                bmImage.BeginInit();
                bmImage.CacheOption = BitmapCacheOption.OnLoad;
                bmImage.StreamSource = stream;
                bmImage.EndInit();
                bmImage.Freeze();
            }
            return bmImage;
        }

        private void ChangeBrightness()
        {
            changeValue = Math.Round(SBrightness.Value);

            BitmapImage img = ImageBitmap;

            double rows = img.PixelHeight;
            double columns = img.PixelWidth;
            WriteableBitmap wbitmap = new WriteableBitmap((int)columns, (int)rows, 96, 96, PixelFormats.Bgra32, null);
            int stride = img.PixelWidth * 4;
            int size = img.PixelHeight * stride;
            byte[] pixels = new byte[size];
            img.CopyPixels(pixels, stride, 0);
            for (int y = 0; y < img.PixelHeight; y++)
            {
                for (int x = 0; x < img.PixelWidth; x++)
                {
                    int index = y * stride + 4 * x;
                    if (pixels[index] + changeValue > 255)
                    {
                        pixels[index] = (byte)255;
                        pixels[index + 1] = (byte)255;
                        pixels[index + 2] = (byte)255;
                    }
                    else
                    {
                        if (pixels[index] + changeValue < 0)
                        {
                            pixels[index] = (byte)0;
                            pixels[index + 1] = (byte)0;
                            pixels[index + 2] = (byte)0;
                        }
                        else
                        {
                            pixels[index] = (byte)(pixels[index] + changeValue);
                            pixels[index + 1] = (byte)(pixels[index + 1] + changeValue);
                            pixels[index + 2] = (byte)(pixels[index + 2] + changeValue);
                        }
                    }
                    Int32Rect rect = new Int32Rect(0, 0, x, y);
                    wbitmap.WritePixels(rect, pixels, stride, 0);
                }
            }
            ImageBitmap = ConvertWriteableBitmapToBitmapImage(wbitmap);
            MyImg.Source = ImageBitmap;
        }

        double changeContastValue = 0;
        private void SContrast_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SContrast.Value != 0)
                ChangeContrast();
        }

        private void ChangeContrast()
        {
            changeContastValue = Math.Round(SContrast.Value);

            BitmapImage img = ImageBitmap;

            double rows = img.PixelHeight;
            double columns = img.PixelWidth;
            WriteableBitmap wbitmap = new WriteableBitmap((int)columns, (int)rows, 96, 96, PixelFormats.Bgra32, null);
            int stride = img.PixelWidth * 4;
            int size = img.PixelHeight * stride;
            byte[] pixels = new byte[size];
            img.CopyPixels(pixels, stride, 0);
            for (int y = 0; y < img.PixelHeight; y++)
            {
                for (int x = 0; x < img.PixelWidth; x++)
                {
                    int index = y * stride + 4 * x;
                    if (pixels[index] >= 128)
                    {
                        if (pixels[index] + changeContastValue > 255)
                        {
                            pixels[index] = (byte)255;
                            pixels[index + 1] = (byte)255;
                            pixels[index + 2] = (byte)255;
                        }
                        else
                        {
                            pixels[index] = (byte)(pixels[index] + changeContastValue);
                            pixels[index + 1] = (byte)(pixels[index + 1] + changeContastValue);
                            pixels[index + 2] = (byte)(pixels[index + 2] + changeContastValue);
                        }
                    }
                    else
                    {
                        if (pixels[index] - changeContastValue < 0)
                        {
                            pixels[index] = (byte)0;
                            pixels[index + 1] = (byte)0;
                            pixels[index + 2] = (byte)0;
                        }
                        else
                        {
                            pixels[index] = (byte)(pixels[index] - changeContastValue);
                            pixels[index + 1] = (byte)(pixels[index + 1] - changeContastValue);
                            pixels[index + 2] = (byte)(pixels[index + 2] - changeContastValue);
                        }
                    }
                    Int32Rect rect = new Int32Rect(0, 0, x, y);
                    wbitmap.WritePixels(rect, pixels, stride, 0);
                }
            }
            ImageBitmap = ConvertWriteableBitmapToBitmapImage(wbitmap);
            MyImg.Source = ImageBitmap;
        }

        private void BResetImage_Click(object sender, RoutedEventArgs e)
        {
            ImageBitmap = BitmapHelper.LoadBitmapImage(numberOfImage, bitmapList);
            MyImg.Source = ImageBitmap;
            SContrast.Value = 0;
            SBrightness.Value = 0;
        }

        private void BClearArea_Click(object sender, RoutedEventArgs e)
        {
            canvas.Children.RemoveRange(0, canvas.Children.Count);
            canvasSegm.Children.RemoveRange(0, canvasSegm.Children.Count);
            canvasSegm.Children.Add(MyImg3);
            canvas.Children.Add(MyImg);
        }

        private void BSaveMask_Click(object sender, RoutedEventArgs e)
        {
            bool moveResult = GDCMHelper.MoveImageByPath(globalpath);

            if (moveResult)
            {
                MessageBox.Show("Udało się zapisać maskę", "Informacja", MessageBoxButton.OK);
            }
            else
            {
                MessageBox.Show("Nie udało się zapisać maski", "Błąd", MessageBoxButton.OK);
            }
        }

        private void CbMask_Checked(object sender, RoutedEventArgs e)
        {
            List<string> files = new List<string>(System.IO.Directory.EnumerateFiles(globalpath));

            int i = 0;
            foreach (String fileDcm in files)
            {
                gdcm.Reader reader = new gdcm.Reader();
                reader.SetFileName(fileDcm);
                bool ret = reader.Read();
                gdcm.File file = reader.GetFile();

                gdcm.StringFilter filter = new gdcm.StringFilter();
                filter.SetFile(file);
                string numberInSeries = filter.ToString(new gdcm.Tag(0x0020, 0x0013));
                string description = filter.ToString(new gdcm.Tag(0x0008, 0x103E));

                if (!string.IsNullOrEmpty(numberInSeries) && description == "mask")
                {
                    int numer = int.Parse(numberInSeries);
                    if (numer > i)
                        i = numer;
                }
            }

            string[] tempPaths = new string[i + 1];

            if (maskNumbersOfInstance.Count != 0)
                maskNumbersOfInstance.Clear();

            foreach (String fileDcm in files)
            {
                // fileDcm to ścieżka do pliku zapisanego i ściągnętego z bazy
                gdcm.Reader reader = new gdcm.Reader();
                reader.SetFileName(fileDcm);
                bool ret = reader.Read();
                gdcm.File file = reader.GetFile();

                gdcm.StringFilter filter = new gdcm.StringFilter();
                filter.SetFile(file);
                string numberInSeries = filter.ToString(new gdcm.Tag(0x0020, 0x0013));
                string description = filter.ToString(new gdcm.Tag(0x0008, 0x103E));

                if (!string.IsNullOrEmpty(numberInSeries) && description == "mask")
                {
                    if (description == "mask")
                    {
                        int numer = int.Parse(numberInSeries);
                        maskNumbersOfInstance.Add(numer);
                        tempPaths[numer] = fileDcm;
                        dicomList.Add(fileDcm);
                    }
                    else
                    {

                        maskNumbersOfInstance.Add(0);
                    }
                }

            }

            int j = 1000;
            i = 0;
            if (masksNames.Count != 0)
                masksNames.Clear();

            foreach (String fileDcm in tempPaths)
            {
                if (!string.IsNullOrEmpty(fileDcm))
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

                    BitmapData bmd = X[0].LockBits(new System.Drawing.Rectangle(0, 0, (int)X[0].Height, (int)X[0].Width),
                                                     ImageLockMode.ReadOnly, X[0].PixelFormat);
                    X[0].UnlockBits(bmd);

                    for (int h = 0; h < X[0].Height; h++)
                    {
                        for (int w = 0; w < X[0].Width; w++)
                        {
                            var color = X[0].GetPixel(h, w);
                            var nazwa = X[0].GetPixel(h, w).Name;
                            if (X[0].GetPixel(h, w).Name != "ffcbcbcb")
                            {
                                X[0].SetPixel(h, w, System.Drawing.Color.Tomato);
                            }
                            else
                            {
                                X[0].SetPixel(h, w, System.Drawing.Color.Transparent);
                            }
                        }
                    }

                    String name = String.Format("{0}\\mask{1}.jpg", data, j - 1);
                    
                    masksNames.Add(name);

                    X[0].Save(name);
                }
                j++;
            }


            MyImg1.Width = 280;
            MyImg1.Height = 280;

            var isAscending = maskNumbersOfInstance.OrderBy(x => x);
            if (!maskNumbersOfInstance.SequenceEqual(isAscending))
                maskNumbersOfInstance.Sort();

            if (!globalNumbersOfInstance.SequenceEqual(isAscending))
                globalNumbersOfInstance.Sort();

            if (maskNumbersOfInstance.Count != globalNumbersOfInstance.Count)
            {
                for (int k = 0; k < globalNumbersOfInstance.Count; k++)
                {
                    if (maskNumbersOfInstance.Count <= k || maskNumbersOfInstance[k] != globalNumbersOfInstance[k])
                    {
                        maskNumbersOfInstance.Insert(k, -1);
                        masksNames.Insert(k, "");
                    }
                }
            }

            if (canvasSegm.Children.Contains(MyImg2))
                canvasSegm.Children.Remove(MyImg2);

            canvasSegm.Children.Add(MyImg2);

            ShowMask();

            //if (!globalNumbersOfInstance.SequenceEqual(isAscending))
            //{
            //    masksNames.Reverse();
            //    maskReverted = true;
            //}

        }

        private void CbMask_Unchecked(object sender, RoutedEventArgs e)
        {
            MyImg1.Width = 280;
            MyImg1.Height = 280;
            canvasSegm.Children.Remove(MyImg1);
        }

        private void BNext_Click(object sender, RoutedEventArgs e)
        {
            numberOfImage += 1;

            if (numberOfImage < bitmapList.Count)
            {
                ImageBitmap = BitmapHelper.LoadBitmapImage(numberOfImage, bitmapList);
                MyImg.Source = ImageBitmap;
                if (SContrast.Value != 0)
                    ChangeContrast();
                if (SBrightness.Value != 0)
                    ChangeBrightness();
                MyImg3.Source = BitmapHelper.LoadBitmapImage(numberOfImage, bitmapList);
                ShowMask();
                //MyImg1.Source = BitmapHelper.LoadBitmapImage(numberOfImage, bitmapList);
                Segmentation(MyImg3.Source);
                IPrevious.Source = BitmapHelper.LoadBitmapImage(numberOfImage - 1, bitmapList);
                if (numberOfImage == bitmapList.Count - 1)
                {
                    INext.Source = BitmapHelper.LoadBitmapImage(pathEmpty);//pusty
                }
                else
                {
                    INext.Source = BitmapHelper.LoadBitmapImage(numberOfImage + 1, bitmapList);
                }

            }
            else
            {
                ImageBitmap = BitmapHelper.LoadBitmapImage(numberOfImage - 1, bitmapList);
                MyImg.Source = ImageBitmap;
                if (SContrast.Value != 0)
                    ChangeContrast();
                if (SBrightness.Value != 0)
                    ChangeBrightness();
                MyImg3.Source = BitmapHelper.LoadBitmapImage(numberOfImage - 1, bitmapList);

                if (numberOfImage < bitmapList.Count)
                {
                    ShowMask();

                    Segmentation(MyImg3.Source);
                }

                IPrevious.Source = BitmapHelper.LoadBitmapImage(numberOfImage - 2, bitmapList);
                INext.Source = BitmapHelper.LoadBitmapImage(pathEmpty); //pusty
                numberOfImage = bitmapList.Count - 1;
            }
        }
    }
}

