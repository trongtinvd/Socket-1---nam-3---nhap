using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;

namespace MainServer
{
    internal class ClientHandler : INotifyPropertyChanged
    {
        public TcpClient Client { set; get; }
        private static object lockObj = new object();

        public string IP
        {
            get
            {
                return ((IPEndPoint)Client.Client.RemoteEndPoint).Address.ToString();
            }
        }

        public int Port
        {
            get
            {
                return ((IPEndPoint)Client.Client.RemoteEndPoint).Port;
            }
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


        private string status;

        public string Status
        {
            get => status;
            set
            {
                OnPropertyChanged(nameof(Status));
                status = value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}