namespace ConsoleDemo.Model
{

    /// <summary>
    /// 通用Galois Fields域(通用伽罗华域)
    /// <para>仅适用于QrCode</para>
    /// </summary>
    public class GenericGF
    {

        /// <summary>
        /// 指数表
        /// </summary>
        private static readonly int[] ExpTable;
        /// <summary>
        /// 对数表
        /// </summary>
        private static readonly int[] LogTable;

        static GenericGF()
        {
            // 初始化指数表和对数表
            ExpTable = new int[DIMENSION];
            LogTable = new int[DIMENSION];
            int x = 1;
            for (int i = 0; i < DIMENSION; i++)
            {
                ExpTable[i] = x;
                x <<= 1;
                if (x >= DIMENSION)
                {
                    x ^= POLY;
                    x &= DIMENSION - 1;
                }
            }
            for (int i = 0; i < DIMENSION - 1; i++)
            {
                LogTable[ExpTable[i]] = i;
            }
        }

        /// <summary>
        /// 加法
        /// </summary>
        public static int Addition(int a, int b)
        {
            return a ^ b;
        }

        /// <summary>
        /// 2的次方
        /// </summary>
        public static int Exp(int a)
        {
            return ExpTable[a];
        }

        /// <summary>
        /// 逆运算
        /// </summary>
        public static int Inverse(int a)
        {
            return ExpTable[DIMENSION - LogTable[a] - 1];
        }

        /// <summary>
        /// 乘法
        /// </summary>
        public static int Multiply(int a, int b)
        {
            if (a == 0 || b == 0)
            {
                return 0;
            }
            return ExpTable[(LogTable[a] + LogTable[b]) % (DIMENSION - 1)];
        }

        /// <summary>
        /// 维度
        /// <para>256</para>
        /// </summary>
        private static readonly int DIMENSION = 256;
        /// <summary>
        /// 多项式
        /// <para>0x011D -> 0000 0001 0001 1101 -> x^8 + x^4 + x^3 + x^2 + 1</para>
        /// </summary>
        private static readonly int POLY = 0x011D;

    }
}
