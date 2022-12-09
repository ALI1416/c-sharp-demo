using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ConsoleDemo
{
    /// <summary>
    /// socket服务器
    /// </summary>
    internal class SocketServer
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

        /// <summary>
        /// 启动
        /// </summary>
        public static void Start()
        {
            // 新建socket服务器
            socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                // 指定URI
                socketServer.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8081));
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
            Console.WriteLine("socket服务器已启动");
            new Thread(t =>
            {
                // 定时向客户端发送消息
                IntervalSend();
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
                // 主动关闭socket服务器
                Console.WriteLine("主动关闭socket服务器");
                return;
            }
            // 获取Socket对象
            Socket client = socketServer.EndAccept(ar);
            // 设置超时10秒
            client.SendTimeout = 10000;
            // 把当前客户端添加进列表
            socketClient.Add(client);
            Console.WriteLine("客户端 " + client.RemoteEndPoint + " 已连接");
            // 接收消息
            client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, Recevice, client);
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
                Console.WriteLine("客户端 " + client.RemoteEndPoint + " 断开连接");
                CloseClient(client);
                return;
            }
            // 解码消息
            string msg = Encoding.UTF8.GetString(buffer, 0, length);
            // 继续接收消息
            client.BeginReceive(buffer, 0, length, SocketFlags.None, Recevice, client);
            Console.WriteLine("收到客户端 " + client.RemoteEndPoint + " 消息：" + msg);
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
                var socketClientArray = socketClient.ToArray();
                if (socketClientArray.Length != 0)
                {
                    string s = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    foreach (Socket client in socketClientArray)
                    {
                        Send(client, s);
                    }
                }
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="client">Socket客户端</param>
        /// <param name="s">字符串</param>
        private static void Send(Socket client, string s)
        {
            byte[] data = Encoding.UTF8.GetBytes(s);
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
                Console.WriteLine("向客户端 " + client.RemoteEndPoint + " 发送消息：" + s);
            }, null);
        }

    }
}
