using ConsoleDemo.Model;
using log4net;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using ConsoleDemo.Util;
using System.Text;

namespace ConsoleDemo.Service
{

    /// <summary>
    /// webSocket服务3(使用Socket)
    /// </summary>
    public class WebSocketService3
    {

        /// <summary>
        /// 日志实例
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(typeof(WebSocketService3));

        /// <summary>
        /// 服务器
        /// </summary>
        private SocketServer2 server;
        /// <summary>
        /// 客户端列表
        /// </summary>
        private List<SocketClient2> clientList;
        /// <summary>
        /// 服务器关闭回调函数
        /// </summary>
        private Action serviceCloseCallback;
        /// <summary>
        /// 客户端上下线回调函数&lt;SocketClient2,上线或下线>
        /// </summary>
        private Action<SocketClient2, bool> clientCallback;
        /// <summary>
        /// 响应回调函数&lt;SocketClient2,解码后的byte[]>
        /// </summary>
        private Action<SocketClient2, byte[]> responseCallback;

        /// <summary>
        /// 关闭连接头byte[]
        /// </summary>
        private readonly static byte[] closeHeaderBytes = Encoding.UTF8.GetBytes("HTTP/1.0 200 OK\nConnection: close\n\n");

        /// <summary>
        /// 获取服务端
        /// </summary>
        /// <returns>SocketServer2</returns>
        public SocketServer2 Server
        {
            get { return server; }
        }

        /// <summary>
        /// 获取客户端列表
        /// </summary>
        /// <returns>SocketClient2[]</returns>
        public SocketClient2[] ClientList()
        {
            return clientList.ToArray();
        }

        /// <summary>
        /// 获取在线客户端列表
        /// </summary>
        /// <returns>List&lt;SocketClient2></returns>
        public List<SocketClient2> ClientOnlineList()
        {
            return clientList.FindAll(e => e.Client != null);
        }

        /// <summary>
        /// 获取`在线`并且`可发送数据`的用户列表
        /// </summary>
        /// <returns>List&lt;WebSocketClient2></returns>
        public List<SocketClient2> ClientOnlineAndNotTransmission()
        {
            return clientList.FindAll(e => (e.Client != null && !e.Transmission));
        }

        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口号</param>
        /// <param name="serviceCloseCallback">服务器关闭回调函数</param>
        /// <param name="clientCallback">客户端上下线回调函数&lt;SocketClient2,上线或下线></param>
        /// <param name="responseCallback">响应回调函数&lt;SocketClient2,解码后的byte[]></param>
        /// <returns>是否启动成功</returns>
        public bool Start(IPAddress ip, int port, Action serviceCloseCallback, Action<SocketClient2, bool> clientCallback, Action<SocketClient2, byte[]> responseCallback)
        {
            try
            {
                // 新建服务器
                server = new SocketServer2(server);
                // 指定IP地址和端口号
                server.Server.Bind(new IPEndPoint(ip, port));
                // 设置监听数量
                server.Server.Listen(10);
                // 异步监听客户端请求
                server.Server.BeginAccept(Handle, null);
            }
            // 端口号冲突
            catch
            {
                server.Close();
                server = null;
                log.Error("webSocket服务器3端口号冲突");
                return false;
            }
            if (clientList == null)
            {
                clientList = new List<SocketClient2>();
            }
            this.serviceCloseCallback = serviceCloseCallback;
            this.clientCallback = clientCallback;
            this.responseCallback = responseCallback;
            log.Info("webSocket服务器3已启动");
            return true;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            if (server != null)
            {
                foreach (SocketClient2 client in ClientOnlineList())
                {
                    ClientOffline(client);
                }
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
                server.Server.BeginAccept(Handle, null);
            }
            // 主动关闭服务器
            catch
            {
                // 服务器关闭回调函数
                serviceCloseCallback();
                log.Info("主动关闭socket服务器");
                return;
            }
            // 客户端上线
            ClientOnline(server.Server.EndAccept(ar));
        }

        /// <summary>
        /// 客户端上线
        /// </summary>
        /// <param name="socket">客户端</param>
        private void ClientOnline(Socket socket)
        {
            // 已存在
            if (clientList.Exists(e => e.Client == socket))
            {
                return;
            }
            SocketClient2 client = null;
            try
            {
                client = new SocketClient2(socket);
                // 接收消息
                socket.BeginReceive(client.Buffer, 0, SocketClient2.MAX_BUFFER_LENGTH, SocketFlags.None, Recevice, client);
                clientList.Add(client);
                // 客户端上线回调函数
                clientCallback(client, true);
            }
            catch
            {
                if (client != null)
                {
                    client.Close();
                }
                return;
            }
        }

        /// <summary>
        /// 客户端下线
        /// </summary>
        /// <param name="client">SocketClient2</param>
        private void ClientOffline(SocketClient2 client)
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
        /// 接收消息
        /// </summary>
        /// <param name="ar">IAsyncResult</param>
        private void Recevice(IAsyncResult ar)
        {
            // 获取当前客户端
            SocketClient2 client = ar.AsyncState as SocketClient2;
            try
            {
                // 获取接收数据长度
                int length = client.Client.EndReceive(ar);
                // 客户端主动断开连接时，会发送0字节消息
                if (length == 0)
                {
                    ClientOffline(client);
                    return;
                }
                client.Length += length;
                // 消息全部接收完毕
                if (client.Client.Available == 0)
                {
                    // 首次连接
                    if (client.Buffer[0] == 71)
                    {
                        // 请求握手
                        if (!HandShake(client))
                        {
                            return;
                        }
                    }
                    else
                    {
                        // 解码消息
                        byte[] data = WebSocketUtils.DecodeData(client.Buffer, client.Length);
                        // 客户端请求关闭连接
                        if (data == null)
                        {
                            ClientOffline(client);
                        }
                        else
                        {
                            client.Transmission = false;
                            // 响应回调函数
                            responseCallback(client, data);
                        }
                    }
                    client.Length = 0;
                    // 继续接收消息
                    client.Client.BeginReceive(client.Buffer, 0, SocketClient2.MAX_BUFFER_LENGTH, SocketFlags.None, Recevice, client);
                }
                // 消息还未全部接收
                else
                {
                    // 计算可用容量
                    int available = SocketClient2.MAX_BUFFER_LENGTH - length;
                    // 丢弃溢出消息
                    if (available == 0)
                    {
                        client.Client.BeginReceive(new byte[SocketClient2.MAX_BUFFER_LENGTH], 0, SocketClient2.MAX_BUFFER_LENGTH, SocketFlags.None, Recevice, client);
                    }
                    // 继续接收消息
                    else
                    {
                        client.Client.BeginReceive(client.Buffer, client.Length, available, SocketFlags.None, Recevice, client);
                    }
                }
            }
            // 超时后失去连接、未知错误
            catch
            {
                ClientOffline(client);
                return;
            }
        }

        /// <summary>
        /// 请求握手
        /// </summary>
        /// <param name="client">SocketClient2</param>
        /// <returns>是否已请求握手</returns>
        private bool HandShake(SocketClient2 client)
        {
            // 解码消息
            string msg = Encoding.UTF8.GetString(client.Buffer, 0, client.Length);
            // 获取握手信息
            byte[] data = WebSocketUtils.HandShake(msg);
            if (data != null)
            {
                // 发送握手信息
                SendRaw(client, data);
                return true;
            }
            // 无法握手
            else
            {
                // 关闭连接
                SendRaw(client, closeHeaderBytes);
                client.Close();
                return false;
            }
        }

        /// <summary>
        /// 发送消息 给所有`在线`并且`可发送数据`的用户列表
        /// </summary>
        /// <param name="data">消息</param>
        public void Send(byte[] data)
        {
            List<SocketClient2> list = ClientOnlineAndNotTransmission();
            server.Record(data.Length * list.Count);
            foreach (SocketClient2 client in list)
            {
                Send(client, data);
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="client">SocketClient2</param>
        /// <param name="data">消息</param>
        public void Send(SocketClient2 client, byte[] data)
        {
            client.Transmission = true;
            client.Record(data.Length);
            SendRaw(client, WebSocketUtils.CodedData(data, false));
        }

        /// <summary>
        /// 发送原始消息
        /// </summary>
        /// <param name="client">SocketClient2</param>
        /// <param name="data">byte[]</param>
        private void SendRaw(SocketClient2 client, byte[] data)
        {
            try
            {
                // 发送消息
                client.Client.BeginSend(data, 0, data.Length, SocketFlags.None, asyncResult =>
                {
                    try
                    {
                        int length = client.Client.EndSend(asyncResult);
                        log.Info("向客户端 " + client.Ip + " 发送 " + length + " 字节的原始消息");
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
