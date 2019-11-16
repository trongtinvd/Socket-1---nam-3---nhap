using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MainServer
{
    internal class ClientHandler
    {
        private object locker = new object();
        private List<FileServerHandler> fileServers;
        private Thread workingWithClientThread;
        private MyStreamIO myStream;
        public TcpClient Client { set; get; }


        public string Address => ((IPEndPoint)Client.Client.RemoteEndPoint).Address.ToString();
        public int Port => ((IPEndPoint)Client.Client.RemoteEndPoint).Port;



        public ClientHandler(TcpClient client, List<FileServerHandler> fileServers)
        {
            lock (locker)
            {
                this.Client = client;
                this.fileServers = fileServers;
                myStream = new MyStreamIO(client.GetStream());
            }
        }




        public void Start()
        {
            workingWithClientThread = new Thread(() => WorkingWithClient(Client));
            workingWithClientThread.Start();
        }

        public void Stop()
        {
            if (workingWithClientThread != null)
            {
                workingWithClientThread.Abort();
                workingWithClientThread = null;
            }

        }

        private void WorkingWithClient(TcpClient client)
        {
            try
            {
                client.ReceiveTimeout = 7000;
                while (true)
                {
                    string request = myStream.ReadString();

                    switch (request)
                    {
                        case "<getAllFileInfo>":
                            myStream.Write(fileServers.Count);
                            myStream.GetNEXT();
                            foreach (FileServerHandler fileServer in fileServers)
                            {
                                List<MyFile> files = fileServer.Files;

                                myStream.Write(files.Count);
                                myStream.GetNEXT();
                                foreach (MyFile file in files)
                                {
                                    myStream.Write(file.Name);
                                    myStream.GetNEXT();

                                    myStream.Write(file.Size);
                                    myStream.GetNEXT();

                                    myStream.Write(file.Address);
                                    myStream.GetNEXT();

                                    myStream.Write(file.Port);
                                    myStream.GetNEXT();
                                }
                            }

                            break;
                    }
                }
            }
            catch (TimeoutException)
            {
                
            }
            catch (Exception)
            {

            }
        }

        ~ClientHandler()
        {
            this.Stop();
        }
    }

}