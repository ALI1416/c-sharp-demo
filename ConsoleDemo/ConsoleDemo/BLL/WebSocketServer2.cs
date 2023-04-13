using System.Net;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Text;
using System.Threading;
using ConsoleDemo.Util;
using ConsoleDemo.Model;

namespace ConsoleDemo.BLL
{

    /// <summary>
    /// webSocket2服务器
    /// </summary>
    public class WebSocketServer2
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
        private static readonly List<WebSocketClient> webSocketClientList = new List<WebSocketClient>();

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
            httpServer.Prefixes.Add("http://127.0.0.1:8085/");
            try
            {
                // 开启http服务器
                httpServer.Start();
            }
            // 端口号冲突
            catch
            {
                Console.WriteLine("webSocket服务器2端口号冲突");
                return;
            }
            // 异步监听客户端请求
            httpServer.BeginGetContext(HttpHandle, null);
            isRunning = true;
            Console.WriteLine("webSocket服务器2已启动");
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
            foreach (var webSocketClient in webSocketClientList.FindAll(e => e.Client != null))
            {
                ClientOffline(webSocketClient);
            }
            httpServer.Close();
            Utils.IterateWebSocketClient(webSocketClientList);
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
                Console.WriteLine("主动关闭webSocket服务器2");
                return;
            }
            // 获取context对象，并处理webSocket
            new Task(() => WebSocketHandle(httpServer.EndGetContext(ar))).Start();
        }

        /// <summary>
        /// webSocket处理
        /// </summary>
        /// <param name="context">HttpListenerContext</param>
        private static async void WebSocketHandle(HttpListenerContext context)
        {
            HttpListenerWebSocketContext wsContext;
            try
            {
                wsContext = await context.AcceptWebSocketAsync(null);
            }
            // 不是webSocket连接
            catch
            {
                context.Response.Close();
                return;
            }
            using (var ws = wsContext.WebSocket)
            {
                /* 客户端上线 */
                // 已存在
                if (webSocketClientList.Exists(e => e.Client == ws))
                {
                    return;
                }
                WebSocketClient webSocketClient = new WebSocketClient(ws, context.Request.RemoteEndPoint.ToString());
                webSocketClientList.Add(webSocketClient);
                Console.WriteLine("客户端 " + webSocketClient.Ip + " 已上线");
                Utils.IterateWebSocketClient(webSocketClientList);
                /* 接收消息 */
                WebSocketReceiveResult webSocketReceiveResult;
                while (true)
                {
                    try
                    {
                        // 接收消息
                        webSocketReceiveResult = await ws.ReceiveAsync(webSocketClient.Buffer, CancellationToken.None);
                    }
                    // 断开连接
                    catch
                    {
                        break;
                    }
                    // 解码消息
                    byte[] data = new byte[webSocketReceiveResult.Count];
                    Array.Copy(webSocketClient.Buffer.Array, data, webSocketReceiveResult.Count);
                    string msg = Encoding.UTF8.GetString(data);
                    Console.WriteLine("收到客户端 " + webSocketClient.Ip + " 消息：" + msg);
                }
                /* 客户端下线 */
                ClientOffline(webSocketClient);
            }
        }

        /// <summary>
        /// 客户端下线
        /// </summary>
        /// <param name="webSocketClient">WebSocketClient</param>
        private static void ClientOffline(WebSocketClient webSocketClient)
        {
            // 不存在
            if (!webSocketClientList.FindAll(e => e.Client != null).Contains(webSocketClient))
            {
                return;
            }
            webSocketClient.Close();
            Console.WriteLine("客户端 " + webSocketClient.Ip + " 已下线");
            Utils.IterateWebSocketClient(webSocketClientList);
        }

        /// <summary>
        /// 定时向客户端发送消息
        /// </summary>
        private static void IntervalSend()
        {
            while (isRunning)
            {
                var list = webSocketClientList.FindAll(e => e.Client != null);
                if (list.Count != 0)
                {
                    var data = Utils.GetSendMemoryStream().ToArray();
                    foreach (var webSocketClient in list)
                    {
                        Send(webSocketClient, data);
                    }
                }
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="webSocketClient">WebSocketClient</param>
        /// <param name="data">byte[]</param>
        private static void Send(WebSocketClient webSocketClient, byte[] data)
        {
            try
            {
                // 发送消息
                webSocketClient.Client.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true, CancellationToken.None).Wait();
                Console.WriteLine("向客户端 " + webSocketClient.Ip + " 发送 " + data.Length + " 字节的消息");
            }
            // 未知错误
            catch
            {
                ClientOffline(webSocketClient);
                return;
            }
        }

    }
}
