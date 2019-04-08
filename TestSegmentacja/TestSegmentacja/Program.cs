using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sitk = itk.simple;

namespace TestSegmentacja
{
    class Program
    {
        static void Main(string[] args)
        {
            string catalog = @"C:\Users\vroob\Downloads\projektKWDM\serie";
            string[] pathToSeries = Directory.GetFiles(catalog);

            sitk.VectorString fileNames = sitk.ImageSeriesReader.GetGDCMSeriesFileNames(catalog);

            sitk.ImageSeriesReader imageSeriesReader = new sitk.ImageSeriesReader();
            imageSeriesReader.SetFileNames(fileNames);
            imageSeriesReader.SetOutputPixelType(sitk.PixelIDValueEnum.sitkInt16);
            sitk.Image imageDicomOrg = imageSeriesReader.Execute();

            // binaryzacja
            sitk.BinaryThresholdImageFilter binthr = new sitk.BinaryThresholdImageFilter();
            binthr.SetLowerThreshold(-950);
            binthr.SetUpperThreshold(-720);
            binthr.SetOutsideValue(0);
            binthr.SetInsideValue(1);
            sitk.Image imageDicom = binthr.Execute(imageDicomOrg);

            sitk.VotingBinaryIterativeHoleFillingImageFilter holeFiller = new sitk.VotingBinaryIterativeHoleFillingImageFilter();
            holeFiller.SetRadius(2);
            holeFiller.SetForegroundValue(1);
            holeFiller.SetBackgroundValue(0);
            imageDicom = holeFiller.Execute(imageDicom);

            sitk.ConnectedComponentImageFilter labeler = new sitk.ConnectedComponentImageFilter();
            labeler.SetFullyConnected(true);
            sitk.Image labelImage = labeler.Execute(imageDicom);

            sitk.RelabelComponentImageFilter relabeler = new sitk.RelabelComponentImageFilter();
            relabeler.SetMinimumObjectSize(700);
            labelImage = relabeler.Execute(labelImage);

            sitk.ThresholdImageFilter thresholder = new sitk.ThresholdImageFilter();
            thresholder.SetLower(2);
            thresholder.SetUpper(2);
            thresholder.SetOutsideValue(0);
            labelImage = thresholder.Execute(labelImage);

            sitk.GeodesicActiveContourLevelSetImageFilter geodesicActiveContourLevelSetImageFilter = new sitk.GeodesicActiveContourLevelSetImageFilter();


            SaveImage(imageDicom, "labelImage.vtk");
        }

        static void SaveImage(sitk.Image image, string pathToFile)
        {
            sitk.ImageFileWriter writer = new sitk.ImageFileWriter();
            writer.SetFileName(pathToFile);
            writer.Execute(image);
        }
    }
}
