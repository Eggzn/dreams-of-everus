using System;
using System.Globalization;

namespace DreamsOfEverus.Simulation
{
    /// <summary>
    /// A fixed-point number: a decimal number stored as a whole number (long) underneath.
    /// Whole-number math gives the exact same result on every PC, which rollback netcode needs.
    /// Floats can differ slightly between machines, so gameplay never uses them.
    ///
    /// Format: 16 fractional bits. Raw value 65536 means 1.0, 32768 means 0.5, and so on.
    /// Smallest step is 1/65536 (about 0.000015). Safe range for multiplying is roughly +/- 30,000.
    /// </summary>
    [Serializable]
    public struct Fixed : IEquatable<Fixed>, IComparable<Fixed>
    {
        public const int FractionalBits = 16;
        public const long OneRaw = 1L << FractionalBits;

        // The stored whole number. Public so Unity can save it inside data assets.
        public long Raw;

        public static readonly Fixed Zero = new Fixed { Raw = 0 };
        public static readonly Fixed One = new Fixed { Raw = OneRaw };
        public static readonly Fixed Half = new Fixed { Raw = OneRaw / 2 };

        public static Fixed FromRaw(long raw) => new Fixed { Raw = raw };

        public static Fixed FromInt(int value) => new Fixed { Raw = (long)value << FractionalBits };

        /// <summary>Exact way to write a fraction in code, e.g. FromFraction(3, 2) is 1.5.</summary>
        public static Fixed FromFraction(int numerator, int denominator)
        {
            return new Fixed { Raw = ((long)numerator << FractionalBits) / denominator };
        }

        /// <summary>
        /// Converts a float typed in the Editor into fixed-point. Only use this when loading data,
        /// never inside the per-tick gameplay step. Rounds to the nearest step.
        /// </summary>
        public static Fixed FromFloat(float value)
        {
            return new Fixed { Raw = (long)Math.Round((double)value * OneRaw, MidpointRounding.AwayFromZero) };
        }

        /// <summary>For visuals only (rendering, camera, effects). Never feed this back into gameplay.</summary>
        public float ToFloat() => (float)Raw / OneRaw;

        /// <summary>Rounds down to a whole number (towards negative infinity).</summary>
        public int FloorToInt() => (int)(Raw >> FractionalBits);

        // ----- Operators -----

        public static Fixed operator +(Fixed a, Fixed b) => new Fixed { Raw = a.Raw + b.Raw };
        public static Fixed operator -(Fixed a, Fixed b) => new Fixed { Raw = a.Raw - b.Raw };
        public static Fixed operator -(Fixed a) => new Fixed { Raw = -a.Raw };
        public static Fixed operator *(Fixed a, Fixed b) => new Fixed { Raw = (a.Raw * b.Raw) >> FractionalBits };
        public static Fixed operator /(Fixed a, Fixed b) => new Fixed { Raw = (a.Raw << FractionalBits) / b.Raw };

        public static Fixed operator *(Fixed a, int b) => new Fixed { Raw = a.Raw * b };
        public static Fixed operator /(Fixed a, int b) => new Fixed { Raw = a.Raw / b };

        public static bool operator ==(Fixed a, Fixed b) => a.Raw == b.Raw;
        public static bool operator !=(Fixed a, Fixed b) => a.Raw != b.Raw;
        public static bool operator <(Fixed a, Fixed b) => a.Raw < b.Raw;
        public static bool operator >(Fixed a, Fixed b) => a.Raw > b.Raw;
        public static bool operator <=(Fixed a, Fixed b) => a.Raw <= b.Raw;
        public static bool operator >=(Fixed a, Fixed b) => a.Raw >= b.Raw;

        // ----- Helpers -----

        public static Fixed Abs(Fixed a) => a.Raw < 0 ? -a : a;
        public static Fixed Min(Fixed a, Fixed b) => a.Raw < b.Raw ? a : b;
        public static Fixed Max(Fixed a, Fixed b) => a.Raw > b.Raw ? a : b;
        public static Fixed Clamp(Fixed value, Fixed min, Fixed max) => Max(min, Min(max, value));

        /// <summary>Returns -1, 0 or 1.</summary>
        public static int Sign(Fixed a) => a.Raw > 0 ? 1 : (a.Raw < 0 ? -1 : 0);

        /// <summary>Moves current towards target by at most maxStep, without overshooting.</summary>
        public static Fixed MoveTowards(Fixed current, Fixed target, Fixed maxStep)
        {
            if (Abs(target - current) <= maxStep) return target;
            return current + (target > current ? maxStep : -maxStep);
        }

        // ----- Equality and display -----

        public bool Equals(Fixed other) => Raw == other.Raw;
        public override bool Equals(object obj) => obj is Fixed other && Equals(other);
        public override int GetHashCode() => Raw.GetHashCode();
        public int CompareTo(Fixed other) => Raw.CompareTo(other.Raw);

        public override string ToString()
        {
            return ((double)Raw / OneRaw).ToString("0.#####", CultureInfo.InvariantCulture);
        }
    }
}
