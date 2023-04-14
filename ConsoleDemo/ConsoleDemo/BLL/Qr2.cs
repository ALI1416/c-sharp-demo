using ConsoleDemo.Util;
using System.Drawing;

namespace ConsoleDemo.BLL
{
    /// <summary>
    /// 二维码2
    /// </summary>
    public class Qr2
    {

        /// <summary>
        /// 启动
        /// </summary>
        public static void Start()
        {
            QRCode qr = QrUtils.encode("ConsoleDemo", ErrorCorrectionLevel.H);
            Bitmap bitmap = ImageUtils.QrBytes2Bitmap(qr.Matrix.Array, 10);
            ImageUtils.SaveBitmap(bitmap, "E:/qr2.png");
        }

    }
}
