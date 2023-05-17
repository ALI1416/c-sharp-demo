using System;
using System.IO;
using ConsoleDemo.Properties;
using System.Drawing.Imaging;
using ConsoleDemo.Model;
using ConsoleDemo.Service;
using System.Collections.Generic;

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
            string msg = "\n\n----- 遍历 客户端 开始 -----\n";
            msg += "ip\t\t | 上线时间\t | 下线时间\t | 连接时长(分钟)\n";
            DateTime now = DateTime.Now;
            foreach (SocketClient client in clientList)
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
            msg += "----- 遍历 客户端 结束 -----\n";
            return msg;
        }

        /// <summary>
        /// 遍历客户端
        /// </summary>
        /// <param name="clientList">WebSocketClient[]</param>
        public static string IterateClient(WebSocketClient[] clientList)
        {
            string msg = "\n\n----- 遍历 客户端 开始 -----\n";
            msg += "ip\t\t | 上线时间\t | 下线时间\t | 连接时长(分钟)\n";
            DateTime now = DateTime.Now;
            foreach (WebSocketClient client in clientList)
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
            msg += "----- 遍历 客户端 结束 -----\n";
            return msg;
        }

        /// <summary>
        /// 遍历
        /// </summary>
        /// <param name="webSocketService">WebSocketService3</param>
        public static string Iterate(WebSocketService3 webSocketService)
        {
            SocketClient2[] clientList = webSocketService.ClientList();
            string msg = "\n\n----- 遍历 客户端 开始 -----\n";
            msg += " 状态\t | ip\t\t\t | 上线时间\t | 下线时间\t | 连接时长(分钟)\t | 每秒帧数(帧/秒)\t | 传输速度(Mb/秒)\t | 数据总量(Mb)\n";
            DateTime now = DateTime.Now;
            foreach (SocketClient2 client in clientList)
            {
                // 在线
                if (client.Offline == DateTime.MinValue)
                {
                    // 状态
                    msg += " 在线\t | ";
                    // IP地址、上线时间
                    msg += client.Ip + "\t | "
                        + client.Online.ToString("HH:mm:ss.fff") + "\t | ";
                    // 下线时间、连接时长
                    msg += "-\t\t | " + Convert.ToDouble(now.Subtract(client.Online).TotalMinutes).ToString("0.00");
                    // 每秒帧数、传输速度
                    msg += "\t\t\t | " + (client.FrameAvg / 100f).ToString("0.00") + "\t\t\t | " + (client.ByteAvg / 1048576f).ToString("0.00") + "\t\t\t | ";
                    // 数据总量
                    msg += (client.ByteCount / 1048576f).ToString("0.00") + "\n";
                }
                // 离线
                else
                {
                    // 状态
                    msg += " -\t | ";
                    // IP地址、上线时间
                    msg += client.Ip + "\t | " + client.Online.ToString("HH:mm:ss.fff") + "\t | ";
                    // 下线时间、连接时长
                    msg += client.Offline.ToString("HH:mm:ss.fff") + "\t | " + Convert.ToDouble(client.Offline.Subtract(client.Online).TotalMinutes).ToString("0.00");
                    // 每秒帧数、传输速度
                    msg += "\t\t\t | -\t\t\t | -\t\t\t | ";
                    // 数据总量
                    msg += (client.ByteCount / 1048576f).ToString("0.00") + "\n";
                }
            }
            List<SocketClient2> clientOnlineList = webSocketService.ClientOnlineList();
            msg += " 当前在线用户数量： " + clientOnlineList.Count
                + "\t|\t累计访问用户数量： " + clientList.Length
                + "\t|\t当前帧率(帧/秒)： " + (clientOnlineList.Count > 0 ? (webSocketService.Server.FrameAvg / 100f).ToString("0.00") : "0.00")
                + "\t|\t传输数据总量(Mb)： " + (webSocketService.Server.ByteCount / 1048576f).ToString("0.00")
                + "\n";
            msg += "----- 遍历 客户端 结束 -----\n";
            return msg;
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
