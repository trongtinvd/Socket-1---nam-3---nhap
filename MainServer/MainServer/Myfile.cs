using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainServer
{
    class MyFile
    {
        public string Name { get; }
        public long Size { get; } 
        public string Address { get; }
        public int Port { get; }       

        public MyFile(string name, long size)
        {
            this.Name = name;
            this.Size = size;
        }

        public MyFile(string name, long size, string address, int port) : this(name, size)
        {
            this.Address = address;
            this.Port = port;
        }
    }
}
