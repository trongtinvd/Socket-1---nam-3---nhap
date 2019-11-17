using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
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

namespace FileServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TcpListener listener = null;
        TcpClient client = null;
        MyStreamIO myStream;

        IPEndPoint ipAsClient = IPBuilder.GetIP();
        IPEndPoint ipAsServer = IPBuilder.GetIP();

        Thread connectToMainServerThread;
        Thread makeConnectToClientThread;

        public MainWindow()
        {
            InitializeComponent();

            ListHolder.Clients = new List<ClientHandler>();
            ListHolder.Files = new List<MyFile>();

            ListHolder.ClientsList = ClientList;
            ListHolder.FilesList = FileList;

            ListHolder.ClientsList.ItemsSource = ListHolder.Clients;
            ListHolder.FilesList.ItemsSource = ListHolder.Files;

            MyDispatcher.Dispatcher = this.Dispatcher;

            try
            {
                string[] files = Directory.GetFiles("./file/");

                foreach (string file in files)
                {
                    ListHolder.Files.Add(new MyFile(file));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "File server error: when initialize");
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

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (listener == null && client == null)
            {
                IPEndPoint mainServerIP = GetMainServerIP();

                connectToMainServerThread = new Thread(() => ConnectToMainServer(mainServerIP));
                connectToMainServerThread.Start();

                makeConnectToClientThread = new Thread(() => ListenClientRequest());
                makeConnectToClientThread.Start();

                MessageBox.Show("Your file server had started", "File server: File server started");
            }
            else
            {
                MessageBox.Show("Your file server had already start.", "File server: Error");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
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

            listener?.Stop();
            listener = null;

            makeConnectToClientThread?.Abort();
            makeConnectToClientThread = null;

            foreach (ClientHandler handler in ListHolder.Clients.ToList())
            {
                handler.Stop();
            }

            MessageBox.Show("Your file server had stopped", "File server stop");
        }

        private IPEndPoint GetMainServerIP()
        {
            IPEndPoint ip = IPBuilder.GetIP(MainServerAddress.Text, int.Parse(MainServerPort.Text));
            return ip;
        }

        private void ConnectToMainServer(IPEndPoint serverIP)
        {
            try
            {
                client = new TcpClient(ipAsClient);
                client.Connect(serverIP);
                client.Client.ReceiveTimeout = 7000;
                client.Client.SendTimeout = 7000;

                myStream = new MyStreamIO(client.GetStream());

                myStream.Write("<isFileServer>");
                myStream.GetNEXT();

                while (true)
                {
                    int numberOfFile = ListHolder.Files.Count;

                    myStream.Write("<sendFilesInfo>");
                    myStream.GetNEXT();

                    myStream.Write(numberOfFile);
                    myStream.GetNEXT();

                    foreach (MyFile file in ListHolder.Files)
                    {
                        myStream.Write(file.FileName);
                        myStream.GetNEXT();

                        myStream.Write(file.FileSize);
                        myStream.GetNEXT();

                        myStream.Write(ipAsServer.Address.ToString());
                        myStream.GetNEXT();

                        myStream.Write(ipAsServer.Port);
                        myStream.GetNEXT();
                    }


                    Thread.Sleep(5000);
                }

            }
            catch (ThreadAbortException)
            {

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "File server error: when working with main server");
            }
        }

        private void ListenClientRequest()
        {
            listener = new TcpListener(ipAsServer);
            listener.Start();

            try
            {
                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    ClientHandler handle = new ClientHandler(client);
                    ListHolder.Clients.Add(handle);
                    ListHolder.UpdateList();
                    handle.Start();
                }
            }
            catch (ThreadAbortException)
            {

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "File server error: when waiting for client connection");
            }

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            CloseButton_Click(sender, null);
        }
    }
}
