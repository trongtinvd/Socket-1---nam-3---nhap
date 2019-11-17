using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FileServer
{
    static class ListHolder
    {
        public static List<MyFile> Files { get; set; }
        public static List<ClientHandler> Clients { get; set; }

        public static ListView FilesList { get; set; }
        public static ListView ClientsList { get; set; }

        public static void UpdateList()
        {
            MyDispatcher.Dispatcher.Invoke(() =>
            {
                FilesList.Items.Refresh();
                ClientsList.Items.Refresh();
            });
        }
    }
}
