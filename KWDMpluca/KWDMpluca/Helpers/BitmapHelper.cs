using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

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
                        int f = (int)(255 * (Y[r, c] / m));
                        
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
    }
}
