using System;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace SerialPortDemo
{

    /// <summary>
    /// 串口测试
    /// </summary>
    public class Program
    {

        /// <summary>
        /// 串口配置
        /// </summary>
        private static readonly SerialPort serialPort = new SerialPort
        {
            PortName = "COM4",
            BaudRate = 4800,
            Parity = Parity.None,
            DataBits = 8,
            StopBits = StopBits.One,
        };

        /// <summary>
        /// 串口初始化
        /// </summary>
        private static void SerialPortInit()
        {
            // 开启串口
            if (!serialPort.IsOpen)
            {
                try
                {
                    serialPort.Open();
                    Console.WriteLine("串口 已连接！");
                }
                catch (Exception)
                {
                    Console.WriteLine("串口 已断开！等待重连...");
                }
            }
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        private static void Receive()
        {
            while (true)
            {
                if (serialPort.IsOpen)
                {
                    // 读取串口数据
                    int byteSize = serialPort.BytesToRead;
                    if (byteSize > 0)
                    {
                        // 10毫秒内算同一条内容
                        Thread.Sleep(10);
                        while (serialPort.BytesToRead > byteSize)
                        {
                            byteSize = serialPort.BytesToRead;
                            Thread.Sleep(10);
                        }
                        byte[] buffer = new byte[byteSize];
                        serialPort.Read(buffer, 0, byteSize);
                        Console.WriteLine("收到消息：" + Bytes2Hex(buffer));
                    }
                }
                Thread.Sleep(10);
            }
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
            if (serialPort.IsOpen)
            {
                serialPort.Write(data, 0, data.Length);
                Console.WriteLine("发送消息：" + Bytes2Hex(data));
            }
            else
            {
                Console.WriteLine("发送消息 串口已断开，发送失败！");
            }
        }

        public static void Main(string[] args)
        {
            // 串口初始化
            new Thread(t =>
            {
                while (true)
                {
                    SerialPortInit();
                    Thread.Sleep(10000);
                }
            })
            {
                IsBackground = true
            }.Start();
            // 接收消息
            new Thread(t =>
            {
                while (true)
                {
                    Receive();
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
