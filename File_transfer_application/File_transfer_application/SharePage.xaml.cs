using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace File_transfer_application
{
    /// <summary>
    /// Interaction logic for SharePage.xaml
    /// </summary>
    public enum MessageTypes
    {
        FileItem,
        File,
        FileDownloadRequest
    }

    public partial class SharePage : Page
    {

        Socket _connection;
        List<FileItem> _fileItems = new List<FileItem>();

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
                byte[] buffer = new byte[1];
                connection.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                byte messageType = buffer[0];

                Console.WriteLine("Messagetype: " + (MessageTypes)messageType);

                switch ((MessageTypes)messageType)
                {
                    case MessageTypes.FileItem:
                        ReceieveFileItem(connection);
                        break;
                    case MessageTypes.File:
                        //ReceieveFile(connection);
                        break;
                    case MessageTypes.FileDownloadRequest:
                        SendFileMetadata(connection);
                        //SendFile(connection);
                        break;
                    default:
                        Console.WriteLine("eyy error");
                        break;
                }
            }
        }

        private void ReceieveFileItem(Socket connection)
        {
            byte[] buffer = new byte[8192];
            int received = connection.Receive(buffer, buffer.Length, SocketFlags.None);

            byte[] recBuffer = new byte[received];
            Array.Copy(buffer, recBuffer, received);

            FileItem item = FileItem.ConvertToFileItem(recBuffer);
            item.id = _fileItems.Count + 1;
            _fileItems.Add(item);

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => lbfileList.Items.Add(new FileItemView() { Path = item.GetFileName(), ico = item.GetIcon(), id = item.id })));
        }

        private void SendFileMetadata(Socket connection)
        {
            byte[] buffer = new byte[2048];
            int received = connection.Receive(buffer, buffer.Length, SocketFlags.None);

            byte[] recBuffer = new byte[received];
            Array.Copy(buffer, recBuffer, received);

            DownloadRequest dlr = DownloadRequest.ConvertToDownloadRequest(recBuffer);

            //connection.Send()

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

                //should read from recbuffer i think TODO FIX IN THE FUTURE
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

        private void SendFileItem(string path)
        {
            _connection.Send(new byte[] { (byte)MessageTypes.FileItem });
            _connection.Send(new FileItem(path).ConvertToByteArry());
        }

        private void SendDownloadRequest(int id)
        {
            FileItem item = _fileItems.Find(i => i.id == id);
            _connection.Send(new byte[] { (byte)MessageTypes.FileDownloadRequest });
            _connection.Send(new DownloadRequest(item.GetFullPath()).ConvertToByteArry());
        }

        private void btnDownloadItem_Click(object sender, RoutedEventArgs e)
        {
            if (lbfileList.SelectedItem != null)
            {
                Console.WriteLine((lbfileList.SelectedItem as FileItemView).id);
                SendDownloadRequest((lbfileList.SelectedItem as FileItemView).id);
            }
        }

        private void btnAddItem_Click(object sender, RoutedEventArgs e)
        {
            //will removing the @ destory everything? nobody knows..
            SendFileItem(@tbFilePath.Text);
        }

        private void lbfileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

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

    public class FileItemView
    {
        public string Path { get; set; }
        public ImageSource ico { get; set; }
        public int id { get; set; }
    }

}
