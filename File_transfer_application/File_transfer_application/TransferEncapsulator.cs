using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File_transfer_application
{
    public static class TransferEncapsulator
    {
        public static byte[] GetProtocolHeader(long length, MessageTypes type)
        {
            byte[] tmp = new byte[] { (byte)type };
            byte[] bSize = BitConverter.GetBytes(length);
            return bSize.Concat(tmp).ToArray();
        }
    }
}
