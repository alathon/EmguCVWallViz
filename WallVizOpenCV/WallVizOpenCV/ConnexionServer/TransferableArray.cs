using Connexion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WallVizOpenCV.BlobTracker;

namespace WallVizOpenCV.ConnexionServer
{
    /*
    This class should be consistent with the implementation on the Java side as well,
    or you will run into trouble with using it! The Java implementation can be found in the
    equivalent TransferableArray class there.
    */
    class TransferableArray
    {
        private static String getNumValues(String prefix)
        {
            return prefix + "_numValues";
        }

        private static String getDataItem(String prefix, int idx)
        {
            return prefix + "_val" + idx;
        }

        public static Message Encode<T>(Message msg, String prefix, List<T> data)
        {
            msg.AddField(getNumValues(prefix), data.Count);
            for(int i = 1; i <= data.Count; i++)
            {
                try
                {
                    msg.AddField(getDataItem(prefix, i), data[i - 1]);
                } catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            return msg;
        }

        public static List<T> Decode<T>(Message msg, String prefix, Type classType)
        {
            int values = msg.GetIntField(getNumValues(prefix));
            List<T> retVal = new List<T>();
            for(int i = 1; i <= values; i++)
            {
                T value = (T)msg.GetField(getDataItem(prefix, i), classType);
                retVal.Add(value);
            }
            return retVal;
        }
    }
}
