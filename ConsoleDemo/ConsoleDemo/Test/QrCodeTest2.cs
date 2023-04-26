using ConsoleDemo.Tool;
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

        private static readonly string content = "ConsoleDemo";
        private static readonly int level = 1;
        private static readonly string path = "E:/qr2.png";

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
