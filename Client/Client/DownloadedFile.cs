using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class DownloadedFile
    {
        public string Name { get; set; }
        public string Status { get; set; }

        public DownloadedFile(string name, string status)
        {
            this.Name = name;
            this.Status = status;
        }
    }
}
