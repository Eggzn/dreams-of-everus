using System.Collections.Generic;

namespace DreamsOfEverus.Simulation
{
    /// <summary>
    /// Rewind-and-replay, the heart of rollback: start from a saved snapshot and run the same inputs again.
    /// Because the simulation is deterministic, the result must match what happened live, exactly.
    /// </summary>
    public static class SnapshotReplay
    {
        /// <summary>Returns a new state: the snapshot advanced by the given inputs, one tick each. The snapshot isn't changed.</summary>
        public static GameState Replay(GameSimulation simulation, GameState snapshot, IReadOnlyList<InputFrame> inputs)
        {
            GameState state = snapshot.Clone();
            foreach (InputFrame input in inputs)
            {
                simulation.Step(state, input);
            }
            return state;
        }
    }
}
