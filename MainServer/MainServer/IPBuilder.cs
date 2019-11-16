using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainServer
{
    class IPBuilder
    {
        public static IPEndPoint GetIP(string address, int port)
        {
            if (address == "localhost")
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
                IPEndPoint IP = new IPEndPoint(hostEntry.AddressList[0], port);
                return IP;
            }
            else
            {
                IPEndPoint IP = new IPEndPoint(IPAddress.Parse(address), port);
                return IP;
            }
        }

        public static IPEndPoint GetIP()
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress address = hostEntry.AddressList[0];
            IPEndPoint IP = new IPEndPoint(address, GetPort());
            return IP;
        }

        public static int GetPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }
    }
}
