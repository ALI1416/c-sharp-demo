using System;

namespace ConsoleDemo.Util
{
    public sealed class ErrorCorrectionLevel
    {
        /// <summary> L = ~7% correction</summary>
        public static readonly ErrorCorrectionLevel L = new ErrorCorrectionLevel(0, 0x01, "L");
        /// <summary> M = ~15% correction</summary>
        public static readonly ErrorCorrectionLevel M = new ErrorCorrectionLevel(1, 0x00, "M");
        /// <summary> Q = ~25% correction</summary>
        public static readonly ErrorCorrectionLevel Q = new ErrorCorrectionLevel(2, 0x03, "Q");
        /// <summary> H = ~30% correction</summary>
        public static readonly ErrorCorrectionLevel H = new ErrorCorrectionLevel(3, 0x02, "H");

        private readonly int bits;

        private ErrorCorrectionLevel(int ordinal, int bits, String name)
        {
            this.ordinal_Renamed_Field = ordinal;
            this.bits = bits;
            this.name = name;
        }

        /// <summary>
        /// Gets the bits.
        /// </summary>
        public int Bits
        {
            get
            {
                return bits;
            }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public String Name
        {
            get
            {
                return name;
            }
        }

        private readonly int ordinal_Renamed_Field;
        private readonly String name;

        /// <summary>
        /// Ordinals this instance.
        /// </summary>
        /// <returns></returns>
        public int ordinal()
        {
            return ordinal_Renamed_Field;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override String ToString()
        {
            return name;
        }

    }
}
