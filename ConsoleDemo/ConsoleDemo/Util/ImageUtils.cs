using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Net.NetworkInformation;

namespace ConsoleDemo.Util
{
    /// <summary>
    /// 图像工具
    /// </summary>
    public class ImageUtils
    {

        /// <summary>
        /// 二维码Bytes转Bitmap
        /// </summary>
        /// <param name="bytes">byte[][](1黑0白)</param>
        /// <param name="pixelSize">像素尺寸</param>
        /// <returns>Bitmap</returns>
        public static Bitmap QrBytes2Bitmap(byte[][] bytes, int pixelSize)
        {
            // 原二维码
            int oldSize = bytes.Length;
            Bitmap oldQr = new Bitmap(oldSize, oldSize);
            for (int x = 0; x < oldSize; x++)
            {
                for (int y = 0; y < oldSize; y++)
                {
                    if (bytes[x][y] == 1)
                    {
                        oldQr.SetPixel(x, y, Color.Black);
                    }
                }
            }
            // 新二维码
            int newSize = (oldSize + 2) * pixelSize;
            Bitmap newQr = new Bitmap(newSize, newSize);
            using (Graphics g = Graphics.FromImage(newQr))
            {
                g.FillRectangle(new SolidBrush(Color.White), new Rectangle(0, 0, newSize, newSize));
                g.SmoothingMode = SmoothingMode.None;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(oldQr, new RectangleF(10, 10, newSize - 10, newSize - 10), new Rectangle(0, 0, oldSize, oldSize), GraphicsUnit.Pixel);
            }
            return newQr;
        }

        /// <summary>
        /// 保存Bitmap为PNG图片
        /// </summary>
        /// <param name="bitmap">Bitmap</param>
        /// <param name="path">路径</param>
        public static void SaveBitmap(Bitmap bitmap, string path)
        {
            bitmap.Save(path, ImageFormat.Png);
        }

    }

}
