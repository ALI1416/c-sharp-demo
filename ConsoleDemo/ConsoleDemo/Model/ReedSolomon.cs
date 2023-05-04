using System.Collections.Generic;

namespace ConsoleDemo.Model
{

    /// <summary>
    /// Reed-Solomon(QrCode)
    /// </summary>
    public class ReedSolomon
    {

        /// <summary>
        /// GenericGFPoly列表
        /// </summary>
        private static readonly List<GenericGFPoly> GenericGFPolyList = new List<GenericGFPoly>();

        static ReedSolomon()
        {
            // 初始化GenericGFPoly
            GenericGFPolyList.Add(new GenericGFPoly(new int[] { 1 }));
            for (int i = 1; i < 124; i++)
            {
                GenericGFPolyList.Add(GenericGFPolyList[i - 1].Multiply(new GenericGFPoly(new int[] { 1, GenericGF.Exp(i - 1) })));
            }
        }

        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="coefficients">多项式常数</param>
        /// <param name="degree">多项式次数</param>
        /// <returns>结果</returns>
        public static int[] Encoder(int[] coefficients, int degree)
        {
            GenericGFPoly info = new GenericGFPoly(coefficients);
            info = info.MultiplyByMonomial(degree, 1);
            GenericGFPoly remainder = info.Divide(GenericGFPolyList[degree])[1];
            return remainder.Coefficients;
        }

    }
}
