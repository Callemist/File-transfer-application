using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace File_transfer_application
{
    static class SendNetworkData
    {
        public static void SendDownloadRequest(Socket connection, FileItem item)
        {
            connection.Send(Helpers.GetProtocolHeader(new DownloadRequest(item.GetFullPath()).ConvertToByteArry().Length, MessageTypes.FileDownloadRequest));
            connection.Send(new DownloadRequest(item.GetFullPath()).ConvertToByteArry());
        }

        public static void SendFileItem(Socket connection, string path)
        {
            try
            {
                Console.WriteLine(path);
                connection.Send(Helpers.GetProtocolHeader(new FileItem(path).ConvertToByteArry().Length, MessageTypes.FileItem));
            }
            catch(Exception ex)
            {

                Console.WriteLine("Send fileitem header error: " + ex);
            }

            try
            {
                connection.Send(new FileItem(path).ConvertToByteArry());
            }
            catch(Exception ex)
            {

                Console.WriteLine("Send fileitem error: " + ex);
            }


            Console.WriteLine("sent file item: " + path);
        }

        public static void SendFile(Socket connection, string path)
        {
            connection.Send(Helpers.GetProtocolHeader(new FileMetadata(path).ConvertToByteArry().Length, MessageTypes.File));
            connection.Send(new FileMetadata(path).ConvertToByteArry());

            using(FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {

                //int bufferSize = 64 * 1024; this should be big cause reading from the harddrive cost a lot, but its to big to send over the network.
                //using 2048 while testing need to fix this later so that the harddrive read bigger chunks and split them before sending the data over the network.
                int bufferSize = 2048;
                byte[] buffer = new byte[bufferSize];
                long totalSent = 0;

                while(true)
                {
                    int readCount = fs.Read(buffer, 0, buffer.Length);
                    Console.WriteLine($"Read: {readCount} bytes from drive");

                    if(readCount <= 0)
                    {
                        break;
                    }

                    Console.WriteLine($"bytes: {readCount}");

                    totalSent += connection.Send(buffer, readCount, SocketFlags.None);
                }

                Console.WriteLine($"total data sent: { totalSent }");

            }
        }


    }
}
