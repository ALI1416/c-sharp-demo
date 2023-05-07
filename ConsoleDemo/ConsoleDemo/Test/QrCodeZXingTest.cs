using ConsoleDemo.Util;
using NUnit.Framework;
using System.Collections.Generic;
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

        private static readonly string content = "爱上对方过后就哭了啊123456789012345678901234567890";
        private static readonly ZXing.QrCode.Internal.ErrorCorrectionLevel level = ZXing.QrCode.Internal.ErrorCorrectionLevel.H;
        private static readonly string path = "E:/qr1.png";

        /// <summary>
        /// 测试
        /// </summary>
        [Test]
        public static void Test()
        {
            // 生成二维码
            Dictionary<EncodeHintType, object> hints = new Dictionary<EncodeHintType, object>(1)
            {
                { EncodeHintType.CHARACTER_SET, "UTF-8" },
                { EncodeHintType.DISABLE_ECI, true },
            };
            ZXing.QrCode.Internal.QRCode qr = ZXing.QrCode.Internal.Encoder.encode(content, level, hints);
            Bitmap bitmap = ImageUtils.QrBytes2Bitmap(qr.Matrix.Array, 10);
            ImageUtils.SaveBitmap(bitmap, path);
            // 识别二维码
            //BarcodeReader reader = new BarcodeReader();
            //Bitmap bitmapResult = new Bitmap(path);
            //Result result = reader.Decode(bitmapResult);
            //string contentResult = result.ToString();
            //Console.WriteLine(contentResult);
            //Assert.AreEqual(content, contentResult);
        }

    }
}
