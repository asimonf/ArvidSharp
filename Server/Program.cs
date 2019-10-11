using System;
using System.Threading;

namespace Arvid.Server
{
    internal static class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            PruManager.Init();
            var listener = new Listener();
            
            listener.Listen();

            while (listener.State != Listener.StateEnum.None)
            {
                listener.DoMessageLoop();
            }

            return 0;
        }
    }
}