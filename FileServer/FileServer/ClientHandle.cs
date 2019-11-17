using System;
using System.Threading;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text;
using System.Windows;

namespace FileServer
{
    public class ClientHandler
    {
        private object locker = new object();
        private MyStreamIO myStream;
        private IPEndPoint udpListenerIP;
        private UdpClient udpListener;
        private Thread workWithClientThread;
        private TcpClient Client { get; set; }

        public string Address => (Client != null) ? ((IPEndPoint)Client.Client.RemoteEndPoint).Address.ToString() : "";
        public int Port => (Client != null) ? ((IPEndPoint)Client.Client.RemoteEndPoint).Port : 0;

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
            workWithClientThread = new Thread(WorkWithClient);
            workWithClientThread.Start();
        }

        public void Stop()
        {
            lock (locker)
            {
                try
                {
                    myStream?.Stop();
                    myStream = null;

                    udpListener?.Close();
                    udpListener = null;

                    Client?.Close();
                    Client = null;

                    workWithClientThread?.Abort();
                    workWithClientThread = null;

                    MyDispatcher.Dispatcher.Invoke(() =>
                    {
                        ListHolder.Clients.Remove(this);
                        ListHolder.UpdateList();
                    });

                }
                catch (Exception)
                {

                }
            }
        }

        private void WorkWithClient()
        {
            try
            {
                Client.Client.ReceiveTimeout = 7000;
                Client.Client.SendTimeout = 7000;
                string firstMessage = myStream.ReadString();
                myStream.SendNEXT();

                if (firstMessage != "<isClient>")
                    throw new Exception($"first message = {firstMessage}, unknown client try to connect");


                string fileName = myStream.ReadString();
                if (!ListHolder.Files.Exists(c => c.FileName == fileName))
                {
                    myStream.Write("<unknownFile>");
                    return;
                }
                else
                    myStream.Write("<fileFound>");

                udpListenerIP = IPBuilder.GetIP();
                udpListener = new UdpClient(udpListenerIP);

                string udpClientAddress = myStream.ReadString();
                myStream.Write(udpListenerIP.Address.ToString());

                int udpClientPort = myStream.ReadInt();
                myStream.Write(udpListenerIP.Port);

                IPEndPoint udpClientIP = IPBuilder.GetIP(udpClientAddress, udpClientPort);

                SendFile(fileName, udpClientIP);

                udpListener.Close();
            }
            catch (ThreadAbortException)
            {

            }
            catch (Exception e)
            {
                MyDispatcher.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(e.Message, "File server error: when initial sending file");
                });
            }
        }

        private void SendFile(string fileName, IPEndPoint udpClientIP)
        {
            string fileSize = new FileInfo(fileName).Length.ToString();
            //udpListener.Client.ReceiveTimeout = 7000;
            //udpListener.Client.SendTimeout = 7000;


            byte[] messageBuffer = Encoding.UTF8.GetBytes(fileSize);
            udpListener.Send(messageBuffer, messageBuffer.Length, udpClientIP);

            messageBuffer = udpListener.Receive(ref udpClientIP);


            using (FileStream file = File.OpenRead(fileName))
            {
                byte[] dataBuffer = new byte[1024];
                byte[] relyBuffer;
                bool sendSuccess;
                int size;

                try
                {

                    lock (locker)
                    {
                        while ((size = file.Read(dataBuffer, 0, dataBuffer.Length)) > 0)
                        {
                            string data = Encoding.UTF8.GetString(dataBuffer);

                            byte[] copyBuffer = new byte[size];
                            Array.Copy(dataBuffer, copyBuffer, size);

                            string hash = MyMD5Hash.GetMd5Hash(copyBuffer);
                            byte[] hashBuffer = Encoding.UTF8.GetBytes(hash);

                            do
                            {
                                udpListener.Send(hashBuffer, hashBuffer.Length, udpClientIP);
                                udpListener.Send(dataBuffer, size, udpClientIP);

                                try
                                {
                                    relyBuffer = udpListener.Receive(ref udpClientIP);
                                    string rely = Encoding.UTF8.GetString(relyBuffer);

                                    if (rely == "<ok>")
                                        sendSuccess = true;
                                    else
                                        sendSuccess = false;

                                }
                                catch (TimeoutException e)
                                {
                                    sendSuccess = false;
                                }

                            } while (!sendSuccess);

                        }
                    }
                }
                catch (Exception e)
                {
                    MyDispatcher.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(e.Message, "File server error: when sending file");
                    });
                }
            }
        }
    }
}