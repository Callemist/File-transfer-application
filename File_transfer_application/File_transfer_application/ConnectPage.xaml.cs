using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace File_transfer_application
{
    /// <summary>
    /// Interaction logic for ConnectPage.xaml
    /// </summary>
    public partial class ConnectPage : Page
    {
        Socket _serverSocket;
        public ConnectPage(Socket serverSocket)
        {
            InitializeComponent();

            _serverSocket = serverSocket;
            TCPHolePunching p = new TCPHolePunching();
        
            var t1 = Task.Factory.StartNew(() =>
            {
                Socket connection = p.TCPPunching(serverSocket);

                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    NavigationService.Navigate(new SharePage(connection));
                }));

            });


        }

        private void connect_Click(object sender, RoutedEventArgs e)
        {
            byte[] idBuffer = Encoding.Default.GetBytes("INIT");
            _serverSocket.Send(idBuffer);
        }
    }
}
