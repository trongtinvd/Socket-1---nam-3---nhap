using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Client
{
    static class ListHolder
    {
        public static List<DownloadableFile> DownloadableFiles { get; set; }
        public static List<DownloadedFile> DownloadedFiles { get; set; }

        public static ListView DownloadableFilesList { get; set; }
        public static ListView DownloadedFilesList { get; set; }

        public static void UpdateList()
        {
            MyDispatcher.Dispatcher.Invoke(() =>
            {
                DownloadableFilesList.Items.Refresh();
                DownloadedFilesList.Items.Refresh();
            });
        }
    }
}
