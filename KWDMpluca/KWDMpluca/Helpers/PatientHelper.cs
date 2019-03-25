using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWDMpluca.Helpers
{
    public class PatientHelper
    {
        static List<string> patientList = new List<string>();

        public static List<string> GetPatients(string ip, ushort port, string aet, string call)
        {
            gdcm.ERootType type = gdcm.ERootType.ePatientRootType;

            gdcm.EQueryLevel level = gdcm.EQueryLevel.ePatient;

            gdcm.KeyValuePairArrayType keys = new gdcm.KeyValuePairArrayType();
            gdcm.KeyValuePairType key = new gdcm.KeyValuePairType(new gdcm.Tag(0x0010, 0x0010), "*");
            keys.Add(key);

            gdcm.BaseRootQuery query = gdcm.CompositeNetworkFunctions.ConstructQuery(type, level, keys);

            gdcm.DataSetArrayType dataArray = new gdcm.DataSetArrayType();

            //bool status = gdcm.CompositeNetworkFunctions.CFind(ip, port, query, dataArray, aet, call);

            foreach (gdcm.DataSet x in dataArray)
            {
                patientList.Add(x.GetDataElement(new gdcm.Tag(0x0010, 0x0010)).GetValue().toString());
            }

            return patientList;
        }
    }
}
