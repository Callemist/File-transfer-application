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

                byte[] arr = helpers.ReadParsableClasses(client);
                EndpointPair epp = EndpointPair.ConvertToObject(arr);

                Console.WriteLine($"client private port: {epp.Port} address: {epp.Address}");

                clients.Add(new Client() { socket = client, EpPair = epp });
            }


            byte[] _buffer = new byte[1024];

            Console.WriteLine("Waiting for init command from client 0");
            clients[0].socket.Receive(_buffer);
            Console.WriteLine("Init command received from client 0");

            //_buffer = BitConverter.GetBytes(clients[0].privatePort);
            //clients[1].socket.Send(_buffer);

            //_buffer = BitConverter.GetBytes(clients[1].privatePort);

            //clients[0].socket.Send(_buffer);


            byte[] bArrOne = clients[0].EpPair.ConvertToByteArry();
            long lengthOne = bArrOne.Length;
            clients[1].socket.Send(BitConverter.GetBytes(lengthOne));
            clients[1].socket.Send(bArrOne);

            byte[] bArrTwo = clients[1].EpPair.ConvertToByteArry();
            long lengthTwo = bArrTwo.Length;
            clients[0].socket.Send(BitConverter.GetBytes(lengthTwo));
            clients[0].socket.Send(bArrTwo);


            Console.ReadLine();
        }
    }
    class Client
    {
        public Socket socket { get; set; }
        public EndpointPair EpPair { get; set; }
    }

}
