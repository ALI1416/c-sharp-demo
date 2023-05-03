using System.Collections.Generic;

namespace ConsoleDemo.Model
{

    /// <summary>
    /// Reed-Solomon(QrCode)
    /// </summary>
    public class ReedSolomon
    {

        private static readonly List<GenericGFPoly> cachedGenerators = new List<GenericGFPoly>();

        static ReedSolomon()
        {
            cachedGenerators.Add(new GenericGFPoly(new int[] { 1 }));
            for (int i = 1; i < 124; i++)
            {
                cachedGenerators.Add(cachedGenerators[i - 1].Multiply(new GenericGFPoly(new int[] { 1, GenericGFPoly.Exp(i - 1) })));
            }
        }

        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="ec">纠错</param>
        public static void Encoder(byte[] data, byte[] ec)
        {
            int dataBytes = data.Length;
            int ecBytes = ec.Length;
            int[] d = new int[dataBytes];
            for (int i = 0; i < dataBytes; i++)
            {
                d[i] = data[i];
            }

            var info = new GenericGFPoly(d);
            info = info.MultiplyByMonomial(ecBytes, 1);

            var remainder = info.Divide(cachedGenerators[ecBytes])[1];
            int[] coefficients = remainder.Coefficients;
            var numZeroCoefficients = ecBytes - coefficients.Length;
            //for (var i = 0; i < numZeroCoefficients; i++)
            //{
            //    ec[dataBytes + i] = 0;
            //}

        }

    }
}
