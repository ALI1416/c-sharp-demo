namespace ConsoleDemo.Util
{

    /// <summary>
    /// 二维码工具
    /// </summary>
    public class QrCodeUtils
    {

        /// <summary>
        /// byte[,]转为bool[,]
        /// <para>0 -> false</para>
        /// <para>1 -> true</para>
        /// </summary>
        /// <param name="bytes">byte[,]</param>
        /// <param name="dimension">尺寸</param>
        /// <returns>bool</returns>
        public static bool[,] Convert(byte[,] bytes, int dimension)
        {
            bool[,] data = new bool[dimension, dimension];
            for (int i = 0; i < dimension; i++)
            {
                for (int j = 0; j < dimension; j++)
                {
                    if (bytes[i, j] == 1)
                    {
                        data[i, j] = true;
                    }
                }
            }
            return data;
        }

        /// <summary>
        /// 添加bit
        /// </summary>
        /// <param name="bits">目的数据</param>
        /// <param name="pos">位置</param>
        /// <param name="value">值</param>
        /// <param name="numberBits">添加bit个数</param>
        public static void AddBits(bool[] bits, int pos, int value, int numberBits)
        {
            for (int i = 0; i < numberBits; i++)
            {
                bits[pos + i] = (value & (1 << (numberBits - i - 1))) != 0;
            }
        }

        /// <summary>
        /// 获取bit数组
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="numberBits">获取bit个数</param>
        public static bool[] GetBits(int value, int numberBits)
        {
            bool[] bits = new bool[numberBits];
            for (int i = 0; i < numberBits; i++)
            {
                bits[i] = (value & (1 << (numberBits - i - 1))) != 0;
            }
            return bits;
        }

        /// <summary>
        /// 获取字节数组
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="offset">起始位置</param>
        /// <param name="bytes">字节长度</param>
        /// <returns>字节数组</returns>
        public static int[] GetBytes(bool[] data, int offset, int bytes)
        {
            int[] result = new int[bytes];
            for (int i = 0; i < bytes; i++)
            {
                int ptr = offset + i * 8;
                result[i] = (
                      (data[ptr] ? 0x80 : 0)
                    | (data[ptr + 1] ? 0x40 : 0)
                    | (data[ptr + 2] ? 0x20 : 0)
                    | (data[ptr + 3] ? 0x10 : 0)
                    | (data[ptr + 4] ? 0x08 : 0)
                    | (data[ptr + 5] ? 0x04 : 0)
                    | (data[ptr + 6] ? 0x02 : 0)
                    | (data[ptr + 7] ? 0x01 : 0)
                   );
            }
            return result;
        }

    }
}
