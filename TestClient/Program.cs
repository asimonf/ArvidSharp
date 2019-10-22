using System;
using System.Diagnostics;
using System.Threading;
using Arvid.Client;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new Client("192.168.2.101");

            Console.WriteLine(client.Connect());
            
            Thread.Sleep(100);

            var timestamp = Stopwatch.GetTimestamp();

            while (true)
            {
                var width = client.WaitForVsync();
                var timeElapsed = (Stopwatch.GetTimestamp() - timestamp) / (double)Stopwatch.Frequency;
                timestamp = Stopwatch.GetTimestamp();
                Console.WriteLine(timeElapsed * 1000);                    
            }

            client.Disconnect();

            Thread.Sleep(10000);
        }
    }
}
