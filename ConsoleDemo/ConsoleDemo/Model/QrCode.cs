using System.Collections.Generic;
using System.Text;

namespace ConsoleDemo.Model
{

    // https://blog.csdn.net/hk_5788/article/details/50839790
    // https://blog.csdn.net/ajianyingxiaoqinghan/article/details/78837864
    // https://juejin.cn/post/7071499529995943950

    /// <summary>
    /// 二维码
    /// </summary>
    public class QrCode
    {

        /// <summary>
        /// 版本
        /// </summary>
        private Version version;
        /// <summary>
        /// 版本
        /// </summary>
        public Version Version { get { return version; } }
        /// <summary>
        /// 数据矩阵
        /// <para>true 黑</para>
        /// <para>false 白</para>
        /// </summary>
        private bool[][] dataMatrix;
        /// <summary>
        /// 数据矩阵
        /// <para>true 黑</para>
        /// <para>false 白</para>
        /// </summary>
        public bool[][] DataMatrix { get { return dataMatrix; } }

        /// <summary>
        /// 构造二维码
        /// <para>编码模式 BYTE</para>
        /// <para>编码格式 UTF8</para>
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
            // 内容bits
            byte[] bits = Encoding.UTF8.GetBytes(content);
            // 内容字节数
            int contentBytes = bits.Length;
            // 内容bit数
            int contentBits = contentBytes * 8;
            // 获取版本
            version = new Version(contentBytes, level);
            // 数据bit数
            bool[] dataBits = new bool[version.DataBits];
            // 编码模式(4bit) Byte 0b0100=4
            // 数据来源 ISO/IEC 18004-2015 -> 7.4.1 -> Table 2 -> QR Code symbols列Byte行
            AddBits(dataBits, 0, 4, 4);
            // `内容字节数`所占的bit数
            int contentBytesBits = version.ContentBytesBits;
            AddBits(dataBits, 4, contentBytes, contentBytesBits);
            // 内容
            for (int i = 0; i < contentBytes; i++)
            {
                AddBits(dataBits, 4 + contentBytesBits + 8 * i, bits[i], 8);
            }
            // 结束符(4bit) 0b0000=0
            // 数据来源 ISO/IEC 18004-2015 -> 7.4.9
            AddBits(dataBits, 4 + contentBytesBits + contentBits, 0, 4);
            // 补齐符 交替0b11101100=0xEC和0b00010001=0x11至填满
            // 数据来源 ISO/IEC 18004-2015 -> 7.4.10
            int paddingPos = 8 + contentBytesBits + contentBits;
            int paddingCount = version.ContentBytes - contentBytes;
            for (int i = 0; i < paddingCount; i++)
            {
                if (i % 2 == 0)
                {
                    AddBits(dataBits, paddingPos + i * 8, 0xEC, 8);
                }
                else
                {
                    AddBits(dataBits, paddingPos + i * 8, 0x11, 8);
                }
            }

            // 纠错
            int[,] ec = version.Ec;
            // 纠错块数
            int blocks = version.EcBlocks;
            // 每块纠错块字节数
            int ecBlockBytes = version.EcBytes / blocks;
            List<byte[]> dataBlocks = new List<byte[]>(blocks);
            List<byte[]> ecBlocks = new List<byte[]>(blocks);
            int dataPtr = 0;
            for (int i = 0; i < ec.GetLength(0); i++)
            {
                int count = ec[i, 0];
                int dataCodewords = ec[i, 1];
                for (int j = 0; j < count; j++)
                {
                    // 数据块
                    byte[] dataBlock = new byte[dataCodewords];
                    Copy(dataBits, dataPtr, dataBlock);
                    dataBlocks.Add(dataBlock);
                    // 纠错块
                    byte[] ecBlock = new byte[ecBlockBytes];
                    CalculateEc(dataBlock, ecBlock);
                    ecBlocks.Add(ecBlock);
                    dataPtr += dataCodewords * 8;
                }
            }
        }

        /// <summary>
        /// 添加bit(高位在前)
        /// </summary>
        /// <param name="bits">目的数据</param>
        /// <param name="pos">位置</param>
        /// <param name="value">值</param>
        /// <param name="numberBits">添加bit个数</param>
        private static void AddBits(bool[] bits, int pos, int value, int numberBits)
        {
            for (int i = 0; i < numberBits; i++)
            {
                bits[pos + i] = (value & (1 << (numberBits - i - 1))) != 0;
            }
        }

        /// <summary>
        /// 拷贝数据
        /// </summary>
        /// <param name="source">源数据</param>
        /// <param name="offset">源数据起始位置</param>
        /// <param name="destination">目的数据</param>
        private static void Copy(bool[] source, int offset, byte[] destination)
        {
            for (int i = 0; i < destination.Length; i++)
            {
                int ptr = offset + i * 8;
                destination[i] = (byte)(
                      (source[ptr    ] ? 0x80 : 0)
                    | (source[ptr + 1] ? 0x40 : 0)
                    | (source[ptr + 2] ? 0x20 : 0)
                    | (source[ptr + 3] ? 0x10 : 0)
                    | (source[ptr + 4] ? 0x08 : 0)
                    | (source[ptr + 5] ? 0x04 : 0)
                    | (source[ptr + 6] ? 0x02 : 0)
                    | (source[ptr + 7] ? 0x01 : 0)
                   );
            }
        }

        /// <summary>
        /// 计算纠错码
        /// </summary>
        /// <param name="dataBlock">数据块</param>
        /// <param name="ecBlock">纠错块</param>
        private static void CalculateEc(byte[] dataBlock, byte[] ecBlock)
        {
            ReedSolomon.Encoder(dataBlock, ecBlock);
        }

    }
}
