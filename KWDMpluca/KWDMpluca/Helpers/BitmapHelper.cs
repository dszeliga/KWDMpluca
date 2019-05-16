using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using sitk = itk.simple;

namespace KWDMpluca.Helpers
{
    public class BitmapHelper
    {
        public static System.Drawing.Bitmap[] gdcmBitmap2Bitmap(gdcm.Bitmap bmjpeg2000)
        {
            uint columns = bmjpeg2000.GetDimension(0);
            uint rows = bmjpeg2000.GetDimension(1);

            uint layers = bmjpeg2000.GetDimension(2);

            System.Drawing.Bitmap[] ret = new System.Drawing.Bitmap[layers];

            byte[] bufor = new byte[bmjpeg2000.GetBufferLength()];
            if (!bmjpeg2000.GetBuffer(bufor))
            {
                throw new Exception("Błąd pobrania bufora");
            }

            for (uint l = 0; l < layers; l++)
            {
                System.Drawing.Bitmap X = new System.Drawing.Bitmap((int)columns, (int)rows);
                double[,] Y = new double[columns, rows];
                double m = 0;

                for (int r = 0; r < rows; r++)
                    for (int c = 0; c < columns; c++)
                    {
                        int j = ((int)(l * rows * columns) + (int)(r * columns) + (int)c) * 2;
                         Y[r, c] = (float)bufor[j] *(float)bufor[j + 1]+1000;                        
                        if (Y[r, c] > m)
                        {
                            m = Y[r, c];
                        }
                    }

                for (int r = 0; r < rows; r++)
                    for (int c = 0; c < columns; c++)
                    {
                        int f = (int)(255 * ((Y[r, c]) / m));
                        
                        X.SetPixel(c, r, System.Drawing.Color.FromArgb(f, f, f));
                    }
                ret[l] = X;
            }
            return ret;
        }

        // http://gdcm.sourceforge.net/html/StandardizeFiles_8cs-example.html
        public static gdcm.Bitmap pxmap2jpeg2000(gdcm.Pixmap px)
        {
            gdcm.ImageChangeTransferSyntax change = new gdcm.ImageChangeTransferSyntax();
            change.SetForce(false);
            change.SetCompressIconImage(false);
            change.SetTransferSyntax(new gdcm.TransferSyntax(gdcm.TransferSyntax.TSType.JPEG2000Lossless));

            change.SetInput(px);
            if (!change.Change())
                throw new Exception("Nie przekonwertowano typu bitmapList na jpeg2000");

            return change.GetOutput();

        }

        public static BitmapImage LoadBitmapImage(int index, List<string> bitmapList)
        {
            BitmapImage dicom = new BitmapImage();
            dicom.BeginInit();
            dicom.UriSource = new Uri(bitmapList.ElementAt(index), UriKind.RelativeOrAbsolute);
            dicom.CacheOption = BitmapCacheOption.OnLoad;
            dicom.EndInit();
            return dicom;
        }

        public static Bitmap DicomToBitmap(sitk.Image imagesDicom, uint depth)
        {
            sitk.VectorUInt32 idx = new sitk.VectorUInt32();
            uint[] size = imagesDicom.GetSize().ToArray();
            idx.Add(0); idx.Add(0); idx.Add(depth);

            sitk.CastImageFilter castImageFilter = new sitk.CastImageFilter();
            castImageFilter.SetOutputPixelType(sitk.PixelIDValueEnum.sitkFloat32);
            imagesDicom = castImageFilter.Execute(imagesDicom);

            Bitmap bmp = new Bitmap((int)size[0], (int)size[1], PixelFormat.Format32bppRgb);
            BitmapData bmd = bmp.LockBits(new Rectangle(0, 0, (int)size[0], (int)size[1]),
                                                    ImageLockMode.ReadOnly, bmp.PixelFormat);

            int pixelSize = 4;
            unsafe
            {
                for (int y = 0; y < bmd.Height; y++)
                {
                    byte* row = (byte*)bmd.Scan0 + (y * bmd.Stride);


                    for (int x = 0; x < bmd.Width; x++)
                    {
                        idx[0] = (uint)x;
                        idx[1] = (uint)y;
                        byte rgb = (byte)imagesDicom.GetPixelAsFloat(idx);

                        // Blue  0-255 
                        row[x * pixelSize] = rgb;
                        // Green 0-255
                        row[x * pixelSize + 1] = rgb;
                        // Red   0-255
                        row[x * pixelSize + 2] = rgb;
                        // Alpha 0-255
                        row[x * pixelSize + 3] = rgb;
                        
                    }
                }
            }

            //bmp.UnlockBits(bmd);
           
            return bmp;
        }



    }
}
