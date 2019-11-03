using System;
using System.Collections.Generic;
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
        object lockObj = new object();

        Thread connectToMainServerThread;
        TcpClient client;

        List<DownloadableFile> fileItems = new List<DownloadableFile>();
        List<DownloadFile> downloadItems = new List<DownloadFile>();

        public MainWindow()
        {
            InitializeComponent();

            FileList.ItemsSource = fileItems;
            DownloadList.ItemsSource = downloadItems;
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
            lock (lockObj)
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
            lock (lockObj)
            {
                if (connectToMainServerThread != null)
                {
                    client.Close();
                    connectToMainServerThread.Abort();

                    client = null;
                    connectToMainServerThread = null;
                }

                fileItems.Clear();
                downloadItems.Clear();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            CloseButton_Click(sender, null);
        }

        private void connectToMainServer(IPEndPoint serverIPEndPoint)
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
                lock (lockObj)
                {
                    fileItems.Clear();

                    for (int i = 0; i < numberOfFileServer; i++)
                    {
                        string ip = myStream.ReadString();
                        myStream.SendNEXT();
                        int port = myStream.ReadInt();
                        myStream.SendNEXT();
                        int numberOfFile = myStream.ReadInt();
                        myStream.SendNEXT();

                        for (int j = 0; j < numberOfFile; j++)
                        {
                            string fileName = myStream.ReadString();
                            myStream.SendNEXT();
                            long fileSize = myStream.ReadLong();
                            myStream.SendNEXT();

                            fileItems.Add(new DownloadableFile(fileName, fileSize, ip, port));
                        }
                    }

                    UpdateItemList();
                }

                Thread.Sleep(5000);
            }
        }

        private IPEndPoint GetServerIP()
        {
            IPAddress address;
            int port = int.Parse(MainServerPort.Text);


            if (MainServerIP.Text == "localhost")
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
                address = hostEntry.AddressList[0];
            }
            else
            {
                address = IPAddress.Parse(MainServerIP.Text);
            }


            IPEndPoint IP = new IPEndPoint(address, port);
            return IP;
        }

        public void UpdateItemList()
        {
            this.Dispatcher.Invoke(() =>
            {
                FileList.Items.Refresh();
                DownloadList.Items.Refresh();
            });
        }
    }
}