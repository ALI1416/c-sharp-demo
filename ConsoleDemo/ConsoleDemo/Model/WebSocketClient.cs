using System;
using System.Net.WebSockets;

namespace ConsoleDemo.Model
{

    /// <summary>
    /// webSocket客户端
    /// </summary>
    public class WebSocketClient
    {

        /// <summary>
        /// 接收数据缓冲区长度，超出部分将丢弃
        /// </summary>
        public static readonly int MAX_BUFFER_LENGTH = 4096;

        /// <summary>
        /// 客户端
        /// </summary>
        public WebSocket Client { set; get; }
        /// <summary>
        /// 接收数据缓冲区
        /// </summary>
        public ArraySegment<byte> Buffer { set; get; }
        /// <summary>
        /// 数据接收长度
        /// </summary>
        private int length;
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
        /// 数据接收长度
        /// </summary>
        public int Length
        {
            set { length = value; }
            get { return length > MAX_BUFFER_LENGTH ? MAX_BUFFER_LENGTH : length; }
        }

        /// <summary>
        /// 创建客户端
        /// </summary>
        /// <param name="webSocket">WebSocket</param>
        /// <param name="ip">IP地址</param>
        public WebSocketClient(WebSocket webSocket, string ip)
        {
            Client = webSocket;
            Buffer = new ArraySegment<byte>(new byte[MAX_BUFFER_LENGTH]);
            Length = 0;
            Ip = ip;
            Online = DateTime.Now;
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
            }
        }

    }
}
