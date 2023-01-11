using ConsoleDemo.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ConsoleDemo.Model;

namespace ConsoleDemo.BLL
{

    /// <summary>
    /// socket服务器2
    /// </summary>
    internal class SocketServer2
    {
        /// <summary>
        /// 正在运行
        /// </summary>
        private static bool isRunning = false;
        /// <summary>
        /// socket服务端
        /// </summary>
        private static Model.SocketServer socketServer;
        /// <summary>
        /// socket客户端
        /// </summary>
        private static readonly List<SocketClient> socketClientList = new List<SocketClient>();

        private readonly static byte[] socketResponseHeader = Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\nContent-Type: multipart/x-mixed-replace; boundary=--boundary\n\n");
        private readonly static byte[] socketResponseEnd = Encoding.ASCII.GetBytes("\n\n");

        /// <summary>
        /// 启动
        /// </summary>
        public static void Start()
        {
            try
            {
                // 新建socket服务器
                socketServer = new Model.SocketServer();
                // 指定URI
                socketServer.Server.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8082));
                // 设置监听数量
                socketServer.Server.Listen(10);
                // 异步监听客户端请求
                socketServer.Server.BeginAccept(SocketHandle, null);
            }
            // 端口号冲突、未知错误
            catch
            {
                socketServer.Close();
                Console.WriteLine("socket服务器2端口号冲突 或 未知错误");
                return;
            }
            isRunning = true;
            Console.WriteLine("socket服务器2已启动");
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
            foreach (var socketClient in socketClientList.FindAll(e => e.Client != null))
            {
                ClientOffline(socketClient);
            }
            socketServer.Close();
            Utils.IterateSocketClient(socketClientList);
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
                socketServer.Server.BeginAccept(SocketHandle, null);
            }
            // 主动关闭socket服务器
            catch
            {
                Console.WriteLine("主动关闭socket服务器2");
                return;
            }
            // 客户端上线
            ClientOnline(socketServer.Server.EndAccept(ar));
        }

        /// <summary>
        /// 客户端上线
        /// </summary>
        /// <param name="client">客户端</param>
        private static void ClientOnline(Socket client)
        {
            // 已存在
            if (socketClientList.Exists(e => e.Client == client))
            {
                return;
            }
            SocketClient socketClient = null;
            try
            {
                socketClient = new SocketClient(client);
                // 设置超时10秒
                client.SendTimeout = 10000;
                // 接收消息
                client.BeginReceive(socketClient.Buffer, 0, socketClient.Buffer.Length, SocketFlags.None, Recevice, socketClient);
                // 发送响应头
                Send(socketClient, socketResponseHeader);
                socketClientList.Add(socketClient);
                Console.WriteLine("客户端 " + socketClient.Ip + " 已上线");
                Utils.IterateSocketClient(socketClientList);
            }
            catch
            {
                if (socketClient != null)
                {
                    socketClient.Close();
                }
                return;
            }
        }

        /// <summary>
        /// 客户端下线
        /// </summary>
        /// <param name="socketClient">SocketClient</param>
        private static void ClientOffline(SocketClient socketClient)
        {
            // 不存在
            if (!socketClientList.FindAll(e => e.Client != null).Contains(socketClient))
            {
                return;
            }
            socketClient.Close();
            Console.WriteLine("客户端 " + socketClient.Ip + " 已下线");
            Utils.IterateSocketClient(socketClientList);
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="ar">IAsyncResult</param>
        private static void Recevice(IAsyncResult ar)
        {
            // 获取当前客户端
            SocketClient socketClient = ar.AsyncState as SocketClient;
            try
            {
                // 获取接收数据长度
                int length = socketClient.Client.EndReceive(ar);
                // 客户端主动断开连接时，会发送0字节消息
                if (length == 0)
                {
                    ClientOffline(socketClient);
                    return;
                }
                // 继续接收消息
                socketClient.Client.BeginReceive(socketClient.Buffer, 0, length, SocketFlags.None, Recevice, socketClient);
            }
            // 超时后失去连接，会抛出异常
            catch
            {
                ClientOffline(socketClient);
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
                var list = socketClientList.FindAll(e => e.Client != null);
                if (list.Count != 0)
                {
                    MemoryStream stream = Utils.GetSendMemoryStream();
                    string header = "--boundary\nContent-Type: image/png\nContent-Length: " + stream.Length + "\n\n";
                    byte[] data = new byte[header.Length + stream.Length + 2];
                    Encoding.ASCII.GetBytes(header).CopyTo(data, 0);
                    stream.ToArray().CopyTo(data, header.Length);
                    socketResponseEnd.CopyTo(data, data.Length - 2);
                    foreach (var socketClient in list)
                    {
                        Send(socketClient, data);
                    }
                }
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="socketClient">SocketClient</param>
        /// <param name="data">byte[]</param>
        private static void Send(SocketClient socketClient, byte[] data)
        {
            try
            {
                // 发送消息
                socketClient.Client.BeginSend(data, 0, data.Length, SocketFlags.None, asyncResult =>
                {
                    try
                    {
                        int length = socketClient.Client.EndSend(asyncResult);
                        Console.WriteLine("向客户端 " + socketClient.Ip + " 发送 " + length + " 字节的消息");
                    }
                    // 已失去连接
                    catch
                    {
                        ClientOffline(socketClient);
                        return;
                    }
                }, null);
            }
            // 未知错误
            catch
            {
                ClientOffline(socketClient);
                return;
            }
        }

    }
}
