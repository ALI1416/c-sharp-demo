using ConsoleDemo.Properties;
using ConsoleDemo.Util;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;

namespace ConsoleDemo
{
    /// <summary>
    /// http服务器
    /// </summary>
    internal class HttpServer
    {
        /// <summary>
        /// http服务器
        /// </summary>
        private static HttpListener httpServer;

        /// <summary>
        /// 启动
        /// </summary>
        public static void Start()
        {
            // 新建http服务器
            httpServer = new HttpListener
            {
                // 忽视客户端写入异常
                IgnoreWriteExceptions = true
            };
            // 指定URI
            httpServer.Prefixes.Add("http://127.0.0.1:8080/");
            try
            {
                // 开启http服务器
                httpServer.Start();
            }
            // 端口号冲突
            catch
            {
                Console.WriteLine("http服务器端口号冲突");
                return;
            }
            // 异步监听客户端请求
            httpServer.BeginGetContext(HttpHandle, null);
            Console.WriteLine("http服务器已启动");
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public static void Close()
        {
            httpServer.Close();
        }

        /// <summary>
        /// http处理
        /// </summary>
        /// <param name="ar">IAsyncResult</param>
        private static void HttpHandle(IAsyncResult ar)
        {
            try
            {
                // 继续异步监听客户端请求
                httpServer.BeginGetContext(HttpHandle, null);
            }
            // 主动关闭http服务器
            catch
            {
                Console.WriteLine("主动关闭http服务器");
                return;
            }
            // 获取context对象
            var context = httpServer.EndGetContext(ar);
            var request = context.Request;
            var response = context.Response;
            // 打印request信息：请求方式，URL
            Console.WriteLine("Method:" + request.HttpMethod + " ,URL:" + request.Url.PathAndQuery);
            // 账号密码验证
            //if (!Authorization("admin", "123456", request, response))
            //{
            //    return;
            //}
            // 响应
            Response(request, response);
        }

        /// <summary>
        /// 账号密码验证
        /// </summary>
        /// <param name="account">账号</param>
        /// <param name="password">密码</param>
        /// <param name="request">HttpListenerRequest</param>
        /// <param name="response">HttpListenerResponse</param>
        /// <returns>是否验证通过</returns>
        private static bool Authorization(string account, string password, HttpListenerRequest request, HttpListenerResponse response)
        {
            // 获取输入的账号密码
            string auth = request.Headers["Authorization"];
            if (auth != null)
            {
                // 移除头部"Basic "字符串
                auth = auth.Remove(0, 6);
                // 解码账号密码
                auth = Encoding.UTF8.GetString(Convert.FromBase64String(auth));
                // 账号密码正确
                if (auth == account + ":" + password)
                {
                    return true;
                }
            }
            // 设置response状态码：未授权
            response.StatusCode = (int)HttpStatusCode.Unauthorized;
            // 设置response授权头，名称为Authentication
            response.AddHeader("WWW-Authenticate", "Basic realm=\"Authentication\"");
            response.Close();
            return false;
        }

        /// <summary>
        /// 响应
        /// </summary>
        /// <param name="request">HttpListenerRequest</param>
        /// <param name="response">HttpListenerResponse</param>
        private static void Response(HttpListenerRequest request, HttpListenerResponse response)
        {
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
                        // 返回给客户端
                        ResponseWrite(response, Encoding.UTF8.GetBytes(respMsg));
                        break;
                    }
                // 图标
                case "/favicon.ico":
                    {
                        // 设置response类型：图标
                        response.ContentType = "image/x-icon";
                        MemoryStream faviconStream = new MemoryStream();
                        Resources.favicon.Save(faviconStream);
                        ResponseWrite(response, faviconStream.ToArray());
                        break;
                    }
                // PNG图片
                case "/香蕉.PNG":
                    {
                        // 设置response类型：PNG图片
                        response.ContentType = "image/png";
                        MemoryStream stream = new MemoryStream();
                        Resources.banana.Save(stream, ImageFormat.Png);
                        ResponseWrite(response, stream.ToArray());
                        stream.Dispose();
                        break;
                    }
                // 网页
                case "/socket.html":
                    {
                        // 设置response类型：网页
                        response.ContentType = "text/html;charset=UTF-8";
                        ResponseWrite(response, Encoding.UTF8.GetBytes(Resources.socket));
                        break;
                    }
                case "/webSocket.html":
                    {
                        response.ContentType = "text/html;charset=UTF-8";
                        ResponseWrite(response, Encoding.UTF8.GetBytes(Resources.webSocket));
                        break;
                    }
                case "/webSocket2.html":
                    {
                        response.ContentType = "text/html;charset=UTF-8";
                        ResponseWrite(response, Encoding.UTF8.GetBytes(Resources.webSocket2));
                        break;
                    }
                case "/webSocket3.html":
                    {
                        response.ContentType = "text/html;charset=UTF-8";
                        ResponseWrite(response, Encoding.UTF8.GetBytes(Resources.webSocket3));
                        break;
                    }
                // JSON
                case "/json":
                    {
                        // 设置response类型：JSON
                        response.ContentType = "application/json;charset=UTF-8";
                        string json = "{\"uuid\":\"" + Guid.NewGuid() + "\"}";
                        ResponseWrite(response, Encoding.UTF8.GetBytes(json));
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
                        ResponseWrite(response, stream.ToArray());
                        break;
                    }
            }
            // 关闭连接
            response.Close();
        }

        /// <summary>
        /// 响应回复
        /// </summary>
        /// <param name="response">HttpListenerResponse</param>
        /// <param name="buffer">buffer</param>
        private static void ResponseWrite(HttpListenerResponse response, byte[] buffer)
        {
            try
            {
                // 返回给客户端
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            // 用户主动关闭连接
            catch
            {
            }
        }

    }
}
