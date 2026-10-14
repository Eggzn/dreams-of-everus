namespace DreamsOfEverus.Simulation
{
    /// <summary>
    /// One attack's frame data and hitbox, in fixed-point. Built from an AttackData asset.
    /// Frames are counted from 0 on the tick the attack starts:
    /// [0 .. Startup) windup, [Startup .. Startup+Active) hitbox out, then Recovery until it ends.
    /// </summary>
    public sealed class AttackConfig
    {
        public int StartupTicks;
        public int ActiveTicks;
        public int RecoveryTicks;

        public FixedVector2 HitboxOffset; // Center of the hitbox from the feet. X is forward (flips with facing), Y is up
        public FixedVector2 HitboxSize;

        public int TotalTicks => StartupTicks + ActiveTicks + RecoveryTicks;

        public bool IsActiveOn(int frame) => frame >= StartupTicks && frame < StartupTicks + ActiveTicks;
    }

    /// <summary>The training dummy's setup: where it stands, its hurtbox and how long it reacts to a hit.</summary>
    public sealed class TrainingDummyConfig
    {
        public FixedVector2 Position;
        public Fixed HurtboxWidth;
        public Fixed HurtboxHeight;
        public int HitReactTicks;
    }
}
