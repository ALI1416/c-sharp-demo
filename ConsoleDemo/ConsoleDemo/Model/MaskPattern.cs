namespace ConsoleDemo.Model
{

    /// <summary>
    /// 掩码模板
    /// </summary>
    public class MaskPattern
    {

        /// <summary>
        /// 位置探测图形
        /// <para>索引[x坐标,y坐标]:7x7</para>
        /// <para>数量:3个(左上角、右上角、左下角)</para>
        /// </summary>
        private static readonly bool[,] POSITION_DETECTION_PATTERN =
        {
            {true,  true,  true,  true,  true,  true, true},
            {true, false, false, false, false, false, true},
            {true, false,  true,  true,  true, false, true},
            {true, false,  true,  true,  true, false, true},
            {true, false,  true,  true,  true, false, true},
            {true, false, false, false, false, false, true},
            {true,  true,  true,  true,  true,  true, true},
        };

        /// <summary>
        /// 位置校正图形
        /// <para>索引[x坐标,y坐标]:5x5</para>
        /// <para>数量:根据版本号而定</para>
        /// </summary>
        private static readonly bool[,] POSITION_ADJUSTMENT_PATTERN =
        {
            {true,  true,  true,  true, true},
            {true, false, false, false, true},
            {true, false,  true, false, true},
            {true, false, false, false, true},
            {true,  true,  true,  true, true},
        };

        /// <summary>
        /// 位置校正图形坐标
        /// <para>索引[版本号,坐标]:41x7</para>
        /// </summary>
        private static readonly int[,] POSITION_ADJUSTMENT_PATTERN_COORDINATE =
        {
             {-1, -1, -1, -1,  -1,  -1,  -1},
             {-1, -1, -1, -1,  -1,  -1,  -1},
             { 6, 18, -1, -1,  -1,  -1,  -1},
             { 6, 22, -1, -1,  -1,  -1,  -1},
             { 6, 26, -1, -1,  -1,  -1,  -1},
             { 6, 30, -1, -1,  -1,  -1,  -1},
             { 6, 34, -1, -1,  -1,  -1,  -1},
             { 6, 22, 38, -1,  -1,  -1,  -1},
             { 6, 24, 42, -1,  -1,  -1,  -1},
             { 6, 26, 46, -1,  -1,  -1,  -1},
             { 6, 28, 50, -1,  -1,  -1,  -1},
             { 6, 30, 54, -1,  -1,  -1,  -1},
             { 6, 32, 58, -1,  -1,  -1,  -1},
             { 6, 34, 62, -1,  -1,  -1,  -1},
             { 6, 26, 46, 66,  -1,  -1,  -1},
             { 6, 26, 48, 70,  -1,  -1,  -1},
             { 6, 26, 50, 74,  -1,  -1,  -1},
             { 6, 30, 54, 78,  -1,  -1,  -1},
             { 6, 30, 56, 82,  -1,  -1,  -1},
             { 6, 30, 58, 86,  -1,  -1,  -1},
             { 6, 34, 62, 90,  -1,  -1,  -1},
             { 6, 28, 50, 72,  94,  -1,  -1},
             { 6, 26, 50, 74,  98,  -1,  -1},
             { 6, 30, 54, 78, 102,  -1,  -1},
             { 6, 28, 54, 80, 106,  -1,  -1},
             { 6, 32, 58, 84, 110,  -1,  -1},
             { 6, 30, 58, 86, 114,  -1,  -1},
             { 6, 34, 62, 90, 118,  -1,  -1},
             { 6, 26, 50, 74,  98, 122,  -1},
             { 6, 30, 54, 78, 102, 126,  -1},
             { 6, 26, 52, 78, 104, 130,  -1},
             { 6, 30, 56, 82, 108, 134,  -1},
             { 6, 34, 60, 86, 112, 138,  -1},
             { 6, 30, 58, 86, 114, 142,  -1},
             { 6, 34, 62, 90, 118, 146,  -1},
             { 6, 30, 54, 78, 102, 126, 150},
             { 6, 24, 50, 76, 102, 128, 154},
             { 6, 28, 54, 80, 106, 132, 158},
             { 6, 32, 58, 84, 110, 136, 162},
             { 6, 26, 54, 82, 110, 138, 166},
             { 6, 30, 58, 86, 114, 142, 170},
        };

        /// <summary>
        /// 格式信息坐标(左上角)
        /// <para>索引[x坐标,y坐标]:15x2</para>
        /// </summary>
        private static readonly int[,] TYPE_INFO_COORDINATE =
        {
            {8, 0},
            {8, 1},
            {8, 2},
            {8, 3},
            {8, 4},
            {8, 5},
            {8, 7},
            {8, 8},
            {7, 8},
            {5, 8},
            {4, 8},
            {3, 8},
            {2, 8},
            {1, 8},
            {0, 8},
        };

        /// <summary>
        /// 版本信息多项式
        /// </summary>
        private static readonly int VERSION_INFO_POLY = 0x1F25;
        /// <summary>
        /// 格式信息多项式
        /// </summary>
        private static readonly int TYPE_INFO_POLY = 0x0537;
        /// <summary>
        /// 格式信息掩码模板
        /// </summary>
        private static readonly int TYPE_INFO_MASK_PATTERN = 0x5412;



    }
}
