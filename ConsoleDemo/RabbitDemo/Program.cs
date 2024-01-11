using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

namespace RabbitDemo
{

    /// <summary>
    /// RabbitMQ测试
    /// </summary>
    public class Program
    {

        /// <summary>
        /// 交换机名称
        /// </summary>
        private static readonly string exchange = "demo";
        /// <summary>
        /// 路由名称
        /// </summary>
        private static readonly string routingKey = "test";

        /// <summary>
        /// RabbitMQ连接工厂
        /// </summary>
        private static readonly ConnectionFactory factory = new ConnectionFactory
        {
            HostName = "127.0.0.1",
            Port = 5672,
            UserName = "guest",
            Password = "guest"
        };

        /// <summary>
        /// 连接
        /// </summary>
        private static IConnection connection;
        /// <summary>
        /// 通道
        /// </summary>
        private static IModel channel;

        /// <summary>
        /// RabbitMQ初始化
        /// </summary>
        private static void RabbitInit()
        {
            // 建立连接
            if (connection == null || !connection.IsOpen)
            {
                channel = null;
                try
                {
                    connection = factory.CreateConnection();
                    Console.WriteLine("连接已建立！");
                }
                catch (Exception)
                {
                    Console.WriteLine("连接建立失败！等待重试...");
                }
            }
            // 监听消息
            if ((channel == null || !channel.IsOpen) && connection != null && connection.IsOpen)
            {
                try
                {
                    // 创建交换机
                    channel = connection.CreateModel();
                    channel.ExchangeDeclare(exchange, ExchangeType.Topic, true, false, null);
                    // 创建队列(随机名称)
                    string receiveQueueName = channel.QueueDeclare().QueueName;
                    channel.QueueBind(receiveQueueName, exchange, routingKey);
                    // 监听消息
                    EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
                    channel.BasicQos(0, 1, false);
                    channel.BasicConsume(receiveQueueName, false, consumer);
                    // 接收到消息
                    consumer.Received += (model, msg) =>
                    {
                        string message = Encoding.UTF8.GetString(msg.Body.ToArray());
                        Console.WriteLine("接收到消息：" + message);
                        // 确认消息
                        channel.BasicAck(msg.DeliveryTag, false);
                    };
                    Console.WriteLine("消息监听已建立！");
                }
                catch (Exception)
                {
                    Console.WriteLine("消息监听建立失败！等待重试...");
                }
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message">消息</param>
        private static void Send(string message)
        {
            if (channel != null && channel.IsOpen)
            {
                try
                {
                    byte[] body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(exchange, routingKey, null, body);
                    Console.WriteLine("发送消息：" + message);
                }
                catch (Exception)
                {
                    Console.WriteLine("消息发送失败！");
                }
            }
            else
            {
                Console.WriteLine("连接未建立，发送失败！");
            }
        }

        public static void Main(string[] args)
        {
            // RabbitMQ初始化
            new Thread(t =>
            {
                while (true)
                {
                    RabbitInit();
                    Thread.Sleep(10000);
                }
            })
            {
                IsBackground = true
            }.Start();
            // 发送消息
            while (true)
            {
                string message = Console.ReadLine();
                Send(message);
            }
        }

    }
}
