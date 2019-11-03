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

        public MyFile(string name, long size)
        {
            this.Name = name;
            this.Size = size;
        }

    }
}
