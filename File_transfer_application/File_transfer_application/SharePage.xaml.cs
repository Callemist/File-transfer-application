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
                        ReceieveFile(connection);
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

        private void ReceieveFileItem(Socket connection)
        {
            byte[] byteArr = Helpers.ReadParsableClasses(connection);

            FileItem item = FileItem.ConvertToObject(byteArr);
            Console.WriteLine("Receieved file item: " + item.GetFileName());
            item.id = _fileItems.Count + 1;
            _fileItems.Add(item);

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => lbFileItems.Items.Add(new FileItemViewModel() { Path = item.GetFileName(), ico = item.GetIcon(), id = item.id })));
        }

        private void ReceieveDownloadRequest(Socket connection)
        {
            byte[] byteArr = Helpers.ReadParsableClasses(connection);
            DownloadRequest dlr = DownloadRequest.ConvertToObject(byteArr);
            SendFile(connection, dlr.FullPath);
        }


        private void SendFile(Socket connection, string path)
        {
            _connection.Send(Helpers.GetProtocolHeader(new FileMetadata(path).ConvertToByteArry().Length, MessageTypes.File));
            _connection.Send(new FileMetadata(path).ConvertToByteArry());

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {

                //int bufferSize = 64 * 1024; this should be big cause reading from the harddrive cost a lot, but its to big to send over the network.
                //using 2048 while testing need to fix this later so that the harddrive read bigger chunks and split them before sending the data over the network.
                int bufferSize = 2048;
                byte[] buffer = new byte[bufferSize];
                long totalSent = 0;

                while (true)
                {
                    int readCount = fs.Read(buffer, 0, buffer.Length);
                    Console.WriteLine($"Read: {readCount} bytes from drive");

                    if (readCount <= 0)
                    {
                        break;
                    }

                    Console.WriteLine($"bytes: {readCount}");

                    totalSent += _connection.Send(buffer, readCount, SocketFlags.None);
                }

                Console.WriteLine($"total data sent: { totalSent }");

            }
        }

        private void ReceieveFile(Socket connection)
        {
            byte[] byteArr = Helpers.ReadParsableClasses(connection);
            FileMetadata metadata = FileMetadata.ConvertToObject(byteArr);

            string finalPath = GetFileName(@"C:\testfile\" + metadata.Name + metadata.Extension);
            FileStream fsWrite = new FileStream(finalPath, FileMode.Create, FileAccess.Write);

            long totalDataReceived = 0;

            while (totalDataReceived != metadata.FileSize)
            {
                int bufferSize = 4096;

                if (metadata.FileSize - totalDataReceived < bufferSize)
                {
                    bufferSize = (int)(metadata.FileSize - totalDataReceived);
                }

                byte[] buffer = new byte[bufferSize];

                int received = connection.Receive(buffer, buffer.Length, SocketFlags.None);
                totalDataReceived += received;
                Console.WriteLine($"file data received: {totalDataReceived}");

                fsWrite.Write(buffer, 0, received);
            }

            Console.WriteLine($"total file data received: {totalDataReceived}");
            fsWrite.Flush();
            fsWrite.Close();
        }

        private void SendFileItem(string path)
        {
            _connection.Send(Helpers.GetProtocolHeader(new FileItem(path).ConvertToByteArry().Length, MessageTypes.FileItem));
            _connection.Send(new FileItem(path).ConvertToByteArry());
            Console.WriteLine("sent file item: " + path);
        }

        private void SendDownloadRequest(int id)
        {
            FileItem item = _fileItems.Find(i => i.id == id);
            _connection.Send(Helpers.GetProtocolHeader(new DownloadRequest(item.GetFullPath()).ConvertToByteArry().Length, MessageTypes.FileDownloadRequest));
            _connection.Send(new DownloadRequest(item.GetFullPath()).ConvertToByteArry());
        }

        private void btnDownloadItem_Click(object sender, RoutedEventArgs e)
        {
            if (lbFileItems.SelectedItem != null)
            {
                Console.WriteLine((lbFileItems.SelectedItem as FileItemViewModel).id);
                SendDownloadRequest((lbFileItems.SelectedItem as FileItemViewModel).id);
            }
        }

        private void btnAddItem_Click(object sender, RoutedEventArgs e)
        {
            //will removing the @ destory everything? nobody knows..
            Console.WriteLine("added file");
            SendFileItem(@tbFilePath.Text);
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

        private void lbFileItems_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string filePath in files)
                {
                    Console.WriteLine(filePath);
                    SendFileItem(@filePath);
                }
            }
        }
    }

    public class FileItemViewModel
    {
        public string Path { get; set; }
        public ImageSource ico { get; set; }
        public int id { get; set; }
    }

}
