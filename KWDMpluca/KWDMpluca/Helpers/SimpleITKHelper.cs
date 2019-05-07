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
        public static void SegmentArea(Point point, ImageSource image)
        {
            string imagePath = GetFolderName(image) + "segmented_" + GetDicomFileName(image);
            uint seedX = uint.Parse(Math.Round(point.X).ToString());
            uint seedY = uint.Parse(Math.Round(point.Y).ToString());
            uint seedZ = FindInstance();

            sitk.VectorUInt32 seed = new sitk.VectorUInt32(new uint[] {seedX, seedY, seedZ});

            sitk.ImageFileReader imageFileReader = new sitk.ImageFileReader();
            imageFileReader.SetFileName(GetFolderName(image) + GetDicomFileName(image));
            imageFileReader.SetOutputPixelType(sitk.PixelIDValueEnum.sitkUnknown);
            sitk.Image imageDicomOrg = imageFileReader.Execute();

            sitk.CastImageFilter castImageFilter = new sitk.CastImageFilter();
            castImageFilter.SetOutputPixelType(sitk.PixelIDValueEnum.sitkInt16);
            imageDicomOrg = castImageFilter.Execute(imageDicomOrg);

            //Wygładzenie
            //sitk.GradientAnisotropicDiffusionImageFilter gradientAnisotropicDiffusionImageFilter = new sitk.GradientAnisotropicDiffusionImageFilter();
            //gradientAnisotropicDiffusionImageFilter.SetNumberOfIterations(5);
            //gradientAnisotropicDiffusionImageFilter.SetTimeStep(0.05);
            //gradientAnisotropicDiffusionImageFilter.SetConductanceParameter(1.0);
            //imageDicomOrg = gradientAnisotropicDiffusionImageFilter.Execute(imageDicomOrg);
            //SaveImage(imageDicomOrg, imagePath);
            sitk.CurvatureFlowImageFilter curvatureFlowImageFilter = new sitk.CurvatureFlowImageFilter();
            curvatureFlowImageFilter.SetNumberOfIterations(5);
            curvatureFlowImageFilter.SetTimeStep(0.125);
            imageDicomOrg = curvatureFlowImageFilter.Execute(imageDicomOrg);

            //Segmentacja
            sitk.ConfidenceConnectedImageFilter confidenceConnectedImageFilter = new sitk.ConfidenceConnectedImageFilter();
            confidenceConnectedImageFilter.SetReplaceValue(1);
            confidenceConnectedImageFilter.SetInitialNeighborhoodRadius(1);
            confidenceConnectedImageFilter.SetNumberOfIterations(5);
            imageDicomOrg = confidenceConnectedImageFilter.Execute(imageDicomOrg);
            SaveImage(imageDicomOrg, imagePath);

            //Dylacja
            sitk.GrayscaleDilateImageFilter grayscaleDilateImageFilter = new sitk.GrayscaleDilateImageFilter();
            grayscaleDilateImageFilter.SetKernelType(sitk.KernelEnum.sitkBall);
            grayscaleDilateImageFilter.SetKernelRadius(5);
            sitk.Image dilateDicomImage = grayscaleDilateImageFilter.Execute(imageDicomOrg);
            SaveImage(dilateDicomImage, imagePath);

            //Krawędzie
            sitk.GradientMagnitudeRecursiveGaussianImageFilter gradientMagnitudeRecursiveGaussianImageFilter = new sitk.GradientMagnitudeRecursiveGaussianImageFilter();
            gradientMagnitudeRecursiveGaussianImageFilter.SetSigma(1.0);
            imageDicomOrg = gradientMagnitudeRecursiveGaussianImageFilter.Execute(imageDicomOrg);
            SaveImage(imageDicomOrg, imagePath);

            //Negacja
            sitk.ExpNegativeImageFilter expNegativeImageFilter = new sitk.ExpNegativeImageFilter();
            imageDicomOrg = expNegativeImageFilter.Execute(imageDicomOrg);
            SaveImage(imageDicomOrg, imagePath);

            //Mapa odległości
            sitk.FastMarchingImageFilter fastMarchingImageFilter = new sitk.FastMarchingImageFilter();
            sitk.VectorUIntList seeds = new sitk.VectorUIntList();
            seeds.Add(seed);
            fastMarchingImageFilter.SetTrialPoints(seeds);
            imageDicomOrg = fastMarchingImageFilter.Execute(imageDicomOrg);
            SaveImage(imageDicomOrg, imagePath);

            //Aktywne kontury
            sitk.GeodesicActiveContourLevelSetImageFilter geodesicActiveContourLevelSetImageFilter = new sitk.GeodesicActiveContourLevelSetImageFilter();
            geodesicActiveContourLevelSetImageFilter.SetPropagationScaling(100);
            geodesicActiveContourLevelSetImageFilter.SetCurvatureScaling(1);
            geodesicActiveContourLevelSetImageFilter.SetMaximumRMSError(0.0005);
            geodesicActiveContourLevelSetImageFilter.SetNumberOfIterations(800);
            imageDicomOrg = geodesicActiveContourLevelSetImageFilter.Execute(imageDicomOrg, dilateDicomImage);
            SaveImage(imageDicomOrg, imagePath);

            //sitk.CurvatureFlowImageFilter curvatureFlowImageFilter = new sitk.CurvatureFlowImageFilter();
            //curvatureFlowImageFilter.SetNumberOfIterations(5);
            //curvatureFlowImageFilter.SetTimeStep(0.125);
            //imageDicomOrg = curvatureFlowImageFilter.Execute(imageDicomOrg);

            //sitk.CastImageFilter castImageFilter = new sitk.CastImageFilter();
            //castImageFilter.SetOutputPixelType(sitk.PixelIDValueEnum.sitkInt16);
            //imageDicomOrg = castImageFilter.Execute(imageDicomOrg);

            //sitk.BinaryThresholdImageFilter binthr = new sitk.BinaryThresholdImageFilter();
            //binthr.SetLowerThreshold(-1042);
            //binthr.SetUpperThreshold(-64);
            //binthr.SetOutsideValue(1);
            //binthr.SetInsideValue(0);
            //imageDicomOrg = binthr.Execute(imageDicomOrg);

            //sitk.BinaryErodeImageFilter binaryDilateImageFilter = new sitk.BinaryErodeImageFilter();
            //binaryDilateImageFilter.SetKernelRadius(2);
            //binaryDilateImageFilter.SetBackgroundValue(0);
            //binaryDilateImageFilter.SetForegroundValue(1);
            //imageDicomOrg = binaryDilateImageFilter.Execute(imageDicomOrg);

            //SaveImage(imageDicomOrg, imagePath);
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
