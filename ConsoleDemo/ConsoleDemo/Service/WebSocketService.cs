using log4net;
using System.Net;
using System;
using System.Text;
using ConsoleDemo.Model;
using System.Threading.Tasks;
using ConsoleDemo.Util;
using System.Net.WebSockets;
using System.Threading;
using System.Collections.Generic;

namespace ConsoleDemo.Service
{
    /// <summary>
    /// webSocket服务(文字)
    /// </summary>
    public class WebSocketService
    {

        /// <summary>
        /// 日志实例
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(typeof(HttpService));

        /// <summary>
        /// 服务器
        /// </summary>
        private HttpListener server;
        /// <summary>
        /// 客户端列表
        /// </summary>
        private List<WebSocketClient> clientList;
        /// <summary>
        /// 服务器关闭回调函数
        /// </summary>
        private Action serviceCloseCallback;

        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口号</param>
        /// <param name="serviceCloseCallback">服务器关闭回调函数</param>
        /// <returns>是否启动成功</returns>
        public bool Start(IPAddress ip, int port, Action serviceCloseCallback)
        {
            // 新建服务器
            server = new HttpListener
            {
                // 忽视客户端写入异常
                IgnoreWriteExceptions = true
            };
            // 指定IP地址和端口号
            server.Prefixes.Add("http://" + ip + ":" + port + "/");
            try
            {
                // 启动服务器
                server.Start();
            }
            // 端口号冲突
            catch
            {
                log.Error("webSocket服务器端口号冲突");
                return false;
            }
            this.serviceCloseCallback = serviceCloseCallback;
            // 异步监听客户端请求
            server.BeginGetContext(Handle, null);
            if (clientList == null)
            {
                clientList = new List<WebSocketClient>();
            }
            log.Info("webSocket服务器已启动");
            return true;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            server.Close();
        }

        /// <summary>
        /// 处理
        /// </summary>
        /// <param name="ar">IAsyncResult</param>
        private void Handle(IAsyncResult ar)
        {
            try
            {
                // 继续异步监听客户端请求
                server.BeginGetContext(Handle, null);
            }
            // 主动关闭服务器
            catch
            {
                log.Info("主动关闭webSocket服务器");
                return;
            }
            // 获取context对象，并处理webSocket
            new Task(() => WebSocketHandle(server.EndGetContext(ar))).Start();
        }

        /// <summary>
        /// webSocket处理
        /// </summary>
        /// <param name="context">HttpListenerContext</param>
        private async void WebSocketHandle(HttpListenerContext context)
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
            using (WebSocket ws = wsContext.WebSocket)
            {
                /* 客户端上线 */
                // 已存在
                if (clientList.Exists(e => e.Client == ws))
                {
                    return;
                }
                WebSocketClient client = new WebSocketClient(ws, context.Request.RemoteEndPoint.ToString());
                clientList.Add(client);
                Console.WriteLine("客户端 " + client.Ip + " 已上线");
                Utils.IterateWebSocketClient(clientList);
                /* 接收消息 */
                WebSocketReceiveResult wsResult;
                while (true)
                {
                    try
                    {
                        // 接收消息
                        wsResult = await ws.ReceiveAsync(client.Buffer, CancellationToken.None);
                    }
                    // 断开连接
                    catch
                    {
                        break;
                    }
                    // 解码消息
                    byte[] data = new byte[wsResult.Count];
                    Array.Copy(client.Buffer.Array, data, wsResult.Count);
                    string msg = Encoding.UTF8.GetString(data);
                    Console.WriteLine("收到客户端 " + client.Ip + " 消息：" + msg);
                }
                /* 客户端下线 */
                ClientOffline(client);
            }
        }

        /// <summary>
        /// 客户端下线
        /// </summary>
        /// <param name="client">WebSocketClient</param>
        private void ClientOffline(WebSocketClient client)
        {
            // 不存在
            if (!clientList.FindAll(e => e.Client != null).Contains(client))
            {
                return;
            }
            client.Close();
            Console.WriteLine("客户端 " + client.Ip + " 已下线");
            Utils.IterateWebSocketClient(clientList);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="client">WebSocketClient</param>
        /// <param name="s">字符串</param>
        private void Send(WebSocketClient client, byte[] data)
        {
            try
            {
                // 发送消息
                client.Client.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, CancellationToken.None).Wait();
                log.Info("向客户端 " + client.Ip + " 发送 " + data.Length + " 字节的消息");
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
