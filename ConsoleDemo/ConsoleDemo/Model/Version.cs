namespace ConsoleDemo.Model
{

    /// <summary>
    /// 版本
    /// </summary>
    public class Version
    {

        /// <summary>
        /// 版本号
        /// <para>[1,40]</para>
        /// </summary>
        public readonly int VersionNumber;
        /// <summary>
        /// 尺寸
        /// <para>尺寸 = (版本号 - 1) * 4 + 21</para>
        /// <para>[21,177]</para>
        /// </summary>
        public readonly int Dimension;
        /// <summary>
        /// `内容长度`bit数
        /// </summary>
        public readonly int ContentLengthBits;
        /// <summary>
        /// 数据bit数
        /// <para>ECI模式指示符+ECI指定符+模式指示符+`内容长度`bit数+内容+结束符+补齐符</para>
        /// </summary>
        public readonly int DataBits;
        /// <summary>
        /// 数据和纠错bit数
        /// <para>数据bit数+纠错bit数</para>
        /// </summary>
        public readonly int DataAndEcBits;
        /// <summary>
        /// 纠错
        /// <para>[纠错块,(块数量,纠错码)]</para>
        /// </summary>
        public readonly int[,] Ec;

        /// <summary>
        /// 构造版本
        /// </summary>
        /// <param name="length">
        /// 内容字节数
        /// </param>
        /// <param name="level">
        /// 纠错等级
        /// <para>0 L 7%</para>
        /// <para>1 M 15%</para>
        /// <para>2 Q 25%</para>
        /// <para>3 H 30%</para>
        /// </param>
        /// <param name="mode">
        /// 编码模式
        /// <para>0 NUMERIC 数字0-9</para>
        /// <para>1 ALPHANUMERIC 数字0-9、大写字母A-Z、符号(空格)$%*+-./:</para>
        /// <para>2 BYTE(ISO-8859-1)</para>
        /// <para>3 BYTE(UTF-8)</para>
        /// </param>
        public Version(int length, int level, int mode)
        {
            // 编码模式
            switch (mode)
            {
                // NUMERIC 数字0-9
                case 0:
                    {
                        VersionNumber = ModeNumeric(length, level) + 1;
                        // `内容长度`bit数 1-9版本10bit 10-26版本12bit 27-40版本14bit
                        // 数据来源 ISO/IEC 18004-2015 -> 7.4.1 -> Table 3 -> Numeric mode列
                        ContentLengthBits = VersionNumber < 10 ? 10 : (VersionNumber < 27 ? 12 : 14);
                        break;
                    }
                // ALPHANUMERIC 数字0-9、大写字母A-Z、符号(空格)$%*+-./:
                case 1:
                    {
                        VersionNumber = ModeAlphaNumeric(length, level) + 1;
                        // `内容长度`bit数 1-9版本9bit 10-26版本11bit 27-40版本13bit
                        // 数据来源 ISO/IEC 18004-2015 -> 7.4.1 -> Table 3 -> Alphanumeric mode列
                        ContentLengthBits = VersionNumber < 10 ? 9 : (VersionNumber < 27 ? 11 : 13);
                        break;
                    }
                // BYTE(ISO-8859-1)
                case 2:
                    {
                        VersionNumber = ModeByte(length, level) + 1;
                        // `内容长度`bit数 1-9版本8bit 10-40版本16bit
                        // 数据来源 ISO/IEC 18004-2015 -> 7.4.1 -> Table 3 -> Byte mode列
                        ContentLengthBits = VersionNumber < 10 ? 8 : 16;
                        break;
                    }
                // BYTE(UTF-8)
                default:
                case 3:
                    {
                        // 相比ISO-8859-1多1字节(不需要补齐符的情况下)
                        // ECI模式指示符(4bit)+ECI指定符(8bit)-结束符(4bit)=1字节
                        VersionNumber = ModeByte(length + 1, level) + 1;
                        ContentLengthBits = VersionNumber < 10 ? 8 : 16;
                        break;
                    }
            }
            Dimension = (VersionNumber - 1) * 4 + 21;
            DataBits = DATA_BYTES[VersionNumber - 1, level] * 8;
            DataAndEcBits = DATA_AND_EC_BITS[VersionNumber - 1];
            Ec = EC[VersionNumber - 1, level];
        }

        /// <summary>
        /// 获取编码模式为NUMERIC的版本号
        /// </summary>
        /// <param name="length">内容字节数</param>
        /// <param name="level">纠错等级</param>
        /// <returns>版本号</returns>
        private static int ModeNumeric(int length, int level)
        {
            // `内容长度`bit数 1-9版本10bit
            for (int i = 0; i < 9; i++)
            {
                // 模式指示符(4bit)+`内容长度`bit数(10bit)=14bit
                if (length <= ModeNumericMaxLength(DATA_BYTES[i, level] * 8 - 14))
                {
                    return i;
                }
            }
            // `内容长度`bit数 10-26版本12bit
            for (int i = 9; i < 26; i++)
            {
                // 模式指示符(4bit)+`内容长度`bit数(12bit)=16bit
                if (length <= ModeNumericMaxLength(DATA_BYTES[i, level] * 8 - 16))
                {
                    return i;
                }
            }
            // `内容长度`bit数 27-40版本14bit
            for (int i = 26; i < 40; i++)
            {
                // 模式指示符(4bit)+`内容长度`bit数(14bit)=18bit
                if (length <= ModeNumericMaxLength(DATA_BYTES[i, level] * 8 - 18))
                {
                    return i;
                }
            }
            return 0;
        }

        /// <summary>
        /// 获取编码模式为NUMERIC的最大字符长度
        /// </summary>
        /// <param name="maxBits">最大bit数</param>
        /// <returns>最大字符长度</returns>
        private static int ModeNumericMaxLength(int maxBits)
        {
            // 3个字符10bit 2个字符7bit 1个字符4bit
            int maxLength = maxBits / 10;
            int remainder = maxBits % 10;
            if (remainder > 6)
            {
                maxLength += 2;
            }
            else if (remainder > 3)
            {
                maxLength++;
            }
            return maxLength;
        }

        /// <summary>
        /// 获取编码模式为ALPHANUMERIC的版本号
        /// </summary>
        /// <param name="length">内容字节数</param>
        /// <param name="level">纠错等级</param>
        /// <returns>版本号</returns>
        private static int ModeAlphaNumeric(int length, int level)
        {
            // `内容长度`bit数 1-9版本9bit
            for (int i = 0; i < 9; i++)
            {
                // 模式指示符(4bit)+`内容长度`bit数(9bit)=13bit
                if (length <= ModeAlphaNumericMaxLength(DATA_BYTES[i, level] * 8 - 13))
                {
                    return i;
                }
            }
            // `内容长度`bit数 10-26版本11bit
            for (int i = 9; i < 26; i++)
            {
                // 模式指示符(4bit)+`内容长度`bit数(11bit)=15bit
                if (length <= ModeAlphaNumericMaxLength(DATA_BYTES[i, level] * 8 - 15))
                {
                    return i;
                }
            }
            // `内容长度`bit数 27-40版本13bit
            for (int i = 26; i < 40; i++)
            {
                // 模式指示符(4bit)+`内容长度`bit数(13bit)=17bit
                if (length <= ModeAlphaNumericMaxLength(DATA_BYTES[i, level] * 8 - 17))
                {
                    return i;
                }
            }
            return 0;
        }

        /// <summary>
        /// 获取编码模式为ALPHANUMERIC的最大字符长度
        /// </summary>
        /// <param name="maxBits">最大bit数</param>
        /// <returns>最大字符长度</returns>
        private static int ModeAlphaNumericMaxLength(int maxBits)
        {
            // 2个字符11bit 1个字符6bit
            int maxLength = maxBits / 11;
            int remainder = maxBits % 11;
            if (remainder > 5)
            {
                maxLength++;
            }
            return maxLength;
        }

        /// <summary>
        /// 获取编码模式为BYTE的版本号
        /// </summary>
        /// <param name="length">内容字节数</param>
        /// <param name="level">纠错等级</param>
        /// <returns>版本号</returns>
        private static int ModeByte(int length, int level)
        {
            // `内容长度`bit数 1-9版本8bit
            for (int i = 0; i < 9; i++)
            {
                // 模式指示符(4bit)+`内容长度`bit数(8bit)+结束符(4bit)=2字节
                if (length < DATA_BYTES[i, level] - 1)
                {
                    return i;
                }
            }
            // `内容长度`bit数 10-40版本16bit
            for (int i = 9; i < 40; i++)
            {
                // 模式指示符(4bit)+`内容长度`bit数(16bit)+结束符(4bit)=3字节
                if (length < DATA_BYTES[i, level] - 2)
                {
                    return i;
                }
            }
            return 0;
        }

        /// <summary>
        /// 数据bit数+纠错bit数
        /// <para>索引[版本号]:40</para>
        /// <para>数据来源 ISO/IEC 18004-2015 -> 7.1 -> Table 1 -> Data modules except(C)列</para>
        /// </summary>
        private static readonly int[] DATA_AND_EC_BITS =
        {
              208,   359,   567,   807,  1079, // 1-5
             1383,  1568,  1936,  2336,  2768, // 6-10
             3232,  3728,  4256,  4651,  5243, // 11-15
             5867,  6523,  7211,  7931,  8683, // 16-20
             9252, 10068, 10916, 11796, 12708, // 21-25
            13652, 14628, 15371, 16411, 17483, // 26-30
            18587, 19723, 20891, 22091, 23008, // 31-35
            24272, 25568, 26896, 28256, 29648, // 36-40
        };

        /// <summary>
        /// 数据字节数
        /// <para>索引[版本号,纠错等级]:40x4</para>
        /// <para>数据来源 ISO/IEC 18004-2015 -> 7.4.10 -> Table 7 -> Number of data codewords列</para>
        /// </summary>
        private static readonly int[,] DATA_BYTES =
        {
            {   19,   16,   13,    9 }, {   34,   28,   22,   16 }, {   55,   44,   34,   26 }, {   80,   64,   48,   36 }, {  108,   86,   62,   46 }, // 1-5
            {  136,  108,   76,   60 }, {  156,  124,   88,   66 }, {  194,  154,  110,   86 }, {  232,  182,  132,  100 }, {  274,  216,  154,  122 }, // 6-10
            {  324,  254,  180,  140 }, {  370,  290,  206,  158 }, {  428,  334,  244,  180 }, {  461,  365,  261,  197 }, {  523,  415,  295,  223 }, // 11-15
            {  589,  453,  325,  253 }, {  647,  507,  367,  283 }, {  721,  563,  397,  313 }, {  795,  627,  445,  341 }, {  861,  669,  485,  385 }, // 16-20
            {  932,  714,  512,  406 }, { 1006,  782,  568,  442 }, { 1094,  860,  614,  464 }, { 1174,  914,  664,  514 }, { 1276, 1000,  718,  538 }, // 21-25
            { 1370, 1062,  754,  596 }, { 1468, 1128,  808,  628 }, { 1531, 1193,  871,  661 }, { 1631, 1267,  911,  701 }, { 1735, 1373,  985,  745 }, // 26-30
            { 1843, 1455, 1033,  793 }, { 1955, 1541, 1115,  845 }, { 2071, 1631, 1171,  901 }, { 2191, 1725, 1231,  961 }, { 2306, 1812, 1286,  986 }, // 31-35
            { 2434, 1914, 1354, 1054 }, { 2566, 1992, 1426, 1096 }, { 2702, 2102, 1502, 1142 }, { 2812, 2216, 1582, 1222 }, { 2956, 2334, 1666, 1276 }, // 36-40
        };

        /// <summary>
        /// 纠错
        /// <para>索引[版本号,纠错等级][纠错块,(块数量,纠错码)]:40x4x?x2</para>
        /// <para>数据来源 ISO/IEC 18004-2015 -> 7.5.1 -> Table 9 -> Number of error correction blocks列 和 Error correction code per block列的k</para>
        /// </summary>
        private static readonly int[,][,] EC =
        {
            // 1
            {
                new int[,] { {  1,  19 } },
                new int[,] { {  1,  16 } },
                new int[,] { {  1,  13 } },
                new int[,] { {  1,   9 } },
            },
            // 2
            {
                new int[,] { {  1,  34 } },
                new int[,] { {  1,  28 } },
                new int[,] { {  1,  22 } },
                new int[,] { {  1,  16 } },
            },
            // 3
            {
                new int[,] { {  1,  55 } },
                new int[,] { {  1,  44 } },
                new int[,] { {  2,  17 } },
                new int[,] { {  2,  13 } },
            },
            // 4
            {
                new int[,] { {  1,  80 } },
                new int[,] { {  2,  32 } },
                new int[,] { {  2,  24 } },
                new int[,] { {  4,   9 } },
            },
            // 5
            {
                new int[,] { {  1, 108 } },
                new int[,] { {  2,  43 } },
                new int[,] { {  2,  15 }, {  2,  16 } },
                new int[,] { {  2,  11 }, {  2,  12 } },
            },
            // 6
            {
                new int[,] { {  2,  68 } },
                new int[,] { {  4,  27 } },
                new int[,] { {  4,  19 } },
                new int[,] { {  4,  15 } },
            },
            // 7
            {
                new int[,] { {  2,  78 } },
                new int[,] { {  4,  31 } },
                new int[,] { {  2,  14 }, {  4,  15 } },
                new int[,] { {  4,  13 }, {  1,  14 } },
            },
            // 8
            {
                new int[,] { {  2,  97 } },
                new int[,] { {  2,  38 }, {  2,  39 } },
                new int[,] { {  4,  18 }, {  2,  19 } },
                new int[,] { {  4,  14 }, {  2,  15 } },
            },
            // 9
            {
                new int[,] { {  2, 116 } },
                new int[,] { {  3,  36 }, {  2,  37 } },
                new int[,] { {  4,  16 }, {  4,  17 } },
                new int[,] { {  4,  12 }, {  4,  13 } },
            },
            // 10
            {
                new int[,] { {  2,  68 }, {  2,  69 } },
                new int[,] { {  4,  43 }, {  1,  44 } },
                new int[,] { {  6,  19 }, {  2,  20 } },
                new int[,] { {  6,  15 }, {  2,  16 } },
            },
            // 11
            {
                new int[,] { {  4,  81 } },
                new int[,] { {  1,  50 }, {  4,  51 } },
                new int[,] { {  4,  22 }, {  4,  23 } },
                new int[,] { {  3,  12 }, {  8,  13 } },
            },
            // 12
            {
                new int[,] { {  2,  92 }, {  2,  93 } },
                new int[,] { {  6,  36 }, {  2,  37 } },
                new int[,] { {  4,  20 }, {  6,  21 } },
                new int[,] { {  7,  14 }, {  4,  15 } },
            },
            // 13
            {
                new int[,] { {  4, 107 } },
                new int[,] { {  8,  37 }, {  1,  38 } },
                new int[,] { {  8,  20 }, {  4,  21 } },
                new int[,] { { 12,  11 }, {  4,  12 } },
            },
            // 14
            {
                new int[,] { {  3, 115 }, {  1, 116 } },
                new int[,] { {  4,  40 }, {  5,  41 } },
                new int[,] { { 11,  16 }, {  5,  17 } },
                new int[,] { { 11,  12 }, {  5,  13 } },
            },
            // 15
            {
                new int[,] { {  5,  87 }, {  1,  88 } },
                new int[,] { {  5,  41 }, {  5,  42 } },
                new int[,] { {  5,  24 }, {  7,  25 } },
                new int[,] { { 11,  12 }, {  7,  13 } },
            },
            // 16
            {
                new int[,] { {  5,  98 }, {  1,  99 } },
                new int[,] { {  7,  45 }, {  3,  46 } },
                new int[,] { { 15,  19 }, {  2,  20 } },
                new int[,] { {  3,  15 }, { 13,  16 } },
            },
            // 17
            {
                new int[,] { {  1, 107 }, {  5, 108 } },
                new int[,] { { 10,  46 }, {  1,  47 } },
                new int[,] { {  1,  22 }, { 15,  23 } },
                new int[,] { {  2,  14 }, { 17,  15 } },
            },
            // 18
            {
                new int[,] { {  5, 120 }, {  1, 121 } },
                new int[,] { {  9,  43 }, {  4,  44 } },
                new int[,] { { 17,  22 }, {  1,  23 } },
                new int[,] { {  2,  14 }, { 19,  15 } },
            },
            // 19
            {
                new int[,] { {  3, 113 }, {  4, 114 } },
                new int[,] { {  3,  44 }, { 11,  45 } },
                new int[,] { { 17,  21 }, {  4,  22 } },
                new int[,] { {  9,  13 }, { 16,  14 } },
            },
            // 20
            {
                new int[,] { {  3, 107 }, {  5, 108 } },
                new int[,] { {  3,  41 }, { 13,  42 } },
                new int[,] { { 15,  24 }, {  5,  25 } },
                new int[,] { { 15,  15 }, { 10,  16 } },
            },
            // 21
            {
                new int[,] { {  4, 116 }, {  4, 117 } },
                new int[,] { { 17,  42 } },
                new int[,] { { 17,  22 }, {  6,  23 } },
                new int[,] { { 19,  16 }, {  6,  17 } },
            },
            // 22
            {
                new int[,] { {  2, 111 }, {  7, 112 } },
                new int[,] { { 17,  46 } },
                new int[,] { {  7,  24 }, { 16,  25 } },
                new int[,] { { 34,  13 } },
            },
            // 23
            {
                new int[,] { {  4, 121 }, {  5, 122 } },
                new int[,] { {  4,  47 }, { 14,  48 } },
                new int[,] { { 11,  24 }, { 14,  25 } },
                new int[,] { { 16,  15 }, { 14,  16 } },
            },
            // 24
            {
                new int[,] { {  6, 117 }, {  4, 118 } },
                new int[,] { {  6,  45 }, { 14,  46 } },
                new int[,] { { 11,  24 }, { 16,  25 } },
                new int[,] { { 30,  16 }, {  2,  17 } },
            },
            // 25
            {
                new int[,] { {  8, 106 }, {  4, 107 } },
                new int[,] { {  8,  47 }, { 13,  48 } },
                new int[,] { {  7,  24 }, { 22,  25 } },
                new int[,] { { 22,  15 }, { 13,  16 } },
            },
            // 26
            {
                new int[,] { { 10, 114 }, {  2, 115 } },
                new int[,] { { 19,  46 }, {  4,  47 } },
                new int[,] { { 28,  22 }, {  6,  23 } },
                new int[,] { { 33,  16 }, {  4,  17 } },
            },
            // 27
            {
                new int[,] { {  8, 122 }, {  4, 123 } },
                new int[,] { { 22,  45 }, {  3,  46 } },
                new int[,] { {  8,  23 }, { 26,  24 } },
                new int[,] { { 12,  15 }, { 28,  16 } },
            },
            // 28
            {
                new int[,] { {  3, 117 }, { 10, 118 } },
                new int[,] { {  3,  45 }, { 23,  46 } },
                new int[,] { {  4,  24 }, { 31,  25 } },
                new int[,] { { 11,  15 }, { 31,  16 } },
            },
            // 29
            {
                new int[,] { {  7, 116 }, {  7, 117 } },
                new int[,] { { 21,  45 }, {  7,  46 } },
                new int[,] { {  1,  23 }, { 37,  24 } },
                new int[,] { { 19,  15 }, { 26,  16 } },
            },
            // 30
            {
                new int[,] { {  5, 115 }, { 10, 116 } },
                new int[,] { { 19,  47 }, { 10,  48 } },
                new int[,] { { 15,  24 }, { 25,  25 } },
                new int[,] { { 23,  15 }, { 25,  16 } },
            },
            // 31
            {
                new int[,] { { 13, 115 }, {  3, 116 } },
                new int[,] { {  2,  46 }, { 29,  47 } },
                new int[,] { { 42,  24 }, {  1,  25 } },
                new int[,] { { 23,  15 }, { 28,  16 } },
            },
            // 32
            {
                new int[,] { { 17, 115 } },
                new int[,] { { 10,  46 }, { 23,  47 } },
                new int[,] { { 10,  24 }, { 35,  25 } },
                new int[,] { { 19,  15 }, { 35,  16 } },
            },
            // 33
            {
                new int[,] { { 17, 115 }, {  1, 116 } },
                new int[,] { { 14,  46 }, { 21,  47 } },
                new int[,] { { 29,  24 }, { 19,  25 } },
                new int[,] { { 11,  15 }, { 46,  16 } },
            },
            // 34
            {
                new int[,] { { 13, 115 }, {  6, 116 } },
                new int[,] { { 14,  46 }, { 23,  47 } },
                new int[,] { { 44,  24 }, {  7,  25 } },
                new int[,] { { 59,  16 }, {  1,  17 } },
            },
            // 35
            {
                new int[,] { { 12, 121 }, {  7, 122 } },
                new int[,] { { 12,  47 }, { 26,  48 } },
                new int[,] { { 39,  24 }, { 14,  25 } },
                new int[,] { { 22,  15 }, { 41,  16 } },
            },
            // 36
            {
                new int[,] { {  6, 121 }, { 14, 122 } },
                new int[,] { {  6,  47 }, { 34,  48 } },
                new int[,] { { 46,  24 }, { 10,  25 } },
                new int[,] { {  2,  15 }, { 64,  16 } },
            },
            // 37
            {
                new int[,] { { 17, 122 }, {  4, 123 } },
                new int[,] { { 29,  46 }, { 14,  47 } },
                new int[,] { { 49,  24 }, { 10,  25 } },
                new int[,] { { 24,  15 }, { 46,  16 } },
            },
            // 38
            {
                new int[,] { {  4, 122 }, { 18, 123 } },
                new int[,] { { 13,  46 }, { 32,  47 } },
                new int[,] { { 48,  24 }, { 14,  25 } },
                new int[,] { { 42,  15 }, { 32,  16 } },
            },
            // 39
            {
                new int[,] { { 20, 117 }, {  4, 118 } },
                new int[,] { { 40,  47 }, {  7,  48 } },
                new int[,] { { 43,  24 }, { 22,  25 } },
                new int[,] { { 10,  15 }, { 67,  16 } },
            },
            // 40
            {
                new int[,] { { 19, 118 }, {  6, 119 } },
                new int[,] { { 18,  47 }, { 31,  48 } },
                new int[,] { { 34,  24 }, { 34,  25 } },
                new int[,] { { 20,  15 }, { 61,  16 } },
            },
       };

    }
}
