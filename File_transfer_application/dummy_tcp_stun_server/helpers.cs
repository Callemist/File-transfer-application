using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace dummy_tcp_stun_server
{
    class helpers
    {

        public static byte[] ReadParsableClasses(Socket connection)
        {
            byte[] sizeBuffer = new byte[8];

            try
            {
                connection.Receive(sizeBuffer, 0, sizeBuffer.Length, SocketFlags.None);
            }
            catch(Exception ex)
            {

                Console.WriteLine("Read size bytes error: " + ex);
            }

            long length = BitConverter.ToInt64(sizeBuffer, 0);

            byte[] data = new byte[0];
            long totalDataReceived = 0;


            while(totalDataReceived != length)
            {
                int bufferSize = 4096;

                if(length - totalDataReceived < bufferSize)
                {
                    bufferSize = (int)(length - totalDataReceived);
                }

                byte[] buffer = new byte[bufferSize];
                int received;

                try
                {
                    received = connection.Receive(buffer, buffer.Length, SocketFlags.None);
                }
                catch(Exception ex)
                {
                    received = 0;
                    Console.WriteLine("Read parsable class error: " + ex);
                }

                totalDataReceived += received;
                Console.WriteLine($"Parsable class data received: {totalDataReceived}");

                data = data.Concat(buffer).ToArray();
            }

            return data;

        }
    }
}
