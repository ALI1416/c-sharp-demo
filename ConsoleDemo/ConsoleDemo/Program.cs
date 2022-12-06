using System;

namespace ConsoleDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            HttpServer.Start();
            SocketServer.Start();
            SocketServer2.Start();
            while (true)
            {
                Console.WriteLine("\n关闭程序:0\nhttp服务器\t-> 关闭:Q 启动:A\nsocket服务器\t-> 关闭:W 启动:S\nsocket2服务器\t-> 关闭:E 启动:D\n");
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.D0:
                        {
                            return;
                        }
                    case ConsoleKey.Q:
                        {
                            HttpServer.Close();
                            break;
                        }
                    case ConsoleKey.A:
                        {
                            HttpServer.Start();
                            break;
                        }
                    case ConsoleKey.W:
                        {
                            SocketServer.Close();
                            break;
                        }
                    case ConsoleKey.S:
                        {
                            SocketServer.Start();
                            break;
                        }
                    case ConsoleKey.E:
                        {
                            SocketServer2.Close();
                            break;
                        }
                    case ConsoleKey.D:
                        {
                            SocketServer2.Start();
                            break;
                        }
                }
            }
        }
    }
}
