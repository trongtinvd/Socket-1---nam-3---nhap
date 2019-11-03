using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class DownloadableFile
    {
        private readonly string _ip;
        private readonly int _port;

        public string FileName { get; }
        public long FileSize { get; }

        public DownloadableFile(string fileName, long fileSize, string ip, int port)
        {
            this.FileName = fileName;
            this.FileSize = fileSize;
            this._ip = ip;
            this._port = port;
        }
    }
}
