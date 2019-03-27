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
    /// Interaction logic for FindPatient.xaml
    /// </summary>
    public partial class FindPatient : Window
    {

        public FindPatient()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InsertImage("image/Close.png", B_Close);
            InsertImage("image/SaveSearch.png", B_Select);
            InsertImage("image/Search2.png", B_Search);
            CB_Before.IsEnabled = false;
            CB_After.IsEnabled = false;
            T_OtherSearchValue.IsEnabled = false;
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

        private void B_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void B_Search_Click(object sender, RoutedEventArgs e)
        {
            if ((CB_IDSearch.IsChecked == false && CB_NameSearch.IsChecked == false) ||
                (CB_All.IsChecked == false && CB_Other.IsChecked == false))
            {
                L_Info.Visibility = Visibility.Visible;
                return;
            }
            else
                L_Info.Visibility = Visibility.Hidden; 
            string searchValue = "";
            ushort searchType;
            if (CB_IDSearch.IsChecked == true)
                searchType = 0x0020;
            else
                searchType = 0x0010;

            if (CB_All.IsChecked == false)
            {
                searchValue = T_OtherSearchValue.Text;
                if (CB_Before.IsChecked == true)
                    searchValue = "*" + searchValue;
                if(CB_After.IsChecked == true)
                    searchValue = searchValue + "*";
            }
            SearchPatient(0x0010, searchType, searchValue);
        }

        public string[,] PatientInfo;
        private void SearchPatient(ushort a, ushort b, string searchValue)
        {
            
            List<string> patientList = new List<string>();
            gdcm.ERootType type = gdcm.ERootType.ePatientRootType;

            gdcm.EQueryLevel level = gdcm.EQueryLevel.ePatient;

            gdcm.KeyValuePairArrayType keys = new gdcm.KeyValuePairArrayType();
            if (b == 0x0020)
            {
                keys.Add(new gdcm.KeyValuePairType(new gdcm.Tag(0x0010, 0x0020), searchValue));
                keys.Add(new gdcm.KeyValuePairType(new gdcm.Tag(0x0010, 0x0010), ""));
            }
            else
            {
                keys.Add(new gdcm.KeyValuePairType(new gdcm.Tag(0x0010, 0x0010), searchValue));
                keys.Add(new gdcm.KeyValuePairType(new gdcm.Tag(0x0010, 0x0020), ""));
            }
            keys.Add(new gdcm.KeyValuePairType(new gdcm.Tag(0x0010, 0x0030), ""));
            keys.Add(new gdcm.KeyValuePairType(new gdcm.Tag(0x0010, 0x0040), ""));

            gdcm.BaseRootQuery query = gdcm.CompositeNetworkFunctions.ConstructQuery(type, level, keys);

            gdcm.DataSetArrayType dataArray = new gdcm.DataSetArrayType();

            bool status = gdcm.CompositeNetworkFunctions.CFind(Properties.Settings.Default.IP, ushort.Parse(Properties.Settings.Default.Port), query, dataArray, Properties.Settings.Default.AET, Properties.Settings.Default.AEC);
            int i = 0;
            foreach (gdcm.DataSet x in dataArray)
            {
                patientList.Add(x.GetDataElement(new gdcm.Tag(a, b)).GetValue().toString());
                i++;
            }
            PatientInfo = new string[i,3];
            int j = 0;
            foreach (gdcm.DataSet x in dataArray)
            {
                PatientInfo[j, 0] = x.GetDataElement(new gdcm.Tag(0x0010, 0x0020)).GetValue().toString();
                PatientInfo[j, 1] = x.GetDataElement(new gdcm.Tag(0x0010, 0x0010)).GetValue().toString();
                PatientInfo[j, 2] = x.GetDataElement(new gdcm.Tag(0x0010, 0x0030)).GetValue().toString(); // Data urodzenia
                //PatientInfo[j, 3] = x.GetDataElement(new gdcm.Tag(0x0010, 0x0040)).GetValue().toString(); // Płeć
                j++;
            }
            
            L_Patient.ItemsSource = patientList;
        }

        private void L_Patient_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Zapobieganie sprawdzenia wyboru podczas zmiany wyświetlanej listy.
            if (L_Patient.SelectedIndex >= 0)
            {
                int number = L_Patient.SelectedIndex;
                L_SelectedID.Content = PatientInfo[number, 0];
                L_SelectedName.Content = PatientInfo[number, 1];
                L_SelectedDateB.Content = PatientInfo[number, 2];
            }
            
        }

        private void B_Select_Click(object sender, RoutedEventArgs e)
        {
            if(L_Patient.SelectedIndex>=0)
            {
                int number = L_Patient.SelectedIndex;
                Properties.Settings.Default.SelectedPatientID = PatientInfo[number, 0];
                Properties.Settings.Default.Save();
            }
        }

        private void CB_After_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CB_Before_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CB_ID_Checked(object sender, RoutedEventArgs e)
        {
            CB_NameSearch.IsChecked = false;
        }

        private void CB_Name_Checked(object sender, RoutedEventArgs e)
        {
            CB_IDSearch.IsChecked = false;
        }

        private void CB_All_Checked(object sender, RoutedEventArgs e)
        {
            CB_Other.IsChecked = false;
            CB_Before.IsEnabled = false;
            CB_After.IsEnabled = false;
            T_OtherSearchValue.IsEnabled = false;
        }

        private void CB_Other_Checked(object sender, RoutedEventArgs e)
        {
            CB_All.IsChecked = false;
            CB_Before.IsEnabled = true;
            CB_After.IsEnabled = true;
            T_OtherSearchValue.IsEnabled = true;
        }

    }
}
