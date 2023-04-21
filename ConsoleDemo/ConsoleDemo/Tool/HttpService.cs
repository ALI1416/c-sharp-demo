using ConsoleDemo.Properties;
using ConsoleDemo.Util;
using log4net;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Security.Principal;
using System.Text;

namespace ConsoleDemo.Tool
{

    /// <summary>
    /// http服务
    /// </summary>
    public class HttpService
    {

        /// <summary>
        /// 日志实例
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(typeof(HttpService));

        /// <summary>
        /// 服务器
        /// </summary>
        private HttpListener server;
        /// <summary>
        /// 响应处理函数&lt;HttpListenerRequest,HttpListenerResponse,返回值>
        /// </summary>
        private Func<HttpListenerRequest, HttpListenerResponse, byte[]> responseHandleFunc;
        /// <summary>
        /// 账号和密码
        /// </summary>
        private string accountAndPassword = null;


        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="uri">URI</param>
        /// <param name="responseHandleFunc">响应处理函数&lt;HttpListenerRequest,HttpListenerResponse,返回值></param>
        public void Start(string uri, Func<HttpListenerRequest, HttpListenerResponse, byte[]> responseHandleFunc)
        {
            Start(uri, null, null, responseHandleFunc);
        }

        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="uri">URI</param>
        /// <param name="account">账号</param>
        /// <param name="password">密码</param>
        /// <param name="responseHandleFunc">响应处理函数&lt;HttpListenerRequest,HttpListenerResponse,返回值></param>
        public void Start(string uri, string account, string password, Func<HttpListenerRequest, HttpListenerResponse, byte[]> responseHandleFunc)
        {
            // 新建服务器
            server = new HttpListener
            {
                // 忽视客户端写入异常
                IgnoreWriteExceptions = true
            };
            // 指定URI
            server.Prefixes.Add(uri);
            try
            {
                // 启动服务器
                server.Start();
            }
            // 端口号冲突
            catch
            {
                log.Error("http服务器端口号冲突");
                return;
            }
            if (account != null && password != null)
            {
                accountAndPassword = account + ":" + password;
            }
            this.responseHandleFunc = responseHandleFunc;
            // 异步监听客户端请求
            server.BeginGetContext(Handle, null);
            log.Info("http服务器已启动");
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            server.Close();
        }

        /// <summary>
        /// 处理
        /// </summary>
        /// <param name="ar">IAsyncResult</param>
        private void Handle(IAsyncResult ar)
        {
            try
            {
                // 继续异步监听客户端请求
                server.BeginGetContext(Handle, null);
            }
            // 主动关闭服务器
            catch
            {
                log.Info("主动关闭http服务器");
                return;
            }
            // 获取context对象
            var context = server.EndGetContext(ar);
            var request = context.Request;
            var response = context.Response;
            // 打印request信息：请求方式，URL
            log.Info("Method:" + request.HttpMethod + " ,URL:" + request.Url.PathAndQuery);
            // 账号密码验证
            if (accountAndPassword != null && !Authorization(request, response))
            {
                log.Warn("密码错误");
                return;
            }
            // 响应
            Response(request, response);
        }

        /// <summary>
        /// 账号密码验证
        /// </summary>
        /// <param name="request">HttpListenerRequest</param>
        /// <param name="response">HttpListenerResponse</param>
        /// <returns>是否验证通过</returns>
        private bool Authorization(HttpListenerRequest request, HttpListenerResponse response)
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
                if (auth == accountAndPassword)
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
        private void Response(HttpListenerRequest request, HttpListenerResponse response)
        {
            // 响应处理函数
            var data = responseHandleFunc(request, response);
            ResponseWrite(response, data);
            // 关闭连接
            response.Close();
        }

        /// <summary>
        /// 响应回复
        /// </summary>
        /// <param name="response">HttpListenerResponse</param>
        /// <param name="buffer">buffer</param>
        private void ResponseWrite(HttpListenerResponse response, byte[] buffer)
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
