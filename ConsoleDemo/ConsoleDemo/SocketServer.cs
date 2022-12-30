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
        /// socket客户端历史
        /// </summary>
        private static readonly Dictionary<int, SocketHistory> socketClientHistory = new Dictionary<int, SocketHistory>();
        /// <summary>
        /// 接收数据缓冲区
        /// </summary>
        private static readonly byte[] buffer = new byte[1024];

        /// <summary>
        /// 启动
        /// </summary>
        public static void Start()
        {
            try
            {
                // 新建socket服务器
                socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // 指定URI
                socketServer.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8081));
                // 设置监听数量
                socketServer.Listen(10);
                // 异步监听客户端请求
                socketServer.BeginAccept(SocketHandle, null);
            }
            // 端口号冲突、未知错误
            catch
            {
                socketServer.Close();
                Console.WriteLine("socket服务器端口号冲突 或 未知错误");
                return;
            }
            isRunning = true;
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
                ClientOffline(client);
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
            // 主动关闭socket服务器
            catch
            {
                Console.WriteLine("主动关闭socket服务器");
                return;
            }
            // 客户端上线
            ClientOnline(socketServer.EndAccept(ar));
        }

        /// <summary>
        /// 客户端上线
        /// </summary>
        /// <param name="client">客户端</param>
        private static void ClientOnline(Socket client)
        {
            // 已存在
            if (socketClient.Contains(client))
            {
                return;
            }
            try
            {
                // 获取IP地址
                string ip = client.RemoteEndPoint.ToString();
                // 设置超时10秒
                client.SendTimeout = 10000;
                // 接收消息
                client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, Recevice, client);
                socketClient.Add(client);
                socketClientHistory.Add(client.GetHashCode(), new SocketHistory(ip, DateTime.Now));
                Console.WriteLine("客户端 " + ip + " 已上线");
                SocketHistory.Iterate(socketClientHistory);
            }
            catch
            {
                client.Close();
                return;
            }
        }

        /// <summary>
        /// 客户端下线
        /// </summary>
        /// <param name="client">客户端</param>
        private static void ClientOffline(Socket client)
        {
            // 不存在
            if (!socketClient.Contains(client))
            {
                return;
            }
            client.Close();
            socketClient.Remove(client);
            // 获取该客户端
            if (socketClientHistory.TryGetValue(client.GetHashCode(), out SocketHistory history))
            {
                // 没有下线的客户端才可以下线
                if (history.Offline == DateTime.MinValue)
                {
                    history.Offline = DateTime.Now;
                    Console.WriteLine("客户端 " + history.Ip + " 已下线");
                    SocketHistory.Iterate(socketClientHistory);
                }
            }
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="ar">IAsyncResult</param>
        private static void Recevice(IAsyncResult ar)
        {
            // 获取当前客户端
            Socket client = ar.AsyncState as Socket;
            try
            {
                // 获取接收数据长度
                int length = client.EndReceive(ar);
                // 客户端主动断开连接时，会发送0字节消息
                if (length == 0)
                {
                    ClientOffline(client);
                    return;
                }
                // 继续接收消息
                client.BeginReceive(buffer, 0, length, SocketFlags.None, Recevice, client);
                // 解码消息
                string msg = Encoding.UTF8.GetString(buffer, 0, length);
                Console.WriteLine("收到客户端 " + client.RemoteEndPoint + " 消息：" + msg);
            }
            // 超时后失去连接、未知错误
            catch
            {
                ClientOffline(client);
                return;
            }
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
            try
            {
                // 发送消息
                client.BeginSend(data, 0, data.Length, SocketFlags.None, asyncResult =>
                {
                    try
                    {
                        client.EndSend(asyncResult);
                        Console.WriteLine("向客户端 " + client.RemoteEndPoint + " 发送消息：" + s);
                    }
                    // 已失去连接
                    catch
                    {
                        ClientOffline(client);
                        return;
                    }
                }, null);
            }
            // 未知错误
            catch
            {
                ClientOffline(client);
                return;
            }
        }

    }
}
