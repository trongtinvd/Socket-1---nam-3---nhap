using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public MainWindow()
        {
            InitializeComponent();
            UITest();
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
    }

    class SampleConection
    {
        public string IP { get; set; }
        public string Port { get; set; }
    }
    
}
