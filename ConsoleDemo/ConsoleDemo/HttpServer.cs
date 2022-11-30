using System;
using System.Linq;
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
        static HttpListener httpServer;

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
            // 清空URI
            httpServer.Prefixes.Clear();
            // 指定URI
            httpServer.Prefixes.Add("http://127.0.0.1:8080/");
            // 开启http服务器
            httpServer.Start();
            // 异步监听客户端请求
            httpServer.BeginGetContext(HttpHandle, null);
        }

        /// <summary>
        /// http处理
        /// </summary>
        /// <param name="ar">IAsyncResult</param>
        private static void HttpHandle(IAsyncResult ar)
        {
            // 继续异步监听客户端请求
            httpServer.BeginGetContext(HttpHandle, null);
            // 获取context对象
            var context = httpServer.EndGetContext(ar);
            var request = context.Request;
            var response = context.Response;
            // 打印request信息：请求方式，URL
            Console.WriteLine("Method:" + request.HttpMethod + " ,URL:" + request.Url.LocalPath);
            // 账号密码验证
            if (!Authorization("admin", "123456", request, response))
            {
                return;
            }
            // 设置response状态码：请求成功
            response.StatusCode = (int)HttpStatusCode.OK;
            // 设置response类型：UTF-8纯文本
            response.ContentType = "text/plain;charset=UTF-8";
            // 数据
            string respMsg = "随机UUID：" + Guid.NewGuid();
            // 返回给客户端
            response.OutputStream.Write(Encoding.UTF8.GetBytes(respMsg), 0, respMsg.Length);
            // 关闭连接
            response.Close();
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
            // 正确的账号密码
            string correctAuth = account + ":" + password;
            // 未输入账号密码
            if (!request.Headers.AllKeys.Contains("Authorization"))
            {
                // 设置response状态码：未授权
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                // 设置response授权头，名称为Authentication
                response.AddHeader("WWW-Authenticate", "Basic realm=\"Authentication\"");
                response.Close();
                return false;
            }
            else
            {
                // 获取输入的账号密码
                string auth = request.Headers["Authorization"];
                // 移除头部"Basic "字符串
                auth = auth.Remove(0, 6);
                // 解码账号密码
                auth = Encoding.UTF8.GetString(Convert.FromBase64String(auth));
                // 账号密码错误
                if (auth != correctAuth)
                {
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.AddHeader("WWW-Authenticate", "Basic realm=\"Authentication\"");
                    response.Close();
                    return false;
                }
                return true;
            }
        }

    }
}
