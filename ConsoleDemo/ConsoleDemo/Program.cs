using System;

namespace ConsoleDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            HttpServer.Start();
            SocketServer.Start();
            Console.ReadKey();
        }
    }
}
