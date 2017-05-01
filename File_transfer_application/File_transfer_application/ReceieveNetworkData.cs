using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace File_transfer_application
{
    static class ReceieveNetworkData
    {

        public static void ReceiveData(Socket connection, NetworkEvent nEvent)
        {
            while(true)
            {
                byte[] buffer = new byte[1];

                try
                {
                    connection.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Receive type error: " + ex);
                }

                byte messageType = buffer[0];

                Console.WriteLine("Messagetype: " + (MessageTypes)messageType);

                switch((MessageTypes)messageType)
                {
                    case MessageTypes.FileItem:
                        ReceieveFileItem(connection, nEvent);
                        break;
                    case MessageTypes.File:
                        ReceieveFile(connection, nEvent);
                        break;
                    case MessageTypes.FileDownloadRequest:
                        ReceieveDownloadRequest(connection);
                        break;
                    default:
                        Console.WriteLine("eyy error");
                        break;
                }
            }
        }

        private static void ReceieveFileItem(Socket connection, NetworkEvent nEvent)
        {
            byte[] byteArr = Helpers.ReadParsableClasses(connection);

            FileItem item = FileItem.ConvertToObject(byteArr);
            Console.WriteLine("Receieved file item: " + item.GetFileName());

            nEvent.ReceievedFileItem(item);
        }

        private static void ReceieveDownloadRequest(Socket connection)
        {
            byte[] byteArr = Helpers.ReadParsableClasses(connection);
            DownloadRequest dlr = DownloadRequest.ConvertToObject(byteArr);
            SendNetworkData.SendFile(connection, dlr.FullPath);
        }


        private static void ReceieveFile(Socket connection, NetworkEvent nEvent)
        {
            byte[] byteArr = Helpers.ReadParsableClasses(connection);
            FileMetadata metadata = FileMetadata.ConvertToObject(byteArr);

            string finalPath = GetFileName(@"D:\testfile\" + metadata.Name + metadata.Extension);
            FileStream fsWrite = new FileStream(finalPath, FileMode.Create, FileAccess.Write);

            long totalDataReceived = 0;

            while(totalDataReceived != metadata.FileSize)
            {
                int bufferSize = 4096;

                if(metadata.FileSize - totalDataReceived < bufferSize)
                {
                    bufferSize = (int)(metadata.FileSize - totalDataReceived);
                }

                byte[] buffer = new byte[bufferSize];

                int received = connection.Receive(buffer, buffer.Length, SocketFlags.None);
                totalDataReceived += received;
                Console.WriteLine($"file data received: {totalDataReceived}");

                nEvent.DownloadInProgress((int)(((double)totalDataReceived / metadata.FileSize) * 100));

                fsWrite.Write(buffer, 0, received);
            }

            Console.WriteLine($"total file data received: {totalDataReceived}");
            fsWrite.Flush();
            fsWrite.Close();

            nEvent.DownloadInProgress(100);

        }

        private static string GetFileName(string fullPath)
        {
            int count = 1;

            string fileNameOnly = Path.GetFileNameWithoutExtension(fullPath);
            string extension = Path.GetExtension(fullPath);
            string path = Path.GetDirectoryName(fullPath);
            string newFullPath = fullPath;

            while(File.Exists(newFullPath))
            {
                string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
                newFullPath = Path.Combine(path, tempFileName + extension);
            }

            return newFullPath;
        }

    }
}