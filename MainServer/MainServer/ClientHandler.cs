using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;

namespace MainServer
{
    internal class ClientHandler
    {
        public TcpClient Client { set; get; }
        private static object lockObj = new object();

        public string IP
        {
            get => ((IPEndPoint)Client.Client.RemoteEndPoint).Address.ToString();
        }

        public int Port
        {
            get => ((IPEndPoint)Client.Client.RemoteEndPoint).Port;
        }






        public ClientHandler(TcpClient client)
        {
            lock (lockObj)
            {
                this.Client = client;
            }
        }

        public void Start()
        {

        }
    }
}