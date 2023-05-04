namespace ConsoleDemo.Model
{

    /// <summary>
    /// GenericGF(QrCode)
    /// </summary>
    public class GenericGF
    {

        /// <summary>
        /// 多项式
        /// <para>0x011D -> 0000 0001 0001 1101 -> x^8 + x^4 + x^3 + x^2 + 1</para>
        /// </summary>
        private static readonly int Primitive = 0x011D;
        /// <summary>
        /// 大小
        /// <para>256</para>
        /// </summary>
        private static readonly int Size = 256;
        /// <summary>
        /// 指数表
        /// </summary>
        private static readonly int[] ExpTable;
        /// <summary>
        /// 对数表
        /// </summary>
        private static readonly int[] LogTable;

        /// <summary>
        /// 0
        /// </summary>
        public static readonly GenericGFPoly Zero = new GenericGFPoly(new int[] { 0 });

        static GenericGF()
        {
            // 生成指数表和对数表
            ExpTable = new int[Size];
            LogTable = new int[Size];
            int x = 1;
            for (int i = 0; i < Size; i++)
            {
                ExpTable[i] = x;
                x <<= 1;
                if (x >= Size)
                {
                    x ^= Primitive;
                    x &= Size - 1;
                }
            }
            for (int i = 0; i < Size - 1; i++)
            {
                LogTable[ExpTable[i]] = i;
            }
        }

        /// <summary>
        /// 构建单项式
        /// </summary>
        public static GenericGFPoly BuildMonomial(int degree, int coefficient)
        {
            if (coefficient == 0)
            {
                return Zero;
            }
            int[] coefficients = new int[degree + 1];
            coefficients[0] = coefficient;
            return new GenericGFPoly(coefficients);
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
        /// 以2位底的对数
        /// </summary>
        public static int Log(int a)
        {
            return LogTable[a];
        }

        /// <summary>
        /// 反转
        /// </summary>
        public static int Inverse(int a)
        {
            return ExpTable[Size - LogTable[a] - 1];
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
            return ExpTable[(LogTable[a] + LogTable[b]) % (Size - 1)];
        }

    }
}
