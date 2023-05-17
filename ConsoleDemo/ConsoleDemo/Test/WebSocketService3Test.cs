using ConsoleDemo.Model;
using ConsoleDemo.Service;
using ConsoleDemo.Util;
using log4net;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace ConsoleDemo.Test
{

    /// <summary>
    /// webSocket服务3(使用Socket)
    /// </summary>
    public class WebSocketService3Test
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(WebSocketService3Test));

        private static readonly WebSocketService3 webSocketService = new WebSocketService3();
        private static readonly IPAddress ip = IPAddress.Parse("127.0.0.1");
        private static readonly int port = 8085;

        private static bool isStarted = false;

        /// <summary>
        /// 启动
        /// </summary>
        public static void Start()
        {
            if (!isStarted)
            {
                if (webSocketService.Start(ip, port, ServiceCloseCallback, ClientCallback, ResponseCallback))
                {
                    isStarted = true;
                    new Thread(t =>
                    {
                        IntervalSend();
                    })
                    {
                        IsBackground = true
                    }.Start();
                }
            }
            else
            {
                log.Warn("请先关闭服务");
            }
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public static void Close()
        {
            if (webSocketService != null)
            {
                webSocketService.Close();
            }
        }

        /// <summary>
        /// 服务器关闭回调函数
        /// </summary>
        private static void ServiceCloseCallback()
        {
            isStarted = false;
            log.Warn("服务器关闭回调函数");
        }

        /// <summary>
        /// 客户端上下线回调函数
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="online">上线或下线</param>
        private static void ClientCallback(SocketClient2 client, bool online)
        {
            log.Info("客户端 " + client.Ip + (online ? " 已上线" : " 已下线"));
            log.Debug(Utils.Iterate(webSocketService));
        }

        /// <summary>
        /// 响应回调函数
        /// </summary>
        /// <param name="client">客户端</param>
        private static void ResponseCallback(SocketClient2 client, byte[] data)
        {
            log.Info("收到客户端 " + client.Ip + " 消息：" + Encoding.UTF8.GetString(data));
        }

        /// <summary>
        /// 定时向客户端发送消息
        /// </summary>
        private static void IntervalSend()
        {
            while (isStarted)
            {
                MemoryStream stream = Utils.GetSendMemoryStream();
                var data = stream.ToArray();
                // 获取`在线`并且`可发送数据`的用户列表
                var list = webSocketService.ClientOnlineAndNotTransmission();
                // 记录webSocket服务端访问记录
                webSocketService.Server.RecordAccess(list.Count * data.Length);
                // 发送给webSocket客户端
                webSocketService.SendDataByClientList(list, data);
                Thread.Sleep(100);
            }
        }

    }
}
