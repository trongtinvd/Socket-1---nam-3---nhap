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

        List<DownloadableFile> files = new List<DownloadableFile>();
        List<DownloadFile> downloads = new List<DownloadFile>();

        public MainWindow()
        {
            InitializeComponent();

            FileList.ItemsSource = files;
            DownloadList.ItemsSource = downloads;
        }

        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            GridView gView = listView.View as GridView;

            var workingWidth = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth - 10; // take into account vertical scrollbar
            var col1 = 0.60;
            var col2 = 0.20;
            var col3 = 0.20;

            gView.Columns[0].Width = workingWidth * col1;
            gView.Columns[1].Width = workingWidth * col2;
            gView.Columns[2].Width = workingWidth * col3;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            lock (locker)
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
                    MessageBox.Show("Client has already started,", "Client: started");
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            lock (locker)
            {
                client?.Close();
                client = null;

                connectToMainServerThread?.Abort();
                connectToMainServerThread = null;

                files.Clear();
                downloads.Clear();

                MessageBox.Show("Client close", "Close");
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            CloseButton_Click(sender, null);
        }

        private void connectToMainServer(IPEndPoint serverIPEndPoint)
        {
            try
            {
                client = new TcpClient(AddressFamily.InterNetworkV6);
                client.Connect(serverIPEndPoint);
                MyStreamIO myStream = new MyStreamIO(client.GetStream());
                myStream.Write("<isClient>");
                myStream.GetNEXT();

                while (true)
                {
                    myStream.Write("<getAllFileInfo>");
                    int numberOfFileServer = myStream.ReadInt();
                    myStream.SendNEXT();
                    lock (locker)
                    {
                        files.Clear();

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

                                files.Add(new DownloadableFile(fileName, fileSize, ip, port));
                            }
                        }

                        UpdateItemList();
                    }

                    Thread.Sleep(5000);
                }
            }
            catch (Exception)
            {

            }
        }

        private IPEndPoint GetServerIP()
        {
            IPEndPoint serverIP = IPBuilder.GetIP(MainServerIP.Text, int.Parse(MainServerPort.Text));
            return serverIP;
        }

        public void UpdateItemList()
        {
            this.Dispatcher.Invoke(() =>
            {
                FileList.Items.Refresh();
                DownloadList.Items.Refresh();
            });
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

                MyStreamIO myStream = new MyStreamIO(client.GetStream());

                myStream.Write("<isClient>");
                myStream.GetNEXT();

                myStream.Write(file.FileName);
                string rely = myStream.ReadString();

                if (rely != "<fileFound>")
                {
                    client.Close();
                    return;
                }

                IPEndPoint udpClientIP = IPBuilder.GetIP();
                UdpClient udpClient = new UdpClient(udpClientIP);


                myStream.Write(udpClientIP.Address.ToString());
                string udpListenerIP = myStream.ReadString();

                myStream.Write(udpClientIP.Port);
                int udpListenerPort = myStream.ReadInt();

                client.Close();

                UdpDownload(udpClient, IPBuilder.GetIP(udpListenerIP, udpListenerPort), file);

                udpClient.Close();
            }
            catch (Exception)
            {

            }

        }

        private void UdpDownload(UdpClient udpClient, IPEndPoint udpListenerIP, DownloadableFile file)
        {
            byte[] hashBuffer;
            byte[] dataBuffer;
            byte[] messageBuffer;
            long filesize = file.FileSize;
            long received = 0;


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
            }
            catch (Exception)
            {
                
            }

        }
    }
}