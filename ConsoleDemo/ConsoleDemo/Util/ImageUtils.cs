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

        /// <summary>
        /// 黑色刷子
        /// </summary>
        private static readonly Brush BLACK_BRUSH = new SolidBrush(Color.Black);

        /// <summary>
        /// 二维码bool[,]转Bitmap
        /// </summary>
        /// <param name="bytes">bool[,](false白 true黑)</param>
        /// <param name="pixelSize">像素尺寸</param>
        /// <returns>Bitmap</returns>
        public static Bitmap QrBytes2Bitmap(bool[,] bytes, int pixelSize)
        {
            int length = bytes.GetLength(0);
            List<Rectangle> rects = new List<Rectangle>();
            for (int x = 0; x < length; x++)
            {
                for (int y = 0; y < length; y++)
                {
                    if (bytes[x, y])
                    {
                        rects.Add(new Rectangle((x + 1) * pixelSize, (y + 1) * pixelSize, pixelSize, pixelSize));
                    }
                }
            }
            int size = (length + 2) * pixelSize;
            Bitmap bitmap = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.FillRectangles(BLACK_BRUSH, rects.ToArray());
            }
            return bitmap;
        }

        /// <summary>
        /// 二维码byte[][]转Bitmap
        /// </summary>
        /// <param name="bytes">byte[][](0白 1黑)</param>
        /// <param name="pixelSize">像素尺寸</param>
        /// <returns>Bitmap</returns>
        public static Bitmap QrBytes2Bitmap(byte[][] bytes, int pixelSize)
        {
            int length = bytes.Length;
            List<Rectangle> rects = new List<Rectangle>();
            // ZXing反转了xy轴
            for (int y = 0; y < length; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    if (bytes[y][x] == 1)
                    {
                        rects.Add(new Rectangle((x + 1) * pixelSize, (y + 1) * pixelSize, pixelSize, pixelSize));
                    }
                }
            }
            int size = (length + 2) * pixelSize;
            Bitmap bitmap = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.FillRectangles(BLACK_BRUSH, rects.ToArray());
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
