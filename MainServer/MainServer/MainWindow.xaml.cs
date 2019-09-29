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

namespace MainServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TcpListener Listener;
        Thread serverListener;
        List<FileServerHandler> fileServersItem;
        List<ClientHandler> clientsItem;

        public MainWindow()
        {
            InitializeComponent();
            //UITest();
            fileServersItem = new List<FileServerHandler>();
            clientsItem = new List<ClientHandler>();

            FileServerList.ItemsSource = fileServersItem;
            ClientList.ItemsSource = clientsItem;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (serverListener == null)
            {
                IPEndPoint localEP;
                if (MainServerIP.Text == "localhost")
                {
                    IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
                    localEP = new IPEndPoint(hostEntry.AddressList[0], int.Parse(MainServerPort.Text));
                }
                else
                {
                    localEP = new IPEndPoint(IPAddress.Parse(MainServerIP.Text), int.Parse(MainServerPort.Text));
                }
                serverListener = new Thread(() => StartServer(localEP));
                serverListener.Start();
                MessageBox.Show("Your server had started.", "Server is started");
            }
            else
            {
                MessageBox.Show("your server had already started.", "Error");
            }

        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (Listener != null)
            {
                Listener.Stop();
                Listener = null;
            }
            if (serverListener != null)
            {
                serverListener.Abort();
                serverListener = null;
            }
        }

        private void ClientList_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            GridView gView = listView.View as GridView;

            var workingWidth = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth - 10; // take into account vertical scrollbar
            var col1 = 0.70;
            var col2 = 0.30;

            gView.Columns[0].Width = workingWidth * col1;
            gView.Columns[1].Width = workingWidth * col2;
        }

        private void FileServerList_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            GridView gView = listView.View as GridView;

            var workingWidth = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth - 10; // take into account vertical scrollbar
            var col1 = 0.70;
            var col2 = 0.30;

            gView.Columns[0].Width = workingWidth * col1;
            gView.Columns[1].Width = workingWidth * col2;
        }

        public void UITest()
        {
            List<SampleConection> sampleConectionsList = new List<SampleConection>()
            {
                new SampleConection(){IP="172.16.0.4", Port="7792"},
                new SampleConection(){IP="82.9.0.4", Port="7704"},
                new SampleConection(){IP="192.16.0.4", Port="0492"},
                new SampleConection(){IP="10.16.0.4", Port="7042"},
                new SampleConection(){IP="7.16.0.4", Port="7704"},
                new SampleConection(){IP="113.16.0.4", Port="0492"},
                new SampleConection(){IP="201.16.0.4", Port="0492"},
                new SampleConection(){IP="204.16.0.4", Port="0492"},
                new SampleConection(){IP="85.16.0.4", Port="7042"},
                new SampleConection(){IP="167.16.0.4", Port="0492"},
                new SampleConection(){IP="42.16.0.4", Port="7042"},
                new SampleConection(){IP="5.16.0.4", Port="7704"},
                new SampleConection(){IP="10.16.0.4", Port="7042"},
            };
            FileServerList.ItemsSource = sampleConectionsList;
            ClientList.ItemsSource = sampleConectionsList;
        }

        private void StartServer(IPEndPoint localEP)
        {
            try
            {                
                Listener = new TcpListener(localEP);
                Listener.Start();

                while (true)
                {
                    TcpClient client = new TcpClient();
                    client.Client = Listener.AcceptSocket();
                    NetworkStream stream = client.GetStream();
                    string firstMessage = StreamTranslator.Read(stream);

                    if (firstMessage == "<isFileServer>")
                    {
                        FileServerHandler handler = new FileServerHandler(client);
                        handler.Start();
                        fileServersItem.Add(handler);                        
                    }
                    else if (firstMessage == "<isClient>")
                    {
                        ClientHandler handler = new ClientHandler(client);
                        handler.Start();
                        clientsItem.Add(handler);
                    }
                    else
                    {
                        MessageBox.Show("Unknown client trying to connect to this server. Connection abort.", "Unknown client trying to connect");
                    }

                }
            }
            catch (ThreadAbortException e)
            {
                MessageBox.Show("Your server had closed.", "Server close");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, e.ToString());
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            CloseButton_Click(this, null);
        }
    }

    class SampleConection
    {
        public string IP { get; set; }
        public string Port { get; set; }
    }

}
