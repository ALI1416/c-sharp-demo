using System;
using System.Net.WebSockets;

namespace ConsoleDemo
{
    /// <summary>
    /// webSocket客户端
    /// </summary>
    internal class WebSocketClient
    {
        /// <summary>
        /// 客户端
        /// </summary>
        public WebSocket Client { set; get; }
        /// <summary>
        /// 接收数据缓冲区
        /// </summary>
        public ArraySegment<byte> Buffer { set; get; }
        /// <summary>
        /// IP地址
        /// </summary>
        public string Ip { set; get; }
        /// <summary>
        /// 上线时间
        /// </summary>
        public DateTime Online { set; get; }
        /// <summary>
        /// 下线时间(`DateTime.MinValue`表示未下线)
        /// </summary>
        public DateTime Offline { set; get; }

        /// <summary>
        /// 新建客户端
        /// </summary>
        /// <param name="client">WebSocket</param>
        /// <param name="ip">IP地址</param>
        public WebSocketClient(WebSocket client, string ip)
        {
            Client = client;
            Buffer = new ArraySegment<byte>(new byte[1024]);
            Ip = ip;
            Online = DateTime.Now;
            Console.WriteLine("客户端 " + Ip + " 已上线");
        }

        /// <summary>
        /// 关闭客户端
        /// </summary>
        public void Close()
        {
            if (Client != null)
            {
                Client = null;
                Offline = DateTime.Now;
                Console.WriteLine("客户端 " + Ip + " 已下线");
            }
        }

    }
}
