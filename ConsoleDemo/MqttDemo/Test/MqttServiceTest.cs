using log4net;
using MqttDemo.Service;
using System.Text;
using System.Threading;

namespace MqttDemo.Test
{

    /// <summary>
    /// MQTT服务测试
    /// </summary>
    public class MqttServiceTest
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        private static readonly MqttService mqttService = new MqttService();
        private static readonly string ip = "127.0.0.1";
        private static readonly int port = 1883;
        private static readonly string username = null;
        private static readonly string password = null;
        private static readonly int reconnectTime = 10;

        private static readonly string[] subscribeTopicArray = new string[] { "test/#" };
        private static readonly string sendTopic = "demo";

        private static bool isStarted = false;

        /// <summary>
        /// 启动
        /// </summary>
        public static void Start()
        {
            if (!isStarted)
            {
                if (mqttService.Start(ip, port, username, password, reconnectTime, subscribeTopicArray, ReceiveCallback))
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
            if (mqttService != null)
            {
                isStarted = false;
                mqttService.Close();
            }
        }

        /// <summary>
        /// 接收消息回调函数
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="data">消息</param>
        private static void ReceiveCallback(string topic, byte[] data)
        {
            log.Info("接收到主题：" + topic + " ，消息：" + Encoding.UTF8.GetString(data));
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message">消息</param>
        private static void Send(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            if (mqttService.Send(sendTopic, data))
            {
                log.Info("发送消息：" + message);
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
