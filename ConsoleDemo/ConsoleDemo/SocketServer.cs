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
            // 新建线程
            new Thread(t =>
            {
                // 新建socket服务器
                socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // 指定URI
                socketServer.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8081));
                // 设置监听数量
                socketServer.Listen(10);
                // 异步监听客户端请求
                socketServer.BeginAccept(SocketHandle, null);
            })
            {
                IsBackground = true
            }.Start();
        }

        /// <summary>
        /// socket处理
        /// </summary>
        /// <param name="ar">IAsyncResult</param>
        private static void SocketHandle(IAsyncResult ar)
        {
            // 继续异步监听客户端请求
            socketServer.BeginAccept(SocketHandle, null);
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
            // 获取接收数据长度
            int length = client.EndReceive(ar);
            // 断开连接时，会发送0字节消息
            if (length == 0)
            {
                Console.WriteLine("客户端 " + client.RemoteEndPoint + " 已断开连接");
                // 关闭当前客户端连接
                client.Close();
                // 移除当前客户端
                socketClient.Remove(client);
                return;
            }
            // 解码消息
            string msg = Encoding.UTF8.GetString(buffer, 0, length);
            // 继续接收消息
            client.BeginReceive(buffer, 0, length, SocketFlags.None, Recevice, client);
            Console.WriteLine("收到客户端 " + client.RemoteEndPoint + " 消息：" + msg);
        }

        /// <summary>
        /// 定时向客户端发送消息
        /// </summary>
        private static void IntervalSend()
        {
            while (true)
            {
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="client">Socket客户端</param>
        /// <param name="s">字符串</param>
        private static void Send(Socket client, string s)
        {
            byte[] data = new byte[1024];
            data = Encoding.UTF8.GetBytes(s);
            // 发送消息
            client.BeginSend(data, 0, data.Length, SocketFlags.None, asyncResult =>
            {
                int length = client.EndSend(asyncResult);
                Console.WriteLine("向客户端 " + client.RemoteEndPoint + " 发送消息：" + s);
            }, null);
        }

    }
}
