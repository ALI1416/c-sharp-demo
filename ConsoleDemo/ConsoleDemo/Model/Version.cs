namespace ConsoleDemo.Model
{

    /// <summary>
    /// 版本
    /// </summary>
    public class Version
    {

        /// <summary>
        /// 所有版本
        /// </summary>
        private static readonly Version[][] VERSIONS = BuildVersions();

        /// <summary>
        /// 版本号
        /// <para>[1,40]</para>
        /// </summary>
        private int versionNumber;
        /// <summary>
        /// 版本号
        /// <para>[1,40]</para>
        /// </summary>
        public int VersionNumber { get { return versionNumber; } }
        /// <summary>
        /// 尺寸
        /// <para>尺寸 = (版本号 - 1) * 4 + 21</para>
        /// <para>[21,177]</para>
        /// </summary>
        public int Dimension { get { return (versionNumber - 1) * 4 + 21; } }
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
        /// 数据位数
        /// </summary>
        private int dataBits;
        /// <summary>
        /// 数据位数
        /// </summary>
        public int DataBits { get { return dataBits; } }
        /// <summary>
        /// 数据字节数
        /// </summary>
        private int dataBytes;
        /// <summary>
        /// 数据字节数
        /// </summary>
        public int DataBytes { get { return dataBytes; } }
        /// <summary>
        /// 内容字节数
        /// </summary>
        private int contentBytes;
        /// <summary>
        /// 内容字节数
        /// </summary>
        public int ContentBytes { get { return contentBytes; } }


        /// <summary>
        /// 获取版本
        /// </summary>
        /// <param name="length">
        /// 内容长度(字节)
        /// </param>
        /// <param name="level">
        /// 错误纠正级别
        /// <para>0 L 7%</para>
        /// <para>1 M 15%</para>
        /// <para>2 Q 25%</para>
        /// <para>3 H 30%</para>
        /// </param>
        public static Version Get(int length, int level)
        {
            return null;
        }

        /// <summary>
        /// 构建版本
        /// </summary>
        private Version(int versionNumber, int level, int dataBits, int dataBytes, int contentBytes)
        {
            this.versionNumber = versionNumber;
            this.level = level;
            this.dataBits = dataBits;
            this.dataBytes = dataBytes;
            this.contentBytes = contentBytes;
        }

        /// <summary>
        /// 构建1个versionNumber中的4个level
        /// </summary>
        private static Version[] BuildVersions(int versionNumber, int dataBits, int dataBytes, int[] contentBytes)
        {
            return new Version[] {
                new Version(versionNumber, 0, dataBits, dataBytes , contentBytes[0]),
                new Version(versionNumber, 1, dataBits, dataBytes , contentBytes[1]),
                new Version(versionNumber, 2, dataBits, dataBytes , contentBytes[2]),
                new Version(versionNumber, 3, dataBits, dataBytes , contentBytes[3]),
            };
        }

        /// <summary>
        /// 构建所有版本
        /// </summary>
        private static Version[][] BuildVersions()
        {
            return new Version[][] {
                BuildVersions( 1,   208,   26, new int[] { 17,    14,   11,    7}),
                BuildVersions( 2,   359,   44, new int[] { 32,    26,   20,   14}),
                BuildVersions( 3,   567,   70, new int[] { 53,    42,   32,   24}),
                BuildVersions( 4,   807,  100, new int[] { 78,    62,   46,   34}),
                BuildVersions( 5,  1079,  134, new int[] { 106,   84,   60,   44}),
                BuildVersions( 6,  1383,  172, new int[] { 134,  106,   74,   58}),
                BuildVersions( 7,  1568,  196, new int[] { 154,  122,   86,   64}),
                BuildVersions( 8,  1936,  242, new int[] { 192,  152,  108,   84}),
                BuildVersions( 9,  2336,  292, new int[] { 230,  180,  130,   98}),
                BuildVersions(10,  2768,  346, new int[] { }),
                BuildVersions(11,  3232,  404, new int[] { }),
                BuildVersions(12,  3728,  466, new int[] { }),
                BuildVersions(13,  4256,  532, new int[] { }),
                BuildVersions(14,  4651,  581, new int[] { }),
                BuildVersions(15,  5243,  655, new int[] { }),
                BuildVersions(16,  5867,  733, new int[] { }),
                BuildVersions(17,  6523,  815, new int[] { }),
                BuildVersions(18,  7211,  901, new int[] { }),
                BuildVersions(19,  7931,  991, new int[] { }),
                BuildVersions(20,  8683, 1085, new int[] { }),
                BuildVersions(21,  9252, 1156, new int[] { }),
                BuildVersions(22, 10068, 1258, new int[] { }),
                BuildVersions(23, 10916, 1364, new int[] { }),
                BuildVersions(24, 11796, 1474, new int[] { }),
                BuildVersions(25, 12708, 1588, new int[] { }),
                BuildVersions(26, 13652, 1706, new int[] { }),
                BuildVersions(27, 14628, 1828, new int[] { }),
                BuildVersions(28, 15371, 1921, new int[] { }),
                BuildVersions(29, 16411, 2051, new int[] { }),
                BuildVersions(30, 17483, 2185, new int[] { }),
                BuildVersions(31, 18587, 2323, new int[] { }),
                BuildVersions(32, 19723, 2465, new int[] { }),
                BuildVersions(33, 20891, 2611, new int[] { }),
                BuildVersions(34, 22091, 2761, new int[] { }),
                BuildVersions(35, 23008, 2876, new int[] { }),
                BuildVersions(36, 24272, 3034, new int[] { }),
                BuildVersions(37, 25568, 3196, new int[] { }),
                BuildVersions(38, 26896, 3362, new int[] { }),
                BuildVersions(39, 28256, 3532, new int[] { }),
                BuildVersions(40, 29648, 3706, new int[] { }),
            };
        }

    }
}
