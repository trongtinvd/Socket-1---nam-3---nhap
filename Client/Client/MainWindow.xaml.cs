using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        object locker = new object();

        Thread connectToMainServerThread;
        TcpClient client;

        public MainWindow()
        {
            InitializeComponent();

            ListHolder.DownloadableFiles = new List<DownloadableFile>();
            ListHolder.DownloadedFiles = new List<DownloadedFile>();

            ListHolder.DownloadableFilesList = DownloadableFileList;
            ListHolder.DownloadedFilesList = DownloadedFileList;

            ListHolder.DownloadableFilesList.ItemsSource = ListHolder.DownloadableFiles;
            ListHolder.DownloadedFilesList.ItemsSource = ListHolder.DownloadedFiles;

            MyDispatcher.Dispatcher = this.Dispatcher;
        }

        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            GridView gView = listView.View as GridView;

            var workingWidth = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth - 10;
            var col1 = 0.65;
            var col2 = 0.30;

            gView.Columns[0].Width = workingWidth * col1;
            gView.Columns[1].Width = workingWidth * col2;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (connectToMainServerThread == null)
            {
                IPEndPoint serverIPEndPoint = GetServerIP();
                connectToMainServerThread = new Thread(() => connectToMainServer(serverIPEndPoint));
                connectToMainServerThread.Start();
                MessageBox.Show("Client has started,", "Client: started");
            }
            else
            {
                MessageBox.Show("Client has already started,", "Error");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            lock (locker)
            {
                if (client != null)
                {
                    if (client.Connected)
                        client?.GetStream()?.Close();
                    client?.Close();
                    client = null;
                }

                connectToMainServerThread?.Abort();
                connectToMainServerThread = null;

                ListHolder.DownloadableFiles.Clear();
                ListHolder.UpdateList();

                MessageBox.Show("Client close", "Close");
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            CloseButton_Click(sender, null);
        }

        private void connectToMainServer(IPEndPoint serverIPEndPoint)
        {
            MyStreamIO myStream = null;
            try
            {
                client = new TcpClient(AddressFamily.InterNetworkV6);
                client.Connect(serverIPEndPoint);
                myStream = new MyStreamIO(client.GetStream());
                myStream.Write("<isClient>");
                myStream.GetNEXT();


                client.ReceiveTimeout = 7000;
                client.SendTimeout = 7000;
                while (true)
                {
                    lock (locker)
                    {
                        myStream.Write("<getAllFileInfo>");
                        int numberOfFileServer = myStream.ReadInt();
                        myStream.SendNEXT();
                        ListHolder.DownloadableFiles.Clear();

                        for (int i = 0; i < numberOfFileServer; i++)
                        {
                            int numberOfFile = myStream.ReadInt();
                            myStream.SendNEXT();

                            for (int j = 0; j < numberOfFile; j++)
                            {
                                string fileName = myStream.ReadString();
                                myStream.SendNEXT();

                                long fileSize = myStream.ReadLong();
                                myStream.SendNEXT();

                                string ip = myStream.ReadString();
                                myStream.SendNEXT();

                                int port = myStream.ReadInt();
                                myStream.SendNEXT();

                                ListHolder.DownloadableFiles.Add(new DownloadableFile(fileName, fileSize, ip, port));
                            }
                        }

                        ListHolder.UpdateList();
                    }

                    Thread.Sleep(5000);
                }
            }
            catch (ThreadAbortException)
            {

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Client error: when getting file info from main server");
            }
        }

        private IPEndPoint GetServerIP()
        {
            IPEndPoint serverIP = IPBuilder.GetIP(MainServerIP.Text, int.Parse(MainServerPort.Text));
            return serverIP;
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            DownloadableFile file = (sender as Button).DataContext as DownloadableFile;
            Thread downloadThread = new Thread(() => Download(file));
            downloadThread.Start();
        }

        private void Download(DownloadableFile file)
        {
            try
            {
                TcpClient client = new TcpClient(IPBuilder.GetIP());
                IPEndPoint serverIP = IPBuilder.GetIP(file.IP, file.Port);

                client.Connect(serverIP);
                client.Client.ReceiveTimeout = 7000;
                client.Client.SendTimeout = 7000;

                MyStreamIO myStream = new MyStreamIO(client.GetStream());

                myStream.Write("<isClient>");
                myStream.GetNEXT();

                myStream.Write(file.FileName);
                string rely = myStream.ReadString();

                if (rely != "<fileFound>")
                {
                    client.GetStream().Close();
                    client.Close();
                    return;
                }

                IPEndPoint udpClientIP = IPBuilder.GetIP();
                UdpClient udpClient = new UdpClient(udpClientIP);


                myStream.Write(udpClientIP.Address.ToString());
                string udpListenerIP = myStream.ReadString();

                myStream.Write(udpClientIP.Port);
                int udpListenerPort = myStream.ReadInt();

                client.GetStream().Close();
                client.Close();

                UdpDownload(udpClient, IPBuilder.GetIP(udpListenerIP, udpListenerPort), file);

                udpClient.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Client error: when getting file info for download from file server");
            }

        }

        private void UdpDownload(UdpClient udpClient, IPEndPoint udpListenerIP, DownloadableFile file)
        {
            byte[] hashBuffer;
            byte[] dataBuffer;
            byte[] messageBuffer;
            long filesize = file.FileSize;
            long received = 0;

            DownloadedFile newFileInfo = new DownloadedFile(file.FileName, "Downloading...");
            ListHolder.DownloadedFiles.Add(newFileInfo);
            ListHolder.UpdateList();

            udpClient.Client.ReceiveTimeout = 7000;
            udpClient.Client.SendTimeout = 7000;
            try
            {
                byte[] relyBuffer = udpClient.Receive(ref udpListenerIP);

                messageBuffer = Encoding.UTF8.GetBytes("<ok>");
                udpClient.Send(messageBuffer, messageBuffer.Length, udpListenerIP);

                using (FileStream newFile = File.OpenWrite(file.ShortenFileName))
                {
                    while (received < filesize)
                    {
                        hashBuffer = udpClient.Receive(ref udpListenerIP);
                        dataBuffer = udpClient.Receive(ref udpListenerIP);
                        
                        string hash = Encoding.UTF8.GetString(hashBuffer);
                        string data = Encoding.UTF8.GetString(dataBuffer);

                        if (MyMD5Hash.VerifyMd5Hash(dataBuffer, hash) == false)
                        {
                            messageBuffer = Encoding.UTF8.GetBytes("<error>");
                            udpClient.Send(messageBuffer, messageBuffer.Length, udpListenerIP);
                            continue;
                        }

                        messageBuffer = Encoding.UTF8.GetBytes("<ok>");
                        udpClient.Send(messageBuffer, messageBuffer.Length, udpListenerIP);

                        newFile.Write(dataBuffer, 0, dataBuffer.Length);
                        received += dataBuffer.Length;

                    }
                }

                udpClient.Close();
                newFileInfo.Status = "Finish";
                ListHolder.UpdateList();

                MyDispatcher.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("finish download file: " + file.FileName);
                });
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Client error: when download file from file server");
                newFileInfo.Status = "Download error";
                ListHolder.UpdateList();
            }

        }
    }
}