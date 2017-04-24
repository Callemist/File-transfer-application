using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace dummy_tcp_stun_server
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Client> clients = new List<Client>();

            Socket sSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sSocket.Bind(new IPEndPoint(IPAddress.Any, 3333));
            sSocket.Listen(5);


            for(int i = 0; i < 2; i++)
            {
                Socket client = sSocket.Accept();
                Console.WriteLine($"client connected at {client.RemoteEndPoint.ToString()}");

                byte[] buffer = new byte[4];

                client.Receive(buffer);

                int port = BitConverter.ToInt32(buffer, 0);
                Console.WriteLine($"clients private port is: {port}");

                clients.Add(new Client() { socket = client, privatePort = port });
            }


            byte[] _buffer = new byte[1024];

            Console.WriteLine("Waiting for init command from client 0");
            clients[0].socket.Receive(_buffer);
            Console.WriteLine("Init command received from client 0");

            _buffer = BitConverter.GetBytes(clients[0].privatePort);

            clients[1].socket.Send(_buffer);

            _buffer = BitConverter.GetBytes(clients[1].privatePort);

            clients[0].socket.Send(_buffer);


            Console.ReadLine();
        }
    }
    class Client
    {
        public Socket socket { get; set; }
        public int privatePort { get; set; }
    }

}
