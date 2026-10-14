using System;

namespace DreamsOfEverus.Simulation
{
    /// <summary>Buttons the simulation cares about. Each is one bit, so several can be held at once.</summary>
    [Flags]
    public enum InputButtons : byte
    {
        None = 0,
        Jump = 1 << 0,
        Attack = 1 << 1,
    }

    /// <summary>
    /// One player's input for one tick. The simulation reads exactly one of these per tick.
    /// The stick is stored as whole numbers from -100 to 100 so that inputs are exact
    /// (needed later for rollback, where inputs are sent over the network and replayed).
    /// </summary>
    [Serializable]
    public struct InputFrame : IEquatable<InputFrame>
    {
        public const int StickMax = 100;

        public sbyte MoveX; // -100 = full left, 100 = full right
        public sbyte MoveY; // -100 = full down, 100 = full up
        public InputButtons Held;

        public bool IsHeld(InputButtons button) => (Held & button) != 0;

        public bool Equals(InputFrame other) => MoveX == other.MoveX && MoveY == other.MoveY && Held == other.Held;
        public override bool Equals(object obj) => obj is InputFrame other && Equals(other);
        public override int GetHashCode() => (MoveX << 16) ^ (MoveY << 8) ^ (int)Held;
    }
}
