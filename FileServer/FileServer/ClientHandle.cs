using System;
using System.Threading;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text;

namespace FileServer
{
    internal class ClientHandler
    {
        private TcpClient client;
        private List<MyFile> fileList;
        private Thread workWithClientThread;
        private MyStreamIO myStream;
        private IPEndPoint udpListenerIP;
        private UdpClient udpListener;

        public ClientHandler(TcpClient client, List<MyFile> fileList)
        {
            this.client = client;
            myStream = new MyStreamIO(client.GetStream());
            this.fileList = fileList;
        }

        internal void Start()
        {
            workWithClientThread = new Thread(workWithClient);
            workWithClientThread.Start();
        }

        private void workWithClient()
        {
            try
            {
                string firstMessage = myStream.ReadString();
                myStream.SendNEXT();

                if (firstMessage != "<isClient>")
                    throw new Exception($"first message = {firstMessage}, unknown client try to connect");


                string fileName = myStream.ReadString();
                if (!fileList.Exists(c => c.FileName == fileName))
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
            catch
            {

            }
        }

        private void SendFile(string fileName, IPEndPoint udpClientIP)
        {
            string fileSize = new FileInfo(fileName).Length.ToString();

            byte[] messageBuffer = Encoding.UTF8.GetBytes(fileSize);
            udpListener.Send(messageBuffer, messageBuffer.Length, udpClientIP);

            messageBuffer = udpListener.Receive(ref udpClientIP);


            using (FileStream file = File.OpenRead(fileName))
            {
                byte[] buffer = new byte[1024];
                byte[] relyBuffer;
                bool sendSuccess;
                int size;

                //udpListener.Client.ReceiveTimeout = 5000;
                while ((size = file.Read(buffer, 0, buffer.Length)) > 0)
                {
                    string data = Encoding.UTF8.GetString(buffer).Substring(0, size);
                    string hash = MyMD5Hash.GetMd5Hash(data);
                    byte[] hashBuffer = Encoding.UTF8.GetBytes(hash);

                    do
                    {
                        udpListener.Send(hashBuffer, hashBuffer.Length, udpClientIP);
                        udpListener.Send(buffer, size, udpClientIP);

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


        ~ClientHandler()
        {
            if (client != null)
            {
                client.Close();
            }
        }
    }
}