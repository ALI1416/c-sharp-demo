using ConsoleDemo.Model;
using ConsoleDemo.Service;
using ConsoleDemo.Util;
using log4net;
using System;
using System.Net;
using System.Text;

namespace ConsoleDemo.Test
{

    /// <summary>
    /// socket服务测试
    /// </summary>
    public class SocketServiceTest
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(SocketServiceTest));

        private static readonly SocketService socketService = new SocketService();
        private static readonly IPAddress ip = IPAddress.Parse("127.0.0.1");
        private static readonly int port = 8082;

        private static bool isStarted = false;

        /// <summary>
        /// 启动
        /// </summary>
        public static void Start()
        {
            if (socketService.Start(ip, port, ServiceCloseCallback, ClientCallback, ResponseCallback))
            {
                isStarted = true;
            }
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public static void Close()
        {
            socketService.Close();
        }

        /// <summary>
        /// 服务器关闭回调函数
        /// </summary>
        private static void ServiceCloseCallback()
        {
            isStarted = false;
        }

        /// <summary>
        /// 客户端上下线回调函数
        /// </summary>
        /// <param name="client">SocketClient</param>
        /// <param name="online">上线或下线</param>
        private static void ClientCallback(SocketClient client, bool online)
        {
            log.Info("客户端 " + client.Ip + (online ? " 已上线" : " 已下线"));
            Utils.IterateClient(socketService.ClientList());
        }

        /// <summary>
        /// 响应回调函数
        /// </summary>
        /// <param name="client">SocketClient</param>
        private static void ResponseCallback(SocketClient client)
        {
            log.Info("收到客户端 " + client.Ip + " 消息：" + Encoding.UTF8.GetString(client.Buffer, 0, client.Length));
        }

    }
}
