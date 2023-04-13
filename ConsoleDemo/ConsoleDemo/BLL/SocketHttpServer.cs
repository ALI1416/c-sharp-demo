using ConsoleDemo.Model;
using ConsoleDemo.Properties;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web;

namespace ConsoleDemo.BLL
{

    /// <summary>
    /// socket模拟http服务器
    /// </summary>
    public class SocketHttpServer
    {
        /// <summary>
        /// socket服务端
        /// </summary>
        private static Model.SocketServer socketServer;

        private static readonly string httpHtmlHeader = "HTTP/1.0 200 OK\nContent-Type: text/html;charset=utf-8\nConnection: close\n\n";
        private static readonly string httpJsonHeader = "HTTP/1.0 200 OK\nContent-Type: application/json;charset=utf-8\nConnection: close\n\n";
        private static byte[] httpIconHeaderBytes;

        /// <summary>
        /// 启动
        /// </summary>
        public static void Start()
        {
            // 图标
            byte[] bytes = Encoding.UTF8.GetBytes("HTTP/1.0 200 OK\nContent-Type: image/x-icon\nConnection: close\n\n");
            MemoryStream faviconStream = new MemoryStream();
            Resources.favicon.Save(faviconStream);
            httpIconHeaderBytes = new byte[bytes.Length + faviconStream.Length];
            bytes.CopyTo(httpIconHeaderBytes, 0);
            faviconStream.ToArray().CopyTo(httpIconHeaderBytes, bytes.Length);
            try
            {
                // 新建socket服务器
                socketServer = new Model.SocketServer();
                // 指定URI
                socketServer.Server.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8083));
                // 设置监听数量
                socketServer.Server.Listen(10);
                // 异步监听客户端请求
                socketServer.Server.BeginAccept(SocketHandle, null);
            }
            // 端口号冲突、未知错误
            catch
            {
                socketServer.Close();
                Console.WriteLine("socket模拟http服务器端口号冲突 或 未知错误");
                return;
            }
            Console.WriteLine("socket模拟http服务器已启动");
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public static void Close()
        {
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
                socketServer.Server.BeginAccept(SocketHandle, null);
            }
            // 主动关闭socket服务器
            catch
            {
                Console.WriteLine("主动关闭socket模拟http服务器");
                return;
            }
            // 客户端上线
            ClientOnline(socketServer.Server.EndAccept(ar));
        }

        /// <summary>
        /// 客户端上线
        /// </summary>
        /// <param name="client">客户端</param>
        private static void ClientOnline(Socket client)
        {
            HttpSocketClient httpSocketClient = null;
            try
            {
                httpSocketClient = new HttpSocketClient(client);
                // 设置超时10秒
                client.SendTimeout = 10000;
                // 接收消息
                client.BeginReceive(httpSocketClient.Buffer, 0, httpSocketClient.Buffer.Length, SocketFlags.None, Recevice, httpSocketClient);
            }
            catch
            {
                if (httpSocketClient != null)
                {
                    httpSocketClient.Close();
                }
                return;
            }
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="ar">IAsyncResult</param>
        private static void Recevice(IAsyncResult ar)
        {
            // 获取当前客户端
            HttpSocketClient httpSocketClient = ar.AsyncState as HttpSocketClient;
            try
            {
                // 获取接收数据长度
                int length = httpSocketClient.Client.EndReceive(ar);
                // 客户端主动断开连接时，会发送0字节消息
                if (length == 0)
                {
                    httpSocketClient.Close();
                    return;
                }
                // 解码消息
                string msg = Encoding.UTF8.GetString(httpSocketClient.Buffer, 0, length);
                // request处理
                RequestHandle(httpSocketClient, msg);
                // 关闭连接
                httpSocketClient.Close();
            }
            // 超时后失去连接、未知错误
            catch
            {
                httpSocketClient.Close();
                return;
            }
        }

        /// <summary>
        /// request处理
        /// </summary>
        /// <param name="httpSocketClient">HttpSocketClient</param>
        /// <param name="request">request字符串</param>
        private static void RequestHandle(HttpSocketClient httpSocketClient, string request)
        {
            string[] pathAndQuery = HttpUtility.UrlDecode(request.Substring(4, request.IndexOf('\r') - 13)).Split('?');
            string path = pathAndQuery[0];
            NameValueCollection param = null;
            if (pathAndQuery.Length > 1)
            {
                param = HttpUtility.ParseQueryString(pathAndQuery[1]);
            }
            byte[] data;
            switch (path)
            {
                // 网页
                default:
                    {
                        data = Encoding.UTF8.GetBytes(httpHtmlHeader + Resources.socket);
                        break;
                    }
                // 图标
                case "/favicon.ico":
                    {
                        data = httpIconHeaderBytes;
                        break;
                    }
                // JSON
                case "/json":
                    {
                        string json = "{";
                        if (param == null)
                        {
                            json += "}";
                        }
                        else
                        {
                            foreach (string key in param.Keys)
                            {
                                json += "\"" + key + "\":\"" + param[key].ToString() + "\",";
                            }
                            if (json.Length > 1)
                            {
                                json = json.Substring(0, json.Length - 1);
                            }
                            json += "}";
                        }
                        data = Encoding.UTF8.GetBytes(httpJsonHeader + json);
                        break;
                    }
            }
            Send(httpSocketClient, data);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="httpSocketClient">HttpSocketClient</param>
        /// <param name="data">byte[]</param>
        private static void Send(HttpSocketClient httpSocketClient, byte[] data)
        {
            try
            {
                // 发送消息
                httpSocketClient.Client.BeginSend(data, 0, data.Length, SocketFlags.None, null, null);
            }
            // 未知错误
            catch
            {
                httpSocketClient.Close();
                return;
            }
        }

    }
}
