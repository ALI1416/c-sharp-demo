using System.Collections.Generic;

namespace ConsoleDemo.Model
{

    /// <summary>
    /// Reed-Solomon(里德-所罗门码)
    /// <para>仅适用于QrCode</para>
    /// </summary>
    public class ReedSolomon
    {

        /// <summary>
        /// GenericGFPoly列表
        /// </summary>
        private static readonly List<GenericGFPoly> GenericGFPolyList = new List<GenericGFPoly>();

        static ReedSolomon()
        {
            // 初始化GenericGFPoly列表
            GenericGFPolyList.Add(new GenericGFPoly(new int[] { 1 }));
            // 最大值68
            // 数据来源 ISO/IEC 18004-2015 -> Annex A -> Table A.1 -> Number of error correction codewords列最大值
            for (int i = 1; i < 69; i++)
            {
                GenericGFPolyList.Add(GenericGFPolyList[i - 1].Multiply(new GenericGFPoly(new int[] { 1, GenericGF.Exp(i - 1) })));
            }
        }

        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="coefficients">系数</param>
        /// <param name="degree">次数</param>
        /// <returns>结果</returns>
        public static int[] Encoder(int[] coefficients, int degree)
        {
            GenericGFPoly info = new GenericGFPoly(coefficients);
            info = info.MultiplyByMonomial(degree, 1);
            GenericGFPoly remainder = info.RemainderOfDivide(GenericGFPolyList[degree]);
            return remainder.Coefficients;
        }

    }
}
