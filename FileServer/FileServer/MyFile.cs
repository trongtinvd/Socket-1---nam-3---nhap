using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileServer
{
    class MyFile
    {
        public MyFile(string fileName)
        {
            this.FileName = fileName;
            this.FileSize = new FileInfo(fileName).Length;

        }

        public string FileName { get; internal set; }
        public long FileSize { get; internal set; }
    }
}
