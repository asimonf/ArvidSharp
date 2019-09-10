using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace Arvid.Server
{
    public class Listener
    {
        public enum StateEnum
        {
            None = 0,
            WaitingForConnections = 1,
            Initializing = 2,
            Initialized = 3
        }

        private readonly Socket _controlListener;
        private readonly Socket _dataListener;

        private readonly ConcurrentQueue<ListenerMessage> _listenerMessages;
        
        private DataServer _dataServer;
        private ControlServer _controlServer;

        public StateEnum State { get; private set; }
        
        public Listener()
        {
            var ip = IPAddress.Any;
            
            var controlEndPoint = new IPEndPoint(ip, 32100);
            var dataEndPoint = new IPEndPoint(ip, 32101);
            
            _controlListener = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _dataListener = new Socket(ip.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            
            _controlListener.Bind(controlEndPoint);
            _dataListener.Bind(dataEndPoint);
            
            _controlListener.Listen(1);
            _dataListener.Listen(1);
            
            _listenerMessages = new ConcurrentQueue<ListenerMessage>();
        }

        private void _resetServers()
        {
            _controlServer?.Stop();
            _controlServer = null;
            _dataServer?.Stop();
            _dataServer = null;
        }

        private void _acceptControlConnection(IAsyncResult ar)
        {
            var listener = (Socket) ar.AsyncState;

            var handler = listener.EndAccept(ar);
            
            _controlServer = new ControlServer(handler, _listenerMessages);
            _controlServer.Start();
            
            State = State == StateEnum.WaitingForConnections ? StateEnum.Initializing : StateEnum.Initialized;
        }

        private void _acceptDataConnection(IAsyncResult ar)
        {
            var listener = (Socket) ar.AsyncState;

            var handler = listener.EndAccept(ar);
            
            _dataServer = new DataServer(handler);
            _dataServer.Start();
            
            State = State == StateEnum.WaitingForConnections ? StateEnum.Initializing : StateEnum.Initialized;
        }

        public void Listen()
        {
            if (State >= StateEnum.WaitingForConnections) return;

            _resetServers();

            _controlListener.BeginAccept(_acceptControlConnection, _controlListener);
            _dataListener.BeginAccept(_acceptDataConnection, _dataListener);
            
            State = StateEnum.WaitingForConnections;
        }

        public void DoMessageLoop()
        {
            while (_listenerMessages.TryDequeue(out var result))
            {
                switch (result)
                {
                    case ListenerMessage.Init:
                        break;
                    case ListenerMessage.Stop:
                        Disconnect();
                        break;
                    case ListenerMessage.None:
                        break;
                    case ListenerMessage.ShutDown:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }                
            }
        }
        
        public void Disconnect()
        {
            _resetServers();
            State = StateEnum.None;
        }
    }
}