using System;
using System.Net.Sockets;

namespace FileServer
{
    internal class ClientHandler
    {
        private TcpClient client;

        public ClientHandler(TcpClient client)
        {
            this.client = client;
        }

        internal void Start()
        {

        }
    }
}