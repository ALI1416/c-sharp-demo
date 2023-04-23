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
            //SocketServiceTest.Start();
            SocketService2Test.Start();
            while (true)
            {
                Console.WriteLine("\n关闭程序:0\n" +
                    "http服务(使用HttpListener)\t-> 关闭:1 启动:2 认证启动:3\n" +
                    "http服务2(使用Socket)\t-> 关闭:4 启动:5\n" +
                    "socket服务(文本)\t-> 关闭:6 启动:7\n" +
                    "socket2服务(图片)\t-> 关闭:8 启动:9\n" +
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
                            HttpServiceTest.Start2();
                            break;
                        }
                    case ConsoleKey.D4:
                        {
                            HttpService2Test.Close();
                            break;
                        }
                    case ConsoleKey.D5:
                        {
                            HttpService2Test.Start();
                            break;
                        }
                    case ConsoleKey.D6:
                        {
                            SocketServiceTest.Close();
                            break;
                        }
                    case ConsoleKey.D7:
                        {
                            SocketServiceTest.Start();
                            break;
                        }
                    case ConsoleKey.D8:
                        {
                            SocketService2Test.Close();
                            break;
                        }
                    case ConsoleKey.D9:
                        {
                            SocketService2Test.Start();
                            break;
                        }
                    case ConsoleKey.W:
                        {
                            break;
                        }
                    case ConsoleKey.S:
                        {
                            break;
                        }
                    case ConsoleKey.E:
                        {
                            break;
                        }
                    case ConsoleKey.D:
                        {
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
