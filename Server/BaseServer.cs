using System.Net.Sockets;
using System.Threading;

namespace Arvid.Server
{
    internal abstract class BaseServer
    {
        private readonly Thread _thread;
        protected readonly Socket Socket;

        public bool Receiving => _thread.IsAlive;

        protected BaseServer(Socket socket)
        {
            Socket = socket;
            _thread = new Thread(DoWork);
        }

        protected abstract void DoWork();

        public void Start()
        {
            _thread.Start();
        }

        public void Stop()
        {
            if (!_thread.IsAlive) return;
            
            _thread.Interrupt();
            _thread.Join();
        }
    }
}