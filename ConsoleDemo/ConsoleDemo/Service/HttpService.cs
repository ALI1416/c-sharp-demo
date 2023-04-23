using log4net;
using System;
using System.Net;
using System.Text;

namespace ConsoleDemo.Service
{

    /// <summary>
    /// http服务(使用HttpListener)
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
        /// 服务器关闭回调函数
        /// </summary>
        private Action serviceCloseCallback;
        /// <summary>
        /// 响应回调函数&lt;HttpListenerRequest,HttpListenerResponse,返回值>
        /// </summary>
        private Func<HttpListenerRequest, HttpListenerResponse, byte[]> responseCallback;
        /// <summary>
        /// 账号和密码
        /// </summary>
        private string accountAndPassword = null;

        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口号</param>
        /// <param name="responseCallback">响应回调函数&lt;HttpListenerRequest,HttpListenerResponse,返回值></param>
        /// <returns>是否启动成功</returns>
        public bool Start(IPAddress ip, int port, Func<HttpListenerRequest, HttpListenerResponse, byte[]> responseCallback)
        {
            return Start(ip, port, null, null, () => { }, responseCallback);
        }

        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口号</param>
        /// <param name="serviceCloseCallback">服务器关闭回调函数</param>
        /// <param name="responseCallback">响应回调函数&lt;HttpListenerRequest,HttpListenerResponse,返回值></param>
        /// <returns>是否启动成功</returns>
        public bool Start(IPAddress ip, int port, Action serviceCloseCallback, Func<HttpListenerRequest, HttpListenerResponse, byte[]> responseCallback)
        {
            return Start(ip, port, null, null, serviceCloseCallback, responseCallback);
        }

        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口号</param>
        /// <param name="account">账号</param>
        /// <param name="password">密码</param>
        /// <param name="responseCallback">响应回调函数&lt;HttpListenerRequest,HttpListenerResponse,返回值></param>
        /// <returns>是否启动成功</returns>
        public bool Start(IPAddress ip, int port, string account, string password, Func<HttpListenerRequest, HttpListenerResponse, byte[]> responseCallback)
        {
            return Start(ip, port, account, password, () => { }, responseCallback);
        }

        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口号</param>
        /// <param name="account">账号</param>
        /// <param name="password">密码</param>
        /// <param name="serviceCloseCallback">服务器关闭回调函数</param>
        /// <param name="responseCallback">响应回调函数&lt;HttpListenerRequest,HttpListenerResponse,返回值></param>
        /// <returns>是否启动成功</returns>
        public bool Start(IPAddress ip, int port, string account, string password, Action serviceCloseCallback, Func<HttpListenerRequest, HttpListenerResponse, byte[]> responseCallback)
        {
            // 新建服务器
            server = new HttpListener
            {
                // 忽视客户端写入异常
                IgnoreWriteExceptions = true
            };
            // 指定IP地址和端口号
            server.Prefixes.Add("http://" + ip + ":" + port + "/");
            try
            {
                // 启动服务器
                server.Start();
            }
            // 端口号冲突
            catch
            {
                log.Error("http服务器端口号冲突");
                return false;
            }
            if (account != null && password != null)
            {
                // 把账号密码转换为`Authorization`形式
                accountAndPassword = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(account + ":" + password));
            }
            this.serviceCloseCallback = serviceCloseCallback;
            this.responseCallback = responseCallback;
            // 异步监听客户端请求
            server.BeginGetContext(Handle, null);
            log.Info("http服务器已启动");
            return true;
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
                // 服务器关闭回调函数
                serviceCloseCallback();
                log.Info("主动关闭http服务器");
                return;
            }
            // 获取context对象
            HttpListenerContext context = server.EndGetContext(ar);
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            // 打印request信息：请求方式，URL
            log.Info("Method:" + request.HttpMethod + " ,URL:" + request.Url.PathAndQuery);
            // 账号密码验证
            if (accountAndPassword != null && !Authorization(request, response))
            {
                log.Warn("密码错误");
                return;
            }
            // 请求消息处理
            RequestHandle(request, response);
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
        /// 请求消息处理
        /// </summary>
        /// <param name="request">HttpListenerRequest</param>
        /// <param name="response">HttpListenerResponse</param>
        private void RequestHandle(HttpListenerRequest request, HttpListenerResponse response)
        {
            // 请求消息处理函数
            byte[] data = responseCallback(request, response);
            Response(response, data);
            // 关闭连接
            response.Close();
        }

        /// <summary>
        /// 响应回复
        /// </summary>
        /// <param name="response">HttpListenerResponse</param>
        /// <param name="buffer">buffer</param>
        private void Response(HttpListenerResponse response, byte[] buffer)
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
