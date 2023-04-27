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
        /// 内容
        /// </summary>
        private string content;
        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get { return content; } }
        /// <summary>
        /// 编码后的内容bool[]
        /// <para>true 1</para>
        /// <para>false 0</para>
        /// </summary>
        private bool[] contentBits;
        /// <summary>
        /// 编码后的内容bool[]
        /// <para>true 1</para>
        /// <para>false 0</para>
        /// </summary>
        public bool[] ContentBits { get { return contentBits; } }
        /// <summary>
        /// 错误纠正级别
        /// <para>0 L 7%</para>
        /// <para>1 M 15%</para>
        /// <para>2 Q 25%</para>
        /// <para>3 H 30%</para>
        /// </summary>
        private int level;
        /// <summary>
        /// 错误纠正级别
        /// <para>0 L 7%</para>
        /// <para>1 M 15%</para>
        /// <para>2 Q 25%</para>
        /// <para>3 H 30%</para>
        /// </summary>
        public int Level { get { return level; } }
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
        /// <para>编码模式 byte</para>
        /// <para>编码格式 UTF8</para>
        /// </summary>
        /// <param name="content">
        /// 内容
        /// </param>
        /// <param name="level">
        /// 错误纠正级别
        /// <para>0 L 7%</para>
        /// <para>1 M 15%</para>
        /// <para>2 Q 25%</para>
        /// <para>3 H 30%</para>
        /// </param>
        public QrCode(string content, int level)
        {
            this.content = content;
            this.level = level;
            this.contentBits = GetContentBits();
        }

        /// <summary>
        /// 编码后的内容bool[]
        /// </summary>
        /// <returns>内容bool[]</returns>
        private bool[] GetContentBits()
        {
            // 编码模式byte(编码模式 byte) 4位
            byte modeByte = 4;
            // 内容byte[](编码格式 UTF8) 8*Length位
            byte[] contentBytes = Encoding.UTF8.GetBytes(content);
            // 内容字节数 1-9版本8位 10-40版本16位
            int contentLength = contentBytes.Length;

            /* 组装内容bool[] */
            bool[] bits = new bool[0];
            return bits;
        }



    }
}
