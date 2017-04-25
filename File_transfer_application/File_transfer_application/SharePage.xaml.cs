using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace File_transfer_application
{
    /// <summary>
    /// Interaction logic for SharePage.xaml
    /// </summary>
    public enum MessageTypes
    {
        FileMetadata,
        File,
        FileDownloadRequest
    }

    public partial class SharePage : Page
    {

        Socket _connection;

        public SharePage(Socket connection)
        {
            _connection = connection;
            InitializeComponent();
            //label.Content = connection.ToString();
            Networking(connection);

        }

        void Networking(Socket connection)
        {
            var receivingTask = Task.Factory.StartNew(() => ReceiveData(connection));
        }

        //this probably needs to moved into the methods to avoid reading wrong data.
        void ReceiveData(Socket connection)
        {
            //Listening for new data to be received
            while (true)
            {
                byte[] buffer = new byte[9];
                //Blocks the thread while waiting for data
                //When data is received read the first 9 bytes
                //The first 8 bytes contains the length of the total data, the 9th byte is the message type.
                connection.Receive(buffer, 0, 9, SocketFlags.None);
                byte messageType = buffer[8];

                long length = BitConverter.ToInt64(buffer.Take(8).ToArray(), 0);
                Console.WriteLine("Messagetype: " + (MessageTypes)messageType);

                switch ((MessageTypes)messageType)
                {
                    case MessageTypes.FileMetadata:
                        ReceieveFileMetadata(connection, length);
                        break;
                    case MessageTypes.File:
                        ReceieveFile(connection, length);
                        break;
                    case MessageTypes.FileDownloadRequest:
                        SendFile(connection, length);
                        break;
                    default:
                        Console.WriteLine("eyy error");
                        break;
                }
            }
        }

        private void ReceieveFileMetadata(Socket connection, long length)
        {
            //Might need to run this in its own thread aswell.
            byte[] data = new byte[0];
            long totalDataReceived = 0;

            // Read data as chunks until all data has been read.
            while (totalDataReceived < length)
            {
                byte[] buffer = new byte[2048];

                int received = connection.Receive(buffer, buffer.Length, SocketFlags.None);
                totalDataReceived += received;
                Console.WriteLine($"data received: {totalDataReceived}");

                byte[] recBuffer = new byte[received];
                Array.Copy(buffer, recBuffer, received);

                // Concat the read bytes onto the total data.
                data = data.Concat(recBuffer).ToArray();
            }

            string fileMetadata = Encoding.Default.GetString(data);
            Console.WriteLine($"FileMetadata: {fileMetadata}");

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => lbfileList.Items.Add(new TmpFileItem() { Path = fileMetadata })));

        }

        private void ReceieveFile(Socket connection, long length)
        {
            //Cant access ui componenets outside the ui thread
            //Console.WriteLine("path before filestream write: " + (lbfileList.SelectedItem as FileItem).Path);
            string finalPath = GetFileName(@"C:\school project\resources\image.PNG");
            FileStream fsWrite = new FileStream(finalPath, FileMode.Create, FileAccess.Write);

            long totalDataReceived = 0;

            // Read data as chunks until all data has been read.
            while (totalDataReceived < length)
            {
                byte[] buffer = new byte[2048];

                int received = connection.Receive(buffer, buffer.Length, SocketFlags.None);
                totalDataReceived += received;
                Console.WriteLine($"data received: {totalDataReceived}");

                byte[] recBuffer = new byte[received];
                Array.Copy(buffer, recBuffer, received);

                fsWrite.Write(buffer, 0, received);
            }

            Console.WriteLine($"total data receieved: {totalDataReceived}");
            fsWrite.Flush();
            fsWrite.Close();
        }

        private void SendFile(Socket connection, long length)
        {
            //Console.WriteLine("send file running");

            byte[] data = new byte[0];
            long totalDataReceived = 0;

            //On a download request the message will contain the path to the wanted file this part reads it.
            while (totalDataReceived < length)
            {
                byte[] buffer = new byte[2048];

                int received = connection.Receive(buffer, buffer.Length, SocketFlags.None);
                totalDataReceived += received;
                Console.WriteLine($"data received: {totalDataReceived}");

                byte[] recBuffer = new byte[received];
                Array.Copy(buffer, recBuffer, received);

                //Concat the read bytes onto the total data.
                data = data.Concat(recBuffer).ToArray();
            }

            string path = Encoding.Default.GetString(data);
            Console.WriteLine($"Path: {path}");

            //Send back a protocolheader that let the program know that there is an incomming file and the files length(size)
            byte[] protocolHeader = TransferEncapsulator.GetProtocolHeader(new FileInfo(@"C:\school project\resources\ca_code.PNG").Length, MessageTypes.File);
            _connection.Send(protocolHeader);
            //_connection.Send(data);

            //Read the data from the disk and send it over the network
            using (FileStream fs = new FileStream(@"C:\school project\resources\ca_code.PNG", FileMode.Open, FileAccess.Read))
            {

                //int bufferSize = 64 * 1024; this should be big cause reading from the harddrive cost a lot, but its to big to send over the network.
                //using 2048 while testing need to fix this later so that the harddrive read bigger chunks and split them before sending the data over the network.
                int bufferSize = 2048;
                byte[] buffer = new byte[bufferSize];
                long totalSent = 0;

                while (true)
                {
                    int readCount = fs.Read(buffer, 0, buffer.Length);
                    totalDataReceived += readCount;
                    Console.WriteLine($"Read: {readCount} bytes from drive");

                    if (readCount <= 0)
                    {
                        break;
                    }

                    Console.WriteLine($"bytes: {readCount}");

                    //network send
                    //fsWrite.Write(buffer, 0, readCount);
                    totalSent += _connection.Send(buffer, readCount, SocketFlags.None);
                }

                Console.WriteLine($"total data sent: { totalSent }");

            }
        }

        private void lbfileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnDownloadItem_Click(object sender, RoutedEventArgs e)
        {
            if (lbfileList.SelectedItem != null)
            {
                byte[] data = Encoding.Default.GetBytes((lbfileList.SelectedItem as TmpFileItem).Path);
                byte[] protocolHeader = TransferEncapsulator.GetProtocolHeader(data.Length, MessageTypes.FileDownloadRequest);
                _connection.Send(protocolHeader);
                _connection.Send(data);
            }
        }

        private void btnAddItem_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(tbFilePath.Text);
            byte[] data = Encoding.Default.GetBytes(tbFilePath.Text);
            byte[] protocolHeader = TransferEncapsulator.GetProtocolHeader(data.Length, MessageTypes.FileMetadata);
            _connection.Send(protocolHeader);
            _connection.Send(data);
        }

        private string GetFileName(string fullPath)
        {
            int count = 1;

            string fileNameOnly = Path.GetFileNameWithoutExtension(fullPath);
            string extension = Path.GetExtension(fullPath);
            string path = Path.GetDirectoryName(fullPath);
            string newFullPath = fullPath;

            while (File.Exists(newFullPath))
            {
                string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
                newFullPath = Path.Combine(path, tempFileName + extension);
            }

            return newFullPath;
        }

    }

    public class TmpFileItem
    {
        public string Path { get; set; }
    }

}
