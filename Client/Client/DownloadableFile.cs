using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Client
{
    class DownloadableFile
    {

        public string FileName { get; }
        public long FileSize { get; }
        public string IP { get; }
        public int Port { get; }

        public string ShortenFileName
        {
            get
            {
                string result = "";
                for(int i= FileName.Length - 1; i >= 0; i--)
                {
                    if (FileName[i] == '/' || FileName[i] == '\\')
                        break;


                    result = result.Insert(0, FileName[i].ToString());
                }
                result.Reverse();
                return result;
            }
        }

        public DownloadableFile(string fileName, long fileSize, string ip, int port)
        {
            this.FileName = fileName;
            this.FileSize = fileSize;
            this.IP = ip;
            this.Port = port;
        }
    }
}
