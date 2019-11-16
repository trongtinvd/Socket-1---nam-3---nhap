using System;
using System.ComponentModel;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;

namespace MainServer
{
    internal class FileServerHandler
    {
        private object locker = new object();

        private Thread workingWithFileServerThread;
        private MyStreamIO myStream;
        private List<MyFile> _files = new List<MyFile>();

        public TcpClient Client { get; }

        public List<MyFile> Files
        {
            get
            {
                lock (locker)
                {
                    return _files;
                }
            }
            set
            {
                lock (locker)
                {
                    _files = value;
                }
            }
        }

        public string Address => ((IPEndPoint)Client.Client.RemoteEndPoint).Address.ToString();
        public int Port => ((IPEndPoint)Client.Client.RemoteEndPoint).Port;






        public FileServerHandler(TcpClient client)
        {
            lock (locker)
            {
                this.Client = client;
                myStream = new MyStreamIO(client.GetStream());
            }
        }

        ~FileServerHandler()
        {
            Stop();
        }




        public void Start()
        {
            workingWithFileServerThread = new Thread(() => WorkingWithFileServer(Client));
            workingWithFileServerThread.Start();
        }

        public void Stop()
        {
            if (workingWithFileServerThread != null)
            {
                workingWithFileServerThread.Abort();
                workingWithFileServerThread = null;
            }
        }

        private void WorkingWithFileServer(TcpClient client)
        {
            try
            {
                client.ReceiveTimeout = 7000;
                while (true)
                {
                    string request = myStream.ReadString();
                    myStream.SendNEXT();

                    switch (request)
                    {
                        case "<sendFilesInfo>":
                            lock (locker)
                            {
                                Files.Clear();
                                int number0fFile = myStream.ReadInt();
                                myStream.SendNEXT();
                                for (int i = 0; i < number0fFile; i++)
                                {
                                    string fileName = myStream.ReadString();
                                    myStream.SendNEXT();

                                    long fileSize = myStream.ReadLong();
                                    myStream.SendNEXT();

                                    string fileIp = myStream.ReadString();
                                    myStream.SendNEXT();

                                    int filePort = myStream.ReadInt();
                                    myStream.SendNEXT();

                                    MyFile file = new MyFile(fileName, fileSize, fileIp, filePort);
                                    Files.Add(file);
                                }

                            }
                            break;
                    }
                }
            }
            catch (TimeoutException)
            {
                Files.Clear();
            }
            catch (Exception)
            {

            }
        }
    }
}