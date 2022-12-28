using ConsoleDemo.Properties;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace ConsoleDemo
{
    /// <summary>
    /// socket模拟http服务器
    /// </summary>
    internal class SocketHttpServer
    {
        /// <summary>
        /// socket服务器
        /// </summary>
        private static Socket socketServer;
        /// <summary>
        /// 接收数据缓冲区
        /// </summary>
        static readonly byte[] buffer = new byte[1024];

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
                socketServer.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8083));
                // 设置监听数量
                socketServer.Listen(10);
                // 异步监听客户端请求
                socketServer.BeginAccept(SocketHandle, null);
            }
            // 端口号冲突、未知错误
            catch
            {
                socketServer.Close();
                Console.WriteLine("socket模拟http服务器端口号冲突 或 未知错误");
                return;
            }
            Console.WriteLine("socket模拟http服务器已启动");
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public static void Close()
        {
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
                Console.WriteLine("主动关闭socket模拟http服务器");
                return;
            }
            try
            {
                // 客户端上线
                ClientOnline(socketServer.EndAccept(ar));
            }
            // 未知错误
            catch
            {
            }
        }

        /// <summary>
        /// 客户端上线
        /// </summary>
        /// <param name="client">客户端</param>
        private static void ClientOnline(Socket client)
        {
            try
            {
                // 设置超时10秒
                client.SendTimeout = 10000;
                // 接收消息
                client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, Recevice, client);
            }
            catch
            {
                client.Close();
                return;
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
                    client.Close();
                    return;
                }
                // 解码消息
                string msg = Encoding.UTF8.GetString(buffer, 0, length);
                // request处理
                RequestHandle(client, msg);
                // 关闭连接
                client.Close();
            }
            // 超时后失去连接、未知错误
            catch
            {
                client.Close();
                return;
            }
        }

        /// <summary>
        /// request处理
        /// </summary>
        /// <param name="client">Socket客户端</param>
        /// <param name="request">request字符串</param>
        private static void RequestHandle(Socket client, string request)
        {
            string pathAndQuery = request.Substring(4, request.IndexOf('\r') - 13);
            byte[] data;
            switch (pathAndQuery)
            {
                default:
                    {
                        data = Encoding.UTF8.GetBytes("HTTP/1.0 200 OK\nContent-Type: text/html;charset=utf-8\nConnection: close\n\n" + Resources.socket);
                        break;
                    }
                case "/favicon.ico":
                    {
                        string header = "HTTP/1.0 200 OK\nContent-Type: image/x-icon;charset=utf-8\nConnection: close\n\n";
                        MemoryStream faviconStream = new MemoryStream();
                        Resources.favicon.Save(faviconStream);
                        data = new byte[header.Length + faviconStream.Length];
                        Encoding.ASCII.GetBytes(header).CopyTo(data, 0);
                        faviconStream.ToArray().CopyTo(data, header.Length);
                        break;
                    }
            }
            Send(client, data);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="client">Socket客户端</param>
        /// <param name="data">byte[]</param>
        private static void Send(Socket client, byte[] data)
        {
            try
            {
                // 发送消息
                client.BeginSend(data, 0, data.Length, SocketFlags.None, asyncResult =>
                {
                    try
                    {
                        client.EndSend(asyncResult);
                    }
                    // 已失去连接
                    catch
                    {
                        client.Close();
                        return;
                    }
                }, null);
            }
            // 未知错误
            catch
            {
                client.Close();
                return;
            }
        }

    }
}
