using System;
using System.Windows;
using System.Windows.Media;
using sitk = itk.simple;
namespace KWDMpluca.Helpers
{
    public class SimpleITKHelper
    {
        public static int SegmentArea(Point point, ImageSource image)
        {

            //wczytanie pliku
            sitk.ImageFileReader imageFileReader = new sitk.ImageFileReader();
            imageFileReader.SetFileName(GetFolderName(image) + GetDicomFileName(image));
            imageFileReader.SetOutputPixelType(sitk.PixelIDValueEnum.sitkInt16);
            sitk.Image imageDicomOrg = imageFileReader.Execute();


            uint seedX = (uint.Parse(Math.Round(point.X).ToString()) * imageDicomOrg.GetWidth()) / 280;
            uint seedY = (uint.Parse(Math.Round(point.Y).ToString()) * imageDicomOrg.GetHeight()) / 280;
            uint seedZ = 0;//FindInstance();

            //wspolrzedne punktu
            sitk.VectorUInt32 seed = new sitk.VectorUInt32(new uint[] { seedX, seedY, seedZ });

            //odszumienie obrazu?
            sitk.CurvatureFlowImageFilter curvatureFlowImageFilter = new sitk.CurvatureFlowImageFilter();
            curvatureFlowImageFilter.SetNumberOfIterations(5);
            curvatureFlowImageFilter.SetTimeStep(0.125);
            imageDicomOrg = curvatureFlowImageFilter.Execute(imageDicomOrg);


            //zmiana typu piksela na int16
            sitk.CastImageFilter castImageFilter = new sitk.CastImageFilter();
            castImageFilter.SetOutputPixelType(sitk.PixelIDValueEnum.sitkInt16);
            imageDicomOrg = castImageFilter.Execute(imageDicomOrg);
            SaveImage(imageDicomOrg, GetFolderName(image) + "org16" + GetDicomFileName(image) + ".dcm");

            //progowanie
            sitk.BinaryThresholdImageFilter binthr = new sitk.BinaryThresholdImageFilter();
            binthr.SetLowerThreshold(100);
            binthr.SetUpperThreshold(150);
            binthr.SetOutsideValue(0);
            binthr.SetInsideValue(1);
            sitk.Image imageDicomOrg_beforeErode = binthr.Execute(imageDicomOrg);

            //erozja
            sitk.BinaryErodeImageFilter binaryErodeImageFilter = new sitk.BinaryErodeImageFilter();
            binaryErodeImageFilter.SetKernelRadius(2);
            binaryErodeImageFilter.SetBackgroundValue(0);
            binaryErodeImageFilter.SetForegroundValue(1);
            sitk.Image imageDicomErode = binaryErodeImageFilter.Execute(imageDicomOrg_beforeErode);

            SaveImage(imageDicomErode, GetFolderName(image) + "erode" + GetDicomFileName(image) + ".dcm");

            //otwarcie (erozja potem dylatacja )
            sitk.BinaryMorphologicalOpeningImageFilter binaryMorphologicalOpeningImageFilter = new sitk.BinaryMorphologicalOpeningImageFilter();
            binaryMorphologicalOpeningImageFilter.SetKernelRadius(4);
            binaryMorphologicalOpeningImageFilter.SetBackgroundValue(0);
            binaryMorphologicalOpeningImageFilter.SetForegroundValue(1);
            binaryMorphologicalOpeningImageFilter.SetKernelType(sitk.KernelEnum.sitkBall);
            sitk.Image imageDicomOpen = binaryMorphologicalOpeningImageFilter.Execute(imageDicomErode);

            SaveImage(imageDicomOpen, GetFolderName(image) + "open" + GetDicomFileName(image) + ".dcm");
            
            //rozrost ze wskazanego punktu
            sitk.ConfidenceConnectedImageFilter confidenceConnectedImageFilter = new sitk.ConfidenceConnectedImageFilter();
            confidenceConnectedImageFilter.AddSeed(seed);
            confidenceConnectedImageFilter.SetReplaceValue(255);
            confidenceConnectedImageFilter.SetInitialNeighborhoodRadius(1);
            confidenceConnectedImageFilter.SetNumberOfIterations(20);
            confidenceConnectedImageFilter.SetMultiplier(2);
            sitk.Image imageDicomSegmented = confidenceConnectedImageFilter.Execute(imageDicomOpen);

            //zmiana piksela na int16
            castImageFilter.SetOutputPixelType(sitk.PixelIDValueEnum.sitkInt16);
            imageDicomSegmented = castImageFilter.Execute(imageDicomSegmented);

            SaveImage(imageDicomSegmented, GetFolderName(image) + "segmentedMask" + GetDicomFileName(image) + ".dcm");


           //obliczenie pola guza w pikselach
            int area = 0;
            for (int i = 0; i < imageDicomSegmented.GetWidth(); i++)
            {
                for (int j = 0; j < imageDicomSegmented.GetHeight(); j++)
                {
                    seed[0] = (uint)i;
                    seed[1] = (uint)j;
                    var piksel = imageDicomSegmented.GetPixelAsInt16(seed);


                    if (piksel == 255)
                    {
                        area++;
                    }
                }
            }
            return area;
        }

        private static void SaveImage(sitk.Image image, string pathToFile)
        {
            sitk.ImageFileWriter writer = new sitk.ImageFileWriter();
            writer.SetFileName(pathToFile);
            writer.Execute(image);
        }

        public static string GetDicomFileName(ImageSource image)
        {
            if (image.ToString().Contains("pack"))
            {
                string[] pathSegments = image.ToString().Split('/');
                string[] fileNameSegments = pathSegments[5].Split('_');
                string dicomName = fileNameSegments[0];
                return dicomName;
            }
            else
            {
                string[] pathSegments = image.ToString().Split('\\');
                string[] fileNameSegments = pathSegments[3].Split('_');
                string dicomName = fileNameSegments[0];
                return dicomName;
            }
        }

        private static string GetImageFileName(ImageSource image)
        {
            string[] pathSegments = image.ToString().Split('\\');
            string[] fileNameSegments = pathSegments[3].Split('_');
            string sliceName = "segmentowana" + fileNameSegments[1];
            sliceName = fileNameSegments[0] + "_" + sliceName;
            return sliceName;
        }

        public static string GetFolderName(ImageSource image)
        {
            if (image.ToString().Contains("pack"))
            {
                string[] pathSegments = image.ToString().Split('/');
                string folderName = ".\\" + pathSegments[3] + "\\" + pathSegments[4] + "\\";
                return folderName;
            }
            else
            {
                string[] pathSegments = image.ToString().Split('\\');
                string folderName = pathSegments[0] + "\\" + pathSegments[1] + "\\" + pathSegments[2] + "\\";
                return folderName;
            }
        }

        //private static uint FindInstance()
        //{
        //    uint instanceID = 0;

        //    gdcm.ERootType type = gdcm.ERootType.ePatientRootType;

        //    gdcm.EQueryLevel level = gdcm.EQueryLevel.ePatient;

        //    gdcm.KeyValuePairArrayType keys = new gdcm.KeyValuePairArrayType();
        //    gdcm.KeyValuePairType key = new gdcm.KeyValuePairType(new gdcm.Tag(0x0020, 0x0013), "");
        //    keys.Add(key);
        //    keys.Add(new gdcm.KeyValuePairType(new gdcm.Tag(0x0010, 0x0020), Properties.Settings.Default.SelectedPatientID));

        //    gdcm.BaseRootQuery query = gdcm.CompositeNetworkFunctions.ConstructQuery(type, level, keys);

        //    gdcm.DataSetArrayType dataArray = new gdcm.DataSetArrayType();

        //    bool status = gdcm.CompositeNetworkFunctions.CFind(Properties.Settings.Default.IP, ushort.Parse(Properties.Settings.Default.Port), query, dataArray, Properties.Settings.Default.AET, Properties.Settings.Default.AEC);

        //    if (status)
        //    {
        //        foreach (gdcm.DataSet x in dataArray)
        //        {
        //            instanceID = uint.Parse(x.GetDataElement(new gdcm.Tag(0x0020, 0x0013)).GetValue().toString());
        //        }

        //        return instanceID;
        //    }

        //    return 0;
        //}
    }
}
