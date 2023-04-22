using System.Collections.Generic;
using System;
using System.IO;
using ConsoleDemo.Properties;
using System.Drawing.Imaging;
using ConsoleDemo.Model;

namespace ConsoleDemo.Util
{

    /// <summary>
    /// 通用工具
    /// </summary>
    public class Utils
    {
        private static int index = 0;
        private static readonly MemoryStream appleStream = new MemoryStream();
        private static readonly MemoryStream bananaStream = new MemoryStream();
        private static readonly MemoryStream grapeStream = new MemoryStream();
        private static readonly MemoryStream pearStream = new MemoryStream();
        private static readonly MemoryStream tangerineStream = new MemoryStream();
        private static readonly MemoryStream watermelonStream = new MemoryStream();

        /// <summary>
        /// 静态初始化
        /// </summary>
        static Utils()
        {
            Resources.apple.Save(appleStream, ImageFormat.Png);
            Resources.banana.Save(bananaStream, ImageFormat.Png);
            Resources.grape.Save(grapeStream, ImageFormat.Png);
            Resources.pear.Save(pearStream, ImageFormat.Png);
            Resources.tangerine.Save(tangerineStream, ImageFormat.Png);
            Resources.watermelon.Save(watermelonStream, ImageFormat.Png);
        }

        /// <summary>
        /// 遍历客户端
        /// </summary>
        /// <param name="clientList">SocketClient[]</param>
        public static string IterateClient(SocketClient[] clientList)
        {
            string msg = "\n\n----- 遍历客户端 开始 -----\n";
            msg += "ip\t\t | 开始时间\t | 结束时间\t | 连接时长(分钟)\n";
            var now = DateTime.Now;
            foreach (var client in clientList)
            {
                msg += client.Ip + "\t | " + client.Online.ToString("HH:mm:ss.fff") + "\t | ";
                // 在线
                if (client.Offline == DateTime.MinValue)
                {
                    msg += "-\t\t | " + Convert.ToDouble(now.Subtract(client.Online).TotalMinutes).ToString("0.00") + "\n";
                }
                // 离线
                else
                {
                    msg += client.Offline.ToString("HH:mm:ss.fff") + "\t | " + Convert.ToDouble(client.Offline.Subtract(client.Online).TotalMinutes).ToString("0.00") + "\n";
                }
            }
            msg += "----- 遍历客户端 结束 -----\n\n";
            return msg;
        }

        /// <summary>
        /// 遍历socket客户端
        /// </summary>
        /// <param name="socketClientList">List SocketClient</param>
        public static void IterateSocketClient(List<SocketClient> socketClientList)
        {
            string msg = "\n----- 遍历socket客户端 开始 -----\n";
            msg += "ip\t\t | 开始时间\t | 结束时间\t | 连接时长(分钟)\n";
            var now = DateTime.Now;
            foreach (var client in socketClientList.ToArray())
            {
                msg += client.Ip + "\t | " + client.Online.ToString("HH:mm:ss.fff") + "\t | ";
                // 在线
                if (client.Offline == DateTime.MinValue)
                {
                    msg += "-\t\t | " + Convert.ToDouble(now.Subtract(client.Online).TotalMinutes).ToString("0.00") + "\n";
                }
                // 离线
                else
                {
                    msg += client.Offline.ToString("HH:mm:ss.fff") + "\t | " + Convert.ToDouble(client.Offline.Subtract(client.Online).TotalMinutes).ToString("0.00") + "\n";
                }
            }
            msg += "----- 遍历socket客户端 结束 -----\n";
            Console.WriteLine(msg);
        }

        /// <summary>
        /// 遍历socket客户端2
        /// </summary>
        /// <param name="socketClientList">List SocketClient2</param>
        public static void IterateSocketClient2(List<SocketClient2> socketClientList)
        {
            string msg = "\n----- 遍历socket客户端2 开始 -----\n";
            msg += "ip\t\t | 开始时间\t | 结束时间\t | 连接时长(分钟)\n";
            var now = DateTime.Now;
            foreach (var client in socketClientList.ToArray())
            {
                msg += client.Ip + "\t | " + client.Online.ToString("HH:mm:ss.fff") + "\t | ";
                // 在线
                if (client.Offline == DateTime.MinValue)
                {
                    msg += "-\t\t | " + Convert.ToDouble(now.Subtract(client.Online).TotalMinutes).ToString("0.00") + "\n";
                }
                // 离线
                else
                {
                    msg += client.Offline.ToString("HH:mm:ss.fff") + "\t | " + Convert.ToDouble(client.Offline.Subtract(client.Online).TotalMinutes).ToString("0.00") + "\n";
                }
            }
            msg += "----- 遍历socket客户端2 结束 -----\n";
            Console.WriteLine(msg);
        }

        /// <summary>
        /// 遍历webSocket客户端
        /// </summary>
        /// <param name="socketClientList">List WebSocketClient</param>
        public static void IterateWebSocketClient(List<WebSocketClient> webSocketClientList)
        {
            string msg = "\n----- 遍历webSocket客户端 开始 -----\n";
            msg += "ip\t\t | 开始时间\t | 结束时间\t | 连接时长(分钟)\n";
            var now = DateTime.Now;
            foreach (var client in webSocketClientList.ToArray())
            {
                msg += client.Ip + "\t | " + client.Online.ToString("HH:mm:ss.fff") + "\t | ";
                // 在线
                if (client.Offline == DateTime.MinValue)
                {
                    msg += "-\t\t | " + Convert.ToDouble(now.Subtract(client.Online).TotalMinutes).ToString("0.00") + "\n";
                }
                // 离线
                else
                {
                    msg += client.Offline.ToString("HH:mm:ss.fff") + "\t | " + Convert.ToDouble(client.Offline.Subtract(client.Online).TotalMinutes).ToString("0.00") + "\n";
                }
            }
            msg += "----- 遍历webSocket客户端 结束 -----\n";
            Console.WriteLine(msg);
        }

        /// <summary>
        /// 获取发送字符串
        /// </summary>
        /// <returns>字符串</returns>
        public static string GetSendString()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        /// <summary>
        /// 获取发送MemoryStream
        /// </summary>
        /// <returns>MemoryStream</returns>
        public static MemoryStream GetSendMemoryStream()
        {
            if (index == 6)
            {
                index = 0;
            }
            return GetSendMemoryStream(index++);
        }

        /// <summary>
        /// 获取发送MemoryStream
        /// </summary>
        /// <param name="index">index</param>
        /// <returns>MemoryStream</returns>
        public static MemoryStream GetSendMemoryStream(int index)
        {
            switch (index)
            {
                default:
                case 0: return appleStream;
                case 1: return bananaStream;
                case 2: return grapeStream;
                case 3: return pearStream;
                case 4: return tangerineStream;
                case 5: return watermelonStream;
            }
        }

    }
}
