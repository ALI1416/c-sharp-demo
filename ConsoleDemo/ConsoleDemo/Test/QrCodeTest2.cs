using ConsoleDemo.Model;
using NUnit.Framework;
using System;

namespace ConsoleDemo.Test
{

    /// <summary>
    /// 二维码测试
    /// </summary>
    [TestFixture]
    public class QrCodeTest2
    {

        private static readonly string content = "爱上对方过后就哭了1234567890";
        private static readonly int level = 3;

        /// <summary>
        /// 测试
        /// </summary>
        [Test]
        public static void Test()
        {
            // 生成二维码
            QrCode qrCode = new QrCode(content,level);
            Console.WriteLine(qrCode);
        }

    }
}
