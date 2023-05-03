using System;
using System.Text;

namespace ConsoleDemo.Model
{

    /// <summary>
    /// GenericGFPoly(QrCode)
    /// </summary>
    public class GenericGFPoly
    {

        /* GenericGF */
        private static readonly int primitive = 0x011D;
        private static readonly int size = 256;
        private static readonly int[] expTable;
        private static readonly int[] logTable;

        /* GenericGFPoly */
        private readonly int[] coefficients;

        static GenericGFPoly()
        {
            /* GenericGF */
            expTable = new int[size];
            logTable = new int[size];
            int x = 1;
            for (int i = 0; i < size; i++)
            {
                expTable[i] = x;
                x <<= 1;
                if (x >= size)
                {
                    x ^= primitive;
                    x &= size - 1;
                }
            }
            for (int i = 0; i < size - 1; i++)
            {
                logTable[expTable[i]] = i;
            }
        }

        /* GenericGF */
        public static int AddOrSubtract(int a, int b)
        {
            return a ^ b;
        }

        public static int Exp(int a)
        {
            return expTable[a];
        }

        public int Log(int a)
        {
            return logTable[a];
        }

        public static int Inverse(int a)
        {
            return expTable[size - logTable[a] - 1];
        }

        public static int Multiply(int a, int b)
        {
            if (a == 0 || b == 0)
            {
                return 0;
            }
            return expTable[(logTable[a] + logTable[b]) % (size - 1)];
        }

        /* GenericGFPoly */
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
                    this.coefficients = new int[] { 0 };
                }
                else
                {
                    this.coefficients = new int[coefficientsLength - firstNonZero];
                    Array.Copy(coefficients, firstNonZero, this.coefficients, 0, this.coefficients.Length);
                }
            }
            else
            {
                this.coefficients = coefficients;
            }
        }

        public int[] Coefficients
        {
            get { return coefficients; }
        }

        public int Degree
        {
            get
            {
                return coefficients.Length - 1;
            }
        }

        public bool IsZero
        {
            get { return coefficients[0] == 0; }
        }

        public int GetCoefficient(int degree)
        {
            return coefficients[coefficients.Length - 1 - degree];
        }

        public GenericGFPoly AddOrSubtract(GenericGFPoly other)
        {
            if (IsZero)
            {
                return other;
            }
            if (other.IsZero)
            {
                return this;
            }
            int[] smallerCoefficients = this.coefficients;
            int[] largerCoefficients = other.coefficients;
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
                sumDiff[i] = AddOrSubtract(smallerCoefficients[i - lengthDiff], largerCoefficients[i]);
            }
            return new GenericGFPoly(sumDiff);
        }

        public GenericGFPoly Multiply(GenericGFPoly other)
        {
            int[] aCoefficients = this.coefficients;
            int aLength = aCoefficients.Length;
            int[] bCoefficients = other.coefficients;
            int bLength = bCoefficients.Length;
            int[] product = new int[aLength + bLength - 1];
            for (int i = 0; i < aLength; i++)
            {
                int aCoeff = aCoefficients[i];
                for (int j = 0; j < bLength; j++)
                {
                    product[i + j] = AddOrSubtract(product[i + j], Multiply(aCoeff, bCoefficients[j]));
                }
            }
            return new GenericGFPoly(product);
        }

        public GenericGFPoly MultiplyByMonomial(int degree, int coefficient)
        {
            int size = coefficients.Length;
            int[] product = new int[size + degree];
            for (int i = 0; i < size; i++)
            {
                product[i] = Multiply(coefficients[i], coefficient);
            }
            return new GenericGFPoly(product);
        }

        public GenericGFPoly[] Divide(GenericGFPoly other)
        {
            GenericGFPoly quotient = new GenericGFPoly(new int[] { 0 });
            GenericGFPoly remainder = this;
            int denominatorLeadingTerm = other.GetCoefficient(other.Degree);
            int inverseDenominatorLeadingTerm = Inverse(denominatorLeadingTerm);
            while (remainder.Degree >= other.Degree && !remainder.IsZero)
            {
                int degreeDifference = remainder.Degree - other.Degree;
                int scale = Multiply(remainder.GetCoefficient(remainder.Degree), inverseDenominatorLeadingTerm);
                GenericGFPoly term = other.MultiplyByMonomial(degreeDifference, scale);
                int[] coefficients = new int[degreeDifference + 1];
                coefficients[0] = scale;
                GenericGFPoly iterationQuotient = new GenericGFPoly(coefficients);
                quotient = quotient.AddOrSubtract(iterationQuotient);
                remainder = remainder.AddOrSubtract(term);
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
                        int alphaPower = Log(coefficient);
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
