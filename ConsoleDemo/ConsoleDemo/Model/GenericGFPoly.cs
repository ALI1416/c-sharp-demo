using System;

namespace ConsoleDemo.Model
{

    /// <summary>
    /// 通用Galois Fields域多项式(通用伽罗华域多项式)
    /// <para>仅适用于QrCode</para>
    /// </summary>
    public class GenericGFPoly
    {

        /// <summary>
        /// 多项式系数([0]常数项、[1]一次方的系数、[2]二次方的系数...)
        /// </summary>
        public readonly int[] Coefficients;
        /// <summary>
        /// 多项式次数
        /// </summary>
        private readonly int Degree;
        /// <summary>
        /// 多项式是否为0(常数项为0)
        /// </summary>
        private readonly bool IsZero;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="coefficients">多项式常数</param>
        public GenericGFPoly(int[] coefficients)
        {
            int coefficientsLength = coefficients.Length;
            // 常数项为0
            if (coefficients[0] == 0)
            {
                // 查找第一个非0的下标
                int firstNonZero = 1;
                while (firstNonZero < coefficientsLength && coefficients[firstNonZero] == 0)
                {
                    firstNonZero++;
                }
                // 全为0
                if (firstNonZero == coefficientsLength)
                {
                    // 该多项式为0
                    Coefficients = new int[] { 0 };
                }
                else
                {
                    // 去除前面的0
                    Coefficients = new int[coefficientsLength - firstNonZero];
                    Array.Copy(coefficients, firstNonZero, Coefficients, 0, Coefficients.Length);
                }
            }
            else
            {
                Coefficients = coefficients;
            }
            Degree = Coefficients.Length - 1;
            IsZero = Coefficients[0] == 0;
        }

        /// <summary>
        /// 获取多项式中`次数`的系数
        /// </summary>
        /// <param name="degree">次数</param>
        /// <returns>系数</returns>
        public int GetCoefficient(int degree)
        {
            return Coefficients[Coefficients.Length - 1 - degree];
        }

        /// <summary>
        /// 加法
        /// </summary>
        public GenericGFPoly Addition(GenericGFPoly other)
        {
            if (IsZero)
            {
                return other;
            }
            if (other.IsZero)
            {
                return this;
            }
            int[] smallerCoefficients = Coefficients;
            int[] largerCoefficients = other.Coefficients;
            if (smallerCoefficients.Length > largerCoefficients.Length)
            {
                (largerCoefficients, smallerCoefficients) = (smallerCoefficients, largerCoefficients);
            }
            int[] sumDiff = new int[largerCoefficients.Length];
            int lengthDiff = largerCoefficients.Length - smallerCoefficients.Length;
            Array.Copy(largerCoefficients, 0, sumDiff, 0, lengthDiff);
            for (int i = lengthDiff; i < largerCoefficients.Length; i++)
            {
                sumDiff[i] = GenericGF.Addition(smallerCoefficients[i - lengthDiff], largerCoefficients[i]);
            }
            return new GenericGFPoly(sumDiff);
        }

        /// <summary>
        /// 乘法
        /// </summary>
        public GenericGFPoly Multiply(GenericGFPoly other)
        {
            if (IsZero || other.IsZero)
            {
                return ReedSolomon.Zero;
            }
            int[] aCoefficients = Coefficients;
            int[] bCoefficients = other.Coefficients;
            int aLength = aCoefficients.Length;
            int bLength = bCoefficients.Length;
            int[] product = new int[aLength + bLength - 1];
            for (int i = 0; i < aLength; i++)
            {
                int aCoeff = aCoefficients[i];
                for (int j = 0; j < bLength; j++)
                {
                    product[i + j] = GenericGF.Addition(product[i + j], GenericGF.Multiply(aCoeff, bCoefficients[j]));
                }
            }
            return new GenericGFPoly(product);
        }

        /// <summary>
        /// 单项式乘法
        /// </summary>
        /// <param name="degree">次数</param>
        /// <param name="coefficient">系数</param>
        public GenericGFPoly MultiplyByMonomial(int degree, int coefficient)
        {
            if (coefficient == 0)
            {
                return ReedSolomon.Zero;
            }
            int size = Coefficients.Length;
            int[] product = new int[size + degree];
            for (int i = 0; i < size; i++)
            {
                product[i] = GenericGF.Multiply(Coefficients[i], coefficient);
            }
            return new GenericGFPoly(product);
        }

        /// <summary>
        /// 除法的余数
        /// </summary>
        public GenericGFPoly RemainderOfDivide(GenericGFPoly other)
        {
            GenericGFPoly remainder = this;
            int denominatorLeadingTerm = other.GetCoefficient(other.Degree);
            int inverseDenominatorLeadingTerm = GenericGF.Inverse(denominatorLeadingTerm);
            while (remainder.Degree >= other.Degree && !remainder.IsZero)
            {
                int degreeDifference = remainder.Degree - other.Degree;
                int scale = GenericGF.Multiply(remainder.GetCoefficient(remainder.Degree), inverseDenominatorLeadingTerm);
                GenericGFPoly term = other.MultiplyByMonomial(degreeDifference, scale);
                remainder = remainder.Addition(term);
            }
            return remainder;
        }

    }
}
