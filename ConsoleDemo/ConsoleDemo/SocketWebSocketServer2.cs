using ConsoleDemo.Properties;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace ConsoleDemo
{
    /// <summary>
    /// socket模拟webSocket服务器2
    /// </summary>
    internal class SocketWebSocketServer2
    {
        /// <summary>
        /// 正在运行
        /// </summary>
        private static bool isRunning = false;
        /// <summary>
        /// socket服务器
        /// </summary>
        private static Socket socketServer;
        /// <summary>
        /// socket客户端
        /// </summary>
        private static readonly List<Socket> socketClient = new List<Socket>();
        /// <summary>
        /// socket客户端历史
        /// </summary>
        private static readonly Dictionary<int, SocketHistory> socketClientHistory = new Dictionary<int, SocketHistory>();
        /// <summary>
        /// 接收数据缓冲区
        /// </summary>
        private static readonly byte[] buffer = new byte[1024];

        /// <summary>
        /// 启动
        /// </summary>
        public static void Start()
        {
            try
            {
                // 新建socket服务器
                socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // 指定URI
                socketServer.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8087));
                // 设置监听数量
                socketServer.Listen(10);
                // 异步监听客户端请求
                socketServer.BeginAccept(SocketHandle, null);
            }
            // 端口号冲突、未知错误
            catch
            {
                socketServer.Close();
                Console.WriteLine("socket模拟webSocket2服务器端口号冲突 或 未知错误");
                return;
            }
            isRunning = true;
            Console.WriteLine("socket模拟webSocket2服务器已启动");
            new Thread(t =>
            {
                // 定时向客户端发送消息
                IntervalSend();
            })
            {
                IsBackground = true
            }.Start();
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public static void Close()
        {
            isRunning = false;
            foreach (Socket client in socketClient.ToArray())
            {
                ClientOffline(client);
            }
            socketServer.Close();
        }

        /// <summary>
        /// socket处理
        /// </summary>
        /// <param name="ar">IAsyncResult</param>
        private static void SocketHandle(IAsyncResult ar)
        {
            try
            {
                // 继续异步监听客户端请求
                socketServer.BeginAccept(SocketHandle, null);
            }
            // 主动关闭socket服务器
            catch
            {
                Console.WriteLine("主动关闭socket2模拟webSocket服务器");
                return;
            }
            // 客户端上线
            ClientOnline(socketServer.EndAccept(ar));
        }

        /// <summary>
        /// 客户端上线
        /// </summary>
        /// <param name="client">客户端</param>
        private static void ClientOnline(Socket client)
        {
            // 已存在
            if (socketClient.Contains(client))
            {
                return;
            }
            try
            {
                // 获取IP地址
                string ip = client.RemoteEndPoint.ToString();
                // 设置超时10秒
                client.SendTimeout = 10000;
                // 接收消息
                client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, Recevice, client);
                socketClient.Add(client);
                socketClientHistory.Add(client.GetHashCode(), new SocketHistory(ip, DateTime.Now));
                Console.WriteLine("客户端 " + ip + " 已上线");
                SocketHistory.Iterate(socketClientHistory);
            }
            catch
            {
                client.Close();
                return;
            }
        }

        /// <summary>
        /// 客户端下线
        /// </summary>
        /// <param name="client">客户端</param>
        private static void ClientOffline(Socket client)
        {
            // 不存在
            if (!socketClient.Contains(client))
            {
                return;
            }
            client.Close();
            socketClient.Remove(client);
            // 获取该客户端
            if (socketClientHistory.TryGetValue(client.GetHashCode(), out SocketHistory history))
            {
                // 没有下线的客户端才可以下线
                if (history.Offline == DateTime.MinValue)
                {
                    history.Offline = DateTime.Now;
                    Console.WriteLine("客户端 " + history.Ip + " 已下线");
                    SocketHistory.Iterate(socketClientHistory);
                }
            }
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="ar">IAsyncResult</param>
        private static void Recevice(IAsyncResult ar)
        {
            // 获取当前客户端
            Socket client = ar.AsyncState as Socket;
            try
            {
                // 获取接收数据长度
                int length = client.EndReceive(ar);
                // 客户端主动断开连接时，会发送0字节消息
                if (length == 0)
                {
                    ClientOffline(client);
                    return;
                }
                // 首次连接
                if (buffer[0] == 71)
                {
                    // 解码消息
                    string msg = Encoding.UTF8.GetString(buffer, 0, length);
                    // 判断是否符合webSocket报文格式
                    if (msg.Contains("Sec-WebSocket-Key"))
                    {
                        SendRaw(client, HandShake(msg));
                        // 继续接收消息
                        client.BeginReceive(buffer, 0, length, SocketFlags.None, Recevice, client);
                    }
                    // 不符合格式，关闭连接
                    else
                    {
                        ClientOffline(client);
                        return;
                    }
                }
                else
                {
                    // 继续接收消息
                    client.BeginReceive(buffer, 0, length, SocketFlags.None, Recevice, client);
                    string data = DecodeDataString(buffer, length);
                    // 客户端关闭连接
                    if (data == null)
                    {
                        ClientOffline(client);
                        return;
                    }
                    else
                    {
                        Console.WriteLine("收到客户端 " + client.RemoteEndPoint + " 消息：" + data);
                    }
                }
            }
            // 超时后失去连接、未知错误
            catch
            {
                ClientOffline(client);
                return;
            }
        }

        /// <summary>
        /// 握手
        /// </summary>
        /// <param name="msg">握手消息</param>
        /// <returns>byte[]</returns>
        private static byte[] HandShake(string msg)
        {
            string key;
            var reader = new StringReader(msg);
            while (true)
            {
                string line = reader.ReadLine();
                if (line.Contains("Sec-WebSocket-Key"))
                {
                    key = line.Substring(19);
                    break;
                }
            }
            byte[] secret = SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"));
            string secretKey = Convert.ToBase64String(secret);
            string data = "HTTP/1.1 101 Switching Protocols\nUpgrade: websocket\nConnection: Upgrade\nSec-WebSocket-Accept: " + secretKey + "\n\n";
            return Encoding.UTF8.GetBytes(data);
        }

        /// <summary>
        /// 解码数据获取字符串(不处理超过1帧的数据)
        /// </summary>
        /// <param name="msg">数据</param>
        /// <param name="length">数据长度</param>
        /// <returns>字符串</returns>
        private static string DecodeDataString(byte[] msg, int length)
        {
            byte[] data = DecodeData(msg, length);
            if (data == null)
            {
                return null;
            }
            else
            {
                return Encoding.UTF8.GetString(data);
            }
        }

        /// <summary>
        /// 解码数据(不处理超过1帧的数据)
        /// </summary>
        /// <param name="msg">数据</param>
        /// <param name="length">数据长度</param>
        /// <returns>byte[]</returns>
        private static byte[] DecodeData(byte[] msg, int length)
        {
            // 长度太短、有后续帧、不包含mask
            if (length < 6 || msg[0] >> 7 != 1 || msg[1] >> 7 != 1)
            {
                return new byte[0];
            }
            // 检查opcode是否为关闭连接
            if ((msg[0] & 0x8) == 8)
            {
                return null;
            }
            // mask所在字节数
            int maskByte = 2;
            // 获取数据长度
            int len = msg[1] & 0x7F;
            if (len == 126)
            {
                maskByte = 4;
            }
            else if (len == 127)
            {
                maskByte = 12;
            }
            // mask
            byte[] mask = new byte[4];
            mask[0] = msg[maskByte];
            mask[1] = msg[maskByte + 1];
            mask[2] = msg[maskByte + 2];
            mask[3] = msg[maskByte + 3];
            // 解码前的数据
            byte[] data = new byte[length - maskByte - 4];
            Buffer.BlockCopy(msg, maskByte + 4, data, 0, data.Length);
            // 解码数据
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(data[i] ^ mask[i % 4]);
            }
            return data;
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="msg">byte[]</param>
        private static void Send(Socket client, byte[] data)
        {
            SendRaw(client, CodedData(data, false));
        }

        /// <summary>
        /// 编码数据
        /// </summary>
        /// <param name="msg">数据</param>
        /// <param name="text">是否为文本数据</param>
        /// <returns>byte[]</returns>
        private static byte[] CodedData(byte[] msg, bool text)
        {
            byte[] data;
            int length = msg.Length;
            if (length < 126)
            {
                data = new byte[length + 2];
                data[0] = (byte)(text ? 0x81 : 0x82);
                data[1] = (byte)length;
                msg.CopyTo(data, 2);
            }
            else if (length < 0xFFFF)
            {
                data = new byte[length + 4];
                data[0] = (byte)(text ? 0x81 : 0x82);
                data[1] = 126;
                data[2] = (byte)(length >> 8 & 0xFF);
                data[3] = (byte)(length & 0xFF);
                msg.CopyTo(data, 4);
            }
            else
            {
                data = new byte[length + 10];
                data[0] = (byte)(text ? 0x81 : 0x82);
                data[1] = 127;
                data[2] = 0;
                data[3] = 0;
                data[4] = 0;
                data[5] = 0;
                data[6] = (byte)(length >> 24 & 0xFF);
                data[7] = (byte)(length >> 16 & 0xFF);
                data[8] = (byte)(length >> 8 & 0xFF);
                data[9] = (byte)(length & 0xFF);
                msg.CopyTo(data, 10);
            }
            return data;
        }

        /// <summary>
        /// 发送原始消息
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="data">byte[]</param>
        private static void SendRaw(Socket client, byte[] data)
        {
            try
            {
                // 发送消息
                client.BeginSend(data, 0, data.Length, SocketFlags.None, asyncResult =>
                {
                    try
                    {
                        client.EndSend(asyncResult);
                    }
                    // 已失去连接
                    catch
                    {
                        ClientOffline(client);
                        return;
                    }
                }, null);
            }
            // 未知错误
            catch
            {
                ClientOffline(client);
                return;
            }
        }

        /// <summary>
        /// 定时向客户端发送消息
        /// </summary>
        private static void IntervalSend()
        {
            int index = 0;
            while (isRunning)
            {
                var socketClientArray = socketClient.ToArray();
                if (socketClientArray.Length != 0)
                {
                    MemoryStream stream = new MemoryStream();
                    switch (index)
                    {
                        case 0:
                        default:
                            {
                                Resources.apple.Save(stream, ImageFormat.Png);
                                break;
                            }
                        case 1:
                            {
                                Resources.banana.Save(stream, ImageFormat.Png);
                                break;
                            }
                        case 2:
                            {
                                Resources.grape.Save(stream, ImageFormat.Png);
                                break;
                            }
                        case 3:
                            {
                                Resources.pear.Save(stream, ImageFormat.Png);
                                break;
                            }
                        case 4:
                            {
                                Resources.tangerine.Save(stream, ImageFormat.Png);
                                break;
                            }
                        case 5:
                            {
                                Resources.watermelon.Save(stream, ImageFormat.Png);
                                break;
                            }
                    }
                    foreach (Socket client in socketClientArray)
                    {
                        Send(client, stream.ToArray());
                        Console.WriteLine("向客户端 " + client.RemoteEndPoint + " 发送 " + stream.Length + " 字节的消息");
                    }
                    if (++index > 5)
                    {
                        index = 0;
                    }
                }
                Thread.Sleep(1000);
            }
        }

    }
}
