using log4net;
using System.Net;
using System;
using ConsoleDemo.Model;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using System.Collections.Generic;

namespace ConsoleDemo.Service
{

    /// <summary>
    /// webSocket服务(使用HttpListener,性能差)
    /// </summary>
    public class WebSocketService
    {

        /// <summary>
        /// 日志实例
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(typeof(WebSocketService));

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
        /// 客户端上下线回调函数&lt;客户端,上线或下线>
        /// </summary>
        private Action<WebSocketClient, bool> clientCallback;
        /// <summary>
        /// 响应回调函数&lt;客户端>
        /// </summary>
        private Action<WebSocketClient> responseCallback;

        /// <summary>
        /// 获取客户端列表
        /// </summary>
        /// <returns>客户端列表</returns>
        public WebSocketClient[] ClientList()
        {
            return clientList.ToArray();
        }

        /// <summary>
        /// 获取在线客户端列表
        /// </summary>
        /// <returns>在线客户端列表</returns>
        public List<WebSocketClient> ClientOnlineList()
        {
            return clientList.FindAll(e => e.Client != null);
        }

        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口号</param>
        /// <param name="serviceCloseCallback">服务器关闭回调函数</param>
        /// <param name="clientCallback">客户端上下线回调函数&lt;客户端,上线或下线></param>
        /// <param name="responseCallback">响应回调函数&lt;客户端></param>
        /// <returns>是否启动成功</returns>
        public bool Start(IPAddress ip, int port, Action serviceCloseCallback, Action<WebSocketClient, bool> clientCallback, Action<WebSocketClient> responseCallback)
        {
            try
            {
                // 新建服务器
                server = new HttpListener
                {
                    // 忽视客户端写入异常
                    IgnoreWriteExceptions = true
                };
                // 指定IP地址和端口号
                server.Prefixes.Add("http://" + ip + ":" + port + "/");
                // 启动服务器
                server.Start();
                // 异步监听客户端请求
                server.BeginGetContext(Handle, null);
            }
            // 端口号冲突
            catch
            {
                server.Close();
                server = null;
                log.Error("webSocket服务器端口号冲突");
                return false;
            }
            if (clientList == null)
            {
                clientList = new List<WebSocketClient>();
            }
            this.serviceCloseCallback = serviceCloseCallback;
            this.clientCallback = clientCallback;
            this.responseCallback = responseCallback;
            log.Info("webSocket服务器已启动");
            return true;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            if (server != null)
            {
                server.Close();
            }
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
                // webSocket处理
                new Task(() => WebSocketHandle(server.EndGetContext(ar))).Start();
            }
            // 主动关闭服务器
            catch
            {
                // 服务器关闭回调函数
                serviceCloseCallback();
                log.Info("主动关闭webSocket服务器");
                return;
            }
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
                // 接受webSocket连接
                wsContext = await context.AcceptWebSocketAsync(null);
            }
            // 不是webSocket连接
            catch
            {
                // 关闭
                context.Response.Close();
                return;
            }
            // 客户端上线
            ClientOnline(wsContext.WebSocket, context.Request.LocalEndPoint.ToString());
        }

        /// <summary>
        /// 客户端上线
        /// </summary>
        /// <param name="webSocket">WebSocket</param>
        /// <param name="ip">IP地址</param>
        private async void ClientOnline(WebSocket webSocket, string ip)
        {
            // 已存在
            if (clientList.Exists(e => e.Client == webSocket))
            {
                return;
            }
            WebSocketClient client = null;
            try
            {
                client = new WebSocketClient(webSocket, ip);
                clientList.Add(client);
                // 客户端上线回调函数
                clientCallback(client, true);
                // 首次接收消息
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(client.Buffer, CancellationToken.None);
                client.Length += result.Count;
                while (true)
                {
                    // 消息全部接收完毕
                    if (result.EndOfMessage)
                    {
                        // 响应回调函数
                        responseCallback(client);
                        // 继续接收消息
                        result = await webSocket.ReceiveAsync(client.Buffer, CancellationToken.None);
                    }
                    // 消息还未全部接收
                    else
                    {
                        // 丢弃溢出消息
                        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(new byte[WebSocketClient.MAX_BUFFER_LENGTH]), CancellationToken.None);
                    }
                    client.Length += result.Count;
                }
            }
            // 断开连接
            catch
            {
                ClientOffline(client);
            }
        }

        /// <summary>
        /// 客户端下线
        /// </summary>
        /// <param name="client">客户端</param>
        private void ClientOffline(WebSocketClient client)
        {
            // 不存在
            if (!ClientOnlineList().Contains(client))
            {
                return;
            }
            client.Close();
            // 客户端下线回调函数
            clientCallback(client, false);
        }

        /// <summary>
        /// 发送文本消息 给所有在线客户端
        /// </summary>
        /// <param name="data">文本消息</param>
        public void Send(byte[] data)
        {
            foreach (WebSocketClient client in ClientOnlineList())
            {
                Send(client, data, true);
            }
        }

        /// <summary>
        /// 发送文本消息
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="data">文本消息</param>
        public void Send(WebSocketClient client, byte[] data)
        {
            Send(client, data, true);
        }

        /// <summary>
        /// 发送消息 给所有在线客户端
        /// </summary>
        /// <param name="data">消息</param>
        /// <param name="isText">是否为文本数据</param>
        public void Send(byte[] data, bool isText)
        {
            foreach (WebSocketClient client in ClientOnlineList())
            {
                Send(client, data, isText);
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="data">消息</param>
        /// <param name="isText">是否为文本数据</param>
        public void Send(WebSocketClient client, byte[] data, bool isText)
        {
            try
            {
                // 发送消息
                client.Client.SendAsync(new ArraySegment<byte>(data), isText ? WebSocketMessageType.Text : WebSocketMessageType.Binary, true, CancellationToken.None).Wait();
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
