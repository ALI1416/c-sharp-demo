using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleDemo.Util
{

    /// <summary>
    /// 二维码工具
    /// https://juejin.cn/post/7071499529995943950
    /// </summary>
    public class QrUtils
    {

        public static QRCode encode(string content, ErrorCorrectionLevel ecLevel)
        {
            // 头部
            var headerBits = new BitArray();
            headerBits.appendBits(4, 4);

            // 数据
            var dataBits = new BitArray();
            foreach (byte b in Encoding.UTF8.GetBytes(content))
            {
                dataBits.appendBits(b, 8);
            }

            // 获取版本
            Version version = ChooseVersion(headerBits.Size + GetBitNumber(ChooseVersion(headerBits.Size + 8 + dataBits.Size, ecLevel)) + dataBits.Size, ecLevel);

            // 头部和数据
            BitArray headerAndDataBits = new BitArray();
            headerAndDataBits.appendBitArray(headerBits);
            headerAndDataBits.appendBits(dataBits.SizeInBytes, GetBitNumber(version));
            headerAndDataBits.appendBitArray(dataBits);

            // 写入校验
            var ecBlocks = version.getECBlocksForLevel(ecLevel);
            var numDataBytes = version.TotalCodewords - ecBlocks.TotalECCodewords;
            for (int i = 0; i < 4 && headerAndDataBits.Size < (numDataBytes << 3); ++i)
            {
                headerAndDataBits.appendBit(false);
            }
            int numBitsInLastByte = headerAndDataBits.Size & 0x07;
            if (numBitsInLastByte > 0)
            {
                for (int i = numBitsInLastByte; i < 8; i++)
                {
                    headerAndDataBits.appendBit(false);
                }
            }
            int numPaddingBytes = numDataBytes - headerAndDataBits.SizeInBytes;
            for (int i = 0; i < numPaddingBytes; ++i)
            {
                headerAndDataBits.appendBits((i & 0x01) == 0 ? 0xEC : 0x11, 8);
            }

            // 交叉校验
            int numTotalBytes = version.TotalCodewords;
            int numRSBlocks = ecBlocks.NumBlocks;
            int dataBytesOffset = 0;
            int maxNumDataBytes = 0;
            int maxNumEcBytes = 0;
            var blocks = new List<BlockPair>(numRSBlocks);
            for (int i = 0; i < numRSBlocks; ++i)
            {
                int numDataBytesInBlock;
                int numEcBytesInBlock;
                if (i < (numRSBlocks - (numTotalBytes % numRSBlocks)))
                {
                    numDataBytesInBlock = (numDataBytes / numRSBlocks);
                    numEcBytesInBlock = (numTotalBytes / numRSBlocks) - (numDataBytes / numRSBlocks);
                }
                else
                {
                    numDataBytesInBlock = (numDataBytes / numRSBlocks) + 1;
                    numEcBytesInBlock = (numTotalBytes / numRSBlocks) - (numDataBytes / numRSBlocks);
                }
                byte[] dataBytes = new byte[numDataBytesInBlock];
                headerAndDataBits.toBytes(8 * dataBytesOffset, dataBytes, 0, numDataBytesInBlock);
                int numDataBytes2 = dataBytes.Length;
                int[] toEncode = new int[numDataBytes2 + numEcBytesInBlock];
                for (int j = 0; j < numDataBytes2; j++)
                {
                    toEncode[j] = dataBytes[j] & 0xFF;
                }
                new ReedSolomonEncoder(GenericGF.QR_CODE_FIELD_256).encode(toEncode, numEcBytesInBlock);
                byte[] ecBytes = new byte[numEcBytesInBlock];
                for (int j = 0; j < numEcBytesInBlock; j++)
                {
                    ecBytes[j] = (byte)toEncode[numDataBytes2 + j];
                }
                blocks.Add(new BlockPair(dataBytes, ecBytes));
                maxNumDataBytes = Math.Max(maxNumDataBytes, numDataBytesInBlock);
                maxNumEcBytes = Math.Max(maxNumEcBytes, ecBytes.Length);
                dataBytesOffset += numDataBytesInBlock;
            }
            BitArray finalBits = new BitArray();
            for (int i = 0; i < maxNumDataBytes; ++i)
            {
                foreach (BlockPair block in blocks)
                {
                    byte[] dataBytes = block.DataBytes;
                    if (i < dataBytes.Length)
                    {
                        finalBits.appendBits(dataBytes[i], 8);
                    }
                }
            }
            for (int i = 0; i < maxNumEcBytes; ++i)
            {
                foreach (BlockPair block in blocks)
                {
                    byte[] ecBytes = block.ErrorCorrectionBytes;
                    if (i < ecBytes.Length)
                    {
                        finalBits.appendBits(ecBytes[i], 8);
                    }
                }
            }

            // 构建二维码
            var qrCode = new QRCode
            {
                ECLevel = ecLevel,
                Version = version
            };
            var dimension = 17 + 4 * version.VersionNumber;
            var matrix = new ByteMatrix(dimension, dimension);

            // 选择掩码模板
            int minPenalty = int.MaxValue;
            int maskPattern = -1;
            for (int i = 0; i < 8; i++)
            {
                MatrixUtil.buildMatrix(finalBits, ecLevel, version, i, matrix);
                int penalty = MaskUtil.applyMaskPenaltyRule1(matrix) + MaskUtil.applyMaskPenaltyRule2(matrix) + MaskUtil.applyMaskPenaltyRule3(matrix) + MaskUtil.applyMaskPenaltyRule4(matrix);
                if (penalty < minPenalty)
                {
                    minPenalty = penalty;
                    maskPattern = i;
                }
            }
            qrCode.MaskPattern = maskPattern;

            // 根据模板构建
            MatrixUtil.buildMatrix(finalBits, ecLevel, version, maskPattern, matrix);
            qrCode.Matrix = matrix;
            return qrCode;
        }

        /// <summary>
        /// 选择版本
        /// </summary>
        /// <param name="bitNumber">比特数</param>
        /// <param name="ecLevel">ErrorCorrectionLevel</param>
        /// <returns></returns>
        private static Version ChooseVersion(int bitNumber, ErrorCorrectionLevel ecLevel)
        {
            for (int versionNum = 1; versionNum <= 40; versionNum++)
            {
                var version = Version.getVersionForNumber(versionNum);
                if ((version.TotalCodewords - version.getECBlocksForLevel(ecLevel).TotalECCodewords) >= ((bitNumber + 7) / 8))
                {
                    return version;
                }
            }
            throw new Exception("数据过大");
        }

        /// <summary>
        /// 获取指定版本所占的比特数
        /// </summary>
        /// <param name="version">Version</param>
        /// <returns></returns>
        public static int GetBitNumber(Version version)
        {
            if (version.VersionNumber <= 9)
            {
                return 8;
            }
            else
            {
                return 16;
            }
        }

    }
}
