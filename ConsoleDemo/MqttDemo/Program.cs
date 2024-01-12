using MQTTnet;
using MQTTnet.Client;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MqttDemo
{

    /// <summary>
    /// MQTT
    /// </summary>
    public class Program
    {

        /// <summary>
        /// 订阅主题
        /// </summary>
        private static readonly string subscribeTopic = "test/#";
        /// <summary>
        /// 发送主题
        /// </summary>
        private static readonly string sendTopic = "test/demo";
        /// <summary>
        /// 连接选项
        /// </summary>
        private static readonly MqttClientOptions connectOptions = new MqttClientOptionsBuilder()
            .WithTcpServer("127.0.0.1", 1883)
            //.WithCredentials("admin", "123456")
            //.WithCleanSession(true)
            .Build();

        /// <summary>
        /// 客户端
        /// </summary>
        private static IMqttClient client;

        /// <summary>
        /// MQTT初始化
        /// </summary>
        private static async void MqttInit()
        {
            client = new MqttFactory().CreateMqttClient();
            // 建立连接(失败10秒后重连)
            while (true)
            {
                try
                {
                    await client.ConnectAsync(connectOptions);
                    // 连接关闭处理
                    client.DisconnectedAsync += Disconnected;
                    Console.WriteLine("建立连接成功！");
                    break;
                }
                catch
                {
                    Console.WriteLine("建立连接失败！等待重连...");
                    await Task.Delay(10000);
                }
            }
            // 订阅主题
            try
            {
                await client.SubscribeAsync(subscribeTopic);
                Console.WriteLine("订阅主题成功！");
            }
            catch
            {
                Console.WriteLine("订阅主题失败！");
                return;
            }
            // 接收消息处理
            client.ApplicationMessageReceivedAsync += Receive;
        }

        /// <summary>
        /// 连接关闭处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">MqttClientDisconnectedEventArgs</param>
        /// <returns>Task</returns>
        private static Task Disconnected(MqttClientDisconnectedEventArgs arg)
        {
            Console.WriteLine("连接关闭！等待重连...");
            MqttInit();
            return Task.CompletedTask;
        }

        /// <summary>
        /// 接收消息处理
        /// </summary>
        /// <param name="msg">MqttApplicationMessageReceivedEventArgs</param>
        /// <returns>Task</returns>
        private static Task Receive(MqttApplicationMessageReceivedEventArgs msg)
        {
            string topic = msg.ApplicationMessage.Topic;
            string message = Encoding.UTF8.GetString(msg.ApplicationMessage.PayloadSegment.Array);
            Console.WriteLine("接收到主题：" + topic + " ，消息：" + message);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message">消息</param>
        private static void Send(string message)
        {
            if (client != null && client.IsConnected)
            {
                try
                {
                    // 发送消息
                    client.PublishStringAsync(sendTopic, message);
                    Console.WriteLine("发送消息：" + message);
                }
                catch
                {
                    Console.WriteLine("发送消息失败！");
                }
            }
            else
            {
                Console.WriteLine("连接未建立，发送失败！");
            }
        }

        public static Task Main(string[] args)
        {
            // MQTT初始化
            new Thread(t =>
            {
                MqttInit();
            })
            {
                IsBackground = true
            }.Start();
            // 发送消息
            while (true)
            {
                Send(Console.ReadLine());
            }
        }

    }
}
