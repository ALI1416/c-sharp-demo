using ConsoleDemo.Properties;
using ConsoleDemo.Service;
using ConsoleDemo.Util;
using System.Drawing.Imaging;
using System.IO;
using System;
using System.Net;
using System.Text;
using log4net;

namespace ConsoleDemo.Test
{

    /// <summary>
    /// http服务(使用HttpListener)测试
    /// </summary>
    public class HttpServiceTest
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(HttpServiceTest));

        private static readonly HttpService httpService = new HttpService();
        private static readonly IPAddress ip = IPAddress.Parse("127.0.0.1");
        private static readonly int port = 8080;
        private static readonly string account = "admin";
        private static readonly string password = "123456";

        /// <summary>
        /// 启动
        /// </summary>
        public static void Start()
        {
            httpService.Start(ip, port, ResponseCallback);
        }

        /// <summary>
        /// 启动2
        /// </summary>
        public static void Start2()
        {
            httpService.Start(ip, port, account, password, ServiceCloseCallback, ResponseCallback);
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public static void Close()
        {
            httpService.Close();
        }

        /// <summary>
        /// 服务器关闭回调函数
        /// </summary>
        private static void ServiceCloseCallback()
        {
            log.Warn("服务器关闭回调函数");
        }

        /// <summary>
        /// 响应回调函数
        /// </summary>
        /// <param name="request">HttpListenerRequest</param>
        /// <param name="response">HttpListenerResponse</param>
        private static byte[] ResponseCallback(HttpListenerRequest request, HttpListenerResponse response)
        {
            byte[] data;
            // 设置response状态码：请求成功
            response.StatusCode = (int)HttpStatusCode.OK;
            switch (request.Url.LocalPath)
            {
                // 纯文本
                case "/":
                default:
                    {
                        // 设置response类型：UTF-8纯文本
                        response.ContentType = "text/plain;charset=UTF-8";
                        // 数据
                        string respMsg = "随机UUID：" + Guid.NewGuid();
                        data = Encoding.UTF8.GetBytes(respMsg);
                        break;
                    }
                // 图标
                case "/favicon.ico":
                    {
                        // 设置response类型：图标
                        response.ContentType = "image/x-icon";
                        MemoryStream faviconStream = new MemoryStream();
                        Resources.favicon.Save(faviconStream);
                        data = faviconStream.ToArray();
                        break;
                    }
                // PNG图片
                case "/香蕉.PNG":
                    {
                        // 设置response类型：PNG图片
                        response.ContentType = "image/png";
                        MemoryStream stream = new MemoryStream();
                        Resources.banana.Save(stream, ImageFormat.Png);
                        data = stream.ToArray();
                        stream.Dispose();
                        break;
                    }
                // 网页
                case "/socket.html":
                    {
                        // 设置response类型：网页
                        response.ContentType = "text/html;charset=UTF-8";
                        data = Encoding.UTF8.GetBytes(Resources.socket);
                        break;
                    }
                case "/webSocket.html":
                    {
                        response.ContentType = "text/html;charset=UTF-8";
                        data = Encoding.UTF8.GetBytes(Resources.webSocket);
                        break;
                    }
                case "/webSocket2.html":
                    {
                        response.ContentType = "text/html;charset=UTF-8";
                        data = Encoding.UTF8.GetBytes(Resources.webSocket2);
                        break;
                    }
                case "/webSocket3.html":
                    {
                        response.ContentType = "text/html;charset=UTF-8";
                        data = Encoding.UTF8.GetBytes(Resources.webSocket3);
                        break;
                    }
                // JSON
                case "/json":
                    {
                        // 设置response类型：JSON
                        response.ContentType = "application/json;charset=UTF-8";
                        string json = "{\"uuid\":\"" + Guid.NewGuid() + "\"}";
                        data = Encoding.UTF8.GetBytes(json);
                        break;
                    }
                // 带参数
                case "/fruit":
                    {
                        MemoryStream stream;
                        int index = -1;
                        try
                        {
                            index = int.Parse(request.QueryString.GetValues("index")[0]);
                        }
                        catch
                        {

                        }
                        if (index < 0 || index > 6)
                        {
                            stream = Utils.GetSendMemoryStream();
                        }
                        else
                        {
                            stream = Utils.GetSendMemoryStream(index);
                        }
                        response.ContentType = "image/png";
                        data = stream.ToArray();
                        break;
                    }
            }
            return data;
        }

    }
}
