namespace DreamsOfEverus.Simulation
{
    /// <summary>What a fighter is standing on.</summary>
    public enum GroundType : byte
    {
        None,        // In the air
        Solid,
        PassThrough,
    }

    /// <summary>What a fighter is doing right now. Each action has its own movement rules (see FighterMovement).</summary>
    public enum FighterAction : byte
    {
        Idle,          // Standing still (or sliding to a stop)
        Walk,          // Tilting the stick: speed follows how far it's tilted
        Dash,          // Started by a hard sideways press. Lasts for the character's trot; a hard press back dash dances
        Run,           // Holding the direction after the trot ends
        RunTurnaround, // Pressing back while running: the slower skid-and-turn
        JumpSquat,     // The few frames of crouch before leaving the ground
        Airborne,      // In the air (jumping, falling, or after walking off a ledge)
        Crouch,        // Holding Down on the ground: shorter body
        Crawl,         // Crouching and moving (only characters that can crawl)
    }

    /// <summary>
    /// Everything about one fighter that changes during play.
    /// Only plain values (numbers, vectors, enums) go in here, so copying the struct copies everything,
    /// and comparing two of them (Equals) compares every field automatically.
    /// If you ever add an array or a class here, GameState.CopyFrom must be updated to copy it deeply.
    /// </summary>
    public struct FighterState
    {
        public FixedVector2 Position;     // Feet position (bottom-center of the body box), in world units
        public FixedVector2 Velocity;     // World units per tick
        public GroundType Ground;         // What the feet are on right now
        public sbyte Facing;              // 1 = facing right, -1 = facing left

        public FighterAction Action;      // What the fighter is doing
        public int ActionTick;            // How many ticks the current action has lasted

        public byte AirJumpsUsed;         // Reset on landing
        public bool IsFastFalling;        // Set by a hard press of Down while falling; cleared on landing or air jump
        public bool ShortHopQueued;       // Jump was released during jump squat, so this jump will be a short hop

        // Input history, used to spot hard presses. Part of the state so rollback replays them exactly.
        public InputFrame PreviousInput;  // Last tick's input
        public byte TicksPushedLeft;      // How many ticks in a row the stick has been pushed left (stops at 255)
        public byte TicksPushedRight;     // ... pushed right
        public byte TicksPushedDown;      // ... pushed down

        public bool IsGrounded => Ground != GroundType.None;
        public bool IsLow => Action == FighterAction.Crouch || Action == FighterAction.Crawl;
    }

    /// <summary>
    /// A full snapshot of gameplay at one tick. Plain data, no Unity objects.
    /// Saving a copy of this and restoring it later is exactly what rollback needs.
    /// </summary>
    public sealed class GameState
    {
        public int Tick;
        public FighterState Player;

        public void CopyFrom(GameState other)
        {
            Tick = other.Tick;
            Player = other.Player;
        }

        public GameState Clone()
        {
            var copy = new GameState();
            copy.CopyFrom(this);
            return copy;
        }

        public bool SameAs(GameState other)
        {
            return Tick == other.Tick && Player.Equals(other.Player);
        }
    }
}
