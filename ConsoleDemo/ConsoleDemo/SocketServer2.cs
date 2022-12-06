using ConsoleDemo.Properties;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ConsoleDemo
{
    /// <summary>
    /// socket服务器
    /// </summary>
    internal class SocketServer2
    {
        /// <summary>
        /// 正在运行
        /// </summary>
        private static bool isRunning = false;
        /// <summary>
        /// socket服务器
        /// </summary>
        private static Socket socketServer;
        /// <summary>
        /// socket客户端
        /// </summary>
        private static readonly List<Socket> socketClient = new List<Socket>();
        /// <summary>
        /// 接收数据缓冲区
        /// </summary>
        static readonly byte[] buffer = new byte[1024];

        /********** 常量 **********/
        private readonly static byte[] socketResponseHeader = Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\nContent-Type: multipart/x-mixed-replace; boundary=--boundary\n");
        private readonly static byte[] socketResponseEnd = Encoding.ASCII.GetBytes("\n");

        /// <summary>
        /// 启动
        /// </summary>
        public static void Start()
        {
            // 新建线程
            new Thread(t =>
            {
                // 新建socket服务器
                socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    // 指定URI
                    socketServer.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8082));
                }
                catch
                {
                    // 端口号冲突
                    Console.WriteLine("端口号冲突");
                    return;
                }
                isRunning = true;
                // 设置监听数量
                socketServer.Listen(10);
                // 异步监听客户端请求
                socketServer.BeginAccept(SocketHandle, null);
                Console.WriteLine("socket2服务器已启动");
                // 定时向客户端发送消息
                IntervalSend();
                Console.WriteLine("socket2进程已退出");
            })
            {
                IsBackground = true
            }.Start();
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public static void Close()
        {
            isRunning = false;
            foreach (Socket client in socketClient.ToArray())
            {
                CloseClient(client);
            }
            socketServer.Close();
        }

        /// <summary>
        /// socket处理
        /// </summary>
        /// <param name="ar">IAsyncResult</param>
        private static void SocketHandle(IAsyncResult ar)
        {
            try
            {
                // 继续异步监听客户端请求
                socketServer.BeginAccept(SocketHandle, null);
            }
            catch
            {
                // 主动关闭socket服务器会触发异常
                Console.WriteLine("主动关闭socket服务器");
                return;
            }
            // 获取Socket对象
            Socket client = socketServer.EndAccept(ar);
            // 设置超时10秒
            client.SendTimeout = 10000;
            // 把当前客户端添加进列表
            socketClient.Add(client);
            Console.WriteLine("用户 " + client.RemoteEndPoint + " 已连接");
            // 接收消息
            client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, Recevice, client);
            // 发送响应头
            Send(client, socketResponseHeader);
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="ar">IAsyncResult</param>
        private static void Recevice(IAsyncResult ar)
        {
            // 获取当前客户端
            Socket client = ar.AsyncState as Socket;
            int length = 0;
            try
            {
                // 获取接收数据长度
                length = client.EndReceive(ar);
            }
            // 超时后失去连接，会抛出异常
            catch
            {
                try
                {
                    // 超时后失去连接，会抛出异常
                    Console.WriteLine("客户端 " + client.RemoteEndPoint + " 失去连接");
                }
                catch
                {
                    // 客户端已被移除
                    Console.WriteLine("客户端 ?? 失去连接");
                }
                CloseClient(client);
                return;
            }
            // 用户主动断开连接时，会发送0字节消息
            if (length == 0)
            {
                Console.WriteLine("用户 " + client.RemoteEndPoint + " 断开连接");
                CloseClient(client);
                return;
            }
            // 继续接收消息
            client.BeginReceive(buffer, 0, length, SocketFlags.None, Recevice, client);
        }

        /// <summary>
        /// 关闭客户端
        /// </summary>
        /// <param name="client">客户端</param>
        private static void CloseClient(Socket client)
        {
            client.Close();
            socketClient.Remove(client);
        }

        /// <summary>
        /// 定时向客户端发送消息
        /// </summary>
        private static void IntervalSend()
        {
            while (isRunning)
            {
                MemoryStream stream = new MemoryStream();
                Random random = new Random();
                int index = random.Next(6);
                switch (index)
                {
                    case 0:
                    default:
                        {
                            Resources.apple.Save(stream, ImageFormat.Png);
                            break;
                        }
                    case 1:
                        {
                            Resources.banana.Save(stream, ImageFormat.Png);
                            break;
                        }
                    case 2:
                        {
                            Resources.grape.Save(stream, ImageFormat.Png);
                            break;
                        }
                    case 3:
                        {
                            Resources.pear.Save(stream, ImageFormat.Png);
                            break;
                        }
                    case 4:
                        {
                            Resources.tangerine.Save(stream, ImageFormat.Png);
                            break;
                        }
                    case 5:
                        {
                            Resources.watermelon.Save(stream, ImageFormat.Png);
                            break;
                        }
                }
                string header = "\n--boundary\nContent-Type: image/png\nContent-Length: " + stream.Length + "\n\n";
                byte[] data = new byte[header.Length + stream.Length + 1];
                Encoding.ASCII.GetBytes(header).CopyTo(data, 0);
                stream.ToArray().CopyTo(data, header.Length);
                socketResponseEnd.CopyTo(data, data.Length - 1);
                foreach (var client in socketClient)
                {
                    Send(client, data);
                }
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="data">byte[]</param>
        private static void Send(Socket client, byte[] data)
        {
            // 发送消息
            client.BeginSend(data, 0, data.Length, SocketFlags.None, asyncResult =>
            {
                int length = 0;
                try
                {
                    length = client.EndSend(asyncResult);
                }
                catch
                {
                    // 已失去连接
                    CloseClient(client);
                    return;
                }
                Console.WriteLine("向用户 " + client.RemoteEndPoint + " 发送 " + length + " 字节的消息");
            }, null);
        }

    }
}
