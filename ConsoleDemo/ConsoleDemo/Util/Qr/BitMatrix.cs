using System.Text;
using System;

namespace ConsoleDemo.Util
{
    public sealed class BitMatrix
    {

        private int width;
        private int height;
        private int rowSize;
        private int[] bits;

        /// <returns> The width of the matrix
        /// </returns>
        public int Width
        {
            get { return width; }
        }

        /// <returns> The height of the matrix
        /// </returns>
        public int Height
        {
            get { return height; }
        }

        /// <returns>
        /// The rowsize of the matrix
        /// </returns>
        public int RowSize
        {
            get { return rowSize; }
        }

        /// <summary>
        /// Creates an empty square <see cref="BitMatrix"/>.
        /// </summary>
        /// <param name="dimension">height and width</param>
        public BitMatrix(int dimension)
           : this(dimension, dimension)
        {
        }

        /// <summary>
        /// Creates an empty square <see cref="BitMatrix"/>.
        /// </summary>
        /// <param name="width">bit matrix width</param>
        /// <param name="height">bit matrix height</param>
        public BitMatrix(int width, int height)
        {
            if (width < 1 || height < 1)
            {
                throw new System.ArgumentException("Both dimensions must be greater than 0");
            }
            this.width = width;
            this.height = height;
            this.rowSize = (width + 31) >> 5;
            bits = new int[rowSize * height];
        }

        internal BitMatrix(int width, int height, int rowSize, int[] bits)
        {
            this.width = width;
            this.height = height;
            this.rowSize = rowSize;
            this.bits = bits;
        }

        /// <summary> <p>Gets the requested bit, where true means black.</p>
        /// 
        /// </summary>
        /// <param name="x">The horizontal component (i.e. which column)
        /// </param>
        /// <param name="y">The vertical component (i.e. which row)
        /// </param>
        /// <returns> value of given bit in matrix
        /// </returns>
        public bool this[int x, int y]
        {
            get
            {
                int offset = y * rowSize + (x >> 5);
                return (((int)((uint)(bits[offset]) >> (x & 0x1f))) & 1) != 0;
            }
            set
            {
                if (value)
                {
                    int offset = y * rowSize + (x >> 5);
                    bits[offset] |= 1 << (x & 0x1f);
                }
                else
                {
                    int offset = y * rowSize + (x / 32);
                    bits[offset] &= ~(1 << (x & 0x1f));
                }
            }
        }

        /// <summary> <p>Sets a square region of the bit matrix to true.</p>
        /// 
        /// </summary>
        /// <param name="left">The horizontal position to begin at (inclusive)
        /// </param>
        /// <param name="top">The vertical position to begin at (inclusive)
        /// </param>
        /// <param name="width">The width of the region
        /// </param>
        /// <param name="height">The height of the region
        /// </param>
        public void setRegion(int left, int top, int width, int height)
        {
            if (top < 0 || left < 0)
            {
                throw new System.ArgumentException("Left and top must be nonnegative");
            }
            if (height < 1 || width < 1)
            {
                throw new System.ArgumentException("Height and width must be at least 1");
            }
            int right = left + width;
            int bottom = top + height;
            if (bottom > this.height || right > this.width)
            {
                throw new System.ArgumentException("The region must fit inside the matrix");
            }
            for (int y = top; y < bottom; y++)
            {
                int offset = y * rowSize;
                for (int x = left; x < right; x++)
                {
                    bits[offset + (x >> 5)] |= 1 << (x & 0x1f);
                }
            }
        }

        /// <summary> A fast method to retrieve one row of data from the matrix as a BitArray.
        /// 
        /// </summary>
        /// <param name="y">The row to retrieve
        /// </param>
        /// <param name="row">An optional caller-allocated BitArray, will be allocated if null or too small
        /// </param>
        /// <returns> The resulting BitArray - this reference should always be used even when passing
        /// your own row
        /// </returns>
        public BitArray getRow(int y, BitArray row)
        {
            if (row == null || row.Size < width)
            {
                row = new BitArray(width);
            }
            else
            {
                row.clear();
            }
            int offset = y * rowSize;
            for (int x = 0; x < rowSize; x++)
            {
                row.setBulk(x << 5, bits[offset + x]);
            }
            return row;
        }

        /// <summary>
        /// Sets the row.
        /// </summary>
        /// <param name="y">row to set</param>
        /// <param name="row">{@link BitArray} to copy from</param>
        public void setRow(int y, BitArray row)
        {
            Array.Copy(row.Array, 0, bits, y * rowSize, rowSize);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (!(obj is BitMatrix))
            {
                return false;
            }
            var other = (BitMatrix)obj;
            if (width != other.width || height != other.height ||
                rowSize != other.rowSize || bits.Length != other.bits.Length)
            {
                return false;
            }
            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i] != other.bits[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            int hash = width;
            hash = 31 * hash + width;
            hash = 31 * hash + height;
            hash = 31 * hash + rowSize;
            foreach (var bit in bits)
            {
                hash = 31 * hash + bit.GetHashCode();
            }
            return hash;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override String ToString()
        {
            return ToString("X ", "  ", Environment.NewLine);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="setString">The set string.</param>
        /// <param name="unsetString">The unset string.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public String ToString(String setString, String unsetString)
        {
            return buildToString(setString, unsetString, Environment.NewLine);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="setString">The set string.</param>
        /// <param name="unsetString">The unset string.</param>
        /// <param name="lineSeparator">The line separator.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public String ToString(String setString, String unsetString, String lineSeparator)
        {
            return buildToString(setString, unsetString, lineSeparator);
        }

        private String buildToString(String setString, String unsetString, String lineSeparator)
        {
            var result = new StringBuilder(height * (width + 1));
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    result.Append(this[x, y] ? setString : unsetString);
                }
                result.Append(lineSeparator);
            }
            return result.ToString();
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new BitMatrix(width, height, rowSize, (int[])bits.Clone());
        }
    }
}
