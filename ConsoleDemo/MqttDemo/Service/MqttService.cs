using log4net;
using MQTTnet;
using MQTTnet.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MqttDemo.Service
{

    /// <summary>
    /// MQTT服务
    /// </summary>
    public class MqttService
    {

        /// <summary>
        /// 日志实例
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(typeof(MqttService));

        /// <summary>
        /// 重连时间(秒)
        /// </summary>
        private int reconnectTime;
        /// <summary>
        /// 订阅主题列表
        /// </summary>
        private string[] subscribeTopicArray;
        /// <summary>
        /// 接收消息回调函数 主题,消息
        /// </summary>
        private Action<string, byte[]> receiveCallback;

        /// <summary>
        /// 客户端
        /// </summary>
        private readonly IMqttClient client = new MqttFactory().CreateMqttClient();
        /// <summary>
        /// 连接选项
        /// </summary>
        private MqttClientOptions connectOptions;

        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="ip">IP</param>
        /// <param name="port">端口</param>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="reconnectTime">重连时间(秒，&lt;=0不重连)</param>
        /// <param name="subscribeTopicArray">订阅主题数组</param>
        /// <param name="receiveCallback">接收消息回调函数 主题,消息</param>
        /// <returns>是否启动成功</returns>
        public bool Start(string ip, int port, string username, string password, int reconnectTime, string[] subscribeTopicArray, Action<string, byte[]> receiveCallback)
        {
            MqttClientOptionsBuilder builder = new MqttClientOptionsBuilder().WithTcpServer(ip, port);
            if (username != null && password != null)
            {
                builder.WithCredentials(username, password);
            }
            connectOptions = builder.Build();
            this.reconnectTime = reconnectTime;
            this.subscribeTopicArray = subscribeTopicArray;
            this.receiveCallback = receiveCallback;
            return Connect();
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            client.DisconnectedAsync -= Disconnected;
            client.ApplicationMessageReceivedAsync -= Receive;
            client.DisconnectAsync();
            log.Info("MQTT连接关闭！");
        }

        /// <summary>
        /// 建立连接
        /// </summary>
        /// <returns>是否连接成功</returns>
        private bool Connect()
        {
            try
            {
                // 连接关闭处理
                client.DisconnectedAsync += Disconnected;
                // 接收消息处理
                client.ApplicationMessageReceivedAsync += Receive;
                // 建立连接
                client.ConnectAsync(connectOptions).Wait();
                log.Info("MQTT连接建立成功！");
            }
            catch (Exception e)
            {
                log.Error("MQTT连接建立失败！", e);
                return false;
            }
            for (int i = 0; i < subscribeTopicArray.Length; i++)
            {
                try
                {
                    // 订阅主题
                    client.SubscribeAsync(subscribeTopicArray[i]).Wait();
                }
                catch (Exception e)
                {
                    log.Error("MQTT主题 " + subscribeTopicArray[i] + " 监听失败！", e);

                }
            }
            return true;
        }

        /// <summary>
        /// 连接关闭处理
        /// </summary>
        /// <param name="e">MqttClientDisconnectedEventArgs</param>
        /// <returns>Task</returns>
        private Task Disconnected(MqttClientDisconnectedEventArgs e)
        {
            if (reconnectTime > 0)
            {
                log.Error("MQTT连接关闭！等待重连...", e.Exception);
                // 延时
                Thread.Sleep(reconnectTime * 1000);
                Connect();
            }
            else
            {
                log.Error("MQTT连接关闭！", e.Exception);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 接收消息处理
        /// </summary>
        /// <param name="e">MqttApplicationMessageReceivedEventArgs</param>
        /// <returns>Task</returns>
        private Task Receive(MqttApplicationMessageReceivedEventArgs e)
        {
            receiveCallback(e.ApplicationMessage.Topic, e.ApplicationMessage.PayloadSegment.Array);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="data">消息</param>
        /// <returns>是否发送成功</returns>
        public bool Send(string topic, byte[] data)
        {
            if (client.IsConnected)
            {
                try
                {
                    // 发送消息
                    client.PublishBinaryAsync(topic, data);
                    return true;
                }
                catch
                {
                }
            }
            return false;
        }

    }
}
