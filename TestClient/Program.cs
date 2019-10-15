using System;
using Arvid.Client;
using System.Threading;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new Client("192.168.7.2");

            Console.WriteLine(client.Connect());

            Thread.Sleep(10000);

            client.Disconnect();

            Thread.Sleep(10000);
        }
    }
}
