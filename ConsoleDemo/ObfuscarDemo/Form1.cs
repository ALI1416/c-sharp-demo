using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Z.QRCodeEncoder.Net;

namespace ObfuscarDemo
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// 黑色刷子
        /// </summary>
        private static readonly Brush BLACK_BRUSH = new SolidBrush(Color.Black);

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            QRCode qrCode = new QRCode("爱上对方过后就哭了");
            pictureBox1.Image = QrBytes2Bitmap(qrCode.Matrix, 20);
        }

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

    }
}
