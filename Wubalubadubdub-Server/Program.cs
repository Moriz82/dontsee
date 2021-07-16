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
        private static List<Socket> clients = new List<Socket>();
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
            
            clients.Add(socket);
           socket.BeginReceive(buffer, 0, bufferSize, SocketFlags.None, ReceiveCallback, socket);
           Console.WriteLine("Client connected, waiting for request...");
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
            else if (text.ToLower() == "exit") // Client wants to exit gracefully
            {
                // Always Shutdown before closing
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                clients.Remove(socket);
                Console.WriteLine("Client disconnected");
                return;
            }
            else
            {
                Console.WriteLine("Text is an invalid request");
                byte[] data = Encoding.ASCII.GetBytes("Invalid request");
                socket.Send(data);
                Console.WriteLine("Warning Sent");
            }

            socket.BeginReceive(buffer, 0, bufferSize, SocketFlags.None, ReceiveCallback, socket);
        }
    }
}