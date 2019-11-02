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
        private static object lockObj= new object();

        private Thread workingWithFileServerThread;
        private MyStreamIO myStream;
        private List<string> files = new List<string>();

        public TcpClient Client { set; get; }

        public string IP
        {
            get => ((IPEndPoint)Client.Client.RemoteEndPoint).Address.ToString();
        }

        public int Port
        {
            get => ((IPEndPoint)Client.Client.RemoteEndPoint).Port;
        }






        public FileServerHandler(TcpClient client)
        {
            lock (lockObj)
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
            workingWithFileServerThread = new Thread(() => workingWithFileServer(Client));
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

        private void workingWithFileServer(TcpClient client)
        {
            try
            {
                while (true)
                {
                    string request = myStream.ReadString();
                    myStream.SendNEXT();

                    switch (request)
                    {
                        case "<sendFiles>":
                            files.Clear();
                            int number0fFile = myStream.ReadInt();
                            myStream.SendNEXT();
                            for (int i = 0; i < number0fFile; i++)
                            {
                                string fileName = myStream.ReadString();
                                myStream.SendNEXT();
                                files.Add(fileName);
                            }
                            break;
                    }
                }
            }
            catch (Exception e)
            {

            }            
        }
    }
}