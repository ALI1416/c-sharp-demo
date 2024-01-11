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
        /// MQTT����
        /// </summary>
        private static readonly MqttFactory factory = new MqttFactory();
        /// <summary>
        /// �ͻ���
        /// </summary>
        private static IMqttClient client;

        public static void Main(string[] args)
        {
            factory.CreateMqttClient();

        }

    }
}
