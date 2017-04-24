using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace File_transfer_application
{
    class TCPHolePunching
    {
        public Socket EstablishServerConnection()
        {

            // Create tcp connection with the rendezvous server.
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Make the tcp port reusable.
            serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            serverSocket.Connect(IPAddress.Parse("127.0.0.1"), 3333);

            // Get the local ip and port that the peer uses to communicate with the rendezvous server.
            IPEndPoint localEndPoint = (IPEndPoint)serverSocket.LocalEndPoint;

            // Send the private endpoint as a string to the server (send object in the future?).
            // Alos need to add an identifier here, to send to the server together with the privat endpoint.
            byte[] _buffer = BitConverter.GetBytes(localEndPoint.Port);
            serverSocket.Send(_buffer);

            return serverSocket;
        }

        public Socket TCPPunching(Socket serverSocket)
        {
            // his method could return a Task<Socket> to make it nonblocking and let the consumer handle the waiting.

            IPEndPoint localEndPoint = (IPEndPoint)serverSocket.LocalEndPoint;

            //This is the first part of the TCP punching, It sets up the listening socket waiting for others peers that try to connect.
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var listeningSocketTask = Task<Socket>.Factory.StartNew(() => SetupListeningSocket(listener, localEndPoint.Port));

            //The second part of the TCP punching start tasks that wait for the server to send target peer info to start outgoing connection attempts.
            //This wont run until it have received info from the server. To initiate this send a request to the server containing the identite of the target peer.
            IPEndPoint[] targetPeerEndpoints = GetEndpoints(serverSocket);

            Console.WriteLine("connect task one started");
            var privateEndPointSocketTask = Task<Socket>.Factory.StartNew(() => ConnectToTargetPeer(targetPeerEndpoints[0]));
            Console.WriteLine("connect task two started");
            var publiceEndPointSocketTask = Task<Socket>.Factory.StartNew(() => ConnectToTargetPeer(targetPeerEndpoints[1]));

            Task<Socket>[] tasks = new Task<Socket>[] { listeningSocketTask, privateEndPointSocketTask, publiceEndPointSocketTask };

            int i = Task.WaitAny(tasks);
            Task<Socket> completedTask = tasks[i];

            //listener.Close
        
            return completedTask.Result;
        }

        private IPEndPoint[] GetEndpoints(Socket serverSocket)
        {
            var serverSocketReceive = Task<IPEndPoint[]>.Factory.StartNew(() => GetTargetPeerEndpoints(serverSocket));

            //This is blocking might need to fix this in the future.
            Console.WriteLine("blocking");
            IPEndPoint[] points = serverSocketReceive.Result;
            serverSocketReceive.Wait();
            return points;
        }


        private Socket ConnectToTargetPeer(IPEndPoint endpoint)
        {
            int attempts = 0;

            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            while(!clientSocket.Connected && attempts <= 10)
            {
                try
                {
                    attempts++;
                    Console.WriteLine("Connection attempt: {0}", attempts);
                    //_clientSocket.Connect(IPAddress.Loopback, _SERVER_PORT);
                    clientSocket.Connect(endpoint.Address, endpoint.Port);
                    return clientSocket;
                }
                catch(Exception e)
                {
                    //Console.Clear();
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e);
                }
            }

            return null;

        }



        private IPEndPoint[] GetTargetPeerEndpoints(Socket serverSocket)
        {
            byte[] _buffer = new byte[1024];

            serverSocket.Receive(_buffer);
            int privatePort = BitConverter.ToInt32(_buffer, 0);


            IPEndPoint[] arr = new IPEndPoint[2];
            arr[0] = new IPEndPoint(IPAddress.Parse("127.0.0.1"), privatePort);
            arr[1] = new IPEndPoint(IPAddress.Parse("127.0.0.1"), privatePort);


            return arr;
        }

        private Socket SetupListeningSocket(Socket listener, int port)
        {
            listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            listener.Bind(new IPEndPoint(IPAddress.Any, port));
            listener.Listen(1);
            Console.WriteLine("listening for connections");

            try
            {
                Socket client = listener.Accept();
                Console.WriteLine("listener returned a value");
                if(client == null) Console.WriteLine("client null");
                return client;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }


        }

    }
}
