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
    /// webSocket服务2(使用Socket)
    /// </summary>
    public class WebSocketService2Test
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(WebSocketService2Test));

        private static readonly WebSocketService2 webSocketService = new WebSocketService2();
        private static readonly IPAddress ip = IPAddress.Parse("127.0.0.1");
        private static readonly int port = 8084;

        private static bool isStarted = false;
        private static bool isText = true;

        /// <summary>
        /// 文本启动
        /// </summary>
        public static void Start()
        {
            if (!isStarted)
            {
                if (webSocketService.Start(ip, port, ServiceCloseCallback, ClientCallback, ResponseCallback))
                {
                    isStarted = true;
                    isText = true;
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
        /// 图片启动
        /// </summary>
        public static void Start2()
        {
            if (!isStarted)
            {
                if (webSocketService.Start(ip, port, ServiceCloseCallback, ClientCallback, ResponseCallback))
                {
                    isStarted = true;
                    isText = false;
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
        private static void ClientCallback(SocketClient client, bool online)
        {
            log.Info("客户端 " + client.Ip + (online ? " 已上线" : " 已下线"));
            log.Debug(Utils.IterateClient(webSocketService.ClientList()));
        }

        /// <summary>
        /// 响应回调函数
        /// </summary>
        /// <param name="client">客户端</param>
        private static void ResponseCallback(SocketClient client, byte[] data)
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
                if (isText)
                {
                    webSocketService.Send(Encoding.UTF8.GetBytes(Utils.GetSendString()));
                }
                else
                {
                    MemoryStream stream = Utils.GetSendMemoryStream();
                    webSocketService.Send(stream.ToArray(), false);
                }
                Thread.Sleep(1000);
            }
        }

    }
}
