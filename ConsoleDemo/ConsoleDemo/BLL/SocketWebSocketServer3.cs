﻿using ConsoleDemo.Util;
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
    internal class SocketWebSocketServer3
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
        private static readonly List<SocketClient2> socketClientList = new List<SocketClient2>();

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
                socketServer.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8088));
                // 设置监听数量
                socketServer.Listen(10);
                // 异步监听客户端请求
                socketServer.BeginAccept(SocketHandle, null);
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
            foreach (var socketClient in socketClientList.ToArray())
            {
                ClientOffline(socketClient);
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
                Console.WriteLine("主动关闭socket模拟webSocket服务器3");
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
            if (socketClientList.Exists(e => e.Client == client))
            {
                return;
            }
            SocketClient2 socketClient = null;
            try
            {
                socketClient = new SocketClient2(client);
                // 设置超时10秒
                client.SendTimeout = 10000;
                // 接收消息
                client.BeginReceive(socketClient.Buffer, 0, socketClient.Buffer.Length, SocketFlags.None, Recevice, socketClient);
                socketClientList.Add(socketClient);
                Utils.IterateSocketClient2(socketClientList);
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
        private static void ClientOffline(SocketClient2 socketClient)
        {
            // 不存在
            if (!socketClientList.FindAll(e => e.Client != null).Contains(socketClient))
            {
                return;
            }
            socketClient.Close();
            Utils.IterateSocketClient2(socketClientList);
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="ar">IAsyncResult</param>
        private static void Recevice(IAsyncResult ar)
        {
            // 获取当前客户端
            SocketClient2 socketClient = ar.AsyncState as SocketClient2;
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
                    // 判断是否符合webSocket报文格式
                    if (msg.Contains("Sec-WebSocket-Key"))
                    {
                        // 握手
                        SendRaw(socketClient, WebSocketUtils.HandShake(msg));
                        // 继续接收消息
                        socketClient.Client.BeginReceive(socketClient.Buffer, 0, length, SocketFlags.None, Recevice, socketClient);
                    }
                    // 不符合格式，关闭连接
                    else
                    {
                        ClientOffline(socketClient);
                        return;
                    }
                }
                else
                {
                    // 继续接收消息
                    socketClient.Client.BeginReceive(socketClient.Buffer, 0, length, SocketFlags.None, Recevice, socketClient);
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
        private static void Send(SocketClient2 socketClient, byte[] data)
        {
            SendRaw(socketClient, WebSocketUtils.CodedData(data, false));
            Console.WriteLine("向客户端 " + socketClient.Ip + " 发送 " + data.Length + " 字节的消息");
        }

        /// <summary>
        /// 发送原始消息
        /// </summary>
        /// <param name="socketClient">SocketClient2</param>
        /// <param name="data">byte[]</param>
        private static void SendRaw(SocketClient2 socketClient, byte[] data)
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
