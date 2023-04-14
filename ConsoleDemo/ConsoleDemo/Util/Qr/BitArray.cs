using System;

namespace ConsoleDemo.Util
{
    public sealed class BitArray
    {
        private static int[] EMPTY_BITS = { };
        private static float LOAD_FACTOR = 0.75f;

        private int[] bits;
        private int size;

        /// <summary>
        /// size of the array, number of elements
        /// </summary>
        public int Size
        {
            get
            {
                return size;
            }
        }

        /// <summary>
        /// size of the array in bytes
        /// </summary>
        public int SizeInBytes
        {
            get
            {
                return (size + 7) >> 3;
            }
        }

        /// <summary>
        /// index accessor
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public bool this[int i]
        {
            get
            {
                return (bits[i >> 5] & (1 << (i & 0x1F))) != 0;
            }
            set
            {
                if (value)
                    bits[i >> 5] |= 1 << (i & 0x1F);
            }
        }

        /// <summary>
        /// default constructor
        /// </summary>
        public BitArray()
        {
            this.size = 0;
            this.bits = EMPTY_BITS;
        }

        /// <summary>
        /// initializing constructor
        /// </summary>
        /// <param name="size">desired size of the array</param>
        public BitArray(int size)
        {
            if (size < 1)
            {
                throw new ArgumentException("size must be at least 1");
            }
            this.size = size;
            this.bits = makeArray(size);
        }

        // For testing only
        private BitArray(int[] bits, int size)
        {
            this.bits = bits;
            this.size = size;
        }

        private void ensureCapacity(int newSize)
        {
            if (newSize > bits.Length << 5)
            {
                int[] newBits = makeArray((int)Math.Ceiling(newSize / LOAD_FACTOR));
                System.Array.Copy(bits, 0, newBits, 0, bits.Length);
                bits = newBits;
            }
        }

        /// <summary> Sets a block of 32 bits, starting at bit i.
        /// 
        /// </summary>
        /// <param name="i">first bit to set
        /// </param>
        /// <param name="newBits">the new value of the next 32 bits. Note again that the least-significant bit
        /// corresponds to bit i, the next-least-significant to i+1, and so on.
        /// </param>
        public void setBulk(int i, int newBits)
        {
            bits[i >> 5] = newBits;
        }

        /// <summary> Clears all bits (sets to false).</summary>
        public void clear()
        {
            int max = bits.Length;
            for (int i = 0; i < max; i++)
            {
                bits[i] = 0;
            }
        }

        /// <summary>
        /// Appends the bit.
        /// </summary>
        /// <param name="bit">The bit.</param>
        public void appendBit(bool bit)
        {
            ensureCapacity(size + 1);
            if (bit)
            {
                bits[size >> 5] |= 1 << (size & 0x1F);
            }
            size++;
        }

        /// <returns> underlying array of ints. The first element holds the first 32 bits, and the least
        /// significant bit is bit 0.
        /// </returns>
        public int[] Array
        {
            get { return bits; }
        }

        /// <summary>
        /// Appends the least-significant bits, from value, in order from most-significant to
        /// least-significant. For example, appending 6 bits from 0x000001E will append the bits
        /// 0, 1, 1, 1, 1, 0 in that order.
        /// </summary>
        /// <param name="value"><see cref="int"/> containing bits to append</param>
        /// <param name="numBits">bits from value to append</param>
        public void appendBits(int value, int numBits)
        {
            int nextSize = size;
            ensureCapacity(nextSize + numBits);
            for (int numBitsLeft = numBits - 1; numBitsLeft >= 0; numBitsLeft--)
            {
                if ((value & (1 << numBitsLeft)) != 0)
                {
                    bits[nextSize / 32] |= 1 << (nextSize & 0x1F);
                }
                nextSize++;
            }
            size = nextSize;
        }

        /// <summary>
        /// adds the array to the end
        /// </summary>
        /// <param name="other"></param>
        public void appendBitArray(BitArray other)
        {
            int otherSize = other.size;
            ensureCapacity(size + otherSize);
            for (int i = 0; i < otherSize; i++)
            {
                appendBit(other[i]);
            }
        }

        /// <summary>
        /// XOR operation
        /// </summary>
        /// <param name="other"></param>
        public void xor(BitArray other)
        {
            if (size != other.size)
            {
                throw new ArgumentException("Sizes don't match");
            }
            for (int i = 0; i < bits.Length; i++)
            {
                // The last int could be incomplete (i.e. not have 32 bits in
                // it) but there is no problem since 0 XOR 0 == 0.
                bits[i] ^= other.bits[i];
            }
        }

        /// <summary>
        /// converts to bytes.
        /// </summary>
        /// <param name="bitOffset">first bit to start writing</param>
        /// <param name="array">array to write into. Bytes are written most-significant byte first. This is the opposite
        /// of the internal representation, which is exposed by BitArray</param>
        /// <param name="offset">position in array to start writing</param>
        /// <param name="numBytes">how many bytes to write</param>
        public void toBytes(int bitOffset, byte[] array, int offset, int numBytes)
        {
            for (int i = 0; i < numBytes; i++)
            {
                int theByte = 0;
                for (int j = 0; j < 8; j++)
                {
                    if (this[bitOffset])
                    {
                        theByte |= 1 << (7 - j);
                    }
                    bitOffset++;
                }
                array[offset + i] = (byte)theByte;
            }
        }

        private static int[] makeArray(int size)
        {
            return new int[(size + 31) >> 5];
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="o">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(Object o)
        {
            var other = o as BitArray;
            if (other == null)
                return false;
            if (size != other.size)
                return false;
            for (var index = 0; index < bits.Length; index++)
            {
                if (bits[index] != other.bits[index])
                    return false;
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
            var hash = size;
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
            var result = new System.Text.StringBuilder(size + (size / 8) + 1);
            for (int i = 0; i < size; i++)
            {
                if ((i & 0x07) == 0)
                {
                    result.Append(' ');
                }
                result.Append(this[i] ? 'X' : '.');
            }
            return result.ToString();
        }

        /// <summary>
        /// Erstellt ein neues Objekt, das eine Kopie der aktuellen Instanz darstellt.
        /// </summary>
        /// <returns>
        /// Ein neues Objekt, das eine Kopie dieser Instanz darstellt.
        /// </returns>
        public object Clone()
        {
            return new BitArray((int[])bits.Clone(), size);
        }
    }
}
