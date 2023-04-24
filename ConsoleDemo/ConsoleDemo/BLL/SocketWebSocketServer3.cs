using ConsoleDemo.Util;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ConsoleDemo.Model;

namespace ConsoleDemo.BLL
{

    /// <summary>
    /// socket模拟webSocket服务器3
    /// </summary>
    public class SocketWebSocketServer3
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
        private static readonly List<WebSocketClient2> socketClientList = new List<WebSocketClient2>();

        private readonly static byte[] httpCloseHeader = Encoding.UTF8.GetBytes("HTTP/1.0 200 OK\nConnection: close\n\n");

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
                socketServer.Server.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8088));
                // 设置监听数量
                socketServer.Server.Listen(10);
                // 异步监听客户端请求
                socketServer.Server.BeginAccept(SocketHandle, null);
            }
            // 端口号冲突、未知错误
            catch
            {
                socketServer.Close();
                Console.WriteLine("socket模拟webSocket服务器3端口号冲突 或 未知错误");
                return;
            }
            isRunning = true;
            Console.WriteLine("socket模拟webSocket服务器3已启动");
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
            Utils.IterateSocketClient2(socketClientList);
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
                Console.WriteLine("主动关闭socket模拟webSocket服务器3");
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
            WebSocketClient2 socketClient = null;
            try
            {
                socketClient = new WebSocketClient2(client);
                // 设置超时10秒
                client.SendTimeout = 10000;
                // 接收消息
                client.BeginReceive(socketClient.Buffer, 0, socketClient.Buffer.Length, SocketFlags.None, Recevice, socketClient);
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
        /// <param name="socketClient">SocketClient2</param>
        private static void ClientOffline(WebSocketClient2 socketClient)
        {
            // 不存在
            if (!socketClientList.FindAll(e => e.Client != null).Contains(socketClient))
            {
                return;
            }
            socketClient.Close();
            Console.WriteLine("客户端 " + socketClient.Ip + " 已下线");
            Utils.IterateSocketClient2(socketClientList);
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="ar">IAsyncResult</param>
        private static void Recevice(IAsyncResult ar)
        {
            // 获取当前客户端
            WebSocketClient2 socketClient = ar.AsyncState as WebSocketClient2;
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
                // 首次连接
                if (socketClient.Buffer[0] == 71)
                {
                    // 解码消息
                    string msg = Encoding.UTF8.GetString(socketClient.Buffer, 0, length);
                    // 获取握手信息
                    byte[] data = WebSocketUtils.HandShake(msg);
                    if (data != null)
                    {
                        // 发送握手信息
                        SendRaw(socketClient, data);
                        // 继续接收消息
                        socketClient.Client.BeginReceive(socketClient.Buffer, 0, length, SocketFlags.None, Recevice, socketClient);
                        socketClientList.Add(socketClient);
                        Console.WriteLine("客户端 " + socketClient.Ip + " 已上线");
                        Utils.IterateSocketClient2(socketClientList);
                    }
                    // 无法握手
                    else
                    {
                        // 关闭连接
                        SendRaw(socketClient, httpCloseHeader);
                        socketClient.Close();
                        return;
                    }
                }
                else
                {
                    // 继续接收消息
                    socketClient.Client.BeginReceive(socketClient.Buffer, 0, length, SocketFlags.None, Recevice, socketClient);
                    // 解码消息
                    string data = WebSocketUtils.DecodeDataString(socketClient.Buffer, length);
                    // 客户端关闭连接
                    if (data == null)
                    {
                        ClientOffline(socketClient);
                        return;
                    }
                    else
                    {
                        socketClient.Transmission = false;
                    }
                }
            }
            // 超时后失去连接、未知错误
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
                var list = socketClientList.FindAll(e => (e.Client != null && !e.Transmission));
                if (list.Count != 0)
                {
                    var data = Utils.GetSendMemoryStream().ToArray();
                    foreach (var socketClient in list)
                    {
                        socketClient.Transmission = true;
                        Send(socketClient, data);
                    }
                }
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="socketClient">SocketClient2</param>
        /// <param name="data">byte[]</param>
        private static void Send(WebSocketClient2 socketClient, byte[] data)
        {
            SendRaw(socketClient, WebSocketUtils.CodedData(data, false));
            Console.WriteLine("向客户端 " + socketClient.Ip + " 发送 " + data.Length + " 字节的消息");
        }

        /// <summary>
        /// 发送原始消息
        /// </summary>
        /// <param name="socketClient">SocketClient2</param>
        /// <param name="data">byte[]</param>
        private static void SendRaw(WebSocketClient2 socketClient, byte[] data)
        {
            try
            {
                // 发送消息
                socketClient.Client.BeginSend(data, 0, data.Length, SocketFlags.None, asyncResult =>
                {
                    try
                    {
                        socketClient.Client.EndSend(asyncResult);
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
