using System;
using System.Net.Sockets;

namespace MainServer
{
    internal class FileServerHandler
    {
        private TcpClient client;

        public FileServerHandler(TcpClient client)
        {
            this.client = client;
        }

        internal void Start()
        {
            
        }
    }
}