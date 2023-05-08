using ConsoleDemo.Util;
using System;
using System.Text;

namespace ConsoleDemo.Test
{

    /// <summary>
    /// 二维码数据生成测试
    /// </summary>
    public class QrCodeDataGenerationTest
    {

        /// <summary>
        /// 测试
        /// </summary>
        public static void Test()
        {
            GenerationFormatInfo();
            GenerationVersionInfo();
        }

        /// <summary>
        /// 生成格式信息
        /// </summary>
        public static void GenerationFormatInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("        private static readonly bool[,][] FORMAT_INFO = new bool[,][]\n        {\n");
            for (int i = 0; i < 4; i++)
            {
                sb.Append("            {");
                for (int j = 0; j < 8; j++)
                {
                    sb.Append("\n                new bool[] {");
                    PrintBoolArray(sb, CalculateFormatInfo(i, j));
                    sb.Append("},");
                }
                sb.Append("\n            },\n");
            }
            sb.Append("        };");
            Console.WriteLine(sb);
        }

        /// <summary>
        /// 生成版本信息(版本7+)
        /// </summary>
        public static void GenerationVersionInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("        private static readonly bool[][] VERSION_INFO = new bool[][]\n        {");
            for (int i = 7; i < 41; i++)
            {
                sb.Append("\n            new bool[] {");
                PrintBoolArray(sb, CalculateVersionInfo(i));
                sb.Append("},");
            }
            sb.Append("\n        };");
            Console.WriteLine(sb);
        }

        /// <summary>
        /// 计算版本信息(版本7+)
        /// </summary>
        /// <param name="versionNumber">版本号</param>
        /// <returns>版本信息</returns>
        private static bool[] CalculateVersionInfo(int versionNumber)
        {
            bool[] versionInfo = new bool[18];
            if (versionNumber < 7)
            {
                return versionInfo;
            }
            // 数据信息6bit(版本号6bit)
            int dataInfo = versionNumber;
            // BCH(18,6)纠错码12bit
            int bchCode = CalculateBchCode(dataInfo, VERSION_INFO_POLY);
            int value = (dataInfo << 12) | bchCode;
            QrCodeUtils.AddBits(versionInfo, 0, value, 18);
            return versionInfo;
        }

        /// <summary>
        /// 打印bool数组
        /// </summary>
        /// <param name="sb">StringBuilder</param>
        /// <param name="array">bool[]</param>
        /// <returns>StringBuilder</returns>
        private static StringBuilder PrintBoolArray(StringBuilder sb, bool[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i])
                {
                    sb.Append(" true, ");
                }
                else
                {
                    sb.Append("false, ");
                }
            }
            return sb;
        }

        /// <summary>
        /// 计算格式信息
        /// </summary>
        /// <param name="level">纠错等级</param>
        /// <param name="id">模板序号</param>
        /// <returns>格式信息</returns>
        private static bool[] CalculateFormatInfo(int level, int id)
        {
            // 数据信息5bit(纠错等级2bit+模板序号3bit)
            int dataInfo;
            // 纠错等级2bit
            switch (level)
            {
                default:
                // 0 L 0b01=1
                case 0:
                    {
                        dataInfo = 1;
                        break;
                    }
                // 1 M 0b00=0
                case 1:
                    {
                        dataInfo = 0;
                        break;
                    }
                // 2 Q 0b11=3
                case 2:
                    {
                        dataInfo = 3;
                        break;
                    }
                // 3 H 0b10=2
                case 3:
                    {
                        dataInfo = 2;
                        break;
                    }
            }
            // 模板序号3bit
            dataInfo = (dataInfo << 3) | id;
            // BCH(15,5)纠错码10bit
            int bchCode = CalculateBchCode(dataInfo, FORMAT_INFO_POLY);
            int value = (dataInfo << 10) | bchCode;
            // 进行`异或`运算
            value ^= FORMAT_INFO_MASK_PATTERN;
            bool[] formatInfo = new bool[15];
            QrCodeUtils.AddBits(formatInfo, 0, value, 15);
            return formatInfo;
        }

        /// <summary>
        /// 计算BCH码
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="poly">多项式</param>
        /// <returns>BCH码</returns>
        private static int CalculateBchCode(int value, int poly)
        {
            int msbSetInPoly = FindMsbSet(poly);
            value <<= msbSetInPoly - 1;
            while (FindMsbSet(value) >= msbSetInPoly)
            {
                value ^= poly << (FindMsbSet(value) - msbSetInPoly);
            }
            return value;
        }

        /// <summary>
        /// 查找值的最高有效位
        /// </summary>
        /// <param name="value">值</param>
        /// <returns>最高有效位</returns>
        private static int FindMsbSet(int value)
        {
            int numDigits = 0;
            while (value != 0)
            {
                value = (int)((uint)value >> 1);
                ++numDigits;
            }
            return numDigits;
        }

        /// <summary>
        /// 格式信息多项式
        /// <para>0x0537 -> 0000 0101 0011 0111 -> x^10 + x^8 + x^5 + x^4 + x^2 + x + 1</para>
        /// </summary>
        private static readonly int FORMAT_INFO_POLY = 0x0537;
        /// <summary>
        /// 格式信息掩模
        /// <para>0x5412 -> 0101 0100 0001 0010</para>
        /// </summary>
        private static readonly int FORMAT_INFO_MASK_PATTERN = 0x5412;
        /// <summary>
        /// 版本信息多项式
        /// <para>0x1F25 -> 0001 1111 0010 0101 -> x^12 + x^11 + x^10 + x^9 + x^8 + x^5 + x^2 + 1</para>
        /// </summary>
        private static readonly int VERSION_INFO_POLY = 0x1F25;

    }
}
