using ConsoleDemo.Properties;
using ConsoleDemo.Service;
using ConsoleDemo.Util;
using System;
using System.Collections.Specialized;
using System.Drawing.Imaging;
using System.IO;
using System.Net;

namespace ConsoleDemo.Test
{

    /// <summary>
    /// http服务2(使用Socket)测试
    /// </summary>
    public class HttpService2Test
    {

        private static readonly HttpService2 httpService = new HttpService2();
        private static readonly IPAddress ip = IPAddress.Parse("127.0.0.1");
        private static readonly int port = 8082;

        /// <summary>
        /// 图标byte[]
        /// </summary>
        private static byte[] httpIconHeaderBytes;

        /// <summary>
        /// 启动
        /// </summary>
        public static void Start()
        {
            if (httpService.Start(ip, port, ResponseCallback))
            {
                // 初始化图标
                MemoryStream stream = new MemoryStream();
                Resources.favicon.Save(stream);
                httpIconHeaderBytes = HttpService2.GetBytes(HttpService2.icoHeaderBytes, stream);
            }
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public static void Close()
        {
            httpService.Close();
        }

        /// <summary>
        /// 响应回调函数
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="param">参数</param>
        /// <returns>返回值</returns>
        private static byte[] ResponseCallback(string path, NameValueCollection param)
        {
            byte[] data;
            switch (path)
            {
                // 纯文本
                case "/":
                default:
                    {
                        string content = "随机UUID：" + Guid.NewGuid();
                        data = HttpService2.GetBytes(HttpService2.plainHeader, content);
                        break;
                    }
                // 图标
                case "/favicon.ico":
                    {
                        data = httpIconHeaderBytes;
                        break;
                    }
                // PNG图片
                case "/香蕉.PNG":
                    {
                        MemoryStream stream = new MemoryStream();
                        Resources.banana.Save(stream, ImageFormat.Png);
                        data = HttpService2.GetBytes(HttpService2.pngHeaderBytes, stream);
                        stream.Dispose();
                        break;
                    }
                // 网页
                case "/socket.html":
                    {
                        data = HttpService2.GetBytes(HttpService2.htmlHeader, Resources.socket);
                        break;
                    }
                case "/webSocket.html":
                    {
                        data = HttpService2.GetBytes(HttpService2.htmlHeader, Resources.webSocket);
                        break;
                    }
                case "/webSocket2.html":
                    {
                        data = HttpService2.GetBytes(HttpService2.htmlHeader, Resources.webSocket2);
                        break;
                    }
                case "/webSocket3.html":
                    {
                        data = HttpService2.GetBytes(HttpService2.htmlHeader, Resources.webSocket3);
                        break;
                    }
                // JSON
                case "/json":
                    {
                        string json = "{\"uuid\":\"" + Guid.NewGuid() + "\"}";
                        data = HttpService2.GetBytes(HttpService2.jsonHeader, json);
                        break;
                    }
                // 带参数
                case "/fruit":
                    {
                        MemoryStream stream;
                        int index = -1;
                        if (param != null && param["index"] != null)
                        {
                            try
                            {
                                index = int.Parse(param["index"]);
                            }
                            catch
                            {
                            }
                        }
                        if (index < 0 || index > 6)
                        {
                            stream = Utils.GetSendMemoryStream();
                        }
                        else
                        {
                            stream = Utils.GetSendMemoryStream(index);
                        }
                        data = HttpService2.GetBytes(HttpService2.pngHeaderBytes, stream);
                        break;
                    }
            }
            return data;
        }

    }
}
