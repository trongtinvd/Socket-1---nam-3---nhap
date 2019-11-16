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

        IPEndPoint ipAsClient = IPBuilder.GetIP();
        IPEndPoint ipAsServer = IPBuilder.GetIP();


        object locker = new object();        
        MyStreamIO myStream;

        Thread connectToMainServerThread;
        Thread listenClientRequestThread;

        List<ClientHandler> clientItems = new List<ClientHandler>();
        List<MyFile> fileItems = new List<MyFile>();
            
        public MainWindow()
        {
            InitializeComponent();

            try
            {
                string[] files = Directory.GetFiles("./file/");

                foreach(string file in files)
                {
                    fileItems.Add(new MyFile(file));
                }
    
                FileList.ItemsSource = fileItems;
                ClientList.ItemsSource = clientItems;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "File server - error initialize: " + e.ToString());
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
            lock (locker)
            {
                if (listener == null && client == null)
                {
                    IPEndPoint mainServerIP = GetMainServerIP();

                    connectToMainServerThread = new Thread(() => ConnectToMainServer(mainServerIP));
                    connectToMainServerThread.Start();

                    listenClientRequestThread = new Thread(() => ListenClientRequest());
                    listenClientRequestThread.Start();

                    MessageBox.Show("Your file server had started", "File server: File server started");
                }
                else
                {
                    MessageBox.Show("Your file server had already start.", "File server: Error");
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

                listener?.Stop();
                listener = null;

                listenClientRequestThread?.Abort();
                listenClientRequestThread = null;

                MessageBox.Show("Your file server had stopped", "File server stop");

            }
        }

        private IPEndPoint GetMainServerIP()
        {
            IPEndPoint ip = IPBuilder.GetIP(MainServerIP.Text, int.Parse(MainServerPort.Text));
            return ip;
        }

        private void ConnectToMainServer(IPEndPoint serverIP)
        {
            try
            {
                client = new TcpClient(ipAsClient);
                client.Connect(serverIP);

                myStream = new MyStreamIO(client.GetStream());

                myStream.Write("<isFileServer>");
                myStream.GetNEXT();

                while (true)
                {

                    int numberOfFile = fileItems.Count;

                    myStream.Write("<sendFilesInfo>");
                    myStream.GetNEXT();

                    myStream.Write(numberOfFile);
                    myStream.GetNEXT();

                    foreach (MyFile file in fileItems)
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
                MessageBox.Show("Error when working with main server\n" + e.Message,"File server: " + e.ToString());
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
                    ClientHandler handle = new ClientHandler(client, fileItems);
                    clientItems.Add(handle);
                    handle.Start();
                }
            }
            catch
            {

            }

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            CloseButton_Click(sender, null);
        }
    }
}
