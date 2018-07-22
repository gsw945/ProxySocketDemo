using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace Org.Mentalis.Network.ProxySocket
{
    public class GZip
    {
        public static string DeCode(MemoryStream ms)
        {
            string result;
            using (GZipStream zipStream = new GZipStream(ms, CompressionMode.Decompress))
            {
                using (StreamReader sr = new StreamReader(zipStream, Encoding.ASCII))
                {
                    result = sr.ReadToEnd();
                }
            }
            return result;
        }
    }
}
