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

        /// <summary>A hard sideways press (stick straight from center to full in one tick), like a smash input.</summary>
        private void HardPress(int direction)
        {
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
        public void HardPress_StartsDash()
        {
            HardPress(1);

            Assert.AreEqual(FighterAction.Dash, Player.Action);
            Assert.AreEqual(fighter.DashSpeed, Player.Velocity.X);
        }

        [Test]
        public void SlowTiltToFull_Walks_DoesNotDash()
        {
            for (int x = 30; x <= 100; x += 10) Hold(In(x)); // eases over 8 ticks

            Assert.AreEqual(FighterAction.Walk, Player.Action);
            Assert.AreEqual(fighter.WalkSpeed, Player.Velocity.X);
        }

        [Test]
        public void DashDance_SlowTiltBack_DoesNotDashDance()
        {
            HardPress(1);
            foreach (int x in new[] { 60, 20, -30, -40, -50, -60, -70 }) Hold(In(x)); // eases back over 7 ticks

            Assert.AreEqual(1, Player.Facing);
            Assert.AreNotEqual(-fighter.DashSpeed, Player.Velocity.X);
        }

        // ----- Moonwalk -----

        [Test]
        public void Moonwalk_GentleTiltBackDuringTrot_SlidesBackwardFacingForward()
        {
            HardPress(1);
            Hold(In(-50), fighter.TrotTicks - 1); // gentle tilt back: never reaches the press threshold

            Assert.AreEqual(FighterAction.Dash, Player.Action);
            Assert.AreEqual(1, Player.Facing);            // still facing forward
            Assert.Less(Player.Velocity.X.Raw, 0);         // but moving backward
        }

        [Test]
        public void Moonwalk_HalfRollThroughDown_SlidesBackwardFacingForward()
        {
            HardPress(1);
            // Roll the stick around the bottom edge: down-forward, down, down-back (held a moment), back.
            Hold(In(71, -71));
            Hold(In(0, -100));
            Hold(In(-71, -71), 2);
            Hold(In(-100, 0), 3);

            Assert.AreEqual(FighterAction.Dash, Player.Action); // no crouch, no dash dance
            Assert.AreEqual(1, Player.Facing);
            Assert.Less(Player.Velocity.X.Raw, 0);
        }

        [Test]
        public void FastRollBack_IsStillADashDance()
        {
            HardPress(1);
            Hold(In(71, -71));
            Hold(In(0, -100));
            Hold(In(-71, -71)); // only one tick on the diagonal: too fast to be a tilt
            Hold(In(-100, 0));

            Assert.AreEqual(-1, Player.Facing);
        }

        [Test]
        public void HoldingDownDuringTrot_DoesNotCrouch()
        {
            HardPress(1);
            Hold(In(100, -100), 3);
            Hold(In(0, -100), 3);

            Assert.AreEqual(FighterAction.Dash, Player.Action);
        }

        [Test]
        public void HardDownDuringTrot_DoesNotDropThrough()
        {
            level.Add(Box(-50, -1, 50, 0), LevelBlockKind.PassThrough);
            level.Blocks.RemoveAt(0); // only the pass-through floor remains
            KinematicMover.UpdateGround(ref state.Player, fighter, level);

            HardPress(1);
            Hold(In(0, -100), 3);

            Assert.AreEqual(F(0), Player.Position.Y);
            Assert.IsTrue(Player.IsGrounded);
        }

        [Test]
        public void Moonwalk_ZeroStrength_CannotMoonwalk()
        {
            fighter.MoonwalkAcceleration = Fixed.Zero;
            HardPress(1);
            Hold(In(-50), fighter.TrotTicks - 1);

            Assert.AreEqual(fighter.DashSpeed, Player.Velocity.X);
        }

        /// <summary>Moonwalks for the whole trot, then lets go until stopped. Returns where the fighter ends up.</summary>
        private Fixed MoonwalkEndX()
        {
            HardPress(1);
            Hold(In(-100 / 2), fighter.TrotTicks - 1);
            Hold(In(), 120);
            return Player.Position.X;
        }

        [Test]
        public void Moonwalk_StrongerCharacterGoesFurtherBack()
        {
            fighter.MoonwalkAcceleration = F(1, 50);
            Fixed weak = MoonwalkEndX();

            SetUp();
            fighter.MoonwalkAcceleration = F(1, 10);
            Fixed strong = MoonwalkEndX();

            Assert.Less(strong.Raw, weak.Raw);
        }

        // ----- Traction -----

        [Test]
        public void LettingGoDuringDash_SlidesOnTraction()
        {
            HardPress(1);
            Hold(In());

            Assert.AreEqual(fighter.DashSpeed - fighter.Traction, Player.Velocity.X);
        }

        /// <summary>Runs, lets go, and returns how far the fighter slides before stopping.</summary>
        private Fixed StoppingDistance()
        {
            HardPress(1);
            Hold(In(100), fighter.TrotTicks + 20); // running at full speed
            Fixed releaseX = Player.Position.X;
            Hold(In(), 200);
            return Player.Position.X - releaseX;
        }

        [Test]
        public void LowTraction_SlidesFurther()
        {
            fighter.Traction = F(1, 20); // grippy
            Fixed grippy = StoppingDistance();

            SetUp();
            fighter.Traction = F(1, 100); // slippery
            Fixed slippery = StoppingDistance();

            Assert.Greater(slippery.Raw, grippy.Raw);
        }

        [Test]
        public void FasterThanWalkSpeed_BleedsOffOnTraction()
        {
            fighter.Traction = F(1, 100);
            state.Player.Velocity = V(F(3, 10), F(0)); // e.g. just landed from a dash jump
            Hold(In(50));

            Assert.AreEqual(F(3, 10) - fighter.Traction, Player.Velocity.X);
        }

        // ----- Momentum -----

        /// <summary>Jumps with the stick held right and returns how far right the fighter travels in the air.</summary>
        private Fixed AirDistanceOfJump()
        {
            Hold(In(100, 0, jump: true), fighter.JumpSquatTicks + 1);
            Fixed takeoffX = Player.Position.X;
            while (!Player.IsGrounded) Hold(In(100, 0, jump: true));
            return Player.Position.X - takeoffX;
        }

        [Test]
        public void DashJump_TravelsFurtherThanWalkJump()
        {
            for (int x = 30; x <= 100; x += 10) Hold(In(x)); // walking at full tilt
            Hold(In(100), 10);
            Fixed walkJump = AirDistanceOfJump();

            SetUp();
            HardPress(1);
            Hold(In(100), 2); // still dashing
            Fixed dashJump = AirDistanceOfJump();

            Assert.Greater(dashJump.Raw, walkJump.Raw);
        }

        [Test]
        public void JumpMomentum_IsCappedByCharacter()
        {
            fighter.MaxJumpMomentum = F(1, 10); // lower than dash speed
            HardPress(1);
            Hold(In(100, 0, jump: true), fighter.JumpSquatTicks + 1);

            Assert.LessOrEqual(Player.Velocity.X.Raw, fighter.MaxJumpMomentum.Raw);
        }

        [Test]
        public void HoldingForwardInAir_DoesNotSlowDashMomentumBelowFriction()
        {
            HardPress(1);
            Hold(In(100, 0, jump: true), fighter.JumpSquatTicks + 1);
            Fixed launchSpeed = Player.Velocity.X;
            Hold(In(100), 1);

            Assert.AreEqual(launchSpeed - fighter.AirFriction, Player.Velocity.X);
        }

        // ----- Crouch and crawl -----

        [Test]
        public void HoldDown_Crouches_AndShrinksBody()
        {
            Hold(In(0, -100), 3);

            Assert.AreEqual(FighterAction.Crouch, Player.Action);
            Assert.AreEqual(fighter.CrouchHeight, fighter.HeightFor(Player));
        }

        [Test]
        public void ReleaseDown_StandsUp()
        {
            Hold(In(0, -100), 3);
            Hold(In());

            Assert.AreEqual(FighterAction.Idle, Player.Action);
            Assert.AreEqual(fighter.BodyHeight, fighter.HeightFor(Player));
        }

        [Test]
        public void DownAndSideways_Crawls()
        {
            Hold(In(0, -100), 2);
            Hold(In(100, -100), 30);

            Assert.AreEqual(FighterAction.Crawl, Player.Action);
            Assert.AreEqual(fighter.CrawlSpeed, Player.Velocity.X);
        }

        [Test]
        public void CharacterThatCannotCrawl_StaysCrouched()
        {
            fighter.CanCrawl = false;
            Hold(In(0, -100), 2);
            Hold(In(100, -100), 30);

            Assert.AreEqual(FighterAction.Crouch, Player.Action);
            Assert.AreEqual(F(0), Player.Velocity.X);
        }

        private void AddLowCeiling()
        {
            // Solid ceiling from x = 5 to 10, 1.5 units above the floor: crawl height 1 fits, standing height 2 doesn't.
            level.Add(new FixedBox(V(F(5), F(3, 2)), V(F(10), F(3))), LevelBlockKind.Solid);
        }

        [Test]
        public void Standing_IsBlockedByLowCeiling()
        {
            AddLowCeiling();
            for (int x = 30; x <= 100; x += 10) Hold(In(x));
            Hold(In(100), 120);

            Assert.AreEqual(F(9, 2), Player.Position.X); // stopped at the ceiling's edge (5 minus half the body)
        }

        [Test]
        public void CrawlingUnderLowCeiling_CantStandOrJump()
        {
            AddLowCeiling();
            Hold(In(0, -100), 2);
            Hold(In(100, -100), 150); // crawl in
            Assert.Greater(Player.Position.X.Raw, F(6).Raw);

            Hold(In()); // let go of everything
            Assert.AreEqual(FighterAction.Crouch, Player.Action);

            Hold(In(jump: true), 10);
            Assert.AreEqual(FighterAction.Crouch, Player.Action);
            Assert.AreEqual(F(0), Player.Position.Y);
        }

        [Test]
        public void CrawlingOutFromUnderCeiling_CanStandAgain()
        {
            AddLowCeiling();
            Hold(In(0, -100), 2);
            Hold(In(100, -100), 150);
            Hold(In(100), 200); // keep crawling right, without holding Down, until clear of the ceiling

            Assert.Greater(Player.Position.X.Raw, F(10).Raw);
            Assert.AreNotEqual(FighterAction.Crawl, Player.Action);
            Assert.AreEqual(fighter.BodyHeight, fighter.HeightFor(Player));
        }

        [Test]
        public void Dash_LastsForTrot_ThenRunsIfHeld()
        {
            HardPress(1);
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
            HardPress(1);
            Hold(In(0), fighter.TrotTicks);
            Assert.AreEqual(FighterAction.Idle, Player.Action);
        }

        [Test]
        public void DashDance_OnePressBackDuringTrot_DashesTheOtherWay()
        {
            HardPress(1);
            Hold(In(100), 3);
            Hold(In(-100)); // single press back, still within the trot

            Assert.AreEqual(FighterAction.Dash, Player.Action);
            Assert.AreEqual(-1, Player.Facing);
            Assert.AreEqual(-fighter.DashSpeed, Player.Velocity.X);
        }

        [Test]
        public void PressBackAfterTrot_ForcesSlowRunTurnaround()
        {
            HardPress(1);
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
