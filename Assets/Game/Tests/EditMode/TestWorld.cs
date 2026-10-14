using DreamsOfEverus.Simulation;

namespace DreamsOfEverus.Tests
{
    /// <summary>Small helpers for building test levels, fighters and inputs.</summary>
    public static class TestWorld
    {
        public static Fixed F(int value) => Fixed.FromInt(value);
        public static Fixed F(int numerator, int denominator) => Fixed.FromFraction(numerator, denominator);
        public static FixedVector2 V(Fixed x, Fixed y) => new FixedVector2(x, y);
        public static FixedVector2 V(int x, int y) => new FixedVector2(F(x), F(y));

        /// <summary>
        /// A 1 wide, 2 tall fighter with simple per-tick values (TEST VALUES ONLY, not Bun's real tuning).
        /// </summary>
        public static FighterConfig Fighter() => new FighterConfig
        {
            BodyWidth = F(1),
            BodyHeight = F(2),

            WalkSpeed = F(1, 10),
            GroundAcceleration = F(1, 20),
            Traction = F(1, 20),
            DashSpeed = F(1, 5),
            TrotTicks = 10,
            RunSpeed = F(3, 20),
            RunTurnaroundTicks = 8,
            MoonwalkAcceleration = F(1, 20),

            CrouchHeight = F(1),
            CanCrawl = true,
            CrawlSpeed = F(1, 20),

            JumpSquatTicks = 3,
            FullHopVelocity = F(1, 2),
            ShortHopVelocity = F(3, 10),
            AirJumpVelocity = F(2, 5),
            AirJumps = 1,
            MaxJumpMomentum = F(1, 4),

            Gravity = F(1, 40),
            MaxFallSpeed = F(1, 2),
            FastFallSpeed = F(4, 5),
            AirSpeed = F(1, 10),
            AirAcceleration = F(1, 50),
            AirFriction = F(1, 100),
        };

        public static InputFeelConfig Feel() => new InputFeelConfig
        {
            StickDeadzone = 25,
            PressThreshold = 80,
            HardPressWindowTicks = 2,
            CrouchThreshold = 50,
        };

        /// <summary>A box given by its left, bottom, right and top edges.</summary>
        public static FixedBox Box(int left, int bottom, int right, int top) => new FixedBox(V(left, bottom), V(right, top));

        public static GameSimulation EmptySimulation() => new GameSimulation(new LevelGeometry(), Fighter(), Feel());

        /// <summary>An input frame: stick x/y from -100 to 100, plus Jump.</summary>
        public static InputFrame In(int x = 0, int y = 0, bool jump = false)
        {
            return new InputFrame { MoveX = (sbyte)x, MoveY = (sbyte)y, Held = jump ? InputButtons.Jump : InputButtons.None };
        }
    }
}
