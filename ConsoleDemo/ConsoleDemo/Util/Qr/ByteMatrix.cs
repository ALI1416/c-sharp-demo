using System.Text;
using System;

namespace ConsoleDemo.Util
{
    public sealed class ByteMatrix
    {
        private readonly byte[][] bytes;
        private readonly int width;
        private readonly int height;

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteMatrix"/> class.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public ByteMatrix(int width, int height)
        {
            bytes = new byte[height][];
            for (var i = 0; i < height; i++)
                bytes[i] = new byte[width];
            this.width = width;
            this.height = height;
        }

        /// <summary>
        /// Gets the height.
        /// </summary>
        public int Height
        {
            get { return height; }
        }

        /// <summary>
        /// Gets the width.
        /// </summary>
        public int Width
        {
            get { return width; }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Int32"/> with the specified x.
        /// </summary>
        public int this[int x, int y]
        {
            get { return bytes[y][x]; }
            set { bytes[y][x] = (byte)value; }
        }

        /// <summary>
        /// an internal representation as bytes, in row-major order. array[y][x] represents point (x,y)
        /// </summary>
        public byte[][] Array
        {
            get { return bytes; }
        }

        /// <summary>
        /// Clears the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void clear(byte value)
        {
            for (int y = 0; y < height; ++y)
            {
                var bytesY = bytes[y];
                for (int x = 0; x < width; ++x)
                {
                    bytesY[x] = value;
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        override public String ToString()
        {
            var result = new StringBuilder(2 * width * height + 2);
            for (int y = 0; y < height; ++y)
            {
                var bytesY = bytes[y];
                for (int x = 0; x < width; ++x)
                {
                    switch (bytesY[x])
                    {
                        case 0:
                            result.Append(" 0");
                            break;
                        case 1:
                            result.Append(" 1");
                            break;
                        default:
                            result.Append("  ");
                            break;
                    }
                }
                result.Append('\n');
            }
            return result.ToString();
        }
    }
}
