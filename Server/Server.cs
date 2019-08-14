using System;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public class Server
    {
        private readonly IPEndPoint _controlEndPoint;
        private readonly IPEndPoint _dataEndPoint;
        
        private readonly Socket _control;
        private readonly Socket _data;

        public Server()
        {
            var ip = IPAddress.Any;
            
            _controlEndPoint = new IPEndPoint(ip, 32100);
            _dataEndPoint = new IPEndPoint(ip, 32101);
            
            _control = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _data = new Socket(ip.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        }

        public void Start()
        {
            _control.Bind(_controlEndPoint);
            _data.Bind(_dataEndPoint);
            
            _control.Listen(1);
            _data.Listen(1);
        }

        public void Stop()
        {
        }
    }
}