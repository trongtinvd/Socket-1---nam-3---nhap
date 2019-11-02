using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
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
using System.IO;
using System.Collections.ObjectModel;

namespace MainServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        object lockObject = new object();

        TcpListener Listener;
        Thread startServerThread;

        List<FileServerHandler> fileServersItem = new List<FileServerHandler>();
        List<ClientHandler> clientsItem = new List<ClientHandler>();

        public MainWindow()
        {
            InitializeComponent();

            FileServerList.ItemsSource = fileServersItem;
            ClientList.ItemsSource = clientsItem;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            lock (lockObject)
            {
                if (startServerThread == null)
                {
                    IPEndPoint IP = GetServerIP();
                    startServerThread = new Thread(() => StartServer(IP));
                    startServerThread.Start();
                    MessageBox.Show("Your server has started.", "Main server: Server is started");
                }
                else
                {
                    MessageBox.Show("your server has already started.", "Main server: Error");
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            lock (lockObject)
            {
                if (startServerThread != null)
                {
                    Listener.Stop();
                    startServerThread.Abort();

                    Listener = null;
                    startServerThread = null;
                }

                fileServersItem.Clear();
                clientsItem.Clear();
                UpdateItemList();
            }
        }

        private void List_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            GridView gView = listView.View as GridView;

            var workingWidth = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth - 10;
            var col1 = 0.70;
            var col2 = 0.30;

            gView.Columns[0].Width = workingWidth * col1;
            gView.Columns[1].Width = workingWidth * col2;
        }

        private void StartServer(IPEndPoint localEP)
        {
            try
            {
                Listener = new TcpListener(localEP);
                Listener.Start();


                while (true)
                {
                    TcpClient client = Listener.AcceptTcpClient();
                    MyStreamIO myStream = new MyStreamIO(client.GetStream());
                    string firstMessage = myStream.ReadString();

                    if (firstMessage == "<isFileServer>")
                    {
                        FileServerHandler handler = new FileServerHandler(client);
                        fileServersItem.Add(handler);
                        handler.Start();
                    }
                    else if (firstMessage == "<isClient>")
                    {
                        ClientHandler handler = new ClientHandler(client);
                        clientsItem.Add(handler);
                        handler.Start();
                    }

                    UpdateItemList();
                }

            }
            catch (ThreadAbortException e)
            {
                MessageBox.Show("Your server has closed.", "Main server: Server closed");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Main server: " + e.ToString());
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

        private void Window_Closed(object sender, EventArgs e)
        {
            CloseButton_Click(this, null);
        }

        public void UpdateItemList()
        {
            this.Dispatcher.Invoke(() =>
            {
                FileServerList.Items.Refresh();
                ClientList.Items.Refresh();
            });
        }
    }
}