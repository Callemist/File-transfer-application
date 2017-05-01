using Microsoft.Win32;
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
        List<FileItem> sharedItems = new List<FileItem>();

        public SharePage(Socket connection)
        {
            _connection = connection;
            //Console.WriteLine("Socket available: " + connection.Available);
            Console.WriteLine("Socket local endpoint: " + connection.LocalEndPoint);
            Console.WriteLine("Socket remote endpoint: " + connection.RemoteEndPoint);
            InitializeComponent();
            NetworkEvent nEvent = new NetworkEvent();
            nEvent.NetworkUpdate += (object sender, NetworkEventArgs args) => AddFileItem(args.item);
            nEvent.DownloadProgressUpdate += (object sender, NetworkEventArgs args) => UpdateProgressBar(args.percentage);
            var receivingTask = Task.Factory.StartNew(() => ReceieveNetworkData.ReceiveData(connection, nEvent));

        }

        private void AddFileItem(FileItem item)
        {
            item.id = _fileItems.Count + 1;
            _fileItems.Add(item);
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => lbFileItems.Items.Add(new FileItemViewModel() { Path = item.GetFileName(), ico = item.GetIcon(), id = item.id })));
        }

        private void UpdateProgressBar(int percentage)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => pbDownloadProgressBar.Value = percentage == 100 ? 0 : percentage));
        }

        private void btnDownloadItem_Click(object sender, RoutedEventArgs e)
        {
            if(lbFileItems.SelectedItem != null)
            {
                Console.WriteLine((lbFileItems.SelectedItem as FileItemViewModel).id);
                SendNetworkData.SendDownloadRequest(_connection, _fileItems.Find(i => i.id == (lbFileItems.SelectedItem as FileItemViewModel).id));
            }
        }

        private void btnAddItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if(openFileDialog.ShowDialog() == true)
            {
                AddSharedItem(openFileDialog.FileName);
                SendNetworkData.SendFileItem(_connection, openFileDialog.FileName);
            }

        }

        private void lbFileItems_Drop(object sender, DragEventArgs e)
        {
            if(e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach(string filePath in files)
                {
                    Console.WriteLine(filePath);
                    AddSharedItem(filePath);
                    SendNetworkData.SendFileItem(_connection, filePath);
                }
            }
        }

        private void AddSharedItem(string path)
        {
            FileItem item = new FileItem(path);
            lbSharedItems.Items.Add(new FileItemViewModel() { Path = item.GetFileName(), ico = item.GetIcon() });
        }

    }

    class FileItemViewModel
    {
        public string Path { get; set; }
        public ImageSource ico { get; set; }
        public int id { get; set; }
    }

}
