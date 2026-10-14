namespace DreamsOfEverus.Simulation
{
    /// <summary>
    /// One character's tuning values in fixed-point, ready for the simulation.
    /// Built from the character's data asset (CharacterData). Speeds are in world units per tick,
    /// accelerations in units per tick per tick, durations in ticks (1 tick = 1 frame at 60 fps).
    /// </summary>
    public sealed class FighterConfig
    {
        // Collision box. Its bottom-center is the fighter's position (the feet).
        public Fixed BodyWidth;
        public Fixed BodyHeight;
        public Fixed HalfWidth => BodyWidth / 2;

        // Ground
        public Fixed WalkSpeed;
        public Fixed GroundAcceleration;
        public Fixed GroundFriction;
        public Fixed DashSpeed;
        public int TrotTicks;               // How long the initial dash lasts (from the trot distance)
        public Fixed RunSpeed;
        public int RunTurnaroundTicks;
        public Fixed RunTurnaroundDeceleration;

        // Jumping
        public int JumpSquatTicks;
        public Fixed FullHopVelocity;
        public Fixed ShortHopVelocity;
        public Fixed AirJumpVelocity;
        public int AirJumps;

        // Air
        public Fixed Gravity;
        public Fixed MaxFallSpeed;
        public Fixed FastFallSpeed;
        public Fixed AirSpeed;
        public Fixed AirAcceleration;
        public Fixed AirFriction;
    }

    /// <summary>How inputs are interpreted (shared by all characters). Stick values are -100..100.</summary>
    public sealed class InputFeelConfig
    {
        public int StickDeadzone;        // Below this, the stick counts as centered
        public int PressThreshold;       // How far the stick must go to count as a press
        public int HardPressWindowTicks; // A press counts as "hard" if it gets there within this many ticks of being centered
        public int DoubleTapWindowTicks; // Max ticks between the two presses of a double-tap
    }
}
