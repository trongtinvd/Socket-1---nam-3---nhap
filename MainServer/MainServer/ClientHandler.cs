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
        private object lockObj = new object();
        private List<FileServerHandler> fileServerItems;
        private Thread workingWithClientThread;
        private MyStreamIO myStream;
        public TcpClient Client { set; get; }


        public string IP
        {
            get => ((IPEndPoint)Client.Client.RemoteEndPoint).Address.ToString();
        }

        public int Port
        {
            get => ((IPEndPoint)Client.Client.RemoteEndPoint).Port;
        }






        public ClientHandler(TcpClient client, List<FileServerHandler> fileServerItems)
        {
            lock (lockObj)
            {
                this.Client = client;
                this.fileServerItems = fileServerItems;
                myStream = new MyStreamIO(client.GetStream());
            }
        }

        ~ClientHandler()
        {
            Stop();
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
                while (true)
                {
                    string request = myStream.ReadString();

                    switch (request)
                    {
                        case "<getAllFileInfo>":
                            myStream.Write(fileServerItems.Count);
                            myStream.GetNEXT();
                            foreach(FileServerHandler fileServer in fileServerItems)
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

                                    myStream.Write(file.IP);
                                    myStream.GetNEXT();

                                    myStream.Write(file.Port);
                                    myStream.GetNEXT();
                                }
                            }

                            break;


                        default:
                            throw new Exception("Error communicate with file server");
                    }
                }
            }
            catch(Exception e)
            {

            }
        }
    }
}