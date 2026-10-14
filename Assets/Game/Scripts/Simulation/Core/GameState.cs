using System;

namespace DreamsOfEverus.Simulation
{
    /// <summary>What a fighter is standing on.</summary>
    public enum GroundType : byte
    {
        None,        // In the air
        Solid,
        PassThrough,
    }

    /// <summary>
    /// Everything about one fighter that changes during play.
    /// Only plain values (numbers, vectors, enums) go in here, so copying the struct copies everything.
    /// If you ever add an array or a class here, GameState.CopyFrom must be updated to copy it deeply.
    /// </summary>
    [Serializable]
    public struct FighterState : IEquatable<FighterState>
    {
        public FixedVector2 Position;    // Feet position (bottom-center of the body box), in world units
        public FixedVector2 Velocity;    // World units per tick
        public GroundType Ground;        // What the feet are on right now
        public InputFrame PreviousInput; // Last tick's input, used to detect presses and flicks

        public bool IsGrounded => Ground != GroundType.None;

        public bool Equals(FighterState other)
        {
            return Position == other.Position
                && Velocity == other.Velocity
                && Ground == other.Ground
                && PreviousInput.Equals(other.PreviousInput);
        }

        public override bool Equals(object obj) => obj is FighterState other && Equals(other);
        public override int GetHashCode() => Position.GetHashCode() ^ Velocity.GetHashCode();
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
