using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Web.UI.WebControls;

namespace ConsoleDemo.Model
{

    /// <summary>
    /// 掩模模板
    /// </summary>
    public class MaskPattern
    {

        /// <summary>
        /// 模板列表
        /// </summary>
        public readonly bool[][,] Patterns = new bool[8][,];
        /// <summary>
        /// 惩戒分列表
        /// </summary>
        public readonly int[] Penalty;
        /// <summary>
        /// 最好的模板下标
        /// </summary>
        public readonly int Best;

        /// <summary>
        /// 数据
        /// </summary>
        private readonly bool[] Data;
        /// <summary>
        /// 版本
        /// </summary>
        private readonly Version Version;
        /// <summary>
        /// 纠错等级
        /// <para>0 L 7%</para>
        /// <para>1 M 15%</para>
        /// <para>2 Q 25%</para>
        /// <para>3 H 30%</para>
        /// </summary>
        private readonly int Level;
        /// <summary>
        /// 尺寸
        /// </summary>
        private readonly int Dimension;

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
            Data = data;
            Version = version;
            Dimension = version.Dimension;
            Level = level;
            for (int i = 0; i < 8; i++)
            {
                int[,] pattern = new int[Dimension, Dimension];
                FillEmptyPattern(pattern);
                EmbedBasicPattern(pattern);
                EmbedFormatInfo(pattern, i);
                EmbedVersionInfo(pattern);
                EmbedData(pattern, i);
            }
        }

        /// <summary>
        /// 填充为空模板
        /// </summary>
        /// <param name="pattern">模板</param>
        private void FillEmptyPattern(int[,] pattern)
        {
            for (int i = 0; i < Dimension; i++)
            {
                for (int j = 0; j < Dimension; j++)
                {
                    pattern[i, j] = 2;
                }
            }
        }

        /// <summary>
        /// 判断值是否为空
        /// </summary>
        /// <param name="value">值</param>
        /// <returns></returns>
        private static bool IsEmpty(int value)
        {
            return value == 2;
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
        private void EmbedBasicPattern(int[,] pattern)
        {
            EmbedPositionFinderPatternAndSeparator(pattern);
            EmbedDarkDotAtLeftBottomCorner(pattern);
            EmbedPositionAlignmentPattern(pattern);
            EmbedTimingPattern(pattern);
        }

        /// <summary>
        /// 嵌入位置探测和分隔符图形
        /// </summary>
        /// <param name="pattern">模板</param>
        private void EmbedPositionFinderPatternAndSeparator(int[,] pattern)
        {
            /* 嵌入位置探测图形 */
            int finderDimension = 7;
            // 左上角
            EmbedPositionFinderPattern(pattern, 0, 0);
            // 右上角
            EmbedPositionFinderPattern(pattern, Dimension - finderDimension, 0);
            // 左下角
            EmbedPositionFinderPattern(pattern, 0, Dimension - finderDimension);

            /* 嵌入水平分隔符图形 */
            int horizontalWidth = 8;
            // 左上角
            EmbedHorizontalSeparationPattern(pattern, 0, horizontalWidth - 1);
            // 右上角
            EmbedHorizontalSeparationPattern(pattern, Dimension - horizontalWidth, horizontalWidth - 1);
            // 左下角
            EmbedHorizontalSeparationPattern(pattern, Dimension - horizontalWidth, horizontalWidth);

            /* 嵌入垂直分隔符图形 */
            int verticalHeight = 7;
            // 左上角
            EmbedVerticalSeparationPattern(pattern, verticalHeight, 0);
            // 右上角
            EmbedVerticalSeparationPattern(pattern, Dimension - verticalHeight - 1, 0);
            // 左下角
            EmbedVerticalSeparationPattern(pattern, verticalHeight, Dimension - verticalHeight);
        }

        /// <summary>
        /// 嵌入位置探测图形
        /// </summary>
        /// <param name="pattern">模板</param>
        /// <param name="xStart">x起始坐标</param>
        /// <param name="yStart">y起始坐标</param>
        private static void EmbedPositionFinderPattern(int[,] pattern, int xStart, int yStart)
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
        private static void EmbedHorizontalSeparationPattern(int[,] pattern, int xStart, int yStart)
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
        private static void EmbedVerticalSeparationPattern(int[,] pattern, int xStart, int yStart)
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
        private void EmbedDarkDotAtLeftBottomCorner(int[,] pattern)
        {
            pattern[8, Dimension - 8] = 1;
        }

        /// <summary>
        /// 嵌入位置校正图形(版本2+)
        /// </summary>
        /// <param name="pattern">模板</param>
        private void EmbedPositionAlignmentPattern(int[,] pattern)
        {
            if (Version.VersionNumber < 2)
            {
                return;
            }
            int[] coordinates = POSITION_ALIGNMENT_PATTERN_COORDINATE[Version.VersionNumber];
            for (int x = 0; x < coordinates.Length; x++)
            {
                for (int y = 0; y < coordinates.Length; y++)
                {
                    if (IsEmpty(pattern[x, y]))
                    {
                        EmbedPositionAlignmentPattern(pattern, x - 2, y - 2);
                    }
                }
            }
        }

        /// <summary>
        /// 嵌入位置校正图形
        /// </summary>
        /// <param name="pattern">模板</param>
        /// <param name="xStart">x起始坐标</param>
        /// <param name="yStart">y起始坐标</param>
        private static void EmbedPositionAlignmentPattern(int[,] pattern, int xStart, int yStart)
        {
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    pattern[xStart + x, yStart + y] = POSITION_ALIGNMENT_PATTERN[x, y];
                }
            }
        }

        /// <summary>
        /// 嵌入定位图形
        /// </summary>
        /// <param name="pattern">模板</param>
        private void EmbedTimingPattern(int[,] pattern)
        {
            for (int i = 8; i < Dimension - 8; i++)
            {
                int isBlack = ((i + 1) % 2);
                // 水平
                if (IsEmpty(pattern[i, 6]))
                {
                    pattern[i, 6] = isBlack;
                }
                // 垂直
                if (IsEmpty(pattern[6, i]))
                {
                    pattern[6, i] = isBlack;
                }
            }
        }

        /// <summary>
        /// 嵌入格式信息
        /// </summary>
        /// <param name="pattern">模板</param>
        /// <param name="id">模板序号</param>
        private void EmbedFormatInfo(int[,] pattern, int id)
        {
            bool[] formatInfo = CalculateFormatInfo(id);
            for (int i = 0; i < 15; i++)
            {
                int isBlack = formatInfo[14 - i] ? 1 : 0;
                // 左上角
                pattern[FORMAT_INFO_COORDINATES[i, 0], FORMAT_INFO_COORDINATES[i, 1]] = isBlack;
                int x, y;
                // 右上角
                if (i < 8)
                {
                    x = Dimension - i - 1;
                    y = 8;
                }
                // 左下角
                else
                {
                    x = 8;
                    y = Dimension + i - 15;
                }
                pattern[x, y] = isBlack;
            }
        }

        /// <summary>
        /// 计算格式信息
        /// </summary>
        /// <param name="id">模板序号</param>
        /// <returns>格式信息</returns>
        private bool[] CalculateFormatInfo(int id)
        {
            // 数据信息5bit(纠错等级2bit+模板序号3bit)
            int dataInfo = 0;
            // 纠错等级2bit
            switch (Level)
            {
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
            QrCode.AddBits(formatInfo, 0, value, 15);
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
        /// 嵌入版本信息(版本7+)
        /// </summary>
        /// <param name="pattern"></param>
        private void EmbedVersionInfo(int[,] pattern)
        {
            if (Version.VersionNumber < 7)
            {
                return;
            }
            bool[] versionInfo = CalculateVersionInfo();
            int index = 17;
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int isBlack = versionInfo[index] ? 1 : 0;
                    // 左下角
                    pattern[i, Dimension - 11 + j] = isBlack;
                    // 右上角
                    pattern[Dimension - 11 + i, j] = isBlack;
                }
            }
        }

        /// <summary>
        /// 计算版本信息
        /// </summary>
        /// <returns>版本信息</returns>
        private bool[] CalculateVersionInfo()
        {
            // 数据信息6bit(版本号6bit)
            int dataInfo = Version.VersionNumber;
            // BCH(18,6)纠错码12bit
            int bchCode = CalculateBchCode(dataInfo, VERSION_INFO_POLY);
            int value = (dataInfo << 12) | bchCode;
            bool[] versionInfo = new bool[18];
            QrCode.AddBits(versionInfo, 0, value, 18);
            return versionInfo;
        }

        /// <summary>
        /// 嵌入数据
        /// </summary>
        /// <param name="pattern">模板</param>
        /// <param name="id">模板序号</param>
        private void EmbedData(int[,] pattern, int id)
        {
            int index = 0;
            int direction = -1;
            // 从右下角开始
            int x = Dimension - 1;
            int y = Dimension - 1;
            while (x > 0)
            {
                // 跳过垂直分隔符图形
                if (x == 6)
                {
                    x -= 1;
                }
                while (y >= 0 && y < Dimension)
                {
                    for (int i = 0; i < 2; ++i)
                    {
                        int xx = x - i;
                        // 跳过不为空
                        if (!IsEmpty(pattern[xx, y]))
                        {
                            continue;
                        }
                        int isBlack;
                        if (index < Data.Length)
                        {
                            isBlack = Data[index] ? 1 : 0;
                            ++index;
                        }
                        else
                        {
                            isBlack = 0;
                        }
                        if (GetMaskBit(id, xx, y))
                        {
                            isBlack ^= 0x1;
                        }
                        pattern[xx, y] = isBlack;
                    }
                    y += direction;
                }
                direction = -direction;
                y += direction;
                x -= 2;
            }
        }

        /// <summary>
        /// 获取指定坐标的掩模值
        /// </summary>
        /// <param name="id">模板序号</param>
        /// <param name="x">坐标x</param>
        /// <param name="y">坐标y</param>
        /// <returns></returns>
        public static bool GetMaskBit(int id, int x, int y)
        {
            int value = 0, temp;
            switch (id)
            {
                case 0:
                    {
                        value = (y + x) & 0x1;
                        break;
                    }
                case 1:
                    {
                        value = y & 0x1;
                        break;
                    }
                case 2:
                    {
                        value = x % 3;
                        break;
                    }
                case 3:
                    {
                        value = (y + x) % 3;
                        break;
                    }
                case 4:
                    {
                        value = (((int)((uint)y >> 1)) + (x / 3)) & 0x1;
                        break;
                    }
                case 5:
                    {
                        temp = y * x;
                        value = (temp & 0x1) + (temp % 3);
                        break;
                    }
                case 6:
                    {
                        temp = y * x;
                        value = (((temp & 0x1) + (temp % 3)) & 0x1);
                        break;
                    }
                case 7:
                    {
                        temp = y * x;
                        value = (((temp % 3) + ((y + x) & 0x1)) & 0x1);
                        break;
                    }
            }
            return value == 0;
        }

        /// <summary>
        /// 掩模惩戒规则1
        /// </summary>
        /// <param name="pattern">模板</param>
        /// <param name="isHorizontal">水平</param>
        /// <returns></returns>
        private static int MaskPenaltyRule1(int[,] pattern, bool isHorizontal)
        {
            int dimension = pattern.GetLength(0);
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
        /// 位置探测图形
        /// <para>索引[x坐标,y坐标]:7x7</para>
        /// <para>数量:3个(左上角、右上角、左下角)</para>
        /// <para>数据来源 ISO/IEC 18004-2015 -> 6.3.3.1</para>
        /// </summary>
        private static readonly int[,] POSITION_FINDER_PATTERN =
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
        private static readonly int[,] POSITION_ALIGNMENT_PATTERN =
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
