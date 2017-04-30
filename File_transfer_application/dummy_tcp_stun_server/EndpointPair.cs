using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dummy_tcp_stun_server
{
    class EndpointPair
    {
        public int Port { get; }
        public string Address { get; }
        public EndpointPair() { }

        public EndpointPair(int port, string address)
        {
            Port = port;
            Address = address;
        }


        public static EndpointPair ConvertToObject(byte[] byteArr)
        {
            int port = BitConverter.ToInt32(byteArr, 0);
            string address = Encoding.ASCII.GetString(byteArr.Skip(4).ToArray());

            return new EndpointPair(port, address);
        }

        public byte[] ConvertToByteArry()
        {
            byte[] bPort = BitConverter.GetBytes(Port);
            byte[] bAddress = Encoding.ASCII.GetBytes(Address);

            return bPort.Concat(bAddress).ToArray();
        }
    }
}
