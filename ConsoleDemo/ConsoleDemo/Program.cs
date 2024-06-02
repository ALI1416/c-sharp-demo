using ConsoleDemo.Test;
using System;

namespace ConsoleDemo
{

    /// <summary>
    /// 程序入口
    /// </summary>
    public class Program
    {

        /// <summary>
        /// 启动类
        /// </summary>
        /// <param name="args">参数</param>
        public static void Main(string[] args)
        {

            // 把dll文件打包进exe文件，需要安装`Costura.Fody`包
            // 修改`.csproj`文件，找到`Costura.Fody`，在`IncludeAssets`里加上`compile;`
            // 运行，会生成`FodyWeavers.xml`文件，内容如下
            // <Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
            //   < Costura />
            // </Weavers>

            //Log4NetTest.Test();
            //QrCodeDataGenerationTest.Test();
            //QrCodeZXingTest.Test();
            //QrCodeTest.Test();
            //JsonTest.Test();
            //ExcelReaderTest.Test();
            //HttpService1Test.Start();
            //HttpService1Test.Start2();
            //HttpService2Test.Start();
            //SocketServiceTest.Start();
            //SocketServiceTest.Start2();
            //WebSocketService1Test.Start();
            //WebSocketService1Test.Start2();
            //WebSocketService2Test.Start();
            //WebSocketService2Test.Start2();
            //WebSocketService3Test.Start();
            //SerialPortServiceTest.Start();
            while (true)
            {
                Console.WriteLine("\n关闭程序:0 重放:9\n" +
                    "http服务1(使用HttpListener)8080\t-> 关闭:1 启动:2 授权启动:3\n" +
                    "http服务2(使用Socket)8081\t-> 关闭:4 启动:5\n" +
                    "socket服务8082\t-> 关闭:6 文本启动:7 图片启动:8\n" +
                    "webSocket服务1(使用HttpListener,性能差)8083\t-> 关闭:Q 文本启动:W 图片启动:E\n" +
                    "webSocket服务2(使用Socket)8084\t-> 关闭:R 文本启动:T 图片启动:Y\n" +
                    "webSocket服务3(使用Socket)8085\t-> 关闭:U 启动:I\n"+
                    "串口服务COM3\t-> 关闭:O 启动:P\n"
                    );
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
                            SocketServiceTest.Start2();
                            break;
                        }
                    case ConsoleKey.D9:
                        {
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
                            WebSocketService1Test.Start2();
                            break;
                        }
                    case ConsoleKey.R:
                        {
                            WebSocketService2Test.Close();
                            break;
                        }
                    case ConsoleKey.T:
                        {
                            WebSocketService2Test.Start();
                            break;
                        }
                    case ConsoleKey.Y:
                        {
                            WebSocketService2Test.Start2();
                            break;
                        }
                    case ConsoleKey.U:
                        {
                            WebSocketService3Test.Close();
                            break;
                        }
                    case ConsoleKey.I:
                        {
                            WebSocketService3Test.Start();
                            break;
                        }
                    case ConsoleKey.O:
                        {
                            SerialPortServiceTest.Close();
                            break;
                        }
                    case ConsoleKey.P:
                        {
                            SerialPortServiceTest.Start();
                            break;
                        }
                }
            }
        }

    }
}
