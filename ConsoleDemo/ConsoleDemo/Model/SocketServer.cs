using System.Net.Sockets;

namespace ConsoleDemo.Model
{

    /// <summary>
    /// socket服务端
    /// </summary>
    public class SocketServer
    {
        /// <summary>
        /// 服务端
        /// </summary>
        public Socket Server { get; set; }

        /// <summary>
        /// 创建服务端
        /// </summary>
        public SocketServer()
        {
            Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        /// <summary>
        /// 关闭服务端
        /// </summary>
        public void Close()
        {
            if (Server != null)
            {
                Server.Close();
                Server = null;
            }
        }

    }
}
