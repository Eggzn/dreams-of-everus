using DreamsOfEverus.Simulation;
using NUnit.Framework;

namespace DreamsOfEverus.Tests
{
    public class SimulationTests
    {
        // A fixed, varied input sequence so tests exercise different inputs on different ticks.
        private static InputFrame ScriptedInput(int tick)
        {
            return new InputFrame
            {
                MoveX = (sbyte)((tick * 37 % 201) - 100),
                MoveY = (sbyte)((tick * 53 % 201) - 100),
                Held = (InputButtons)(tick % 4),
            };
        }

        private static void Run(GameSimulation sim, GameState state, int fromTick, int toTick)
        {
            for (int t = fromTick; t < toTick; t++) sim.Step(state, ScriptedInput(t));
        }

        [Test]
        public void Step_AdvancesTickByOne()
        {
            var state = new GameState();
            TestWorld.EmptySimulation().Step(state, default);
            Assert.AreEqual(1, state.Tick);
        }

        [Test]
        public void SameInputs_GiveSameState()
        {
            var a = new GameState();
            var b = new GameState();
            Run(TestWorld.EmptySimulation(), a, 0, 300);
            Run(TestWorld.EmptySimulation(), b, 0, 300);
            Assert.IsTrue(a.SameAs(b));
        }

        [Test]
        public void Clone_IsIndependentCopy()
        {
            var state = new GameState();
            Run(TestWorld.EmptySimulation(), state, 0, 10);
            GameState copy = state.Clone();

            Run(TestWorld.EmptySimulation(), state, 10, 20);

            Assert.AreEqual(10, copy.Tick);
            Assert.AreEqual(20, state.Tick);
        }

        [Test]
        public void RestoreSnapshot_ThenReplay_MatchesOriginal()
        {
            var sim = TestWorld.EmptySimulation();
            var state = new GameState();
            Run(sim, state, 0, 60);
            GameState snapshot = state.Clone();

            Run(sim, state, 60, 120);
            GameState original = state.Clone();

            // Rewind to the snapshot and replay the same inputs: this is what rollback does.
            state.CopyFrom(snapshot);
            Run(sim, state, 60, 120);

            Assert.IsTrue(state.SameAs(original));
        }
    }
}
