using DreamsOfEverus.Simulation;
using NUnit.Framework;
using static DreamsOfEverus.Tests.TestWorld;

namespace DreamsOfEverus.Tests
{
    public class MovementTests
    {
        private FighterConfig fighter;
        private LevelGeometry level;
        private GameSimulation sim;
        private GameState state;

        private FighterState Player => state.Player;

        [SetUp]
        public void SetUp()
        {
            fighter = Fighter();
            level = new LevelGeometry();
            level.Add(Box(-100, -1, 100, 0), LevelBlockKind.Solid); // wide floor, top at y = 0
            level.RespawnPoint = V(0, 0);
            sim = new GameSimulation(level, fighter, Feel());
            state = sim.CreateInitialState();
        }

        private void Hold(InputFrame input, int ticks = 1)
        {
            for (int i = 0; i < ticks; i++) sim.Step(state, input);
        }

        private void DoubleTap(int direction)
        {
            Hold(In(100 * direction));
            Hold(In(0));
            Hold(In(100 * direction));
        }

        /// <summary>Runs until the fighter is back on the ground and returns the highest feet height reached.</summary>
        private Fixed HighestPointUntilLanding(InputFrame input)
        {
            Fixed highest = Player.Position.Y;
            for (int i = 0; i < 300; i++)
            {
                Hold(input);
                highest = Fixed.Max(highest, Player.Position.Y);
                if (Player.Action != FighterAction.JumpSquat && Player.IsGrounded) break;
            }
            return highest;
        }

        // ----- Walk and dash -----

        [Test]
        public void StartsIdleOnTheGround_FacingRight()
        {
            Assert.AreEqual(FighterAction.Idle, Player.Action);
            Assert.AreEqual(1, Player.Facing);
            Assert.IsTrue(Player.IsGrounded);
        }

        [Test]
        public void HalfTilt_WalksAtHalfWalkSpeed()
        {
            Hold(In(50), 10);

            Assert.AreEqual(FighterAction.Walk, Player.Action);
            Assert.AreEqual(fighter.WalkSpeed / 2, Player.Velocity.X);
        }

        [Test]
        public void WalkingLeft_FacesLeft()
        {
            Hold(In(-100), 3);
            Assert.AreEqual(-1, Player.Facing);
        }

        [Test]
        public void SingleTap_Walks_DoesNotDash()
        {
            Hold(In(100));
            Assert.AreEqual(FighterAction.Walk, Player.Action);
        }

        [Test]
        public void DoubleTap_StartsDash()
        {
            DoubleTap(1);

            Assert.AreEqual(FighterAction.Dash, Player.Action);
            Assert.AreEqual(fighter.DashSpeed, Player.Velocity.X);
        }

        [Test]
        public void DoubleTap_TooSlow_JustWalks()
        {
            Hold(In(100));
            Hold(In(0), 20); // longer than the double-tap window
            Hold(In(100));

            Assert.AreEqual(FighterAction.Walk, Player.Action);
        }

        [Test]
        public void Dash_LastsForTrot_ThenRunsIfHeld()
        {
            DoubleTap(1);
            Fixed startX = Player.Position.X - fighter.DashSpeed; // where the dash began
            Hold(In(100), fighter.TrotTicks - 1);
            Assert.AreEqual(FighterAction.Dash, Player.Action);
            Assert.AreEqual(fighter.DashSpeed * fighter.TrotTicks, Player.Position.X - startX); // the trot distance

            Hold(In(100));
            Assert.AreEqual(FighterAction.Run, Player.Action);
        }

        [Test]
        public void Dash_Released_StopsAfterTrot()
        {
            DoubleTap(1);
            Hold(In(0), fighter.TrotTicks);
            Assert.AreEqual(FighterAction.Idle, Player.Action);
        }

        [Test]
        public void DashDance_OnePressBackDuringTrot_DashesTheOtherWay()
        {
            DoubleTap(1);
            Hold(In(100), 3);
            Hold(In(-100)); // single press back, still within the trot

            Assert.AreEqual(FighterAction.Dash, Player.Action);
            Assert.AreEqual(-1, Player.Facing);
            Assert.AreEqual(-fighter.DashSpeed, Player.Velocity.X);
        }

        [Test]
        public void PressBackAfterTrot_ForcesSlowRunTurnaround()
        {
            DoubleTap(1);
            Hold(In(100), fighter.TrotTicks); // now running
            Assert.AreEqual(FighterAction.Run, Player.Action);

            Hold(In(-100));
            Assert.AreEqual(FighterAction.RunTurnaround, Player.Action);
            Hold(In(-100), fighter.RunTurnaroundTicks - 1);
            Assert.AreEqual(FighterAction.RunTurnaround, Player.Action);
            Assert.AreEqual(1, Player.Facing); // still turning

            Hold(In(-100));
            Assert.AreEqual(-1, Player.Facing);
            Assert.AreEqual(FighterAction.Run, Player.Action);
        }

        // ----- Jumping -----

        [Test]
        public void JumpSquat_LeavesGroundAfterSquatFrames()
        {
            Hold(In(jump: true));
            Hold(In(jump: true), fighter.JumpSquatTicks - 1);
            Assert.AreEqual(FighterAction.JumpSquat, Player.Action);
            Assert.AreEqual(F(0), Player.Position.Y);

            Hold(In(jump: true));
            Assert.AreEqual(FighterAction.Airborne, Player.Action);
            Assert.Greater(Player.Position.Y.Raw, 0);
        }

        [Test]
        public void ShortHop_IsLowerThanFullHop()
        {
            Fixed fullHop = HighestPointUntilLanding(In(jump: true));

            SetUp();
            Hold(In(jump: true)); // tap and release during jump squat
            Fixed shortHop = HighestPointUntilLanding(In());

            Assert.Greater(fullHop.Raw, shortHop.Raw);
            Assert.Greater(shortHop.Raw, 0);
        }

        [Test]
        public void AirJump_OnlyOnce()
        {
            Hold(In(jump: true), 10); // full hop
            Hold(In());
            Hold(In(jump: true)); // air jump
            Assert.AreEqual(1, Player.AirJumpsUsed);
            Assert.AreEqual(fighter.AirJumpVelocity, Player.Velocity.Y);

            Hold(In());
            Hold(In(jump: true)); // no jumps left
            Assert.AreEqual(1, Player.AirJumpsUsed);
            Assert.Less(Player.Velocity.Y.Raw, fighter.AirJumpVelocity.Raw);
        }

        [Test]
        public void Landing_GivesAirJumpBack()
        {
            Hold(In(jump: true), 10);
            Hold(In());
            Hold(In(jump: true));
            HighestPointUntilLanding(In());

            Assert.AreEqual(FighterAction.Idle, Player.Action);
            Assert.AreEqual(0, Player.AirJumpsUsed);
        }

        // ----- Fast-fall -----

        private void JumpToPeak()
        {
            Hold(In(jump: true), fighter.JumpSquatTicks + 1);
            while (Player.Velocity.Y > Fixed.Zero) Hold(In());
        }

        [Test]
        public void FastFall_HardPressDownWhileFalling()
        {
            JumpToPeak();
            Hold(In(0, -100));

            Assert.IsTrue(Player.IsFastFalling);
            Assert.AreEqual(-fighter.FastFallSpeed, Player.Velocity.Y);
        }

        [Test]
        public void FastFall_NotWhileRising()
        {
            Hold(In(jump: true), fighter.JumpSquatTicks + 2); // just left the ground, still rising
            Hold(In(0, -100));

            Assert.IsFalse(Player.IsFastFalling);
        }

        [Test]
        public void FastFall_SlowTiltDownDoesNotCount()
        {
            JumpToPeak();
            for (int y = -30; y >= -100; y -= 10) Hold(In(0, y)); // eases down over 8 ticks

            Assert.IsFalse(Player.IsFastFalling);
        }

        // ----- Drop through and ledges -----

        private void StandOnPassThrough()
        {
            level.Add(Box(10, 2, 20, 3), LevelBlockKind.PassThrough);
            state.Player.Position = V(15, 3);
            KinematicMover.UpdateGround(ref state.Player, fighter, level);
            Assert.AreEqual(GroundType.PassThrough, Player.Ground);
        }

        [Test]
        public void HardPressDown_DropsThroughPassThrough()
        {
            StandOnPassThrough();
            Hold(In(0, -100));
            Hold(In(0, -100), 20);

            Assert.Less(Player.Position.Y.Raw, F(3).Raw);
        }

        [Test]
        public void SlowTiltDown_StaysOnPassThrough()
        {
            StandOnPassThrough();
            for (int y = -30; y >= -100; y -= 10) Hold(In(0, y));
            Hold(In(0, -100), 20);

            Assert.AreEqual(F(3), Player.Position.Y);
            Assert.AreEqual(GroundType.PassThrough, Player.Ground);
        }

        [Test]
        public void WalkingOffLedge_FallsWithAirJumpStillAvailable()
        {
            level.Blocks.Clear();
            level.Add(Box(-100, -1, 1, 0), LevelBlockKind.Solid); // ledge at x = 1
            Hold(In(100), 30);

            Assert.AreEqual(FighterAction.Airborne, Player.Action);
            Assert.AreEqual(0, Player.AirJumpsUsed);
        }
    }
}
