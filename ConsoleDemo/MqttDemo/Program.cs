using MqttDemo.Test;
using System;

namespace MqttDemo
{

    public class Program
    {
        public static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("\n关闭程序:0 重放:9\n" +
                    "MQTT服务\t-> 关闭:1 启动:2\n"
                    );
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.D0:
                        {
                            return;
                        }
                    case ConsoleKey.D1:
                        {
                            //MqttServiceTest.Close();
                            break;
                        }
                    case ConsoleKey.D2:
                        {
                            //MqttServiceTest.Start();
                            break;
                        }
                    case ConsoleKey.D9:
                        {
                            break;
                        }
                }
            }
        }

    }
}
