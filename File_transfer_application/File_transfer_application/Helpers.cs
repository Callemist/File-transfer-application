﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace File_transfer_application
{
    public static class Helpers
    {
        public static byte[] GetProtocolHeader(long length, MessageTypes type)
        {
            byte[] bType = new byte[] { (byte)type };
            byte[] bSize = BitConverter.GetBytes(length);
            return bType.Concat(bSize).ToArray();
        }

        public static byte[] ReadParsableClasses(Socket connection)
        {
            byte[] sizeBuffer = new byte[8];
            connection.Receive(sizeBuffer, 0, sizeBuffer.Length, SocketFlags.None);

            long length = BitConverter.ToInt64(sizeBuffer, 0);

            byte[] data = new byte[0];
            long totalDataReceived = 0;


            while (totalDataReceived != length)
            {
                int bufferSize = 4096;

                if (length - totalDataReceived < bufferSize)
                {
                    bufferSize = (int)(length - totalDataReceived);
                }

                byte[] buffer = new byte[bufferSize];

                int received = connection.Receive(buffer, buffer.Length, SocketFlags.None);
                totalDataReceived += received;
                Console.WriteLine($"Parsable class data received: {totalDataReceived}");

                data = data.Concat(buffer).ToArray();
            }

            return data;

        }


    }
}
