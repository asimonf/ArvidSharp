using System.Net.Sockets;

namespace Arvid.Server
{
    internal class DataServer: BaseServer
    {
        public DataServer(Socket socket) : base(socket)
        {
        }

        protected override void DoWork()
        {
            throw new System.NotImplementedException();
        }
    }
}