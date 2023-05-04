using System;
using System.Text;

namespace ConsoleDemo.Model
{

    /// <summary>
    /// GenericGFPoly(QrCode)
    /// </summary>
    public class GenericGFPoly
    {

        /// <summary>
        /// 多项式常数
        /// </summary>
        public readonly int[] Coefficients;
        /// <summary>
        /// 多项式次数
        /// </summary>
        private readonly int Degree;
        /// <summary>
        /// 是否为0
        /// </summary>
        private readonly bool IsZero;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="coefficients">多项式常数</param>
        public GenericGFPoly(int[] coefficients)
        {
            int coefficientsLength = coefficients.Length;
            if (coefficientsLength > 1 && coefficients[0] == 0)
            {
                int firstNonZero = 1;
                while (firstNonZero < coefficientsLength && coefficients[firstNonZero] == 0)
                {
                    firstNonZero++;
                }
                if (firstNonZero == coefficientsLength)
                {
                    Coefficients = new int[] { 0 };
                }
                else
                {
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
        /// 获取多项式中`指定次方`的系数
        /// </summary>
        /// <param name="degree">指定次方</param>
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
                int[] temp = smallerCoefficients;
                smallerCoefficients = largerCoefficients;
                largerCoefficients = temp;
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
                return GenericGF.Zero;
            }
            int[] aCoefficients = Coefficients;
            int aLength = aCoefficients.Length;
            int[] bCoefficients = other.Coefficients;
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
        public GenericGFPoly MultiplyByMonomial(int degree, int coefficient)
        {
            if (coefficient == 0)
            {
                return GenericGF.Zero;
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
        /// 除法
        /// </summary>
        public GenericGFPoly[] Divide(GenericGFPoly other)
        {
            GenericGFPoly quotient = GenericGF.Zero;
            GenericGFPoly remainder = this;
            int denominatorLeadingTerm = other.GetCoefficient(other.Degree);
            int inverseDenominatorLeadingTerm = GenericGF.Inverse(denominatorLeadingTerm);
            while (remainder.Degree >= other.Degree && !remainder.IsZero)
            {
                int degreeDifference = remainder.Degree - other.Degree;
                int scale = GenericGF.Multiply(remainder.GetCoefficient(remainder.Degree), inverseDenominatorLeadingTerm);
                GenericGFPoly term = other.MultiplyByMonomial(degreeDifference, scale);
                GenericGFPoly iterationQuotient = GenericGF.BuildMonomial(degreeDifference, scale);
                quotient = quotient.Addition(iterationQuotient);
                remainder = remainder.Addition(term);
            }
            return new GenericGFPoly[] { quotient, remainder };
        }

        public override string ToString()
        {
            if (IsZero)
            {
                return "0";
            }
            StringBuilder result = new StringBuilder(8 * Degree);
            for (int degree = Degree; degree >= 0; degree--)
            {
                int coefficient = GetCoefficient(degree);
                if (coefficient != 0)
                {
                    if (coefficient < 0)
                    {
                        if (degree == Degree)
                        {
                            result.Append("-");
                        }
                        else
                        {
                            result.Append(" - ");
                        }
                        coefficient = -coefficient;
                    }
                    else
                    {
                        if (result.Length > 0)
                        {
                            result.Append(" + ");
                        }
                    }
                    if (degree == 0 || coefficient != 1)
                    {
                        int alphaPower = GenericGF.Log(coefficient);
                        if (alphaPower == 0)
                        {
                            result.Append('1');
                        }
                        else if (alphaPower == 1)
                        {
                            result.Append('a');
                        }
                        else
                        {
                            result.Append("a^");
                            result.Append(alphaPower);
                        }
                    }
                    if (degree != 0)
                    {
                        if (degree == 1)
                        {
                            result.Append('x');
                        }
                        else
                        {
                            result.Append("x^");
                            result.Append(degree);
                        }
                    }
                }
            }
            return result.ToString();
        }

    }
}
