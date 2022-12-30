using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleDemo
{
    /// <summary>
    /// socket客户端历史
    /// </summary>
    public class SocketHistory
    {
        /// <summary>
        /// IP地址
        /// </summary>
        public string Ip { set; get; }
        /// <summary>
        /// 上线时间
        /// </summary>
        public DateTime Online { set; get; }
        /// <summary>
        /// 下线时间(`DateTime.MinValue`表示未下线)
        /// </summary>
        public DateTime Offline { set; get; }

        public SocketHistory(string ip, DateTime online)
        {
            Ip = ip;
            Online = online;
        }

        /// <summary>
        /// 遍历socket客户端历史
        /// </summary>
        public static void Iterate(Dictionary<int, SocketHistory> socketClientHistory)
        {
            Console.WriteLine("\n----- 遍历socket客户端历史 开始 -----");
            Console.WriteLine("ip\t\t | 开始时间\t | 结束时间\t | 连接时长(分钟)");
            var now = DateTime.Now;
            foreach (var history in socketClientHistory.ToArray())
            {
                var value = history.Value;
                string msg = value.Ip + "\t | " + value.Online.ToString("HH:mm:ss.fff") + "\t | ";
                if (value.Offline == DateTime.MinValue)
                {
                    msg += "-\t\t | " + Convert.ToDouble(now.Subtract(value.Online).TotalMinutes).ToString("0.00");
                }
                else
                {
                    msg += value.Offline.ToString("HH:mm:ss.fff") + "\t | " + Convert.ToDouble(value.Offline.Subtract(value.Online).TotalMinutes).ToString("0.00");
                }
                Console.WriteLine(msg);
            }
            Console.WriteLine("----- 遍历socket客户端历史 结束 -----\n");
        }

    }
}
