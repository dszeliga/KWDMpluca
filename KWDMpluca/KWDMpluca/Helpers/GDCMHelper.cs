using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sitk = itk.simple;

namespace KWDMpluca.Helpers
{
    public static class GDCMHelper
    {
        public static bool MoveImageByPath(string segmentedImagePath)
        {
            List<string> files = new List<string>(System.IO.Directory.EnumerateFiles(segmentedImagePath));
            List<string> fileNames = new List<string>();
            string restoredPath = "";

            foreach (string path in files)
            {
                string[] splittedPath = path.Split('\\');
                fileNames.Add(splittedPath[3]);
                restoredPath = splittedPath[0] + "\\" + splittedPath[1] + "\\" + splittedPath[2];
            }
           

            string pathSegmented = fileNames.Where(x => x.StartsWith("segmentedMask") && x.EndsWith("dcm")).FirstOrDefault();
            string pathMarked = fileNames.Where(x => x.StartsWith("markedMask") && x.EndsWith("dcm")).FirstOrDefault();


            if (!string.IsNullOrEmpty(pathMarked))
            {
                bool result = MoveImage(restoredPath + "\\" + pathMarked, files);
                return result;
            }
            else if (!string.IsNullOrEmpty(pathSegmented))
            {
                bool result = MoveImage(restoredPath + "\\" + pathSegmented, files);
                return result;
            }

            return true;
        }

        private static bool MoveImage(string path, List<string> files)
        {            
            files.Clear();
            files.Add(path);

            bool statusStore = gdcm.CompositeNetworkFunctions.CStore(Properties.Settings.Default.IP, ushort.Parse(Properties.Settings.Default.Port), new gdcm.FilenamesType(files), Properties.Settings.Default.AET, Properties.Settings.Default.AEC);

            return statusStore;
        }
    }
}
