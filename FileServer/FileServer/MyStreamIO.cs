using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FileServer
{
    class MyStreamIO
    {
        private NetworkStream stream;
        private byte[] buffer;

        public MyStreamIO(NetworkStream stream)
        {
            this.stream = stream;
        }

        public void Write(string message)
        {
            buffer = Encoding.ASCII.GetBytes(message);
            stream.Flush();
            stream.Write(buffer, 0, buffer.Length);
        }

        public string ReadString()
        {
            buffer = new byte[1024];
            int size = stream.Read(buffer, 0, buffer.Length);
            string message = Encoding.ASCII.GetString(buffer);
            message = message.TrimEnd(new char[] { (char)0 });
            return message;
        }
    }
}
