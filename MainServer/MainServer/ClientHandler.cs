using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;

namespace MainServer
{
    internal class ClientHandler
    {
        private object locker = new object();
        private Thread workingWithClientThread;
        private MyStreamIO myStream;
        public TcpClient Client { get; }

        public string Address => ((IPEndPoint)Client.Client.RemoteEndPoint).Address.ToString();
        public int Port => ((IPEndPoint)Client.Client.RemoteEndPoint).Port;


        public ClientHandler(TcpClient client)
        {
            this.Client = client;
            myStream = new MyStreamIO(client.GetStream());
        }

        ~ClientHandler()
        {
            this.Stop();
        }

        public void Start()
        {
            if (workingWithClientThread == null)
            {
                workingWithClientThread = new Thread(() => WorkingWithClient(Client));
                workingWithClientThread.Start();
            }
        }

        public void Stop()
        {
            lock (locker)
            {
                Client?.GetStream()?.Close();
                Client?.Close();
                //Client?.Dispose();

                workingWithClientThread?.Abort();
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

                    lock (locker)
                    {
                        switch (request)
                        {
                            case "<getAllFileInfo>":
                                myStream.Write(ListHolder.FileServers.Count);
                                myStream.GetNEXT();
                                foreach (FileServerHandler fileServer in ListHolder.FileServers)
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

                            default:
                                throw new Exception("Receive strange request from client: \"" + request + "\"");
                        }
                    }
                }
            }
            catch (TimeoutException)
            {
                ListHolder.Clients.Remove(this);
                ListHolder.UpdateList();
            }
            catch (Exception e)
            {
                MyDispatcher.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(e.Message, "Main server error: when working with client");
                });

                ListHolder.Clients.Remove(this);
                ListHolder.UpdateList();
            }
        }
    }

}