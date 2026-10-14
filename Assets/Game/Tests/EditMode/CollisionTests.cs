using DreamsOfEverus.Simulation;
using NUnit.Framework;
using static DreamsOfEverus.Tests.TestWorld;

namespace DreamsOfEverus.Tests
{
    public class CollisionTests
    {
        private FighterConfig fighter;
        private LevelGeometry level;

        [SetUp]
        public void SetUp()
        {
            fighter = Fighter();
            level = new LevelGeometry();
        }

        private FighterState MoveOnce(FixedVector2 position, FixedVector2 velocity)
        {
            var state = new FighterState { Position = position, Velocity = velocity };
            KinematicMover.Move(ref state, fighter, level);
            return state;
        }

        [Test]
        public void Falling_LandsOnSolidFloor()
        {
            level.Add(Box(-10, -1, 10, 0), LevelBlockKind.Solid);

            FighterState s = MoveOnce(V(0, 1), V(0, -3));

            Assert.AreEqual(F(0), s.Position.Y);
            Assert.AreEqual(F(0), s.Velocity.Y);
            Assert.AreEqual(GroundType.Solid, s.Ground);
        }

        [Test]
        public void Running_StopsAtWall()
        {
            level.Add(Box(5, 0, 6, 10), LevelBlockKind.Solid);

            FighterState s = MoveOnce(V(3, 0), V(5, 0));

            Assert.AreEqual(F(9, 2), s.Position.X); // wall at x=5, minus half the 1-wide body
            Assert.AreEqual(F(0), s.Velocity.X);
        }

        [Test]
        public void Running_LeftStopsAtWall()
        {
            level.Add(Box(-6, 0, -5, 10), LevelBlockKind.Solid);

            FighterState s = MoveOnce(V(-3, 0), V(-5, 0));

            Assert.AreEqual(F(-9, 2), s.Position.X);
        }

        [Test]
        public void StandingOnFloor_DoesNotBlockRunning()
        {
            level.Add(Box(-10, -1, 10, 0), LevelBlockKind.Solid);

            FighterState s = MoveOnce(V(0, 0), V(2, 0));

            Assert.AreEqual(F(2), s.Position.X);
            Assert.AreEqual(GroundType.Solid, s.Ground);
        }

        [Test]
        public void Rising_HitsHeadOnSolid()
        {
            level.Add(Box(-2, 5, 2, 6), LevelBlockKind.Solid);

            FighterState s = MoveOnce(V(0, 0), V(0, 4)); // head goes from 2 to 6, ceiling bottom at 5

            Assert.AreEqual(F(3), s.Position.Y);
            Assert.AreEqual(F(0), s.Velocity.Y);
        }

        [Test]
        public void Rising_PassesUpThroughPassThrough()
        {
            level.Add(Box(-2, 3, 2, 4), LevelBlockKind.PassThrough);

            FighterState s = MoveOnce(V(0, 0), V(0, 6));

            Assert.AreEqual(F(6), s.Position.Y);
            Assert.AreEqual(GroundType.None, s.Ground);
        }

        [Test]
        public void Falling_LandsOnPassThrough_WhenStartedAbove()
        {
            level.Add(Box(-2, 3, 2, 4), LevelBlockKind.PassThrough);

            FighterState s = MoveOnce(V(0, 6), V(0, -3));

            Assert.AreEqual(F(4), s.Position.Y);
            Assert.AreEqual(GroundType.PassThrough, s.Ground);
        }

        [Test]
        public void Falling_FromInsidePassThrough_IsNotCaught()
        {
            level.Add(Box(-2, 3, 2, 4), LevelBlockKind.PassThrough);

            // Feet start below the top (mid-jump through the platform), so it shouldn't snap them up.
            FighterState s = MoveOnce(V(0, 3), V(0, -1));

            Assert.AreEqual(F(2), s.Position.Y);
        }

        [Test]
        public void SidewaysIntoPassThrough_IsNotBlocked()
        {
            level.Add(Box(2, 0, 6, 1), LevelBlockKind.PassThrough);

            FighterState s = MoveOnce(V(0, 0), V(4, 0));

            Assert.AreEqual(F(4), s.Position.X);
        }

        [Test]
        public void DropThrough_FallsPastPassThrough()
        {
            level.Add(Box(-2, 3, 2, 4), LevelBlockKind.PassThrough);
            var s = new FighterState { Position = V(0, 4) };
            KinematicMover.UpdateGround(ref s, fighter, level);
            Assert.AreEqual(GroundType.PassThrough, s.Ground);

            Assert.IsTrue(KinematicMover.TryDropThrough(ref s));
            s.Velocity = V(0, -2);
            KinematicMover.Move(ref s, fighter, level);

            Assert.IsTrue(s.Position.Y < F(3), "should be below the platform");
            Assert.AreEqual(GroundType.None, s.Ground);
        }

        [Test]
        public void DropThrough_NotAllowedOnSolid()
        {
            level.Add(Box(-10, -1, 10, 0), LevelBlockKind.Solid);
            var s = new FighterState { Position = V(0, 0) };
            KinematicMover.UpdateGround(ref s, fighter, level);

            Assert.IsFalse(KinematicMover.TryDropThrough(ref s));
            Assert.AreEqual(F(0), s.Position.Y);
        }

        [Test]
        public void DropThrough_NotAllowedWithOneFootOnSolid()
        {
            level.Add(Box(-4, 3, 0, 4), LevelBlockKind.Solid);
            level.Add(Box(0, 3, 4, 4), LevelBlockKind.PassThrough);
            var s = new FighterState { Position = V(0, 4) }; // standing across both

            KinematicMover.UpdateGround(ref s, fighter, level);

            Assert.AreEqual(GroundType.Solid, s.Ground);
            Assert.IsFalse(KinematicMover.TryDropThrough(ref s));
        }

        [Test]
        public void FastFall_DoesNotSkipThinPlatform()
        {
            level.Add(Box(-2, 0, 2, 1), LevelBlockKind.PassThrough);

            FighterState s = MoveOnce(V(0, 10), V(0, -50)); // would end far below if only the end point was checked

            Assert.AreEqual(F(1), s.Position.Y);
        }

        [Test]
        public void WalkingOffLedge_ClearsGround()
        {
            level.Add(Box(-10, -1, 0, 0), LevelBlockKind.Solid);

            FighterState s = MoveOnce(V(-2, 0), V(3, 0)); // body now spans 0.5..1.5, past the ledge at 0

            Assert.AreEqual(GroundType.None, s.Ground);
        }

        [Test]
        public void TouchingFallZone_RespawnsPlayer()
        {
            level.Add(Box(-20, -10, 20, -6), LevelBlockKind.FallZone);
            level.RespawnPoint = V(-3, 5);
            var sim = new GameSimulation(level, fighter);
            GameState state = sim.CreateInitialState();

            state.Player.Position = V(0, -4);
            state.Player.Velocity = V(0, -3); // falls into the zone this tick
            sim.Step(state, default);

            Assert.AreEqual(V(-3, 5), state.Player.Position);
            Assert.AreEqual(FixedVector2.Zero, state.Player.Velocity);
        }

        [Test]
        public void InitialState_StandsAtRespawnPoint()
        {
            level.Add(Box(-10, -1, 10, 0), LevelBlockKind.Solid);
            level.RespawnPoint = V(2, 0);

            GameState state = new GameSimulation(level, fighter).CreateInitialState();

            Assert.AreEqual(V(2, 0), state.Player.Position);
            Assert.AreEqual(GroundType.Solid, state.Player.Ground);
        }
    }
}
