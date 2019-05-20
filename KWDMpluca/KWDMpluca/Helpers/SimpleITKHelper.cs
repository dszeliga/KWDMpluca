using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using sitk = itk.simple;
using System.IO;
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

            //progowanie
            sitk.BinaryThresholdImageFilter binthr = new sitk.BinaryThresholdImageFilter();
            binthr.SetLowerThreshold(-1042);
            binthr.SetUpperThreshold(-64);
            binthr.SetOutsideValue(1);
            binthr.SetInsideValue(0);
            sitk.Image imageDicomOrg_beforeErode = binthr.Execute(imageDicomOrg);
            
            //erozja
            sitk.BinaryErodeImageFilter binaryErodeImageFilter = new sitk.BinaryErodeImageFilter();
            binaryErodeImageFilter.SetKernelRadius(4);
            binaryErodeImageFilter.SetBackgroundValue(0);
            binaryErodeImageFilter.SetForegroundValue(1);
            sitk.Image imageDicomErode = binaryErodeImageFilter.Execute(imageDicomOrg_beforeErode);
                        
            //otwarcie (erozja potem dylatacja )
            sitk.BinaryMorphologicalOpeningImageFilter binaryMorphologicalOpeningImageFilter = new sitk.BinaryMorphologicalOpeningImageFilter();
            binaryMorphologicalOpeningImageFilter.SetKernelRadius(4);
            binaryMorphologicalOpeningImageFilter.SetBackgroundValue(0);
            binaryMorphologicalOpeningImageFilter.SetForegroundValue(1);
            binaryMorphologicalOpeningImageFilter.SetKernelType(sitk.KernelEnum.sitkBall);
            sitk.Image imageDicomOpen = binaryMorphologicalOpeningImageFilter.Execute(imageDicomErode);
           
            //rozrost ze wskazanego punktu
            sitk.ConfidenceConnectedImageFilter confidenceConnectedImageFilter = new sitk.ConfidenceConnectedImageFilter();
            confidenceConnectedImageFilter.AddSeed(seed);
            confidenceConnectedImageFilter.SetReplaceValue(1);
            confidenceConnectedImageFilter.SetInitialNeighborhoodRadius(3);
            confidenceConnectedImageFilter.SetNumberOfIterations(3);
            confidenceConnectedImageFilter.SetMultiplier(3);
            sitk.Image imageDicomSegmented = confidenceConnectedImageFilter.Execute(imageDicomOpen);
            //zmiana piksela na int16
            castImageFilter.SetOutputPixelType(sitk.PixelIDValueEnum.sitkInt16);
            imageDicomSegmented = castImageFilter.Execute(imageDicomSegmented);

            SaveImage(imageDicomSegmented, GetFolderName(image) + "segmentedMask" + GetDicomFileName(image) + ".dcm");
           

            var x = imageDicomSegmented.GetPixelIDValue();
            int area = 0;
            for (int i = 0; i < imageDicomSegmented.GetWidth(); i++)
            {
                for (int j = 0; j < imageDicomSegmented.GetHeight(); j++)
                {
                    seed[0] = (uint)i;
                    seed[1] = (uint)j;
                    var piksel = imageDicomSegmented.GetPixelAsInt16(seed);

                   
                    if(piksel==1)
                    {
                        area++;                        
                    }                    
                }
            }




            //maska w kolorze
            //sitk.ScalarToRGBColormapImageFilter scalarToRGBColormap = new sitk.ScalarToRGBColormapImageFilter();
            //scalarToRGBColormap.SetColormap(sitk.ScalarToRGBColormapImageFilter.ColormapType.Blue);            
            //sitk.Image imageDicomSegmentedColor = scalarToRGBColormap.Execute(imageDicomSegmented);
            //SaveImage(imageDicomSegmentedColor, GetFolderName(image) + "segmentedMaskColor" + GetDicomFileName(image) + ".dcm");

            castImageFilter.SetOutputPixelType(sitk.PixelIDValueEnum.sitkInt16);
            imageDicomSegmented = castImageFilter.Execute(imageDicomSegmented);

            castImageFilter.SetOutputPixelType(sitk.PixelIDValueEnum.sitkInt16);
            imageDicomOrg = castImageFilter.Execute(imageDicomOrg);

            //zmiana formatu wektora koloru maski
            //sitk.VectorIndexSelectionCastImageFilter vectorIndexSelectionCastImageFilter = new sitk.VectorIndexSelectionCastImageFilter();
            //vectorIndexSelectionCastImageFilter.SetOutputPixelType(sitk.PixelIDValueEnum.sitkUInt16);
            //imageDicomSegmentedColor = vectorIndexSelectionCastImageFilter.Execute(imageDicomSegmentedColor);
            //SaveImage(imageDicomSegmentedColor, GetFolderName(image) + "segmentedMaskColor" + GetDicomFileName(image) + ".dcm");

            //nałożenie maski na obraz
            sitk.AddImageFilter addImageFilter = new sitk.AddImageFilter();
            sitk.Image imageDicomWithMask = addImageFilter.Execute(imageDicomOrg, imageDicomSegmented);

            //zapis do pliku
            SaveImage(imageDicomWithMask, GetFolderName(image) + "imageWithMask" + GetDicomFileName(image) + ".dcm");

            //string imagePath = GetFolderName(image) + "imageWithMask" + GetDicomFileName(image) + ".dcm";
            //zwraca sciezke do pliku po nałożeniu
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
                string dicomName = fileNameSegments[0];//.TrimEnd(new char[] { '.', 'd', 'c', 'm' });
                return dicomName;
            }
            else
            {
                string[] pathSegments = image.ToString().Split('\\');
                string[] fileNameSegments = pathSegments[3].Split('_');
                string dicomName = fileNameSegments[0];//.TrimEnd(new char[] { '.', 'd', 'c', 'm' });
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
                string folderName = ".\\"+pathSegments[3] + "\\" + pathSegments[4] + "\\";
                return folderName;
            }
            else
            {
                string[] pathSegments = image.ToString().Split('\\');
                string folderName = pathSegments[0] + "\\" + pathSegments[1] + "\\" + pathSegments[2] + "\\";
                return folderName;
            }
        }

        private static uint FindInstance()
        {
            uint instanceID = 0;

            gdcm.ERootType type = gdcm.ERootType.ePatientRootType;

            gdcm.EQueryLevel level = gdcm.EQueryLevel.ePatient;

            gdcm.KeyValuePairArrayType keys = new gdcm.KeyValuePairArrayType();
            gdcm.KeyValuePairType key = new gdcm.KeyValuePairType(new gdcm.Tag(0x0020, 0x0013), "");
            keys.Add(key);
            keys.Add(new gdcm.KeyValuePairType(new gdcm.Tag(0x0010, 0x0020), Properties.Settings.Default.SelectedPatientID));

            gdcm.BaseRootQuery query = gdcm.CompositeNetworkFunctions.ConstructQuery(type, level, keys);

            gdcm.DataSetArrayType dataArray = new gdcm.DataSetArrayType();

            bool status = gdcm.CompositeNetworkFunctions.CFind(Properties.Settings.Default.IP, ushort.Parse(Properties.Settings.Default.Port), query, dataArray, Properties.Settings.Default.AET, Properties.Settings.Default.AEC);

            if (status)
            {
                foreach (gdcm.DataSet x in dataArray)
                {
                    instanceID = uint.Parse(x.GetDataElement(new gdcm.Tag(0x0020, 0x0013)).GetValue().toString());
                }

                return instanceID;
            }

            return 0;
        }
    }
}
