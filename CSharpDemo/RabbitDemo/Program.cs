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
        private static readonly string exchange = "test";
        /// <summary>
        /// 路由名称
        /// </summary>
        private static readonly string routingKey = "demo";
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
        /// 通道
        /// </summary>
        private static IModel channel;

        /// <summary>
        /// RabbitMQ初始化
        /// </summary>
        private static void RabbitInit()
        {
            channel = null;
            IConnection connection;
            // 建立连接(失败10秒后重连)
            while (true)
            {
                try
                {
                    connection = factory.CreateConnection();
                    // 连接关闭处理
                    connection.ConnectionShutdown += ConnectionShutdown;
                    Console.WriteLine("建立连接成功！");
                    break;
                }
                catch
                {
                    Console.WriteLine("建立连接失败！等待重连...");
                    Thread.Sleep(10000);
                }
            }
            // 创建交换机
            try
            {
                channel = connection.CreateModel();
                channel.ExchangeDeclare(exchange, ExchangeType.Topic, false, true);
                Console.WriteLine("创建交换机成功！");
            }
            catch
            {
                Console.WriteLine("创建交换机失败！");
                return;
            }
            // 创建队列
            string queue;
            try
            {
                // 随机队列名
                queue = channel.QueueDeclare().QueueName;
                channel.QueueBind(queue, exchange, routingKey);
                Console.WriteLine("创建队列成功！");
            }
            catch
            {
                Console.WriteLine("创建队列失败！");
                return;
            }
            // 监听消息
            try
            {
                EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
                channel.BasicQos(0, 1, false);
                channel.BasicConsume(queue, false, consumer);
                // 接收消息处理
                consumer.Received += Receive;
                Console.WriteLine("监听消息成功！");
            }
            catch
            {
                Console.WriteLine("监听消息失败！");
            }
        }

        /// <summary>
        /// 连接关闭处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">ShutdownEventArgs</param>
        private static void ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine("连接关闭！等待重连...");
            RabbitInit();
        }

        /// <summary>
        /// 接收消息处理
        /// </summary>
        /// <param name="model">IModel</param>
        /// <param name="msg">BasicDeliverEventArgsparam>
        private static void Receive(object model, BasicDeliverEventArgs msg)
        {
            // 确认消息
            channel.BasicAck(msg.DeliveryTag, false);
            Console.WriteLine("接收到消息：" + Encoding.UTF8.GetString(msg.Body.ToArray()));
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
                    // 发送消息
                    channel.BasicPublish(exchange, routingKey, null, Encoding.UTF8.GetBytes(message));
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

        public static void Main(string[] args)
        {
            // RabbitMQ初始化
            new Thread(t =>
            {
                RabbitInit();
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
