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
            int contentLength = content.Length;
            version = new Version(contentLength, level);
            bool[] contentBits = new bool[version.DataBits];
            // 编码模式 BYTE 0b0100=4
            // 数据来源 ISO/IEC 18004-2015 -> 7.4.1 -> Table 2 -> QR Code symbols列Byte行
            AddBits(contentBits, 0, 4, 4);
            // 储存`内容字节数`所占的bit数 1-9版本8bit 10-40版本16bit
            // 数据来源 ISO/IEC 18004-2015 -> 7.4.1 -> Table 3 -> Byte mode列
            int contentBitsLength = version.VersionNumber < 10 ? 8 : 16;
            AddBits(contentBits, 4, contentLength, contentBitsLength);
            // 内容 8bit*长度
            byte[] bits = Encoding.UTF8.GetBytes(content);
            for (int i = 0; i < bits.Length; i++)
            {
                AddBits(contentBits, 4 + contentBitsLength + 8 * i, bits[i], 8);
            }
            // 结束符 4bit 0b0000
            // 数据来源 ISO/IEC 18004-2015 -> 7.4.9
            AddBits(contentBits, 4 + contentBitsLength + bits.Length * 8, 0, 4);
            // 补齐符 交替11101100=236和00010001=17至填满
            // 数据来源 ISO/IEC 18004-2015 -> 7.4.10
            int paddingPos = 8 + contentBitsLength + bits.Length * 8;
            int paddingCount = version.ContentBytes - contentLength;
            for (int i = 0; i < paddingCount; i++)
            {
                if (i % 2 == 0)
                {
                    AddBits(contentBits, paddingPos + i * 8, 236, 8);
                }
                else
                {
                    AddBits(contentBits, paddingPos + i * 8, 17, 8);
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
        private void AddBits(bool[] bits, int pos, int value, int numberBits)
        {
            for (int i = 0; i < numberBits; i++)
            {
                bits[pos + i] = (value & (1 << (numberBits - i - 1))) != 0;
            }
        }

    }
}
