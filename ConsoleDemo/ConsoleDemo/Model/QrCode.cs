using System;
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
        /// <para>编码模式 自动探测</para>
        /// <para>纠错等级 0 L 7%</para>
        /// <para>版本号 最小版本</para>
        /// </summary>
        /// <param name="content">
        /// 内容
        /// </param>
        public QrCode(string content) : this(content, null, null, null) { }

        /// <summary>
        /// 构造二维码
        /// <para>纠错等级 0 L 7%</para>
        /// <para>版本号 最小版本</para>
        /// </summary>
        /// <param name="content">
        /// 内容
        /// </param>
        /// <param name="mode">
        /// 编码模式(默认自动探测)
        /// <para>0 NUMERIC 数字0-9</para>
        /// <para>1 ALPHANUMERIC 数字0-9、大写字母A-Z、符号(空格)$%*+-./:</para>
        /// <para>2 BYTE(ISO-8859-1)</para>
        /// <para>3 BYTE(UTF-8)</para>
        /// </param>
        public QrCode(string content, int? mode) : this(content, mode, null, null) { }

        /// <summary>
        /// 构造二维码
        /// <para>版本号 最小版本</para>
        /// </summary>
        /// <param name="content">
        /// 内容
        /// </param>
        /// <param name="mode">
        /// 编码模式(默认自动探测)
        /// <para>0 NUMERIC 数字0-9</para>
        /// <para>1 ALPHANUMERIC 数字0-9、大写字母A-Z、符号(空格)$%*+-./:</para>
        /// <para>2 BYTE(ISO-8859-1)</para>
        /// <para>3 BYTE(UTF-8)</para>
        /// </param>
        /// <param name="level">
        /// 纠错等级
        /// <para>0 L 7%(默认)</para>
        /// <para>1 M 15%</para>
        /// <para>2 Q 25%</para>
        /// <para>3 H 30%</para>
        /// </param>
        public QrCode(string content, int? mode, int? level) : this(content, mode, level, null) { }

        /// <summary>
        /// 构造二维码
        /// </summary>
        /// <param name="content">
        /// 内容
        /// </param>
        /// <param name="mode">
        /// 编码模式(默认自动探测)
        /// <para>0 NUMERIC 数字0-9</para>
        /// <para>1 ALPHANUMERIC 数字0-9、大写字母A-Z、符号(空格)$%*+-./:</para>
        /// <para>2 BYTE(ISO-8859-1)</para>
        /// <para>3 BYTE(UTF-8)</para>
        /// </param>
        /// <param name="level">
        /// 纠错等级
        /// <para>0 L 7%(默认)</para>
        /// <para>1 M 15%</para>
        /// <para>2 Q 25%</para>
        /// <para>3 H 30%</para>
        /// </param>
        /// <param name="versionNumber">
        /// 版本号(默认最小版本)
        /// <para>[1,40]</para>
        /// </param>
        public QrCode(string content, int? mode, int? level, int? versionNumber)
        {
            int modeValue;
            int levelValue;
            /* 数据 */
            // 编码模式
            if (mode == null)
            {
                modeValue = DetectionMode(content);
            }
            else if (mode < 0 || mode > 3)
            {
                throw new ArgumentException("编码模式不合法！");
            }
            else
            {
                modeValue = (int)mode;
            }
            // 内容bytes
            byte[] contentBytes;
            if (modeValue == 3)
            {
                contentBytes = UTF8.GetBytes(content);
            }
            else
            {
                contentBytes = ISO88591.GetBytes(content);
            }
            // 纠错等级
            if (level == null)
            {
                levelValue = 0;
            }
            else if (mode < 0 || mode > 3)
            {
                throw new ArgumentException("纠错等级不合法！");
            }
            else
            {
                levelValue = (int)level;
            }
            // 版本
            Version = new Version(contentBytes.Length, levelValue, modeValue, versionNumber);
            // 数据bits
            bool[] dataBits = new bool[Version.DataBits];
            // 填充数据
            switch (modeValue)
            {
                // 填充编码模式为NUMERIC的数据
                case 0:
                    {
                        ModeNumbers(dataBits, contentBytes, Version);
                        break;
                    }
                // 填充编码模式为ALPHANUMERIC的数据
                case 1:
                    {
                        ModeAlphaNumeric(dataBits, contentBytes, Version);
                        break;
                    }
                // 填充编码模式为BYTE编码格式为ISO-8859-1的数据
                case 2:
                    {
                        ModeByteIso88591(dataBits, contentBytes, Version);
                        break;
                    }
                // 填充编码模式为BYTE编码格式为UTF-8的数据
                default:
                case 3:
                    {
                        ModeByteUtf8(dataBits, contentBytes, Version);
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
            MaskPattern = new MaskPattern(dataAndEcBits, Version, levelValue);
            Matrix = QrCodeUtils.Convert(MaskPattern.BestPattern, Version.Dimension);
        }

        /// <summary>
        /// 填充编码模式为NUMERIC的数据
        /// </summary>
        /// <param name="dataBits">数据bits</param>
        /// <param name="contentBytes">内容bytes</param>
        /// <param name="version">版本</param>
        private static void ModeNumbers(bool[] dataBits, byte[] contentBytes, Version version)
        {
            // 数据指针
            int ptr = 0;
            // 模式指示符(4bit) NUMERIC 0b0001=1
            // 数据来源 ISO/IEC 18004-2015 -> 7.4.1 -> Table 2 -> QR Code symbols列Numbers行
            QrCodeUtils.AddBits(dataBits, ptr, 1, 4);
            ptr += 4;
            // 内容字节数
            int contentLength = contentBytes.Length;
            // `内容字节数`bit数(10/12/14bit)
            int contentBytesBits = version.ContentBytesBits;
            QrCodeUtils.AddBits(dataBits, ptr, contentLength, contentBytesBits);
            ptr += contentBytesBits;
            // 内容 3个字符10bit 2个字符7bit 1个字符4bit
            for (int i = 0; i < contentLength - 2; i += 3)
            {
                QrCodeUtils.AddBits(dataBits, ptr, (contentBytes[i] - 48) * 100 + (contentBytes[i + 1] - 48) * 10 + contentBytes[i + 2] - 48, 10);
                ptr += 10;
            }
            switch (contentLength % 3)
            {
                case 2:
                    {
                        QrCodeUtils.AddBits(dataBits, ptr, (contentBytes[contentLength - 2] - 48) * 10 + contentBytes[contentLength - 1] - 48, 7);
                        ptr += 7;
                        break;
                    }
                case 1:
                    {
                        QrCodeUtils.AddBits(dataBits, ptr, contentBytes[contentLength - 1] - 48, 4);
                        ptr += 4;
                        break;
                    }
            }
            // 结束符和补齐符
            TerminatorAndPadding(dataBits, version.DataBits, ptr);
        }

        /// <summary>
        /// 填充编码模式为ALPHANUMERIC的数据
        /// </summary>
        /// <param name="dataBits">数据bits</param>
        /// <param name="contentBytes">内容bytes</param>
        /// <param name="version">版本</param>
        private static void ModeAlphaNumeric(bool[] dataBits, byte[] contentBytes, Version version)
        {
            // 数据指针
            int ptr = 0;
            // 模式指示符(4bit) ALPHANUMERIC 0b0010=2
            // 数据来源 ISO/IEC 18004-2015 -> 7.4.1 -> Table 2 -> QR Code symbols列Alphanumeric行
            QrCodeUtils.AddBits(dataBits, ptr, 2, 4);
            ptr += 4;
            // 内容字节数
            int contentLength = contentBytes.Length;
            // `内容字节数`bit数(9/11/13bit)
            int contentBytesBits = version.ContentBytesBits;
            QrCodeUtils.AddBits(dataBits, ptr, contentLength, contentBytesBits);
            ptr += contentBytesBits;
            // 内容 2个字符11bit 1个字符6bit
            for (int i = 0; i < contentLength - 1; i += 2)
            {
                QrCodeUtils.AddBits(dataBits, ptr, ALPHA_NUMERIC_TABLE[contentBytes[i]] * 45 + ALPHA_NUMERIC_TABLE[contentBytes[i + 1]], 11);
                ptr += 11;
            }
            if (contentLength % 2 == 1)
            {
                QrCodeUtils.AddBits(dataBits, ptr, ALPHA_NUMERIC_TABLE[contentBytes[contentLength - 1]], 6);
                ptr += 6;
            }
            // 结束符和补齐符
            TerminatorAndPadding(dataBits, version.DataBits, ptr);
        }

        /// <summary>
        /// 填充编码模式为BYTE编码格式为ISO-8859-1的数据
        /// </summary>
        /// <param name="dataBits">数据bits</param>
        /// <param name="contentBytes">内容bytes</param>
        /// <param name="version">版本</param>
        private static void ModeByteIso88591(bool[] dataBits, byte[] contentBytes, Version version)
        {
            // 数据指针
            int ptr = 0;
            // 模式指示符(4bit) BYTE 0b0100=4
            // 数据来源 ISO/IEC 18004-2015 -> 7.4.1 -> Table 2 -> QR Code symbols列Byte行
            QrCodeUtils.AddBits(dataBits, ptr, 4, 4);
            ptr += 4;
            // 内容字节数
            int contentLength = contentBytes.Length;
            // `内容字节数`bit数(8/16bit)
            int contentBytesBits = version.ContentBytesBits;
            QrCodeUtils.AddBits(dataBits, ptr, contentLength, contentBytesBits);
            ptr += contentBytesBits;
            // 内容
            for (int i = 0; i < contentLength; i++)
            {
                QrCodeUtils.AddBits(dataBits, ptr, contentBytes[i], 8);
                ptr += 8;
            }
            // 结束符和补齐符
            TerminatorAndPadding(dataBits, version.DataBits, ptr);
        }

        /// <summary>
        /// 填充编码模式为BYTE编码格式为UTF-8的数据
        /// </summary>
        /// <param name="dataBits">数据bits</param>
        /// <param name="contentBytes">内容bytes</param>
        /// <param name="version">版本</param>
        private static void ModeByteUtf8(bool[] dataBits, byte[] contentBytes, Version version)
        {
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
            // 内容字节数
            int contentLength = contentBytes.Length;
            // `内容字节数`bit数(8/16bit)
            int contentBytesBits = version.ContentBytesBits;
            QrCodeUtils.AddBits(dataBits, ptr, contentLength, contentBytesBits);
            ptr += contentBytesBits;
            // 内容
            for (int i = 0; i < contentLength; i++)
            {
                QrCodeUtils.AddBits(dataBits, ptr, contentBytes[i], 8);
                ptr += 8;
            }
            // 结束符和补齐符
            TerminatorAndPadding(dataBits, version.DataBits, ptr);
        }

        /// <summary>
        /// 结束符和补齐符
        /// </summary>
        /// <param name="data">数据bits</param>
        /// <param name="dataBits">数据bits数</param>
        /// <param name="ptr">数据指针</param>
        private static void TerminatorAndPadding(bool[] data, int dataBits, int ptr)
        {
            // 如果有刚好填满，则不需要结束符和补齐符
            // 如果还剩1-8bit，需要1-8bit结束符，不用补齐符
            // 如果还剩8+bit，先填充4bit结束符，再填充结束符使8bit对齐，再交替补齐符至填满
            if (dataBits - ptr > 7)
            {
                // 结束符(4bit)
                // 数据来源 ISO/IEC 18004-2015 -> 7.4.9
                ptr += 4;
                // 结束符(8bit对齐)
                ptr = (((ptr - 1) / 8) + 1) * 8;
                // 补齐符 交替0b11101100=0xEC和0b00010001=0x11至填满
                // 数据来源 ISO/IEC 18004-2015 -> 7.4.10
                for (int i = 0; i < dataBits - ptr; i++)
                {
                    if (i % 2 == 0)
                    {
                        Array.Copy(NUMBER_0xEC_8BITS, 0, data, ptr, 8);
                    }
                    else
                    {
                        Array.Copy(NUMBER_0x11_8BITS, 0, data, ptr, 8);
                    }
                    ptr += 8;
                }
            }
        }

        /// <summary>
        /// 探测编码模式
        /// </summary>
        /// <param name="content">内容</param>
        /// <returns>
        /// 编码模式
        /// <para>0 NUMERIC 数字0-9</para>
        /// <para>1 ALPHANUMERIC 数字0-9、大写字母A-Z、符号(空格)$%*+-./:</para>
        /// <para>2 BYTE(ISO-8859-1)</para>
        /// <para>3 BYTE(UTF-8)</para>
        /// </returns>
        private static int DetectionMode(string content)
        {
            int length = content.Length;
            // BYTE(UTF-8)
            for (int i = 0; i < length; i++)
            {
                if (content[i] > 255)
                {
                    return 3;
                }
            }
            // BYTE(ISO-8859-1)
            for (int i = 0; i < length; i++)
            {
                if (content[i] > 127)
                {
                    return 2;
                }
            }
            // ALPHANUMERIC 数字0-9、大写字母A-Z、符号(空格)$%*+-./:
            for (int i = 0; i < length; i++)
            {
                if (ALPHA_NUMERIC_TABLE[content[i]] > 9)
                {
                    return 1;
                }
            }
            // NUMERIC 数字0-9
            return 0;
        }

        /// <summary>
        /// 字符编码ISO-8859-1
        /// </summary>
        private static readonly Encoding ISO88591 = Encoding.GetEncoding("ISO-8859-1");
        /// <summary>
        /// 字符编码UTF-8
        /// </summary>
        private static readonly Encoding UTF8 = Encoding.UTF8;

        /// <summary>
        /// 数字0xEC前8bit
        /// </summary>
        private static readonly bool[] NUMBER_0xEC_8BITS = { true, true, true, false, true, true, false, false };
        /// <summary>
        /// 数字0x11前8bit
        /// </summary>
        private static readonly bool[] NUMBER_0x11_8BITS = { false, false, false, true, false, false, false, true };

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
        };

    }
}
