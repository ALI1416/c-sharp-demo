using ConsoleDemo.BLL;
using ConsoleDemo.Test;
using System;

namespace ConsoleDemo
{

    public class Program
    {

        /// <summary>
        /// 启动类
        /// </summary>
        /// <param name="args">参数</param>
        public static void Main(string[] args)
        {
            //Log4NetTest.Test();
            //Qr.Start();
            //Qr2.Start();
            HttpServiceTest.Start();
            //SocketServer.Start();
            //SocketServer2.Start();
            //SocketHttpServer.Start();
            //WebSocketServer.Start();
            //WebSocketServer2.Start();
            //SocketWebSocketServer.Start();
            //SocketWebSocketServer2.Start();
            //SocketWebSocketServer3.Start();
            while (true)
            {
                Console.WriteLine("\n关闭程序:0\n" +
                    "http服务器\t-> 关闭:Q 启动:A\n" +
                    "socket服务器\t-> 关闭:W 启动:S\n" +
                    "socket2服务器\t-> 关闭:E 启动:D\n" +
                    "socket模拟http服务器\t-> 关闭:R 启动:F\n" +
                    "webSocket服务器\t-> 关闭:T 启动:G\n" +
                    "webSocket2服务器\t-> 关闭:Y 启动:H\n" +
                    "socket模拟webSocket服务器\t-> 关闭:U 启动:J\n" +
                    "socket模拟webSocket2服务器\t-> 关闭:I 启动:K\n" +
                    "socket模拟webSocket3服务器\t-> 关闭:O 启动:L\n");
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.D0:
                        {
                            return;
                        }
                    case ConsoleKey.Q:
                        {
                            HttpServiceTest.Close();
                            break;
                        }
                    case ConsoleKey.A:
                        {
                            HttpServiceTest.Start();
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
                    case ConsoleKey.R:
                        {
                            SocketHttpServer.Close();
                            break;
                        }
                    case ConsoleKey.F:
                        {
                            SocketHttpServer.Start();
                            break;
                        }
                    case ConsoleKey.T:
                        {
                            WebSocketServer.Close();
                            break;
                        }
                    case ConsoleKey.G:
                        {
                            WebSocketServer.Start();
                            break;
                        }
                    case ConsoleKey.Y:
                        {
                            WebSocketServer2.Close();
                            break;
                        }
                    case ConsoleKey.H:
                        {
                            WebSocketServer2.Start();
                            break;
                        }
                    case ConsoleKey.U:
                        {
                            SocketWebSocketServer.Close();
                            break;
                        }
                    case ConsoleKey.J:
                        {
                            SocketWebSocketServer.Start();
                            break;
                        }
                    case ConsoleKey.I:
                        {
                            SocketWebSocketServer2.Close();
                            break;
                        }
                    case ConsoleKey.K:
                        {
                            SocketWebSocketServer2.Start();
                            break;
                        }
                    case ConsoleKey.O:
                        {
                            SocketWebSocketServer3.Close();
                            break;
                        }
                    case ConsoleKey.L:
                        {
                            SocketWebSocketServer3.Start();
                            break;
                        }
                }
            }
        }
    }
}
