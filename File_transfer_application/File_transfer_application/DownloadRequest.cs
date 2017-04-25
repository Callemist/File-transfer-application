using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File_transfer_application
{
    class DownloadRequest
    {
        public string FullPath{ get; }

        public DownloadRequest() { }

        public DownloadRequest(string path)
        {
            FullPath = path;
        }

        public static DownloadRequest ConvertToObject(byte[] byteArr)
        {
            int pathSize = BitConverter.ToInt32(byteArr, 0);
            string path = Encoding.ASCII.GetString(byteArr, 4, pathSize);

            return new DownloadRequest(path);
        }

        public byte[] ConvertToByteArry()
        {
            byte[] bPath = Encoding.ASCII.GetBytes(FullPath);
            byte[] bPathSize = BitConverter.GetBytes(bPath.Length);

            return bPathSize.Concat(bPath).ToArray();
        }

    }
}
