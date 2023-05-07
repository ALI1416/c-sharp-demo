using System;

namespace ConsoleDemo.Model
{

    /// <summary>
    /// 掩模模板
    /// </summary>
    public class MaskPattern
    {

        /// <summary>
        /// 模板列表
        /// <para>0白 1黑</para>
        /// </summary>
        public readonly byte[][,] Patterns = new byte[8][,];
        /// <summary>
        /// 惩戒分列表
        /// </summary>
        public readonly int[] Penalties = new int[8];
        /// <summary>
        /// 最好的模板下标
        /// </summary>
        public readonly int Best;
        /// <summary>
        /// 最好的模板
        /// <para>false白 true黑</para>
        /// </summary>
        public readonly bool[,] BestPattern;

        /// <summary>
        /// 构建模板
        /// </summary>
        /// <param name="data">
        /// 数据
        /// </param>
        /// <param name="version">
        /// 版本
        /// </param>
        /// <param name="level">
        /// 纠错等级
        /// <para>0 L 7%</para>
        /// <para>1 M 15%</para>
        /// <para>2 Q 25%</para>
        /// <para>3 H 30%</para>
        /// </param>
        public MaskPattern(bool[] data, Version version, int level)
        {
            int dimension = version.Dimension;
            int versionNumber = version.VersionNumber;
            for (int i = 0; i < 8; i++)
            {
                // 新建模板 0白 1黑 2空
                byte[,] pattern = new byte[dimension, dimension];
                // 填充为空模板
                FillEmptyPattern(pattern, dimension);
                // 嵌入基础图形
                EmbedBasicPattern(pattern, dimension, versionNumber);
                // 嵌入格式信息
                EmbedFormatInfo(pattern, dimension, level, i);
                // 嵌入版本信息(版本7+)
                EmbedVersionInfo(pattern, dimension, versionNumber);
                // 嵌入数据
                EmbedData(pattern, dimension, i, data);
                Patterns[i] = pattern;
                // 计算惩戒分
                Penalties[i] = MaskPenaltyRule(pattern, dimension);
            }
            // 找到最好的模板
            int minPenalty = int.MaxValue;
            Best = -1;
            for (int i = 0; i < 8; i++)
            {
                if (Penalties[i] < minPenalty)
                {
                    minPenalty = Penalties[i];
                    Best = i;
                }
            }
            BestPattern = Convert(Patterns[Best], dimension);
        }

        /// <summary>
        /// 转换
        /// </summary>
        /// <param name="bytes">byte</param>
        /// <param name="dimension">尺寸</param>
        /// <returns>bool</returns>
        private static bool[,] Convert(byte[,] bytes, int dimension)
        {
            bool[,] data = new bool[dimension, dimension];
            for (int i = 0; i < dimension; i++)
            {
                for (int j = 0; j < dimension; j++)
                {
                    if (bytes[i, j] == 1)
                    {
                        data[i, j] = true;
                    }
                }
            }
            return data;
        }

        /// <summary>
        /// 填充为空模板
        /// </summary>
        /// <param name="pattern">模板</param>
        /// <param name="dimension">尺寸</param>
        private static void FillEmptyPattern(byte[,] pattern, int dimension)
        {
            for (int i = 0; i < dimension; i++)
            {
                for (int j = 0; j < dimension; j++)
                {
                    pattern[i, j] = 2;
                }
            }
        }

        /// <summary>
        /// 嵌入基础图形
        /// <para>包含：</para>
        /// <para>位置探测图形和分隔符</para>
        /// <para>左下角黑点</para>
        /// <para>位置校正图形(版本2+)</para>
        /// <para>定位图形</para>
        /// </summary>
        /// <param name="pattern">模板</param>
        /// <param name="dimension">尺寸</param>
        /// <param name="versionNumber">版本号</param>
        private static void EmbedBasicPattern(byte[,] pattern, int dimension, int versionNumber)
        {
            // 嵌入位置探测和分隔符图形
            EmbedPositionFinderPatternAndSeparator(pattern, dimension);
            // 嵌入位置校正图形(版本2+)
            EmbedPositionAlignmentPattern(pattern, versionNumber);
            // 嵌入定位图形
            EmbedTimingPattern(pattern, dimension);
            // 嵌入左下角黑点
            EmbedDarkDotAtLeftBottomCorner(pattern, dimension);
        }

        /// <summary>
        /// 嵌入位置探测和分隔符图形
        /// </summary>
        /// <param name="pattern">模板</param>
        /// <param name="dimension">尺寸</param>
        private static void EmbedPositionFinderPatternAndSeparator(byte[,] pattern, int dimension)
        {
            /* 嵌入位置探测图形 */
            int finderDimension = 7;
            // 左上角
            EmbedPositionFinderPattern(pattern, 0, 0);
            // 右上角
            EmbedPositionFinderPattern(pattern, dimension - finderDimension, 0);
            // 左下角
            EmbedPositionFinderPattern(pattern, 0, dimension - finderDimension);

            /* 嵌入水平分隔符图形 */
            int horizontalWidth = 8;
            // 左上角
            EmbedHorizontalSeparationPattern(pattern, 0, horizontalWidth - 1);
            // 右上角
            EmbedHorizontalSeparationPattern(pattern, dimension - horizontalWidth, horizontalWidth - 1);
            // 左下角
            EmbedHorizontalSeparationPattern(pattern, 0, dimension - horizontalWidth);

            /* 嵌入垂直分隔符图形 */
            int verticalHeight = 7;
            // 左上角
            EmbedVerticalSeparationPattern(pattern, verticalHeight, 0);
            // 右上角
            EmbedVerticalSeparationPattern(pattern, dimension - verticalHeight - 1, 0);
            // 左下角
            EmbedVerticalSeparationPattern(pattern, verticalHeight, dimension - verticalHeight);
        }

        /// <summary>
        /// 嵌入位置探测图形
        /// </summary>
        /// <param name="pattern">模板</param>
        /// <param name="xStart">x起始坐标</param>
        /// <param name="yStart">y起始坐标</param>
        private static void EmbedPositionFinderPattern(byte[,] pattern, int xStart, int yStart)
        {
            for (int x = 0; x < 7; x++)
            {
                for (int y = 0; y < 7; y++)
                {
                    pattern[xStart + x, yStart + y] = POSITION_FINDER_PATTERN[x, y];
                }
            }
        }

        /// <summary>
        /// 嵌入水平分隔符图形
        /// </summary>
        /// <param name="pattern">模板</param>
        /// <param name="xStart">x起始坐标</param>
        /// <param name="yStart">y起始坐标</param>
        private static void EmbedHorizontalSeparationPattern(byte[,] pattern, int xStart, int yStart)
        {
            for (int x = 0; x < 8; x++)
            {
                pattern[xStart + x, yStart] = 0;
            }
        }

        /// <summary>
        /// 嵌入垂直分隔符图形
        /// </summary>
        /// <param name="pattern">模板</param>
        /// <param name="xStart">x起始坐标</param>
        /// <param name="yStart">y起始坐标</param>
        private static void EmbedVerticalSeparationPattern(byte[,] pattern, int xStart, int yStart)
        {
            for (int y = 0; y < 7; y++)
            {
                pattern[xStart, yStart + y] = 0;
            }
        }

        /// <summary>
        /// 嵌入左下角黑点
        /// </summary>
        /// <param name="pattern">模板</param>
        /// <param name="dimension">尺寸</param>
        private static void EmbedDarkDotAtLeftBottomCorner(byte[,] pattern, int dimension)
        {
            pattern[8, dimension - 8] = 1;
        }

        /// <summary>
        /// 嵌入位置校正图形(版本2+)
        /// </summary>
        /// <param name="pattern">模板</param>
        /// <param name="versionNumber">版本号</param>
        private static void EmbedPositionAlignmentPattern(byte[,] pattern, int versionNumber)
        {
            if (versionNumber < 2)
            {
                return;
            }
            int[] coordinates = POSITION_ALIGNMENT_PATTERN_COORDINATE[versionNumber];
            int length = coordinates.Length;
            for (int x = 0; x < length; x++)
            {
                for (int y = 0; y < length; y++)
                {
                    // 跳过位置探测图形
                    if ((x == 0 && y == 0) || (x == 0 && y == length - 1) || (y == 0 && x == length - 1))
                    {
                        continue;
                    }
                    EmbedPositionAlignmentPattern(pattern, coordinates[x] - 2, coordinates[y] - 2);
                }
            }
        }

        /// <summary>
        /// 嵌入位置校正图形
        /// </summary>
        /// <param name="pattern">模板</param>
        /// <param name="xStart">x起始坐标</param>
        /// <param name="yStart">y起始坐标</param>
        private static void EmbedPositionAlignmentPattern(byte[,] pattern, int xStart, int yStart)
        {
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    pattern[xStart + x, yStart + y] = POSITION_ALIGNMENT_PATTERN[y, x];
                }
            }
        }

        /// <summary>
        /// 嵌入定位图形
        /// </summary>
        /// <param name="pattern">模板</param>
        /// <param name="dimension">尺寸</param>
        private static void EmbedTimingPattern(byte[,] pattern, int dimension)
        {
            for (int i = 8; i < dimension - 8; i++)
            {
                byte isBlack = (byte)((i + 1) % 2);
                // 不必跳过校正图形
                pattern[i, 6] = isBlack;
                pattern[6, i] = isBlack;
            }
        }

        /// <summary>
        /// 嵌入格式信息
        /// </summary>
        /// <param name="pattern">模板</param>
        /// <param name="dimension">尺寸</param>
        /// <param name="level">纠错等级</param>
        /// <param name="id">模板序号</param>
        private static void EmbedFormatInfo(byte[,] pattern, int dimension, int level, int id)
        {
            bool[] formatInfo = FORMAT_INFO[level, id];
            for (int i = 0; i < 15; i++)
            {
                byte isBlack = (byte)(formatInfo[14 - i] ? 1 : 0);
                // 左上角
                pattern[FORMAT_INFO_COORDINATES[i, 0], FORMAT_INFO_COORDINATES[i, 1]] = isBlack;
                int x, y;
                // 右上角
                if (i < 8)
                {
                    x = dimension - i - 1;
                    y = 8;
                }
                // 左下角
                else
                {
                    x = 8;
                    y = dimension + i - 15;
                }
                pattern[x, y] = isBlack;
            }
        }

        /// <summary>
        /// 嵌入版本信息(版本7+)
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="dimension">尺寸</param>
        /// <param name="versionNumber">版本号</param>
        private static void EmbedVersionInfo(byte[,] pattern, int dimension, int versionNumber)
        {
            if (versionNumber < 7)
            {
                return;
            }
            bool[] versionInfo = VERSION_INFO[versionNumber];
            int index = 17;
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    byte isBlack = (byte)(versionInfo[index--] ? 1 : 0);
                    // 左下角
                    pattern[i, dimension - 11 + j] = isBlack;
                    // 右上角
                    pattern[dimension - 11 + j, i] = isBlack;
                }
            }
        }

        /// <summary>
        /// 嵌入数据
        /// </summary>
        /// <param name="pattern">模板</param>
        /// <param name="dimension">尺寸</param>
        /// <param name="id">模板序号</param>
        /// <param name="data">数据</param>
        private static void EmbedData(byte[,] pattern, int dimension, int id, bool[] data)
        {
            int length = data.Length;
            int index = 0;
            int direction = -1;
            // 从右下角开始
            int x = dimension - 1;
            int y = dimension - 1;
            while (x > 0)
            {
                // 跳过垂直分隔符图形
                if (x == 6)
                {
                    x -= 1;
                }
                while (y >= 0 && y < dimension)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        int xx = x - i;
                        // 跳过不为空
                        if (pattern[xx, y] != 2)
                        {
                            continue;
                        }
                        int isBlack;
                        if (index < length)
                        {
                            isBlack = data[index] ? 1 : 0;
                            index++;
                        }
                        else
                        {
                            isBlack = 0;
                        }
                        // 需要掩模
                        if (GetMaskBit(id, xx, y))
                        {
                            isBlack ^= 1;
                        }
                        pattern[xx, y] = (byte)isBlack;
                    }
                    y += direction;
                }
                direction = -direction;
                y += direction;
                x -= 2;
            }
        }

        /// <summary>
        /// 获取指定坐标是否需要掩模
        /// </summary>
        /// <param name="id">模板序号</param>
        /// <param name="x">坐标x</param>
        /// <param name="y">坐标y</param>
        /// <returns>是否需要掩模</returns>
        public static bool GetMaskBit(int id, int x, int y)
        {
            switch (id)
            {
                default:
                case 0:
                    {
                        return ((x + y) % 2) == 0;
                    }
                case 1:
                    {
                        return (y % 2) == 0;
                    }
                case 2:
                    {
                        return (x % 3) == 0;
                    }
                case 3:
                    {
                        return ((x + y) % 3) == 0;
                    }
                case 4:
                    {
                        return (((y / 2) + (x / 3)) % 2) == 0;
                    }
                case 5:
                    {
                        int temp = x * y;
                        return ((temp % 2) + (temp % 3)) == 0;
                    }
                case 6:
                    {
                        int temp = x * y;
                        return (((temp % 2) + (temp % 3)) % 2) == 0;
                    }
                case 7:
                    {
                        return ((((x * y) % 3) + ((x + y) % 2)) % 2) == 0;
                    }
            }
        }

        /// <summary>
        /// 掩模惩戒规则
        /// </summary>
        /// <param name="pattern">模板</param>
        /// <param name="dimension">尺寸</param>
        /// <returns>惩戒分</returns>
        private static int MaskPenaltyRule(byte[,] pattern, int dimension)
        {
            return MaskPenaltyRule1(pattern, dimension, true) + MaskPenaltyRule1(pattern, dimension, false)
                + MaskPenaltyRule2(pattern, dimension)
                + MaskPenaltyRule3(pattern, dimension)
                + MaskPenaltyRule4(pattern, dimension);
        }

        /// <summary>
        /// 掩模惩戒规则1
        /// </summary>
        /// <param name="pattern">模板</param>
        /// <param name="dimension">尺寸</param>
        /// <param name="isHorizontal">水平</param>
        /// <returns>规则1惩戒分</returns>
        private static int MaskPenaltyRule1(byte[,] pattern, int dimension, bool isHorizontal)
        {
            int penalty = 0;
            for (int i = 0; i < dimension; i++)
            {
                int numSameBitCells = 0;
                int prevBit = -1;
                for (int j = 0; j < dimension; j++)
                {
                    int bit = isHorizontal ? pattern[i, j] : pattern[j, i];
                    if (bit == prevBit)
                    {
                        numSameBitCells++;
                    }
                    else
                    {
                        if (numSameBitCells >= 5)
                        {
                            penalty += PENALTY1 + (numSameBitCells - 5);
                        }
                        numSameBitCells = 1;
                        prevBit = bit;
                    }
                }
                if (numSameBitCells >= 5)
                {
                    penalty += PENALTY1 + (numSameBitCells - 5);
                }
            }
            return penalty;
        }

        /// <summary>
        /// 掩模惩戒规则2
        /// </summary>
        /// <param name="pattern">模板</param>
        /// <param name="dimension">尺寸</param>
        /// <returns>规则2惩戒分</returns>
        private static int MaskPenaltyRule2(byte[,] pattern, int dimension)
        {
            int penalty = 0;
            for (int y = 0; y < dimension - 1; y++)
            {
                for (int x = 0; x < dimension - 1; x++)
                {
                    int value = pattern[y, x];
                    if (value == pattern[y, x + 1] && value == pattern[y + 1, x] && value == pattern[y + 1, x + 1])
                    {
                        penalty++;
                    }
                }
            }
            return PENALTY2 * penalty;
        }

        /// <summary>
        /// 掩模惩戒规则3
        /// </summary>
        /// <param name="pattern">模板</param>
        /// <param name="dimension">尺寸</param>
        /// <returns>规则3惩戒分</returns>
        private static int MaskPenaltyRule3(byte[,] pattern, int dimension)
        {
            int penalty = 0;
            for (int y = 0; y < dimension; y++)
            {
                for (int x = 0; x < dimension; x++)
                {
                    if (x + 6 < dimension &&
                        pattern[y, x] == 1 &&
                        pattern[y, x + 1] == 0 &&
                        pattern[y, x + 2] == 1 &&
                        pattern[y, x + 3] == 1 &&
                        pattern[y, x + 4] == 1 &&
                        pattern[y, x + 5] == 0 &&
                        pattern[y, x + 6] == 1 &&
                        (IsWhiteHorizontal(pattern, dimension, y, x - 4, x) || IsWhiteHorizontal(pattern, dimension, y, x + 7, x + 11)))
                    {
                        penalty++;
                    }
                    if (y + 6 < dimension &&
                        pattern[y, x] == 1 &&
                        pattern[y + 1, x] == 0 &&
                        pattern[y + 2, x] == 1 &&
                        pattern[y + 3, x] == 1 &&
                        pattern[y + 4, x] == 1 &&
                        pattern[y + 5, x] == 0 &&
                        pattern[y + 6, x] == 1 &&
                        (IsWhiteVertical(pattern, dimension, x, y - 4, y) || IsWhiteVertical(pattern, dimension, x, y + 7, y + 11)))
                    {
                        penalty++;
                    }
                }
            }
            return penalty * PENALTY3;
        }

        /// <summary>
        /// 水平全是白色
        /// </summary>
        /// <param name="pattern">模板</param>
        /// <param name="dimension">尺寸</param>
        /// <param name="row">行数</param>
        /// <param name="from">从</param>
        /// <param name="to">到</param>
        /// <returns>是否水平全是白色</returns>
        private static bool IsWhiteHorizontal(byte[,] pattern, int dimension, int row, int from, int to)
        {
            if (from < 0 || dimension < to)
            {
                return false;
            }
            from = Math.Max(from, 0);
            to = Math.Min(to, dimension);
            for (int i = from; i < to; i++)
            {
                if (pattern[row, i] == 1)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 垂直全是白色
        /// </summary>
        /// <param name="pattern">模板</param>
        /// <param name="dimension">尺寸</param>
        /// <param name="col">列数</param>
        /// <param name="from">从</param>
        /// <param name="to">到</param>
        /// <returns>是否垂直全是白色</returns>
        private static bool IsWhiteVertical(byte[,] pattern, int dimension, int col, int from, int to)
        {
            if (from < 0 || dimension < to)
            {
                return false;
            }
            from = Math.Max(from, 0);
            to = Math.Min(to, dimension);
            for (int i = from; i < to; i++)
            {
                if (pattern[i, col] == 1)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 掩模惩戒规则4
        /// </summary>
        /// <param name="pattern">模板</param>
        /// <param name="dimension">尺寸</param>
        /// <returns>规则4惩戒分</returns>
        private static int MaskPenaltyRule4(byte[,] pattern, int dimension)
        {
            int numDarkCells = 0;
            for (int y = 0; y < dimension; y++)
            {
                for (int x = 0; x < dimension; x++)
                {
                    if (pattern[y, x] == 1)
                    {
                        numDarkCells++;
                    }
                }
            }
            var numTotalCells = dimension * dimension;
            var darkRatio = (double)numDarkCells / numTotalCells;
            var fivePercentVariances = (int)(Math.Abs(darkRatio - 0.5) * 20.0);
            return fivePercentVariances * PENALTY4;
        }

        /// <summary>
        /// 位置探测图形
        /// <para>索引[x坐标,y坐标]:7x7</para>
        /// <para>数量:3个(左上角、右上角、左下角)</para>
        /// <para>数据来源 ISO/IEC 18004-2015 -> 6.3.3.1</para>
        /// </summary>
        private static readonly byte[,] POSITION_FINDER_PATTERN =
        {
            {1, 1, 1, 1, 1, 1, 1},
            {1, 0, 0, 0, 0, 0, 1},
            {1, 0, 1, 1, 1, 0, 1},
            {1, 0, 1, 1, 1, 0, 1},
            {1, 0, 1, 1, 1, 0, 1},
            {1, 0, 0, 0, 0, 0, 1},
            {1, 1, 1, 1, 1, 1, 1},
        };

        /// <summary>
        /// 位置校正图形
        /// <para>索引[x坐标,y坐标]:5x5</para>
        /// <para>数量:根据版本号而定</para>
        /// <para>数据来源 ISO/IEC 18004-2015 -> 6.3.6</para>
        /// </summary>
        private static readonly byte[,] POSITION_ALIGNMENT_PATTERN =
        {
            {1, 1, 1, 1, 1},
            {1, 0, 0, 0, 1},
            {1, 0, 1, 0, 1},
            {1, 0, 0, 0, 1},
            {1, 1, 1, 1, 1},
        };

        /// <summary>
        /// 位置校正图形坐标
        /// <para>索引[版本号,坐标]:41x7</para>
        /// <para>数据来源 ISO/IEC 18004-2015 -> Annex E -> Table E.1</para>
        /// </summary>
        private static readonly int[][] POSITION_ALIGNMENT_PATTERN_COORDINATE =
        {
             new int[] {},
             new int[] {},
             new int[] {6, 18},
             new int[] {6, 22},
             new int[] {6, 26},
             new int[] {6, 30},
             new int[] {6, 34},
             new int[] {6, 22, 38},
             new int[] {6, 24, 42},
             new int[] {6, 26, 46},
             new int[] {6, 28, 50},
             new int[] {6, 30, 54},
             new int[] {6, 32, 58},
             new int[] {6, 34, 62},
             new int[] {6, 26, 46, 66},
             new int[] {6, 26, 48, 70},
             new int[] {6, 26, 50, 74},
             new int[] {6, 30, 54, 78},
             new int[] {6, 30, 56, 82},
             new int[] {6, 30, 58, 86},
             new int[] {6, 34, 62, 90},
             new int[] {6, 28, 50, 72,  94},
             new int[] {6, 26, 50, 74,  98},
             new int[] {6, 30, 54, 78, 102},
             new int[] {6, 28, 54, 80, 106},
             new int[] {6, 32, 58, 84, 110},
             new int[] {6, 30, 58, 86, 114},
             new int[] {6, 34, 62, 90, 118},
             new int[] {6, 26, 50, 74,  98, 122},
             new int[] {6, 30, 54, 78, 102, 126},
             new int[] {6, 26, 52, 78, 104, 130},
             new int[] {6, 30, 56, 82, 108, 134},
             new int[] {6, 34, 60, 86, 112, 138},
             new int[] {6, 30, 58, 86, 114, 142},
             new int[] {6, 34, 62, 90, 118, 146},
             new int[] {6, 30, 54, 78, 102, 126, 150},
             new int[] {6, 24, 50, 76, 102, 128, 154},
             new int[] {6, 28, 54, 80, 106, 132, 158},
             new int[] {6, 32, 58, 84, 110, 136, 162},
             new int[] {6, 26, 54, 82, 110, 138, 166},
             new int[] {6, 30, 58, 86, 114, 142, 170},
        };

        /// <summary>
        /// 格式信息坐标(左上角)
        /// <para>索引[x坐标,y坐标]:15x2</para>
        /// <para>数据来源 ISO/IEC 18004-2015 -> 7.9.1 -> Figure 25</para>
        /// </summary>
        private static readonly int[,] FORMAT_INFO_COORDINATES =
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
        /// 格式信息
        /// <para>索引[纠错等级,模板序号]</para>
        /// </summary>
        private static readonly bool[,][] FORMAT_INFO = new bool[4, 8][]
        {
            {
                new bool[] { true,  true,  true, false,  true,  true,  true,  true,  true, false, false, false,  true, false, false, },
                new bool[] { true,  true,  true, false, false,  true, false,  true,  true,  true,  true, false, false,  true,  true, },
                new bool[] { true,  true,  true,  true,  true, false,  true,  true, false,  true, false,  true, false,  true, false, },
                new bool[] { true,  true,  true,  true, false, false, false,  true, false, false,  true,  true,  true, false,  true, },
                new bool[] { true,  true, false, false,  true,  true, false, false, false,  true, false,  true,  true,  true,  true, },
                new bool[] { true,  true, false, false, false,  true,  true, false, false, false,  true,  true, false, false, false, },
                new bool[] { true,  true, false,  true,  true, false, false, false,  true, false, false, false, false, false,  true, },
                new bool[] { true,  true, false,  true, false, false,  true, false,  true,  true,  true, false,  true,  true, false, },
            },
            {
                new bool[] { true, false,  true, false,  true, false, false, false, false, false,  true, false, false,  true, false, },
                new bool[] { true, false,  true, false, false, false,  true, false, false,  true, false, false,  true, false,  true, },
                new bool[] { true, false,  true,  true,  true,  true, false, false,  true,  true,  true,  true,  true, false, false, },
                new bool[] { true, false,  true,  true, false,  true,  true, false,  true, false, false,  true, false,  true,  true, },
                new bool[] { true, false, false, false,  true, false,  true,  true,  true,  true,  true,  true, false, false,  true, },
                new bool[] { true, false, false, false, false, false, false,  true,  true, false, false,  true,  true,  true, false, },
                new bool[] { true, false, false,  true,  true,  true,  true,  true, false, false,  true, false,  true,  true,  true, },
                new bool[] { true, false, false,  true, false,  true, false,  true, false,  true, false, false, false, false, false, },
            },
            {
                new bool[] {false,  true,  true, false,  true, false,  true, false,  true, false,  true,  true,  true,  true,  true, },
                new bool[] {false,  true,  true, false, false, false, false, false,  true,  true, false,  true, false, false, false, },
                new bool[] {false,  true,  true,  true,  true,  true,  true, false, false,  true,  true, false, false, false,  true, },
                new bool[] {false,  true,  true,  true, false,  true, false, false, false, false, false, false,  true,  true, false, },
                new bool[] {false,  true, false, false,  true, false, false,  true, false,  true,  true, false,  true, false, false, },
                new bool[] {false,  true, false, false, false, false,  true,  true, false, false, false, false, false,  true,  true, },
                new bool[] {false,  true, false,  true,  true,  true, false,  true,  true, false,  true,  true, false,  true, false, },
                new bool[] {false,  true, false,  true, false,  true,  true,  true,  true,  true, false,  true,  true, false,  true, },
            },
            {
                new bool[] {false, false,  true, false,  true,  true, false,  true, false, false, false,  true, false, false,  true, },
                new bool[] {false, false,  true, false, false,  true,  true,  true, false,  true,  true,  true,  true,  true, false, },
                new bool[] {false, false,  true,  true,  true, false, false,  true,  true,  true, false, false,  true,  true,  true, },
                new bool[] {false, false,  true,  true, false, false,  true,  true,  true, false,  true, false, false, false, false, },
                new bool[] {false, false, false, false,  true,  true,  true, false,  true,  true, false, false, false,  true, false, },
                new bool[] {false, false, false, false, false,  true, false, false,  true, false,  true, false,  true, false,  true, },
                new bool[] {false, false, false,  true,  true, false,  true, false, false, false, false,  true,  true, false, false, },
                new bool[] {false, false, false,  true, false, false, false, false, false,  true,  true,  true, false,  true,  true, },
            },
        };

        /// <summary>
        /// 版本信息
        /// <para>索引[版本号]</para>
        /// </summary>
        private static readonly bool[][] VERSION_INFO = new bool[41][]
        {
            new bool[] {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, },
            new bool[] {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, },
            new bool[] {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, },
            new bool[] {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, },
            new bool[] {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, },
            new bool[] {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, },
            new bool[] {false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, },
            new bool[] {false, false, false,  true,  true,  true,  true,  true, false, false,  true, false, false,  true, false,  true, false, false, },
            new bool[] {false, false,  true, false, false, false, false,  true, false,  true,  true, false,  true,  true,  true,  true, false, false, },
            new bool[] {false, false,  true, false, false,  true,  true, false,  true, false,  true, false, false,  true,  true, false, false,  true, },
            new bool[] {false, false,  true, false,  true, false, false,  true, false, false,  true,  true, false,  true, false, false,  true,  true, },
            new bool[] {false, false,  true, false,  true,  true,  true, false,  true,  true,  true,  true,  true,  true, false,  true,  true, false, },
            new bool[] {false, false,  true,  true, false, false, false,  true,  true,  true, false,  true,  true, false, false, false,  true, false, },
            new bool[] {false, false,  true,  true, false,  true,  true, false, false, false, false,  true, false, false, false,  true,  true,  true, },
            new bool[] {false, false,  true,  true,  true, false, false,  true,  true, false, false, false, false, false,  true,  true, false,  true, },
            new bool[] {false, false,  true,  true,  true,  true,  true, false, false,  true, false, false,  true, false,  true, false, false, false, },
            new bool[] {false,  true, false, false, false, false,  true, false,  true,  true, false,  true,  true,  true,  true, false, false, false, },
            new bool[] {false,  true, false, false, false,  true, false,  true, false, false, false,  true, false,  true,  true,  true, false,  true, },
            new bool[] {false,  true, false, false,  true, false,  true, false,  true, false, false, false, false,  true, false,  true,  true,  true, },
            new bool[] {false,  true, false, false,  true,  true, false,  true, false,  true, false, false,  true,  true, false, false,  true, false, },
            new bool[] {false,  true, false,  true, false, false,  true, false, false,  true,  true, false,  true, false, false,  true,  true, false, },
            new bool[] {false,  true, false,  true, false,  true, false,  true,  true, false,  true, false, false, false, false, false,  true,  true, },
            new bool[] {false,  true, false,  true,  true, false,  true, false, false, false,  true,  true, false, false,  true, false, false,  true, },
            new bool[] {false,  true, false,  true,  true,  true, false,  true,  true,  true,  true,  true,  true, false,  true,  true, false, false, },
            new bool[] {false,  true,  true, false, false, false,  true,  true,  true, false,  true,  true, false, false, false,  true, false, false, },
            new bool[] {false,  true,  true, false, false,  true, false, false, false,  true,  true,  true,  true, false, false, false, false,  true, },
            new bool[] {false,  true,  true, false,  true, false,  true,  true,  true,  true,  true, false,  true, false,  true, false,  true,  true, },
            new bool[] {false,  true,  true, false,  true,  true, false, false, false, false,  true, false, false, false,  true,  true,  true, false, },
            new bool[] {false,  true,  true,  true, false, false,  true,  true, false, false, false, false, false,  true,  true, false,  true, false, },
            new bool[] {false,  true,  true,  true, false,  true, false, false,  true,  true, false, false,  true,  true,  true,  true,  true,  true, },
            new bool[] {false,  true,  true,  true,  true, false,  true,  true, false,  true, false,  true,  true,  true, false,  true, false,  true, },
            new bool[] {false,  true,  true,  true,  true,  true, false, false,  true, false, false,  true, false,  true, false, false, false, false, },
            new bool[] { true, false, false, false, false, false,  true, false, false,  true,  true,  true, false,  true, false,  true, false,  true, },
            new bool[] { true, false, false, false, false,  true, false,  true,  true, false,  true,  true,  true,  true, false, false, false, false, },
            new bool[] { true, false, false, false,  true, false,  true, false, false, false,  true, false,  true,  true,  true, false,  true, false, },
            new bool[] { true, false, false, false,  true,  true, false,  true,  true,  true,  true, false, false,  true,  true,  true,  true,  true, },
            new bool[] { true, false, false,  true, false, false,  true, false,  true,  true, false, false, false, false,  true, false,  true,  true, },
            new bool[] { true, false, false,  true, false,  true, false,  true, false, false, false, false,  true, false,  true,  true,  true, false, },
            new bool[] { true, false, false,  true,  true, false,  true, false,  true, false, false,  true,  true, false, false,  true, false, false, },
            new bool[] { true, false, false,  true,  true,  true, false,  true, false,  true, false,  true, false, false, false, false, false,  true, },
            new bool[] { true, false,  true, false, false, false,  true,  true, false, false, false,  true,  true, false,  true, false, false,  true, },
        };

        /// <summary>
        /// 惩戒规则1 3分
        /// </summary>
        private static readonly int PENALTY1 = 3;
        /// <summary>
        /// 惩戒规则2 3分
        /// </summary>
        private static readonly int PENALTY2 = 3;
        /// <summary>
        /// 惩戒规则3 40分
        /// </summary>
        private static readonly int PENALTY3 = 40;
        /// <summary>
        /// 惩戒规则4 10分
        /// </summary>
        private static readonly int PENALTY4 = 10;

    }
}
