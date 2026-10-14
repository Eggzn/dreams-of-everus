using System.Collections.Generic;

namespace DreamsOfEverus.Simulation
{
    public enum LevelBlockKind
    {
        Solid,       // Blocks from every side
        PassThrough, // Can be jumped up through and landed on; hard press of Down drops through
        FallZone,    // Touching it sends the fighter back to the respawn point
    }

    /// <summary>One box of level collision.</summary>
    public struct LevelBlock
    {
        public FixedBox Box;
        public LevelBlockKind Kind;

        public LevelBlock(FixedBox box, LevelBlockKind kind)
        {
            Box = box;
            Kind = kind;
        }
    }

    /// <summary>
    /// The level's collision and respawn point. It never changes during play,
    /// so it lives outside GameState and doesn't need to be snapshotted.
    /// </summary>
    public sealed class LevelGeometry
    {
        public readonly List<LevelBlock> Blocks = new List<LevelBlock>();
        public FixedVector2 RespawnPoint;

        public void Add(FixedBox box, LevelBlockKind kind) => Blocks.Add(new LevelBlock(box, kind));

        public bool TouchesFallZone(FixedBox body)
        {
            foreach (LevelBlock block in Blocks)
            {
                if (block.Kind == LevelBlockKind.FallZone && block.Box.Overlaps(body)) return true;
            }
            return false;
        }
    }
}
