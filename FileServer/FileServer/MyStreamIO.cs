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

        public void Write(int number)
        {
            buffer = BitConverter.GetBytes(number);
            stream.Flush();
            stream.Write(buffer, 0, buffer.Length);
        }

        public void Write(long number)
        {
            buffer = BitConverter.GetBytes(number);
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

        public int ReadInt()
        {
            buffer = new byte[sizeof(int)];
            int size = stream.Read(buffer, 0, buffer.Length);
            int result = BitConverter.ToInt32(buffer, 0);
            return result;
        }

        public long ReadLong()
        {
            buffer = new byte[sizeof(long)];
            int size = stream.Read(buffer, 0, buffer.Length);
            long result = BitConverter.ToInt64(buffer, 0);
            return result;
        }

        public void SendNEXT()
        {
            Write("<NEXT>");
        }

        public void GetNEXT()
        {
            string message = ReadString();
            if (message == "<NEXT>")
                return;
            else
                throw new Exception("Error in communicate between main server and file server.\nMessage received: " + message);
        }
    }
}
