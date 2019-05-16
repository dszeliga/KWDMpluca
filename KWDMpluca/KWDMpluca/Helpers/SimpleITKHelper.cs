using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using sitk = itk.simple;

namespace KWDMpluca.Helpers
{
    public class SimpleITKHelper
    {
        public static string SegmentArea(Point point, ImageSource image)
        {
            string imagePath = GetFolderName(image) + "segmented_" + GetDicomFileName(image)+".dcm";
            uint seedX = uint.Parse(Math.Round(point.X).ToString());
            uint seedY = uint.Parse(Math.Round(point.Y).ToString());
            uint seedZ = 1;//FindInstance();

            sitk.VectorUInt32 seed = new sitk.VectorUInt32(new uint[] { seedX, seedY, seedZ });

            sitk.ImageFileReader imageFileReader = new sitk.ImageFileReader();
            imageFileReader.SetFileName(GetFolderName(image) + GetDicomFileName(image));
            imageFileReader.SetOutputPixelType(sitk.PixelIDValueEnum.sitkInt16);
            sitk.Image imageDicomOrg = imageFileReader.Execute();

            sitk.CurvatureFlowImageFilter curvatureFlowImageFilter = new sitk.CurvatureFlowImageFilter();
            curvatureFlowImageFilter.SetNumberOfIterations(5);
            curvatureFlowImageFilter.SetTimeStep(0.125);
            imageDicomOrg = curvatureFlowImageFilter.Execute(imageDicomOrg);

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

            SaveImage(imageDicomOrg_beforeErode, imagePath);

            //erozja
            sitk.BinaryErodeImageFilter binaryErodeImageFilter = new sitk.BinaryErodeImageFilter();
            binaryErodeImageFilter.SetKernelRadius(6);
            binaryErodeImageFilter.SetBackgroundValue(0);
            binaryErodeImageFilter.SetForegroundValue(1);
            imageDicomOrg = binaryErodeImageFilter.Execute(imageDicomOrg_beforeErode);

            //zmiana na uint8
            castImageFilter.SetOutputPixelType(sitk.PixelIDValueEnum.sitkUInt8);
            imageDicomOrg = castImageFilter.Execute(imageDicomOrg);

            castImageFilter.SetOutputPixelType(sitk.PixelIDValueEnum.sitkUInt8);
            imageDicomOrg_beforeErode = castImageFilter.Execute(imageDicomOrg_beforeErode);

            sitk.ConnectedComponentImageFilter labeler = new sitk.ConnectedComponentImageFilter();
            labeler.SetFullyConnected(false);    
            
            imageDicomOrg = labeler.Execute(imageDicomOrg_beforeErode, imageDicomOrg);


            sitk.RelabelComponentImageFilter relabeler = new sitk.RelabelComponentImageFilter();
            relabeler.SetMinimumObjectSize(1000);
            relabeler.SortByObjectSizeOn();
            imageDicomOrg = relabeler.Execute(imageDicomOrg);

            SaveImage(imageDicomOrg, imagePath);

            //sitk.ConfidenceConnectedImageFilter confidenceConnectedImageFilter = new sitk.ConfidenceConnectedImageFilter();
            //confidenceConnectedImageFilter.SetSeed(seed);
            //confidenceConnectedImageFilter.SetReplaceValue(5);
            //confidenceConnectedImageFilter.SetInitialNeighborhoodRadius(5);
            //confidenceConnectedImageFilter.SetNumberOfIterations(100);            
            //imageDicomOrg=confidenceConnectedImageFilter.Execute(imageDicomOrg);


            //sitk.NeighborhoodConnectedImageFilter neighborhoodConnectedImageFilter = new sitk.NeighborhoodConnectedImageFilter();
            //neighborhoodConnectedImageFilter.SetSeed(seed);
            //neighborhoodConnectedImageFilter.SetReplaceValue(1);
            //neighborhoodConnectedImageFilter.SetRadius(5);
            //imageDicomOrg=neighborhoodConnectedImageFilter.Execute(imageDicomOrg);

            SaveImage(imageDicomOrg, imagePath);

            return imagePath.Substring(0, imagePath.Length - 4);
        }

        private static void SaveImage(sitk.Image image, string pathToFile)
        {
            sitk.ImageFileWriter writer = new sitk.ImageFileWriter();
            writer.SetFileName(pathToFile);
            writer.Execute(image);
        }

        private static string GetDicomFileName(ImageSource image)
        {
            string[] pathSegments = image.ToString().Split('\\');
            string[] fileNameSegments = pathSegments[3].Split('_');
            string dicomName = fileNameSegments[0];//.TrimEnd(new char[] { '.', 'd', 'c', 'm' });
            return dicomName;
        }

        private static string GetImageFileName(ImageSource image)
        {
            string[] pathSegments = image.ToString().Split('\\');
            string[] fileNameSegments = pathSegments[3].Split('_');
            string sliceName = "segmentowana" + fileNameSegments[1];
            sliceName = fileNameSegments[0] + "_" + sliceName;
            return sliceName;
        }

        private static string GetFolderName(ImageSource image)
        {
            string[] pathSegments = image.ToString().Split('\\');
            string folderName = pathSegments[0] + "\\" + pathSegments[1] + "\\" + pathSegments[2] + "\\";
            return folderName;
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
