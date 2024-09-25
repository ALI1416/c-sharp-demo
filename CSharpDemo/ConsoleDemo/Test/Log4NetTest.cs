using log4net;
using NUnit.Framework;

namespace ConsoleDemo.Test
{

    /// <summary>
    /// log4net测试
    /// 添加NuGet包`log4net`
    /// <para>
    /// 1. 新建文件`log4net.config`
    /// 2. 修改文件属性`复制到输出目录`选择`始终复制`
    /// 3. 到`AssemblyInfo.cs`里去注册
    /// </para>
    /// 注意：使用Test无`控制台`输出
    /// </summary>
    [TestFixture]
    public class Log4NetTest
    {

        /// <summary>
        /// 日志实例
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(typeof(Log4NetTest));

        /// <summary>
        /// 测试
        /// </summary>
        [Test]
        public static void Test()
        {
            log.Fatal("致命");
            log.Error("错误");
            log.Warn("警告");
            log.Info("信息");
            log.Debug("调试");
        }

    }
}
