using DreamsOfEverus.Simulation;
using NUnit.Framework;
using static DreamsOfEverus.Tests.TestWorld;

namespace DreamsOfEverus.Tests
{
    public class CombatTests
    {
        // Test attack: 3 startup, 2 active, 5 recovery. Hitbox 1x1, centered 1 forward and 1 up from the feet,
        // so facing right from x = 0 it covers x 0.5..1.5.

        private FighterConfig fighter;
        private LevelGeometry level;
        private GameSimulation sim;
        private GameState state;

        private FighterState Player => state.Player;
        private DummyState Dummy => state.Dummy;
        private AttackConfig Attack => fighter.LightAttack;

        private void Build(Fixed dummyX)
        {
            fighter = Fighter();
            level = new LevelGeometry();
            level.Add(Box(-100, -1, 100, 0), LevelBlockKind.Solid);
            level.RespawnPoint = V(0, 0);
            sim = new GameSimulation(level, fighter, Feel(), TestWorld.Dummy(dummyX));
            state = sim.CreateInitialState();
        }

        [SetUp]
        public void SetUp() => Build(F(9, 5)); // dummy at x = 1.8: hurtbox 1.3..2.3, in reach

        private void Hold(InputFrame input, int ticks = 1)
        {
            for (int i = 0; i < ticks; i++) sim.Step(state, input);
        }

        private static InputFrame AttackButton => In(attack: true);

        [Test]
        public void PressingAttack_StartsLightAttack()
        {
            Hold(AttackButton);
            Assert.AreEqual(FighterAction.LightAttack, Player.Action);
        }

        [Test]
        public void HitboxOnlyOutDuringActiveFrames()
        {
            Hold(AttackButton, Attack.StartupTicks);
            Assert.AreEqual(0, Dummy.TimesHit, "no hit during startup");

            Hold(AttackButton);
            Assert.AreEqual(1, Dummy.TimesHit, "hits on the first active frame");
        }

        [Test]
        public void TryGetActiveHitbox_MatchesFrameData()
        {
            int framesWithHitbox = 0;
            Hold(AttackButton);
            for (int frame = 0; frame < Attack.TotalTicks; frame++)
            {
                // Check before stepping: ActionTick is the frame that's about to play.
                var probe = Player;
                probe.ActionTick = frame;
                if (CombatSystem.TryGetActiveHitbox(probe, fighter, out _)) framesWithHitbox++;
            }
            Assert.AreEqual(Attack.ActiveTicks, framesWithHitbox);
        }

        [Test]
        public void OneHitPerAttack()
        {
            Hold(AttackButton, Attack.TotalTicks);
            Assert.AreEqual(1, Dummy.TimesHit);
        }

        [Test]
        public void HoldingAttack_DoesNotRepeat()
        {
            Hold(AttackButton, Attack.TotalTicks * 3);
            Assert.AreEqual(1, Dummy.TimesHit);
        }

        [Test]
        public void AttackEnds_ThenCanAttackAgain()
        {
            Hold(AttackButton, Attack.TotalTicks);
            Hold(In());
            Assert.AreNotEqual(FighterAction.LightAttack, Player.Action);

            Hold(AttackButton, Attack.TotalTicks);
            Assert.AreEqual(2, Dummy.TimesHit);
        }

        [Test]
        public void FacingAway_Misses()
        {
            state.Player.Facing = -1;
            Hold(AttackButton, Attack.TotalTicks);
            Assert.AreEqual(0, Dummy.TimesHit);
        }

        [Test]
        public void HitboxFlipsWhenFacingLeft()
        {
            Build(F(-9, 5)); // dummy on the left
            state.Player.Facing = -1;
            Hold(AttackButton, Attack.TotalTicks);
            Assert.AreEqual(1, Dummy.TimesHit);
        }

        [Test]
        public void OutOfRange_Misses()
        {
            Build(F(4));
            Hold(AttackButton, Attack.TotalTicks);
            Assert.AreEqual(0, Dummy.TimesHit);
        }

        [Test]
        public void DummyReaction_CountsDownToZero()
        {
            Hold(AttackButton, Attack.StartupTicks + 1);
            Assert.AreEqual(TestWorld.Dummy(F(0)).HitReactTicks, Dummy.HitReactTicksLeft);

            Hold(In(), 20);
            Assert.AreEqual(0, Dummy.HitReactTicksLeft);
        }

        [Test]
        public void CannotAttackInTheAir()
        {
            Hold(In(jump: true), fighter.JumpSquatTicks + 2);
            Hold(In(attack: true));
            Assert.AreEqual(FighterAction.Airborne, Player.Action);
        }

        [Test]
        public void AttackIsCommitted_NoJumpOrCrouchOutOfIt()
        {
            Hold(AttackButton);
            Hold(In(0, -100, jump: true, attack: true));
            Assert.AreEqual(FighterAction.LightAttack, Player.Action);
        }

        [Test]
        public void AttackingOutOfDash_SlidesOnTraction()
        {
            Hold(In(100)); // hard press: dash
            Fixed dashSpeed = Player.Velocity.X;
            Hold(In(100, 0, attack: true));

            Assert.AreEqual(FighterAction.LightAttack, Player.Action);
            Assert.AreEqual(dashSpeed - fighter.Traction, Player.Velocity.X);
        }

        [Test]
        public void RestoreSnapshot_ReplaysHitsExactly()
        {
            GameState start = state.Clone();
            Hold(AttackButton, Attack.TotalTicks);
            Hold(In(), 3);
            Hold(AttackButton, 6);
            GameState original = state.Clone();

            state.CopyFrom(start);
            Hold(AttackButton, Attack.TotalTicks);
            Hold(In(), 3);
            Hold(AttackButton, 6);

            Assert.IsTrue(state.SameAs(original));
            Assert.AreEqual(2, state.Dummy.TimesHit);
        }
    }
}
