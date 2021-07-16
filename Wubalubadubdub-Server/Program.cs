using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace Wubalubadubdub_Server
{
    class Program
    {
        private static Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static List<Socket> clients = new List<Socket>();
        private static List<Socket> victims = new List<Socket>();
        private const int bufferSize = 2048;
        private const int PORT = 6969;
        private static byte[] buffer = new byte[bufferSize];

        static void Main()
        {
            Console.Title = "Server";
            SetupServer();
            while (!Console.ReadLine().ToLower().Equals("exit")) {}
            CloseAllSockets();
        }

        private static void SetupServer()
        {
            Console.WriteLine("Starting Server.");
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
            Console.WriteLine("Starting Server..");
            serverSocket.Listen(0);
            Console.WriteLine("Starting Server...");
            serverSocket.BeginAccept(AcceptCallback, null);
            Console.WriteLine("Server setup complete");
        }
        
        private static void CloseAllSockets()
        {
            foreach (Socket socket in clients)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            serverSocket.Close();
        }

        private static void AcceptCallback(IAsyncResult asyncResult)
        {
            Socket socket;

            try
            {
                socket = serverSocket.EndAccept(asyncResult);
            }
            catch (ObjectDisposedException) // I cannot seem to avoid this (on exit when properly closing sockets)
            {
                return;
            }
            socket.Receive(buffer);
            string text = Encoding.ASCII.GetString(buffer);

            if (text.Equals("client"))
            {
                clients.Add(socket);
                Console.WriteLine("Client connected, waiting for request...");
            }
            else
            {
                victims.Add(socket);
                Console.WriteLine("Victim connected at "+socket.RemoteEndPoint.ToString().Split(':')[0]);
            }
            
            socket.BeginReceive(buffer, 0, bufferSize, SocketFlags.None, ReceiveCallback, socket);
            serverSocket.BeginAccept(AcceptCallback, null);
        }

        private static void ReceiveCallback(IAsyncResult asyncResult)
        {
            Socket socket = (Socket)asyncResult.AsyncState;
            int received = 0;

            try
            {
                received = socket.EndReceive(asyncResult);
            }
            catch (SocketException)
            {
                Console.WriteLine("Client forcefully disconnected");
                // Don't shutdown because the socket may be disposed and its disconnected anyway.
                socket.Close(); 
                clients.Remove(socket);
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(buffer, recBuf, received);
            string text = Encoding.ASCII.GetString(recBuf);
            Console.WriteLine("Received Text: " + text);

            if (text.ToLower() == "get time")
            {
                Console.WriteLine("Text is a get time request");
                byte[] data = Encoding.ASCII.GetBytes(DateTime.Now.ToLongTimeString());
                socket.Send(data);
                Console.WriteLine("Time sent to client");
            }
            else if (text.ToLower() == "exit")
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                clients.Remove(socket);
                Console.WriteLine("Client disconnected");
                return;
            }
            else
            {
                Console.WriteLine("Invalid Command entered");
                byte[] data = Encoding.ASCII.GetBytes("Invalid Command");
                socket.Send(data);
            }

            socket.BeginReceive(buffer, 0, bufferSize, SocketFlags.None, ReceiveCallback, socket);
        }
    }
}