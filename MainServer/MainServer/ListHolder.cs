using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MainServer
{
    static class ListHolder
    {
        public static List<FileServerHandler> FileServers { get; set; }
        public static List<ClientHandler> Clients { get; set; }

        public static ListView FileServersList { get; set; }
        public static ListView ClientsList { get; set; }

        public static void UpdateList()
        {
            MyDispatcher.Dispatcher.Invoke(() =>
            {
                FileServersList.Items.Refresh();
                ClientsList.Items.Refresh();
            });
        } 
    }

}
