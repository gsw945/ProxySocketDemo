using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Org.Mentalis.Network.ProxySocket
{
    public class Deflate
    {
        public static string DeCode(MemoryStream ms)
        {
            string result;
            using(DeflateStream ds=new DeflateStream(ms,CompressionMode.Decompress))
            {
                using(StreamReader sr=new StreamReader(ds,Encoding.ASCII))
                {
                    result = sr.ReadToEnd();
                }
            }
            return result;
        }
    }
}
