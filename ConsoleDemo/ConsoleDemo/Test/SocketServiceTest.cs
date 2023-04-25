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
    /// socket服务测试
    /// </summary>
    public class SocketServiceTest
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(SocketServiceTest));

        private static readonly SocketService socketService = new SocketService();
        private static readonly IPAddress ip = IPAddress.Parse("127.0.0.1");
        private static readonly int port = 8082;

        private readonly static byte[] responseHeader = Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\nContent-Type: multipart/x-mixed-replace; boundary=--boundary\n\n");
        private readonly static byte[] responseEnd = Encoding.ASCII.GetBytes("\n\n");

        private static bool isStarted = false;
        private static bool isText = true;

        /// <summary>
        /// 文本启动
        /// </summary>
        public static void Start()
        {
            if (!isStarted)
            {
                if (socketService.Start(ip, port, ServiceCloseCallback, ClientCallback, ResponseCallback))
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
                if (socketService.Start(ip, port, ServiceCloseCallback, ClientCallback, ResponseCallback))
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
            if (socketService != null)
            {
                socketService.Close();
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
        /// <param name="client">SocketClient</param>
        /// <param name="online">上线或下线</param>
        private static void ClientCallback(SocketClient client, bool online)
        {
            log.Info("客户端 " + client.Ip + (online ? " 已上线" : " 已下线"));
            // 上线
            if (!isText && online)
            {
                // 发送响应头
                socketService.Send(client, responseHeader);
            }
            log.Debug(Utils.IterateClient(socketService.ClientList()));
        }

        /// <summary>
        /// 响应回调函数
        /// </summary>
        /// <param name="client">SocketClient</param>
        private static void ResponseCallback(SocketClient client)
        {
            log.Info("收到客户端 " + client.Ip + " 消息：" + Encoding.UTF8.GetString(client.Buffer, 0, client.Length));
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
                    socketService.Send(Encoding.UTF8.GetBytes(Utils.GetSendString()));
                }
                else
                {
                    MemoryStream stream = Utils.GetSendMemoryStream();
                    string header = "--boundary\nContent-Type: image/png\nContent-Length: " + stream.Length + "\n\n";
                    byte[] data = new byte[header.Length + stream.Length + 2];
                    Encoding.UTF8.GetBytes(header).CopyTo(data, 0);
                    stream.ToArray().CopyTo(data, header.Length);
                    responseEnd.CopyTo(data, data.Length - 2);
                    socketService.Send(data);
                }
                Thread.Sleep(1000);
            }
        }

    }
}
