using ConsoleDemo.Model;
using ConsoleDemo.Util;
using NUnit.Framework;
using System.Drawing;

namespace ConsoleDemo.Test
{

    /// <summary>
    /// 二维码测试
    /// </summary>
    [TestFixture]
    public class QrCodeTest
    {

        private static readonly string content = "爱上对方过后就哭了啊123456789012345678901234567890";
        private static readonly int level = 3;
        private static readonly string path = "E:/qr2.png";

        /// <summary>
        /// 测试
        /// </summary>
        [Test]
        public static void Test()
        {
            // 生成二维码
            QrCode qrCode = new QrCode(content, level);
            Bitmap bitmap = ImageUtils.QrBytes2Bitmap(qrCode.Matrix, 10);
            ImageUtils.SaveBitmap(bitmap, path);
        }

    }
}
