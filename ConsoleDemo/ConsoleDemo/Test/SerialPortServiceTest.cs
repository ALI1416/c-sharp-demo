using ConsoleDemo.Service;
using log4net;
using System.Text;
using System.Threading;

namespace ConsoleDemo.Test
{

    /// <summary>
    /// 串口服务测试
    /// </summary>
    public class SerialPortServiceTest
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SerialPortServiceTest));

        private static readonly SerialPortService serialPortService = new SerialPortService();
        private static readonly string portName = "COM3";
        private static readonly int baudRate = 4800;
        private static readonly int reconnectTime = 10;

        private static bool isStarted = false;

        /// <summary>
        /// 启动
        /// </summary>
        public static void Start()
        {
            if (!isStarted)
            {
                if (serialPortService.Start(portName, baudRate, reconnectTime, ReceiveCallback))
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
            if (serialPortService != null)
            {
                isStarted = false;
                serialPortService.Close();
            }
        }

        /// <summary>
        /// 接收消息回调函数
        /// </summary>
        /// <param name="data">消息</param>
        private static void ReceiveCallback(byte[] data)
        {
            log.Info("收到消息：" + Encoding.UTF8.GetString(data) + " ，Hex：" + Bytes2Hex(data));
        }

        /// <summary>
        /// byte[]转Hex字符串
        /// </summary>
        /// <param name="bytes">byte[]</param>
        /// <returns>Hex字符串</returns>
        private static string Bytes2Hex(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("X2"));
                sb.Append(" ");
            }
            return sb.ToString();
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message">消息</param>
        private static void Send(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            if (serialPortService.Send(data))
            {
                log.Info("发送消息：" + message + " ，Hex：" + Bytes2Hex(data));
            }
            else
            {
                log.Warn("发送消息失败！");
            }
        }

        /// <summary>
        /// 定时向客户端发送消息
        /// </summary>
        private static void IntervalSend()
        {
            int n = 1;
            while (isStarted)
            {
                Send(n.ToString());
                n++;
                Thread.Sleep(1000);
            }
        }

    }
}
