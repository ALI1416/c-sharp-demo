using ConsoleDemo.Util;
using System.Drawing;
using ZXing.QrCode.Internal;

namespace ConsoleDemo.BLL
{

    /// <summary>
    /// 二维码
    /// </summary>
    public class Qr
    {

        /// <summary>
        /// 启动
        /// </summary>
        public static void Start()
        {
            QRCode encoder = Encoder.encode("ConsoleDemo", ErrorCorrectionLevel.H);
            Bitmap bitmap = ImageUtils.QrBytes2Bitmap(encoder.Matrix.Array, 10);
            ImageUtils.SaveBitmap(bitmap, "E:/qr.png");
        }

    }

}
