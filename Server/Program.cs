using System;
using System.Diagnostics;
using System.Threading;

namespace Arvid.Server
{
    internal static class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            PruManager.Init();

            Thread.Sleep(100);
            
            Console.WriteLine("Initialized");

            var listener = new Listener();

            listener.Listen();
            Console.WriteLine("Listening...");

            while (listener.State != Listener.StateEnum.None)
            {
                listener.DoMessageLoop();
            }
            
            FrameStreamer.Stop();

            return 0;
        }
    }
}