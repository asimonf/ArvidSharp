using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;

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
        private readonly Socket _dataConnection;

        private readonly AutoResetEvent _listenerResetEvent;
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
            _dataConnection = new Socket(ip.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            
            _controlListener.Bind(controlEndPoint);
            _dataConnection.Bind(dataEndPoint);
            
            _controlListener.Listen(100);
            
            _listenerResetEvent = new AutoResetEvent(false);
            _listenerMessages = new ConcurrentQueue<ListenerMessage>();
        }

        private void _resetServers()
        {
            _controlServer?.Stop();
            _controlServer = null;
            _dataServer?.Stop();
            _dataServer = null;
            GC.Collect();
        }

        private void _acceptControlConnection(IAsyncResult ar)
        {
            Console.WriteLine("Handshaking");
            
            var listener = (Socket) ar.AsyncState;

            var handler = listener.EndAccept(ar);
            
            _controlServer = new ControlServer(handler, this);
            _controlServer.Setup();
            _controlServer.Start();

            _dataServer = new DataServer(_dataConnection);
            _dataServer.Start();
            
            State = State == StateEnum.WaitingForConnections ? StateEnum.Initializing : StateEnum.Initialized;
        }

        public void Listen()
        {
            if (State >= StateEnum.WaitingForConnections) return;

            _resetServers();

            _controlListener.BeginAccept(_acceptControlConnection, _controlListener);
            
            State = StateEnum.WaitingForConnections;
        }

        public void EnqueueMessage(ListenerMessage message)
        {
            _listenerMessages.Enqueue(message);
            _listenerResetEvent.Set();
        }

        public void DoMessageLoop()
        {
            _listenerResetEvent.WaitOne(-1);
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
            State = StateEnum.None;
            Listen();
        }
    }
}