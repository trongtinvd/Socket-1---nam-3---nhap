using System;
using System.ComponentModel;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Windows;

namespace MainServer
{
    internal class FileServerHandler
    {
        private object locker = new object();

        private Thread workingWithFileServerThread;
        private MyStreamIO myStream;
        //private List<MyFile> _files;

        public TcpClient Client { get; }
        public List<MyFile> Files { get; } = new List<MyFile>();

        public string Address => ((IPEndPoint)Client.Client.RemoteEndPoint).Address.ToString();
        public int Port => ((IPEndPoint)Client.Client.RemoteEndPoint).Port;


        public FileServerHandler(TcpClient client)
        {
            this.Client = client;
            myStream = new MyStreamIO(client.GetStream());
        }

        ~FileServerHandler()
        {
            Stop();
        }

        public void Start()
        {
            if (workingWithFileServerThread == null)
            {
                workingWithFileServerThread = new Thread(() => WorkingWithFileServer(Client));
                workingWithFileServerThread.Start();
            }
        }

        public void Stop()
        {
            lock (locker)
            {
                Client?.GetStream()?.Close();
                Client?.Close();
                //Client?.Dispose();

                workingWithFileServerThread?.Abort();
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

                    lock (locker)
                    {
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

                            default:
                                throw new Exception("Receive strange request from file server: \"" + request + "\"");
                        }
                    }
                }
            }
            catch (TimeoutException)
            {
                Files.Clear();
                ListHolder.FileServers.Remove(this);
                ListHolder.UpdateList();
            }
            catch (Exception e)
            {
                MyDispatcher.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(e.Message, "Main server error: when working with file server");
                });


                Files.Clear();
                ListHolder.FileServers.Remove(this);
                ListHolder.UpdateList();
            }
        }
    }
}