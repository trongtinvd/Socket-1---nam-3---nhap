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
        Thread makeConnectionThread;

        public MainWindow()
        {
            InitializeComponent();

            ListHolder.FileServers = new List<FileServerHandler>();
            ListHolder.Clients = new List<ClientHandler>();

            ListHolder.FileServersList = FileServerList;
            ListHolder.ClientsList = ClientList;
            
            ListHolder.FileServersList.ItemsSource = ListHolder.FileServers;
            ListHolder.ClientsList.ItemsSource = ListHolder.Clients;

            MyDispatcher.Dispatcher = this.Dispatcher;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            lock (locker)
            {
                if (makeConnectionThread == null)
                {
                    IPEndPoint mainServerIP = GetMainServerIP();
                    makeConnectionThread = new Thread(() => MakeConnect(mainServerIP));
                    makeConnectionThread.Start();
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
                Listener?.Stop();
                Listener = null;

                makeConnectionThread?.Abort();
                makeConnectionThread = null;


                foreach (FileServerHandler handler in ListHolder.FileServers)
                {
                    handler.Stop();
                }

                foreach (ClientHandler handler in ListHolder.Clients)
                {
                    handler.Stop();
                }

                ListHolder.FileServers.Clear();
                ListHolder.Clients.Clear();
                ListHolder.UpdateList();
                MessageBox.Show("Your server has closed.", "Main server: Server closed");
            }
        }

        private IPEndPoint GetMainServerIP()
        {
            IPEndPoint ip = IPBuilder.GetIP(MainServerAddress.Text, int.Parse(MainServerPort.Text));
            return ip;
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

        private void MakeConnect(IPEndPoint serverIP)
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
                        ListHolder.FileServers.Add(handler);
                        handler.Start();
                    }
                    else if (firstMessage == "<isClient>")
                    {
                        ClientHandler handler = new ClientHandler(client);
                        ListHolder.Clients.Add(handler);
                        handler.Start();
                    }

                    ListHolder.UpdateList();
                }

            }
            catch (ThreadAbortException)
            {
                
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Main server error: when waiting for connection");
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            CloseButton_Click(this, null);
        }
    }
}