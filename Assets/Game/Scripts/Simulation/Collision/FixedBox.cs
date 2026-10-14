using System;

namespace DreamsOfEverus.Simulation
{
    /// <summary>An axis-aligned rectangle in fixed-point. Min is the bottom-left corner, Max the top-right.</summary>
    [Serializable]
    public struct FixedBox : IEquatable<FixedBox>
    {
        public FixedVector2 Min;
        public FixedVector2 Max;

        public FixedBox(FixedVector2 min, FixedVector2 max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>True if the two boxes share some area. Boxes that only touch at an edge do not overlap.</summary>
        public bool Overlaps(FixedBox other)
        {
            return Min.X < other.Max.X && Max.X > other.Min.X
                && Min.Y < other.Max.Y && Max.Y > other.Min.Y;
        }

        public bool Equals(FixedBox other) => Min == other.Min && Max == other.Max;
        public override bool Equals(object obj) => obj is FixedBox other && Equals(other);
        public override int GetHashCode() => Min.GetHashCode() * 397 ^ Max.GetHashCode();
        public override string ToString() => $"[{Min} .. {Max}]";
    }
}
