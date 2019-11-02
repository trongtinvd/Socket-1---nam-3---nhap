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

namespace FileServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TcpListener listener = null;
        TcpClient client = null;

        object lockObject = new object();

        Thread connectToMainServerThread;
        Thread listenClientRequestThread;

        List<ClientHandler> clientList = new List<ClientHandler>();
        //List<MyFile> fileList = new List<MyFile>();

        public MainWindow()
        {
            InitializeComponent();
            //FileList.ItemsSource = fileList;
            ClientList.ItemsSource = clientList;
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
            lock (lockObject)
            {
                if (listener == null && client == null)
                {
                    IPEndPoint mainServerIP = GetMainServerIP();
                    IPEndPoint thisServerIP = GetAnAvaibleIP();

                    connectToMainServerThread = new Thread(() => ConnectToMainServer(mainServerIP));
                    connectToMainServerThread.Start();

                    //listenClientRequestThread = new Thread(() => ListenClientRequest(thisServerIP));
                    //listenClientRequestThread.Start();

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
            lock (lockObject)
            {
                if (connectToMainServerThread != null)
                {
                    client.Close();
                    connectToMainServerThread.Abort();
                    client = null;
                    connectToMainServerThread = null;
                }

                if (listenClientRequestThread != null)
                {
                    listener.Stop();
                    listenClientRequestThread.Abort();
                    listener = null;
                    listenClientRequestThread = null;
                }
            }
        }

        private IPEndPoint GetMainServerIP()
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

        private IPEndPoint GetAnAvaibleIP()
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress address = hostEntry.AddressList[0];

            IPEndPoint IP = new IPEndPoint(address, 0);
            return IP;
        }

        private void ConnectToMainServer(IPEndPoint IP)
        {
            try
            {
                client = new TcpClient(AddressFamily.InterNetworkV6);
                client.Connect(IP);

                MyStreamIO myStream = new MyStreamIO(client.GetStream());

                myStream.Write("<isFileServer>");

                while (true)
                {
                    Thread.Sleep(5000);
                    //work with main server.
                }

            }
            catch (ThreadAbortException e)
            {
                MessageBox.Show("File server stop connect to main server.", "File server: stopped");
            }
            catch (Exception e)
            {
                MessageBox.Show("Error when working with main server\n" + e.Message, e.ToString());
            }
        }

        private void ListenClientRequest(IPEndPoint IP)
        {
            
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            CloseButton_Click(sender, null);
        }
    }
}
