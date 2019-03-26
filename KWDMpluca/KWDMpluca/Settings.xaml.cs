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

namespace KWDMpluca
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            // Wyświetlenie obrazka o określonej ścieżce na określonym przycisku
            BitmapImage bitimg = new BitmapImage();
            bitimg.BeginInit();
            bitimg.UriSource = new Uri("image/Close.png", UriKind.RelativeOrAbsolute);
            bitimg.EndInit();
            Image img = new Image();
            img.Stretch = Stretch.Fill;
            img.Source = bitimg;
            B_Close.Content = img;

            T_AET.Text = Properties.Settings.Default.AET;
            T_AEC.Text = Properties.Settings.Default.AEC;
            T_IP.Text = Properties.Settings.Default.IP;
            T_PORT.Text = Properties.Settings.Default.Port;
        }

        private void B_Close_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AET = T_AET.Text;
            Properties.Settings.Default.AEC = T_AEC.Text;
            Properties.Settings.Default.IP = T_IP.Text;
            Properties.Settings.Default.Port = T_PORT.Text;
            Properties.Settings.Default.Save();
            this.Close();
        }

        private void BSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AET = T_AET.Text;
            Properties.Settings.Default.AEC = T_AEC.Text;
            Properties.Settings.Default.IP = T_IP.Text;
            Properties.Settings.Default.Port = T_PORT.Text;
            Properties.Settings.Default.Save();
            LInfo.Content = "Zapisano dane";
        }

        private void BCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BCheck_Click(object sender, RoutedEventArgs e)
        {

            bool stan = gdcm.CompositeNetworkFunctions.CEcho(T_IP.Text, ushort.Parse(T_PORT.Text), T_AET.Text, T_AEC.Text);
            if (stan)
                EConnect.Fill = Brushes.Green;
            else
                EConnect.Fill = Brushes.Red;
        }
    }
}
