﻿using System.Net;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Text;
using System.Threading;

namespace ConsoleDemo
{
    /// <summary>
    /// webSocket服务器
    /// </summary>
    internal class WebSocketServer
    {
        /// <summary>
        /// 正在运行
        /// </summary>
        private static bool isRunning = false;
        /// <summary>
        /// http服务器
        /// </summary>
        private static HttpListener httpServer;
        /// <summary>
        /// webSocket客户端
        /// </summary>
        private static readonly List<WebSocket> webSocketClient = new List<WebSocket>();
        /// <summary>
        /// webSocket客户端历史
        /// </summary>
        private static readonly Dictionary<int, SocketHistory> webSocketClientHistory = new Dictionary<int, SocketHistory>();
        /// <summary>
        /// 接收数据缓冲区
        /// </summary>
        private static readonly ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[0xFFFF]);

        /// <summary>
        /// 启动
        /// </summary>
        public static void Start()
        {
            // 新建http服务器
            httpServer = new HttpListener
            {
                // 忽视客户端写入异常
                IgnoreWriteExceptions = true
            };
            // 指定URI
            httpServer.Prefixes.Add("http://127.0.0.1:8084/");
            try
            {
                // 开启http服务器
                httpServer.Start();
            }
            // 端口号冲突
            catch
            {
                Console.WriteLine("端口号冲突");
                return;
            }
            // 异步监听客户端请求
            httpServer.BeginGetContext(HttpHandle, null);
            isRunning = true;
            Console.WriteLine("webSocket服务器已启动");
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
            foreach (WebSocket client in webSocketClient.ToArray())
            {
                ClientOffline(client);
            }
            httpServer.Close();
        }

        /// <summary>
        /// http处理
        /// </summary>
        /// <param name="ar">IAsyncResult</param>
        private static void HttpHandle(IAsyncResult ar)
        {
            try
            {
                // 继续异步监听客户端请求
                httpServer.BeginGetContext(HttpHandle, null);
            }
            // 主动关闭http服务器
            catch
            {
                Console.WriteLine("主动关闭webSocket服务器");
                return;
            }
            // 获取context对象，并处理webSocket
            new Task(() => WebSocketHandle(httpServer.EndGetContext(ar))).Start();
        }

        /// <summary>
        /// 客户端上线
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="ip">IP地址</param>
        private static void ClientOnline(WebSocket client, string ip)
        {
            // 已存在
            if (webSocketClient.Contains(client))
            {
                return;
            }
            webSocketClient.Add(client);
            webSocketClientHistory.Add(client.GetHashCode(), new SocketHistory(ip, DateTime.Now));
            Console.WriteLine("客户端 " + ip + " 已上线");
            SocketHistory.Iterate(webSocketClientHistory);
        }

        /// <summary>
        /// 客户端下线
        /// </summary>
        /// <param name="client">客户端</param>
        private static void ClientOffline(WebSocket client)
        {
            // 不存在
            if (!webSocketClient.Contains(client))
            {
                return;
            }
            webSocketClient.Remove(client);
            // 获取该客户端
            if (webSocketClientHistory.TryGetValue(client.GetHashCode(), out SocketHistory history))
            {
                // 没有下线的客户端才可以下线
                if (history.Offline == DateTime.MinValue)
                {
                    history.Offline = DateTime.Now;
                    Console.WriteLine("客户端 " + history.Ip + " 已下线");
                    SocketHistory.Iterate(webSocketClientHistory);
                }
            }
        }

        /// <summary>
        /// webSocket处理
        /// </summary>
        /// <param name="context">HttpListenerContext</param>
        private static async void WebSocketHandle(HttpListenerContext context)
        {
            var wsContext = await context.AcceptWebSocketAsync(null);
            using (var ws = wsContext.WebSocket)
            {
                // 客户端上线
                ClientOnline(ws, context.Request.RemoteEndPoint.ToString());
                // 接收消息
                WebSocketReceiveResult webSocketReceiveResult;
                while (true)
                {
                    try
                    {
                        // 接收消息
                        webSocketReceiveResult = await ws.ReceiveAsync(buffer, CancellationToken.None);
                    }
                    // 断开连接
                    catch
                    {
                        break;
                    }
                    // 解码消息
                    byte[] data = new byte[webSocketReceiveResult.Count];
                    Array.Copy(buffer.Array, data, webSocketReceiveResult.Count);
                    string msg = Encoding.UTF8.GetString(data);
                    Console.WriteLine("收到客户端 " + context.Request.RemoteEndPoint + " 消息：" + msg);
                }
                // 客户端下线
                ClientOffline(ws);
            }
        }

        /// <summary>
        /// 定时向客户端发送消息
        /// </summary>
        private static void IntervalSend()
        {
            while (isRunning)
            {
                var webSocketClientArray = webSocketClient.ToArray();
                if (webSocketClientArray.Length != 0)
                {
                    string s = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    foreach (WebSocket client in webSocketClientArray)
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
        /// <param name="client">webSocket客户端</param>
        /// <param name="s">字符串</param>
        private static void Send(WebSocket client, string s)
        {
            var data = new ArraySegment<byte>(Encoding.UTF8.GetBytes(s));
            try
            {
                // 发送消息
                client.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None).Wait();
                // 获取该客户端
                if (webSocketClientHistory.TryGetValue(client.GetHashCode(), out SocketHistory history))
                {
                    Console.WriteLine("向客户端 " + history.Ip + " 发送消息：" + s);
                }
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
