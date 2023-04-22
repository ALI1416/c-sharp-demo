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
            //HttpServiceTest.Start();
            //HttpService2Test.Start();
            SocketServiceTest.Start();
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
                    "http服务(使用HttpListener)\t-> 关闭:1 启动:2\n" +
                    "http服务2(使用Socket)\t-> 关闭:3 启动:4\n" +
                    "socket服务(文本)\t-> 关闭:5 启动:6\n" +
                    "socket2服务(图片)\t-> 关闭:7 启动:8\n" +
                    "webSocket服务(使用HttpListener,文本)\t-> 关闭:Q 启动:W\n" +
                    "webSocket2服务(使用HttpListener,图片)\t-> 关闭:E 启动:R\n" +
                    "webSocket3服务器(使用Socket,文本)\t-> 关闭:T 启动:Y\n" +
                    "webSocket4服务器(使用Socket,图片)\t-> 关闭:U 启动:I\n" +
                    "webSocket5服务器(使用Socket,图片,确认)\t-> 关闭:O 启动:P\n");
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.D0:
                        {
                            return;
                        }
                    case ConsoleKey.D1:
                        {
                            HttpServiceTest.Close();
                            break;
                        }
                    case ConsoleKey.D2:
                        {
                            HttpServiceTest.Start();
                            break;
                        }
                    case ConsoleKey.D3:
                        {
                            HttpService2Test.Close();
                            break;
                        }
                    case ConsoleKey.D4:
                        {
                            HttpService2Test.Start();
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
