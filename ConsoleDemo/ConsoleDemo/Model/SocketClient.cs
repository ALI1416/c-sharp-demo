using System;
using System.Net.Sockets;

namespace ConsoleDemo.Model
{

    /// <summary>
    /// socket客户端
    /// </summary>
    public class SocketClient
    {

        /// <summary>
        /// 接收数据缓冲区长度，超出部分将丢弃
        /// </summary>
        public static readonly int MAX_BUFFER_LENGTH = 4096;

        /// <summary>
        /// 客户端
        /// </summary>
        public Socket Client { set; get; }
        /// <summary>
        /// 接收数据缓冲区
        /// </summary>
        public byte[] Buffer { set; get; }
        /// <summary>
        /// 数据接收长度
        /// </summary>
        private int length;
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
        /// 数据传输中
        /// </summary>
        public bool Transmission { set; get; }

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
        /// <param name="client">Socket</param>
        public SocketClient(Socket client)
        {
            Client = client;
            Buffer = new byte[MAX_BUFFER_LENGTH];
            Length = 0;
            Ip = client.RemoteEndPoint.ToString();
            Online = DateTime.Now;
            Transmission = false;
        }

        /// <summary>
        /// 关闭客户端
        /// </summary>
        public void Close()
        {
            if (Client != null)
            {
                Client.Close();
                Client = null;
                Buffer = null;
                Offline = DateTime.Now;
            }
        }

    }
}
