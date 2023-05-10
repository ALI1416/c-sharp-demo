using System.Text;
using ConsoleDemo.Util;

namespace ConsoleDemo.Model
{

    /// <summary>
    /// 二维码
    /// </summary>
    public class QrCode
    {

        /// <summary>
        /// 版本
        /// </summary>
        public readonly Version Version;
        /// <summary>
        /// 掩模模板
        /// </summary>
        public readonly MaskPattern MaskPattern;
        /// <summary>
        /// 矩阵
        /// <para>false白 true黑</para>
        /// </summary>
        public readonly bool[,] Matrix;

        /// <summary>
        /// 构造二维码
        /// </summary>
        /// <param name="content">
        /// 内容
        /// </param>
        /// <param name="level">
        /// 纠错等级
        /// <para>0 L 7%</para>
        /// <para>1 M 15%</para>
        /// <para>2 Q 25%</para>
        /// <para>3 H 30%</para>
        /// </param>
        public QrCode(string content, int level)
        {
            /* 数据 */
            // 内容bits
            byte[] bits = Encoding.UTF8.GetBytes(content);
            // 获取模式
            int mode = DetectionMode(bits, content.Length);
            // 获取版本
            Version = new Version(bits.Length, level, mode);
            // 数据bits
            bool[] dataBits = new bool[Version.DataBits];
            // 填充数据
            switch (mode)
            {
                // 填充编码模式为NUMERIC的数据
                case 0:
                    {
                        ModeNumbers(dataBits, bits, Version);
                        break;
                    }
                // 填充编码模式为ALPHANUMERIC的数据
                case 1:
                    {
                        ModeAlphaNumeric(dataBits, bits, Version);
                        break;
                    }
                // 填充编码模式为BYTE编码格式为ISO-8859-1的数据
                case 2:
                    {
                        ModeByteIso88591(dataBits, bits, Version);
                        break;
                    }
                // 填充编码模式为BYTE编码格式为UTF-8的数据
                default:
                case 3:
                    {
                        ModeByteUtf8(dataBits, bits, Version);
                        break;
                    }
            }

            /* 纠错 */
            int[,] ec = Version.Ec;
            // 数据块数 或 纠错块数
            int blocks = 0;
            for (int i = 0; i < Version.Ec.GetLength(0); i++)
            {
                blocks += Version.Ec[i, 0];
            }
            // 纠错块字节数
            int ecBlockBytes = (Version.DataAndEcBits - Version.DataBits) / 8 / blocks;
            int[][] dataBlocks = new int[blocks][];
            int[][] ecBlocks = new int[blocks][];
            int blockNum = 0;
            int dataByteNum = 0;
            for (int i = 0; i < ec.GetLength(0); i++)
            {
                int count = ec[i, 0];
                int dataBytes = ec[i, 1];
                for (int j = 0; j < count; j++)
                {
                    // 数据块
                    int[] dataBlock = QrCodeUtils.GetBytes(dataBits, dataByteNum * 8, dataBytes);
                    dataBlocks[blockNum] = dataBlock;
                    // 纠错块
                    int[] ecBlock = ReedSolomon.Encoder(dataBlock, ecBlockBytes);
                    ecBlocks[blockNum] = ecBlock;
                    blockNum++;
                    dataByteNum += dataBytes;
                }
            }

            /* 交叉数据和纠错 */
            bool[] dataAndEcBits = new bool[Version.DataAndEcBits];
            int dataBlockMaxBytes = dataBlocks[blocks - 1].Length;
            int dataAndEcBitPtr = 0;
            for (int i = 0; i < dataBlockMaxBytes; i++)
            {
                for (int j = 0; j < blocks; j++)
                {
                    if (dataBlocks[j].Length > i)
                    {
                        QrCodeUtils.AddBits(dataAndEcBits, dataAndEcBitPtr, dataBlocks[j][i], 8);
                        dataAndEcBitPtr += 8;
                    }
                }
            }
            for (int i = 0; i < ecBlockBytes; i++)
            {
                for (int j = 0; j < blocks; j++)
                {
                    QrCodeUtils.AddBits(dataAndEcBits, dataAndEcBitPtr, ecBlocks[j][i], 8);
                    dataAndEcBitPtr += 8;
                }
            }

            /* 构造掩模模板 */
            MaskPattern = new MaskPattern(dataAndEcBits, Version, level);
            Matrix = QrCodeUtils.Convert(MaskPattern.BestPattern, Version.Dimension);
        }

        /// <summary>
        /// 填充编码模式为NUMERIC的数据
        /// </summary>
        /// <param name="dataBits">数据bits</param>
        /// <param name="bits">内容bits</param>
        /// <param name="version">版本</param>
        private static void ModeNumbers(bool[] dataBits, byte[] bits, Version version)
        {

        }

        /// <summary>
        /// 填充编码模式为ALPHANUMERIC的数据
        /// </summary>
        /// <param name="dataBits">数据bits</param>
        /// <param name="bits">内容bits</param>
        /// <param name="version">版本</param>
        private static void ModeAlphaNumeric(bool[] dataBits, byte[] bits, Version version)
        {

        }

        /// <summary>
        /// 填充编码模式为BYTE编码格式为ISO-8859-1的数据
        /// </summary>
        /// <param name="dataBits">数据bits</param>
        /// <param name="bits">内容bits</param>
        /// <param name="version">版本</param>
        private static void ModeByteIso88591(bool[] dataBits, byte[] bits, Version version)
        {
            // 内容字节数
            int contentBytes = bits.Length;
            // 数据指针
            int ptr = 0;
            // 模式指示符(4bit) BYTE 0b0100=4
            // 数据来源 ISO/IEC 18004-2015 -> 7.4.1 -> Table 2 -> QR Code symbols列Byte行
            QrCodeUtils.AddBits(dataBits, ptr, 4, 4);
            ptr += 4;
            // `内容长度`bit数(8/16bit)
            int contentBytesBits = version.ContentLengthBits;
            QrCodeUtils.AddBits(dataBits, ptr, contentBytes, contentBytesBits);
            ptr += contentBytesBits;
            // 内容
            for (int i = 0; i < contentBytes; i++)
            {
                QrCodeUtils.AddBits(dataBits, ptr, bits[i], 8);
                ptr += 8;
            }
            // 结束符(4bit) 0b0000=0
            // 数据来源 ISO/IEC 18004-2015 -> 7.4.9
            QrCodeUtils.AddBits(dataBits, ptr, 0, 4);
            ptr += 4;
            // 补齐符 交替0b11101100=0xEC和0b00010001=0x11至填满
            // 数据来源 ISO/IEC 18004-2015 -> 7.4.10
            int paddingCount = (version.DataBits - ptr) / 8;
            if (paddingCount > 0)
            {
                Padding(dataBits, paddingCount, ptr);
            }
        }

        /// <summary>
        /// 填充编码模式为BYTE编码格式为UTF-8的数据
        /// </summary>
        /// <param name="dataBits">数据bits</param>
        /// <param name="bits">内容bits</param>
        /// <param name="version">版本</param>
        private static void ModeByteUtf8(bool[] dataBits, byte[] bits, Version version)
        {
            // 内容字节数
            int contentBytes = bits.Length;
            // 数据指针
            int ptr = 0;
            // ECI模式指示符(4bit) 0b0111=7
            // 数据来源 ISO/IEC 18004-2015 -> 7.4.1 -> Table 2 -> QR Code symbols列ECI行
            QrCodeUtils.AddBits(dataBits, ptr, 7, 4);
            ptr += 4;
            // ECI指定符 UTF-8(8bit) 0b00011010=26
            // 数据来源 ?
            QrCodeUtils.AddBits(dataBits, ptr, 26, 8);
            ptr += 8;
            // 模式指示符(4bit) BYTE 0b0100=4
            // 数据来源 ISO/IEC 18004-2015 -> 7.4.1 -> Table 2 -> QR Code symbols列Byte行
            QrCodeUtils.AddBits(dataBits, ptr, 4, 4);
            ptr += 4;
            // `内容长度`bit数(8/16bit)
            int contentBytesBits = version.ContentLengthBits;
            QrCodeUtils.AddBits(dataBits, ptr, contentBytes, contentBytesBits);
            ptr += contentBytesBits;
            // 内容
            for (int i = 0; i < contentBytes; i++)
            {
                QrCodeUtils.AddBits(dataBits, ptr, bits[i], 8);
                ptr += 8;
            }
            // 如果有刚好填满，则不需要结束符和补齐符
            // 如果还剩8bit，需要8bit结束符，不用补齐符
            // 如果还剩16+bit，需要8bit结束符，交替补齐符至填满
            if (version.DataBits - ptr > 7)
            {
                // 结束符 UTF-8(8bit) 0b00000000=0
                // 数据来源 ISO/IEC 18004-2015 -> 7.4.9
                QrCodeUtils.AddBits(dataBits, ptr, 0, 8);
                ptr += 8;
            }
            // 补齐符 交替0b11101100=0xEC和0b00010001=0x11至填满
            // 数据来源 ISO/IEC 18004-2015 -> 7.4.10
            int paddingCount = (version.DataBits - ptr) / 8;
            if (paddingCount > 0)
            {
                Padding(dataBits, paddingCount, ptr);
            }
        }

        /// <summary>
        /// 填充补齐符
        /// </summary>
        /// <param name="dataBits">数据bits</param>
        /// <param name="ptr">数据指针</param>
        /// <param name="paddingCount">补齐符个数</param>
        private static void Padding(bool[] dataBits, int paddingCount, int ptr)
        {
            bool[] number0xecBits = QrCodeUtils.GetBits(0xEC, 10);
            bool[] number0x11Bits = QrCodeUtils.GetBits(0x11, 8);
            for (int i = 0; i < paddingCount; i++)
            {
                if (i % 2 == 0)
                {
                    QrCodeUtils.AddBits(dataBits, ptr, number0xecBits, 8);
                }
                else
                {
                    QrCodeUtils.AddBits(dataBits, ptr, number0x11Bits, 8);
                }
                ptr += 8;
            }
        }

        /// <summary>
        /// 探测编码模式
        /// </summary>
        /// <param name="bits">bits</param>
        /// <param name="length">内容字符数</param>
        /// <returns>
        /// 编码模式
        /// <para>0 NUMERIC 数字0-9</para>
        /// <para>1 ALPHANUMERIC 数字0-9、大写字母A-Z、符号(空格)$%*+-./:</para>
        /// <para>2 BYTE(ISO-8859-1)</para>
        /// <para>3 BYTE(UTF-8)</para>
        /// </returns>
        private static int DetectionMode(byte[] bits, int length)
        {
            // BYTE(UTF-8)
            if (bits.Length != length)
            {
                return 3;
            }
            // NUMERIC 数字0-9
            else if (IsAllNumbers(bits, length))
            {
                return 0;
            }
            // ALPHANUMERIC 数字0-9、大写字母A-Z、符号(空格)$%*+-./:
            else if (IsAllAlphaNumeric(bits, length))
            {
                return 1;
            }
            // BYTE(ISO-8859-1)
            else
            {
                return 2;
            }
        }

        /// <summary>
        /// 全部是数字
        /// </summary>
        /// <param name="bits">bits</param>
        /// <param name="length">字节数</param>
        /// <returns>是否全部是数字</returns>
        private static bool IsAllNumbers(byte[] bits, int length)
        {
            for (int i = 0; i < length; i++)
            {
                if (ALPHA_NUMERIC_TABLE[bits[i]] > 9)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 全部是数字+字母+符号
        /// </summary>
        /// <param name="bits">bits</param>
        /// <param name="length">字节数</param>
        /// <returns>是否全部是数字+字母+符号</returns>
        private static bool IsAllAlphaNumeric(byte[] bits, int length)
        {
            for (int i = 0; i < length; i++)
            {
                if (ALPHA_NUMERIC_TABLE[bits[i]] > 44)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// ALPHANUMERIC模式映射表
        /// <para>数字0-9 [0x30,0x39] [0,9]</para>
        /// <para>大写字母A-Z [0x41,0x5A] [10,35]</para>
        /// <para>(空格) [0x20] [36]</para>
        /// <para>$ [0x24] [37]</para>
        /// <para>% [0x25] [38]</para>
        /// <para>* [0x2A] [39]</para>
        /// <para>+ [0x2B] [40]</para>
        /// <para>- [0x2D] [41]</para>
        /// <para>. [0x2E] [42]</para>
        /// <para>/ [0x2F] [43]</para>
        /// <para>: [0x3A] [44]</para>
        /// </summary>
        private static readonly byte[] ALPHA_NUMERIC_TABLE = new byte[]
        {
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, // 0x00-0x0F
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, // 0x10-0x1F
             36, 255, 255, 255,  37,  38, 255, 255, 255, 255,  39,  40, 255,  41,  42,  43, // 0x20-0x2F
              0,   1,   2,   3,   4,   5,   6,   7,   8,   9,  44, 255, 255, 255, 255, 255, // 0x30-0x3F
            255,  10,  11,  12,  13,  14,  15,  16,  17,  18,  19,  20,  21,  22,  23,  24, // 0x40-0x4F
             25,  26,  27,  28,  29,  30,  31,  32,  33,  34,  35, 255, 255, 255, 255, 255, // 0x50-0x5F
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, // 0x60-0x6F
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, // 0x70-0x7F
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, // 0x80-0x8F
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, // 0x90-0x9F
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, // 0xA0-0xAF
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, // 0xB0-0xBF
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, // 0xC0-0xCF
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, // 0xD0-0xDF
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, // 0xE0-0xEF
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, // 0xF0-0xFF
        };

    }
}
