using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace ConsoleDemo.Util
{

    /// <summary>
    /// 图像工具
    /// </summary>
    public class ImageUtils
    {


        private static Brush brush = new SolidBrush(Color.Black);

        /// <summary>
        /// 二维码Bytes转Bitmap
        /// </summary>
        /// <param name="bytes">byte[][](1黑0白)</param>
        /// <param name="pixelSize">像素尺寸</param>
        /// <returns>Bitmap</returns>
        public static Bitmap QrBytes2Bitmap(byte[][] bytes, int pixelSize)
        {
            int length = bytes.Length;
            List<Rectangle> rects = new List<Rectangle>();
            for (int x = 0; x < length; x++)
            {
                for (int y = 0; y < length; y++)
                {
                    if (bytes[x][y] == 1)
                    {
                        rects.Add(new Rectangle((x + 1) * pixelSize, (y + 1) * pixelSize, pixelSize, pixelSize));
                    }
                }
            }
            int size = (length + 2) * pixelSize;
            Bitmap bitmap = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.FillRectangles(brush, rects.ToArray());
            }
            return bitmap;
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
