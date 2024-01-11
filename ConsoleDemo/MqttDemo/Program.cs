using MQTTnet;
using MQTTnet.Client;

namespace MqttDemo
{

    /// <summary>
    /// MQTT
    /// </summary>
    public class Program
    {

        /// <summary>
        /// MQTT工厂
        /// </summary>
        private static readonly MqttFactory factory = new MqttFactory();
        /// <summary>
        /// 客户端
        /// </summary>
        private static IMqttClient client;

        public static void Main(string[] args)
        {
            factory.CreateMqttClient();

        }

    }
}
