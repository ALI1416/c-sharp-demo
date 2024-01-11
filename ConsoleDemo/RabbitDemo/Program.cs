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
        /// 发送消息交换机名称
        /// </summary>
        private static readonly string sendExchangeName = "test";
        /// <summary>
        /// 发送消息路由名称
        /// </summary>
        private static readonly string sendRoutingKey = "a";

        /// <summary>
        /// 接收消息交换机名称
        /// </summary>
        private static readonly string receiveExchangeName = "test";
        /// <summary>
        /// 接收消息路由名称
        /// </summary>
        private static readonly string receiveRoutingKey = "a";

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
        /// 发送消息连接
        /// </summary>
        private static IConnection sendConnection;
        /// <summary>
        /// 发送消息通道
        /// </summary>
        private static IModel sendChannel;

        /// <summary>
        /// 接收消息连接
        /// </summary>
        private static IConnection receiveConnection;
        /// <summary>
        /// 接收消息通道
        /// </summary>
        private static IModel receiveChannel;

        /// <summary>
        /// RabbitMQ初始化
        /// </summary>
        private static void RabbitInit()
        {
            // 发送消息初始化
            if (sendConnection == null || !sendConnection.IsOpen)
            {
                sendChannel = null;
                try
                {
                    sendConnection = factory.CreateConnection();
                    Console.WriteLine("发送消息 连接已建立！");
                }
                catch (Exception)
                {
                    Console.WriteLine("发送消息 连接建立失败！等待重连...");
                }
            }
            if ((sendChannel == null || !sendChannel.IsOpen) && sendConnection != null && sendConnection.IsOpen)
            {
                try
                {
                    sendChannel = sendConnection.CreateModel();
                    sendChannel.ExchangeDeclare(sendExchangeName, ExchangeType.Topic, durable: true, autoDelete: false, arguments: null);
                    Console.WriteLine("发送消息 就绪！");
                }
                catch (Exception)
                {
                    Console.WriteLine("发送消息 交换机建立失败！等待重试...");
                }
            }
            // 接收消息初始化
            if (receiveConnection == null || !receiveConnection.IsOpen)
            {
                receiveChannel = null;
                try
                {
                    receiveConnection = factory.CreateConnection();
                    Console.WriteLine("接收消息 连接已建立！");
                }
                catch (Exception)
                {
                    Console.WriteLine("接收消息 连接建立失败！等待重连...");
                }
            }
            if ((receiveChannel == null || !receiveChannel.IsOpen) && receiveConnection != null && receiveConnection.IsOpen)
            {
                try
                {
                    receiveChannel = receiveConnection.CreateModel();
                    receiveChannel.ExchangeDeclare(receiveExchangeName, ExchangeType.Topic, durable: true, autoDelete: false, arguments: null);
                    // 生成随机队列名
                    string receiveQueueName = receiveChannel.QueueDeclare().QueueName;
                    receiveChannel.QueueBind(receiveQueueName, receiveExchangeName, receiveRoutingKey);
                    // 监听消息
                    EventingBasicConsumer consumer = new EventingBasicConsumer(receiveChannel);
                    receiveChannel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                    receiveChannel.BasicConsume(receiveQueueName, false, consumer: consumer);
                    consumer.Received += (model, msg) =>
                    {
                        string message = Encoding.UTF8.GetString(msg.Body.ToArray());
                        Console.WriteLine("接收到消息：" + message);
                        receiveChannel.BasicAck(deliveryTag: msg.DeliveryTag, multiple: false);
                    };
                    Console.WriteLine("接收消息 监听已建立！");
                }
                catch (Exception)
                {
                    Console.WriteLine("接收消息 监听建立失败！等待重连...");
                }
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message">消息</param>
        private static void Send(string message)
        {
            if (sendChannel != null && sendChannel.IsOpen)
            {
                try
                {
                    byte[] body = Encoding.UTF8.GetBytes(message);
                    sendChannel.BasicPublish(sendExchangeName, sendRoutingKey, basicProperties: null, body);
                    Console.WriteLine("发送消息：" + message);
                }
                catch (Exception)
                {
                    Console.WriteLine("发送消息 发生错误，发送失败！");
                }
            }
            else
            {
                Console.WriteLine("发送消息 连接未建立，发送失败！");
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
