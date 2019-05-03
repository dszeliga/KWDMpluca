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

            sitk.VectorUInt32 seed = new sitk.VectorUInt32(new uint[] {seedX, seedY, 10 });

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

            sitk.BinaryThresholdImageFilter binthr = new sitk.BinaryThresholdImageFilter();
            binthr.SetLowerThreshold(-1042);
            binthr.SetUpperThreshold(-64);
            binthr.SetOutsideValue(1);
            binthr.SetInsideValue(0);
            imageDicomOrg = binthr.Execute(imageDicomOrg);

            sitk.BinaryErodeImageFilter binaryDilateImageFilter = new sitk.BinaryErodeImageFilter();
            binaryDilateImageFilter.SetBackgroundValue(0);
            binaryDilateImageFilter.SetForegroundValue(1);
            imageDicomOrg = binaryDilateImageFilter.Execute(imageDicomOrg);

            sitk.ConnectedThresholdImageFilter connectedThresholdImageFilter = new sitk.ConnectedThresholdImageFilter();
            connectedThresholdImageFilter.SetSeed(seed);
            connectedThresholdImageFilter.SetLower(210);
            connectedThresholdImageFilter.SetUpper(250);
            connectedThresholdImageFilter.SetReplaceValue(255);
            imageDicomOrg = connectedThresholdImageFilter.Execute(imageDicomOrg);

            //sitk.BinaryDilateImageFilter binaryDilateImageFilter = new sitk.BinaryDilateImageFilter();
            //binaryDilateImageFilter.SetBackgroundValue(0);
            //binaryDilateImageFilter.SetForegroundValue(1);
            //imageDicomOrg = binaryDilateImageFilter.Execute(imageDicomOrg);


            SaveImage(imageDicomOrg, imagePath);

            

            //SaveImage(imageDicomOrg, imagePath);

            //sitk.BinaryThresholdImageFilter binthr = new sitk.BinaryThresholdImageFilter();
            //binthr.SetLowerThreshold(-950);
            //binthr.SetUpperThreshold(-720);
            //binthr.SetOutsideValue(0);
            //binthr.SetInsideValue(1);
            //imageDicomOrg = binthr.Execute(imageDicomOrg);

            //SaveImage(imageDicomOrg, imagePath);

            //sitk.ConnectedThresholdImageFilter connectedThresholdImageFilter = new sitk.ConnectedThresholdImageFilter();
            //connectedThresholdImageFilter.SetSeed(seed);
            //connectedThresholdImageFilter.SetLower(100);
            //connectedThresholdImageFilter.SetUpper(200);
            //imageDicomOrg = connectedThresholdImageFilter.Execute(imageDicomOrg);

            //SaveImage(imageDicomOrg, imagePath);
            //sitk.CurvatureFlowImageFilter curvatureFlowImageFilter = new sitk.CurvatureFlowImageFilter();

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
    }
}
