using ConsoleDemo.Model;
using ConsoleDemo.Service;
using ConsoleDemo.Util;
using log4net;
using System.Net;
using System.Text;
using System.Threading;

namespace ConsoleDemo.Test
{

    /// <summary>
    /// webSocket服务(使用HttpListener,性能差,文本)测试
    /// </summary>
    public class WebSocketService1Test
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(WebSocketService1Test));

        private static readonly WebSocketService webSocketService = new WebSocketService();
        private static readonly IPAddress ip = IPAddress.Parse("127.0.0.1");
        private static readonly int port = 8084;

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
        /// <param name="client">WebSocketClient</param>
        /// <param name="online">上线或下线</param>
        private static void ClientCallback(WebSocketClient client, bool online)
        {
            log.Info("客户端 " + client.Ip + (online ? " 已上线" : " 已下线"));
            log.Debug(Utils.IterateClient(webSocketService.ClientList()));
        }

        /// <summary>
        /// 响应回调函数
        /// </summary>
        /// <param name="client">WebSocketClient</param>
        private static void ResponseCallback(WebSocketClient client)
        {
            log.Info("收到客户端 " + client.Ip + " 消息：" + Encoding.UTF8.GetString(client.Buffer.Array, 0, client.Length));
        }

        /// <summary>
        /// 定时向客户端发送消息
        /// </summary>
        private static void IntervalSend()
        {
            while (isStarted)
            {
                webSocketService.Send(Encoding.UTF8.GetBytes(Utils.GetSendString()));
                Thread.Sleep(1000);
            }
        }

    }
}
