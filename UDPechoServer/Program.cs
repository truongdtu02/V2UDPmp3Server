using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NetCoreServer;

namespace UDPechoServer
{
    class EchoServer : UdpServer
    {
        public EchoServer(IPAddress address, int port) : base(address, port) { }

        protected override void OnStarted()
        {
            // Start receive datagrams
            ReceiveAsync();
        }

        protected override void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
        {
            Console.WriteLine("Incoming: " + Encoding.UTF8.GetString(buffer, (int)offset, (int)size));

            // Echo the message back to the sender
            SendAsync(endpoint, buffer, 0, size);
        }

        protected override void OnSent(EndPoint endpoint, long sent)
        {
            // Continue receive datagrams
            ReceiveAsync();
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Echo UDP server caught an error with code {error}");
        }
    }

    public class testThread : IDisposable
    {
        Thread print;
        int _id;
        private bool bLoopThread = true;
        internal void terminateThread()
        {
            bLoopThread = false;
            print = null;
        }
        public testThread(int id)
        {
            _id = id;
            print = new Thread(() =>
            {
                while (bLoopThread)
                {
                    Console.WriteLine(id);
                    Thread.Sleep(1000);
                }
            });
            print.Start();
        }
        ~testThread()
        {
            bLoopThread = false;
            Console.WriteLine($"Destructor");
        }

        private bool isDisposed = false;
        public virtual void Dispose()
        {
            if (this.isDisposed)
                return;

            terminateThread();

            isDisposed = true;
        }
    }

    class Program
    {
        public static List<testThread> tt = new List<testThread>();
        public static void AddThread(int id)
        {
            var tmp = new testThread(id);
            tt.Add(tmp);
        }
        static void Main(string[] args)
        {
            DateTime unixTimestamp = DateTime.Now;
            Thread.Sleep(1500);
            double ts = (DateTime.Now - unixTimestamp).TotalSeconds;


            AddThread(1);
            AddThread(2);
            Thread.Sleep(4000);
            tt.RemoveAt(0);
            // UDP server port
            int port = 1308;
            if (args.Length > 0)
                port = int.Parse(args[0]);

            Console.WriteLine($"UDP server port: {port}");

            Console.WriteLine();

            // Create a new UDP echo server
            var server = new EchoServer(IPAddress.Any, port);

            // Start the server
            Console.Write("Server starting...");
            server.Start();
            Console.WriteLine("Done!");

            Console.WriteLine("Press Enter to stop the server or '!' to restart the server...");

            // Perform text input
            for (; ; )
            {
                string line = Console.ReadLine();
                if (string.IsNullOrEmpty(line))
                    break;

                // Restart the server
                if (line == "!")
                {
                    Console.Write("Server restarting...");
                    server.Restart();
                    Console.WriteLine("Done!");
                }
            }

            // Stop the server
            Console.Write("Server stopping...");
            server.Stop();
            Console.WriteLine("Done!");
        }
    }
}