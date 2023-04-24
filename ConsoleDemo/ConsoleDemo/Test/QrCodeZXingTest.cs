using ConsoleDemo.Util;
using NUnit.Framework;
using System;
using System.Drawing;
using ZXing;

namespace ConsoleDemo.Test
{

    /// <summary>
    /// ZXing二维码测试
    /// </summary>
    [TestFixture]
    public class QrCodeZXingTest
    {

        private static readonly string content = "ConsoleDemo";
        private static readonly string path = "E:/qr.png";

        /// <summary>
        /// 测试
        /// </summary>
        [Test]
        public static void Test()
        {
            // 生成二维码
            ZXing.QrCode.Internal.QRCode qr = ZXing.QrCode.Internal.Encoder.encode(content, ZXing.QrCode.Internal.ErrorCorrectionLevel.H);
            Bitmap bitmap = ImageUtils.QrBytes2Bitmap(qr.Matrix.Array, 10);
            ImageUtils.SaveBitmap(bitmap, path);
            // 识别二维码
            BarcodeReader reader = new BarcodeReader();
            Bitmap bitmapResult = new Bitmap(path);
            Result result = reader.Decode(bitmapResult);
            string contentResult = result.ToString();
            Console.WriteLine(contentResult);
            Assert.AreEqual(content, contentResult);
        }

    }
}
