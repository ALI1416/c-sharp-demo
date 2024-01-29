using log4net;
using System;
using System.IO.Ports;
using System.Threading.Tasks;

namespace ConsoleDemo.Service
{

    /// <summary>
    /// 串口服务
    /// </summary>
    public class SerialPortService
    {

        /// <summary>
        /// 日志实例
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(typeof(SocketService));

        /// <summary>
        /// 已启动
        /// </summary>
        private bool isStarted = false;
        /// <summary>
        /// 串口
        /// </summary>
        private SerialPort serialPort;
        /// <summary>
        /// 接收消息回调函数&lt;消息>
        /// </summary>
        private Action<byte[]> receiveCallback;

        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="portName">端口名</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="reconnectTime">重连时间(秒，&lt;=0不重连)</param>
        /// <param name="receiveCallback">接收消息回调函数&lt;消息></param>
        /// <returns>是否启动成功</returns>
        public bool Start(string portName, int baudRate, int reconnectTime, Action<byte[]> receiveCallback)
        {
            bool isOpen = false;
            try
            {
                serialPort = new SerialPort(portName, baudRate);
                // 打开串口
                serialPort.Open();
                isOpen = true;
            }
            catch (Exception e)
            {
                log.Error("串口打开失败！", e);
            }
            this.receiveCallback = receiveCallback;
            // 接收消息
            serialPort.DataReceived += Receive;
            isStarted = true;
            // 重连
            if (reconnectTime > 0)
            {
                Reconnect(reconnectTime);
            }
            return isOpen;
        }

        /// <summary>
        /// 重连
        /// </summary>
        /// <param name="reconnectTime">重连时间(秒)</param>
        private async void Reconnect(int reconnectTime)
        {
            while (isStarted)
            {
                // 延时
                await Task.Delay(reconnectTime * 1000);
                if (!serialPort.IsOpen)
                {
                    try
                    {
                        // 打开串口
                        serialPort.Open();
                        log.Info("串口重连成功！");
                    }
                    catch (Exception e)
                    {
                        log.Error("串口重连失败！", e);
                    }
                }
            }
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            isStarted = false;
            if (serialPort != null)
            {
                serialPort.DataReceived -= Receive;
                serialPort.Close();
            }
        }

        /// <summary>
        /// 打开串口
        /// </summary>
        /// <returns>串口是否打开成功</returns>
        public bool Open()
        {
            if (serialPort == null)
            {
                return false;
            }
            if (!serialPort.IsOpen)
            {
                try
                {
                    // 打开串口
                    serialPort.Open();
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 串口是否打开成功
        /// </summary>
        /// <returns>串口是否打开成功</returns>
        public bool IsOpen()
        {
            if (serialPort == null)
            {
                return false;
            }
            return serialPort.IsOpen;
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="sender">SerialPort</param>
        /// <param name="e">SerialDataReceivedEventArgs</param>
        private void Receive(object sender, SerialDataReceivedEventArgs e)
        {
            int length = serialPort.BytesToRead;
            if (length > 0)
            {
                byte[] buffer = new byte[length];
                // 读取消息
                serialPort.Read(buffer, 0, length);
                receiveCallback(buffer);
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="data">消息</param>
        /// <returns>是否发送成功</returns>
        public bool Send(byte[] data)
        {
            if (serialPort == null)
            {
                return false;
            }
            if (serialPort.IsOpen)
            {
                try
                {
                    // 发送消息
                    serialPort.Write(data, 0, data.Length);
                    return true;
                }
                catch
                {
                }
            }
            return false;
        }

    }
}
