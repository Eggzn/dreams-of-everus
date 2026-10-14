using DreamsOfEverus.Simulation;

namespace DreamsOfEverus.Tests
{
    /// <summary>Small helpers for building test levels and fighters.</summary>
    public static class TestWorld
    {
        public static Fixed F(int value) => Fixed.FromInt(value);
        public static Fixed F(int numerator, int denominator) => Fixed.FromFraction(numerator, denominator);
        public static FixedVector2 V(Fixed x, Fixed y) => new FixedVector2(x, y);
        public static FixedVector2 V(int x, int y) => new FixedVector2(F(x), F(y));

        /// <summary>A 1 wide, 2 tall fighter (test values only).</summary>
        public static FighterConfig Fighter() => new FighterConfig { BodyWidth = F(1), BodyHeight = F(2) };

        /// <summary>A box given by its left, bottom, right and top edges.</summary>
        public static FixedBox Box(int left, int bottom, int right, int top) => new FixedBox(V(left, bottom), V(right, top));

        public static GameSimulation EmptySimulation() => new GameSimulation(new LevelGeometry(), Fighter());
    }
}
