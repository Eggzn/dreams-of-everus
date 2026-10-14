using System;

namespace DreamsOfEverus.Simulation
{
    /// <summary>A 2D position or velocity made of two Fixed numbers. X is right, Y is up.</summary>
    [Serializable]
    public struct FixedVector2 : IEquatable<FixedVector2>
    {
        public Fixed X;
        public Fixed Y;

        public static readonly FixedVector2 Zero = new FixedVector2(Fixed.Zero, Fixed.Zero);

        public FixedVector2(Fixed x, Fixed y)
        {
            X = x;
            Y = y;
        }

        public static FixedVector2 operator +(FixedVector2 a, FixedVector2 b) => new FixedVector2(a.X + b.X, a.Y + b.Y);
        public static FixedVector2 operator -(FixedVector2 a, FixedVector2 b) => new FixedVector2(a.X - b.X, a.Y - b.Y);
        public static FixedVector2 operator -(FixedVector2 a) => new FixedVector2(-a.X, -a.Y);
        public static FixedVector2 operator *(FixedVector2 a, Fixed s) => new FixedVector2(a.X * s, a.Y * s);

        public static bool operator ==(FixedVector2 a, FixedVector2 b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(FixedVector2 a, FixedVector2 b) => !(a == b);

        public bool Equals(FixedVector2 other) => this == other;
        public override bool Equals(object obj) => obj is FixedVector2 other && Equals(other);
        public override int GetHashCode() => X.GetHashCode() * 397 ^ Y.GetHashCode();
        public override string ToString() => $"({X}, {Y})";
    }
}
