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
            //QrCodeZXingTest.Test();
            //QrCodeTest.Test();

            HttpService1Test.Start();
            //HttpService2Test.Start();
            //SocketService1Test.Start();
            //SocketService2Test.Start();
            //WebSocketService1Test.Start();
            //WebSocketService2Test.Start();
            while (true)
            {
                Console.WriteLine("\n关闭程序:0\n" +
                    "http服务1(使用HttpListener)8080\t-> 关闭:1 启动:2 认证启动:3\n" +
                    "http服务2(使用Socket)8081\t-> 关闭:4 启动:5\n" +
                    "socket服务1(文本)8082\t-> 关闭:6 启动:7\n" +
                    "socket服务2(图片)8083\t-> 关闭:8 启动:9\n" +
                    "webSocket服务1(使用HttpListener,性能差,文本)8084\t-> 关闭:Q 启动:W\n" +
                    "webSocket服务2(使用HttpListener,性能差,图片)8085\t-> 关闭:E 启动:R\n" +
                    "webSocket服务3(使用Socket,文本)8086\t-> 关闭:T 启动:Y\n" +
                    "webSocket服务4(使用Socket,图片)8087\t-> 关闭:U 启动:I\n" +
                    "webSocket服务5(使用Socket,图片,确认)8088\t-> 关闭:O 启动:P\n");
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.D0:
                        {
                            return;
                        }
                    case ConsoleKey.D1:
                        {
                            HttpService1Test.Close();
                            break;
                        }
                    case ConsoleKey.D2:
                        {
                            HttpService1Test.Start();
                            break;
                        }
                    case ConsoleKey.D3:
                        {
                            HttpService1Test.Start2();
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
                            SocketService1Test.Close();
                            break;
                        }
                    case ConsoleKey.D7:
                        {
                            SocketService1Test.Start();
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
                    case ConsoleKey.Q:
                        {
                            WebSocketService1Test.Close();
                            break;
                        }
                    case ConsoleKey.W:
                        {
                            WebSocketService1Test.Start();
                            break;
                        }
                    case ConsoleKey.E:
                        {
                            WebSocketService2Test.Close();
                            break;
                        }
                    case ConsoleKey.R:
                        {
                            WebSocketService2Test.Start();
                            break;
                        }
                }
            }
        }
    }
}
