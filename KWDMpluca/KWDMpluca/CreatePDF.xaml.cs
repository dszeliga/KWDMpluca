using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.VisualBasic;
using System.Windows.Media;

namespace KWDMpluca
{
    /// <summary>
    /// Interaction logic for CreatePDF.xaml
    /// </summary>
    public partial class CreatePDF : Window
    {
        ImageSource imageDicom;
        public CreatePDF()
        {
            InitializeComponent();

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

            foreach (gdcm.DataSet x in dataArray)
            {
                L_SelectedID.Content = x.GetDataElement(new gdcm.Tag(0x0010, 0x0020)).GetValue().toString();
                T_SelectedName.Text = x.GetDataElement(new gdcm.Tag(0x0010, 0x0010)).GetValue().toString();
                T_SelectedDateB.Text = x.GetDataElement(new gdcm.Tag(0x0010, 0x0030)).GetValue().toString();
            }
        }

        public CreatePDF(ImageSource image) : this()
        {
            imageDicom = image;
        }

        private void B_AddImage_Click(object sender, RoutedEventArgs e)
        {
            //z pliku wczytanie
            //OpenFileDialog op = new OpenFileDialog();
            //op.Title = "Select a picture";
            //op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
            //  "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
            //  "Portable Network Graphic (*.png)|*.png";
            //if (op.ShowDialog() == true)
            //{
            //    //IImage.Source = new BitmapImage(new Uri(op.FileName));

            //}

            //wczytanie z podglądu 
            IImage.Source = imageDicom;

                       

        }

        private void B_Generate_Click(object sender, RoutedEventArgs e)
        {
            //generowanie dokumentu
            var pdf = new Document(PageSize.LETTER, 40f, 40f, 60f, 60f);
            //sciezka do pliku
            string path = $".\\raport.pdf";
            //string name = "";
            //if (File.Exists(path))
            //{
            //    name = Interaction.InputBox("Raport o tej nazwie już istnieje, czy nadpisać dokument?", "Raport już istnieje","raport", -1,-1);
            //    path = $".\\"+name.ToString()+".pdf";
            //}
            
            //przerwa między kolejnymi liniami
            var spacer = new Paragraph("")
            {
                SpacingBefore = 10f,
                SpacingAfter = 10f,
            };

            //stworzenie pdf
            PdfWriter.GetInstance(pdf, new FileStream(path, FileMode.OpenOrCreate));
            //otwarcie dokumentu (strumienia) - początek pisania raportu
            pdf.Open();
            //czcionka nagłówka z raportu
            Font headerMainFont = FontFactory.GetFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.EMBEDDED, 16, Font.BOLD, new BaseColor(0, 0, 0));
            //czcionki nagłówków sekcji
            Font header = FontFactory.GetFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.EMBEDDED, 12, Font.BOLD, new BaseColor(0, 0, 0));
            //czcionka w komórkach
            var myFont = FontFactory.GetFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.EMBEDDED, 12);

            var headerMain = new Paragraph("RAPORT Z BADANIA TOMOGRAFII KOMPUTEROWEJ PŁUC", headerMainFont);

            //dodanie daty badania
            var date = new Paragraph("Data badania: " + DateTime.Now.ToString(), header);
            pdf.Add(headerMain);
            pdf.Add(spacer);
            pdf.Add(date);
            pdf.Add(spacer);


            var headPatient = new Paragraph("1. Informacje o pacjencie", header);

            pdf.Add(headPatient);
            pdf.Add(spacer);

            //dodanie tabeli z danymi pacjenta
            var patientTable = new PdfPTable(new[] { .75f, 2f })
            {
                WidthPercentage = 75,
                DefaultCell = { MinimumHeight = 22f }
            };

            patientTable.AddCell("ID Pacjenta:");
            patientTable.AddCell(new Phrase(L_SelectedID.Content.ToString(), myFont));
            patientTable.AddCell("Imie i nazwisko:");
            patientTable.AddCell(new Phrase(T_SelectedName.Text, myFont));
            patientTable.AddCell("Data urodzenia:");
            patientTable.AddCell(T_SelectedDateB.Text);
            patientTable.AddCell("Plec:");
            patientTable.AddCell(T_Sex.Text);

            pdf.Add(patientTable);
            pdf.Add(spacer);

            var headDescription = new Paragraph("2. Szczegóły badania", header);

            pdf.Add(headDescription);
            pdf.Add(spacer);

            //dodanie tabeli z opisem badania
            var descriptionOfResearch = new PdfPTable(new[] { .75f, 2f })
            {
                WidthPercentage = 75,
                DefaultCell = { MinimumHeight = 22f }
            };
            descriptionOfResearch.AddCell("Opis badania:");
            descriptionOfResearch.AddCell(new Phrase(T_Description.Text, myFont));

            pdf.Add(descriptionOfResearch);
            pdf.Add(spacer);

            var headDoctor = new Paragraph("3. Informacje o lekarzu zlecającym", header);

            //dodanie tabeli z danymi lekarza
            var doctorTable = new PdfPTable(new[] { .75f, 2f })
            {
                WidthPercentage = 75,
                DefaultCell = { MinimumHeight = 22f }
            };

            doctorTable.AddCell("ID Lekarza:");
            doctorTable.AddCell(T_IDdoctor.Text);
            doctorTable.AddCell("Imię i nazwisko:");
            doctorTable.AddCell(new Phrase(T_DoctorName.Text, myFont));
            doctorTable.AddCell("Specjalność:");
            doctorTable.AddCell(new Phrase(T_DoctorSpeciality.Text, myFont));

            pdf.Add(headDoctor);
            pdf.Add(spacer);
            pdf.Add(doctorTable);
            pdf.Add(spacer);

            // dodanie zdjęcia z badania do załączników
            pdf.NewPage();
            var headAttachment = new Paragraph("Załączniki", headerMainFont);

            string imagePath = "";
            if (IImage.Source != null)
            {
                imagePath = IImage.Source.ToString();
            }

            if (imagePath != "")
            {
                //imagePath = imagePath.Remove(0, 8);
                imagePath = imagePath.Replace('/', '\\');
                var headImage = new Paragraph("2a. Analizowane zdjęcie", header);
                var image = Image.GetInstance(System.Drawing.Image.FromFile(imagePath), ImageFormat.Jpeg);
                image.ScalePercent(40);
                image.Alignment = iTextSharp.text.Image.ALIGN_CENTER;
                pdf.Add(headAttachment);
                pdf.Add(spacer);
                pdf.Add(headImage);
                pdf.Add(spacer);
                pdf.Add(image);
                pdf.Add(spacer);
            }
            //zamknięcie dokumentu (strumienia) - zakonczenie modyfikacji pdfa
            pdf.Close();
            //otwarcie pdfa w programie
            System.Diagnostics.Process.Start(path);
        }
    }
}
