using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using Mono.Unix.Native;

namespace Arvid.Server
{
    public class Listener
    {
        private readonly IPEndPoint _controlEndPoint;
        private readonly IPEndPoint _dataEndPoint;
        
        private readonly Socket _controlListener;
        private readonly Socket _dataListener;

        private readonly ConcurrentQueue<ListenerMessage> _listenerMessages;

        private DataServer _dataServer;
        private ControlServer _controlServer;

        private PruManager _pruManager;
        
        public bool HasClient { get; private set; }
        
        public bool Initialized { get; private set; }

        public Listener(PruManager pruManager)
        {
            var ip = IPAddress.Any;
            
            _controlEndPoint = new IPEndPoint(ip, 32100);
            _dataEndPoint = new IPEndPoint(ip, 32101);
            
            _controlListener = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _dataListener = new Socket(ip.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            
            _controlListener.Bind(_controlEndPoint);
            _dataListener.Bind(_dataEndPoint);
            
            _controlListener.Listen(1);
            _dataListener.Listen(1);
            
            _pruManager = new PruManager();
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
        }

        private void _acceptDataConnection(IAsyncResult ar)
        {
            var listener = (Socket) ar.AsyncState;

            var handler = listener.EndAccept(ar);
            
            _dataServer = new DataServer(handler);
        }

        public bool Listen()
        {
            if (HasClient) return false;
            
            var waitHandles = new WaitHandle[2];

            _resetServers();

            waitHandles[0] = _controlListener.BeginAccept(_acceptControlConnection, _controlListener).AsyncWaitHandle;
            waitHandles[1] = _dataListener.BeginAccept(_acceptDataConnection, _dataListener).AsyncWaitHandle;

            WaitHandle.WaitAll(waitHandles);

            if (_controlServer == null || _dataServer == null)
                _resetServers();            

            return HasClient = _controlServer != null && _dataServer != null;
        }

        public void DoMessageLoop()
        {
            bool hasMessages;
            
            while (hasMessages =_listenerMessages.TryDequeue(out var result))
            {
                switch (result)
                {
                    case ListenerMessage.Init:
                        
                        _dataServer.Start();
                        break;
                    case ListenerMessage.Stop:
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
        }

        public void Init()
        {
            if (Initialized) return;
            
            var euid = Syscall.geteuid();

            if (euid != 0)
            {
                throw new Exception("arvid: error ! permission check failed. Superuser required.\n");
            }
            
            _pruManager.Init();

            Initialized = true;
        }
        
        
    }
}