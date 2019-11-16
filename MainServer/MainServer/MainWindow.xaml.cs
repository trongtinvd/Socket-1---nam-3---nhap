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
        object locker = new object();

        TcpListener Listener;
        Thread startServerThread;

        List<FileServerHandler> fileServers = new List<FileServerHandler>();
        List<ClientHandler> clients = new List<ClientHandler>();

        public MainWindow()
        {
            InitializeComponent();
            
            FileServerList.ItemsSource = fileServers;
            ClientList.ItemsSource = clients;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            lock (locker)
            {
                if (startServerThread == null)
                {
                    IPEndPoint mainServerIP = IPBuilder.GetIP(MainServerIP.Text, int.Parse(MainServerPort.Text));
                    startServerThread = new Thread(() => StartServer(mainServerIP));
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
            lock (locker)
            {
                if (startServerThread != null)
                {
                    Listener.Stop();
                    startServerThread.Abort();

                    Listener = null;
                    startServerThread = null;
                }

                foreach(FileServerHandler handler in fileServers)
                {
                    handler.Stop();
                }

                fileServers.Clear();
                clients.Clear();
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

        private void StartServer(IPEndPoint serverIP)
        {
            try
            {
                Listener = new TcpListener(serverIP);
                Listener.Start();


                while (true)
                {
                    TcpClient client = Listener.AcceptTcpClient();
                    MyStreamIO myStream = new MyStreamIO(client.GetStream());
                    string firstMessage = myStream.ReadString();
                    myStream.SendNEXT();

                    if (firstMessage == "<isFileServer>")
                    {
                        FileServerHandler handler = new FileServerHandler(client);
                        fileServers.Add(handler);
                        handler.Start();
                    }
                    else if (firstMessage == "<isClient>")
                    {
                        ClientHandler handler = new ClientHandler(client, fileServers);
                        this.clients.Add(handler);
                        handler.Start();
                    }

                    UpdateItemList();
                }

            }
            catch (ThreadAbortException)
            {
                MessageBox.Show("Your server has closed.", "Main server: Server closed");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Main server: " + e.ToString());
            }
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