using ConsoleDemo.Util;
using System;
using System.Text;

namespace ConsoleDemo.Model
{

    /// <summary>
    /// 二维码
    /// </summary>
    public class QrCode
    {

        /// <summary>
        /// 版本(编码模式 BYTE)
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
        /// <para>编码模式 BYTE</para>
        /// <para>编码格式 UTF-8</para>
        /// <para>停用ECI true</para>
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
            // 内容字节数
            int contentBytes = bits.Length;
            // 内容bit数
            int contentBits = contentBytes * 8;
            // 获取版本
            Version = new Version(contentBytes, level);
            // 数据bit数
            bool[] dataBits = new bool[Version.DataBits];
            // 编码模式(4bit) Byte 0b0100=4
            // 数据来源 ISO/IEC 18004-2015 -> 7.4.1 -> Table 2 -> QR Code symbols列Byte行
            QrCodeUtils.AddBits(dataBits, 0, 4, 4);
            // `内容字节数`bit数
            int contentBytesBits = Version.ContentBytesBits;
            QrCodeUtils.AddBits(dataBits, 4, contentBytes, contentBytesBits);
            // 内容
            for (int i = 0; i < contentBytes; i++)
            {
                QrCodeUtils.AddBits(dataBits, 4 + contentBytesBits + 8 * i, bits[i], 8);
            }
            // 结束符(4bit) 0b0000=0
            // 数据来源 ISO/IEC 18004-2015 -> 7.4.9
            QrCodeUtils.AddBits(dataBits, 4 + contentBytesBits + contentBits, 0, 4);
            // 补齐符 交替0b11101100=0xEC和0b00010001=0x11至填满
            // 数据来源 ISO/IEC 18004-2015 -> 7.4.10
            int paddingPos = 8 + contentBytesBits + contentBits;
            int paddingCount = Version.ContentBytes - contentBytes;
            bool[] number0xecBits= QrCodeUtils.GetBits(0xEC, 10);
            bool[] number0x11Bits= QrCodeUtils.GetBits(0x11, 8);
            for (int i = 0; i < paddingCount; i++)
            {
                if (i % 2 == 0)
                {
                    QrCodeUtils.AddBits(dataBits, paddingPos + i * 8, number0xecBits, 8);
                }
                else
                {
                    QrCodeUtils.AddBits(dataBits, paddingPos + i * 8, number0x11Bits, 8);
                }
            }

            /* 纠错 */
            int[,] ec = Version.Ec;
            // 数据块数 或 纠错块数
            int blocks = Version.EcBlocks;
            // 纠错块字节数
            int ecBlockBytes = Version.EcBytes / blocks;
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
                    int[] ecBlock = CalculateEc(dataBlock, ecBlockBytes);
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
            Matrix = MaskPattern.BestPattern;
        }

        /// <summary>
        /// 计算纠错码
        /// </summary>
        /// <param name="dataBlock">数据块</param>
        /// <param name="ecBlockLength">纠错块长度</param>
        /// <returns>纠错块</returns>
        private static int[] CalculateEc(int[] dataBlock, int ecBlockLength)
        {
            // 纠错码
            int[] result = ReedSolomon.Encoder(dataBlock, ecBlockLength);
            // 长度不够前面补0
            int padding = ecBlockLength - result.Length;
            if (padding == 0)
            {
                return result;
            }
            else
            {
                int[] ecBlock = new int[ecBlockLength];
                Array.Copy(result, 0, ecBlock, padding, result.Length);
                return ecBlock;
            }
        }

    }
}
