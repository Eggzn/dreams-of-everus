namespace DreamsOfEverus.Simulation
{
    /// <summary>
    /// Advances the game by exactly one tick. Same state + same input always gives the same result.
    /// There is no clock in here: one call to Step is always 1/60th of a second of game time.
    /// </summary>
    public sealed class GameSimulation
    {
        public const int TicksPerSecond = 60;

        private readonly LevelGeometry level;
        private readonly FighterConfig playerConfig;
        private readonly InputFeelConfig inputFeel;
        private readonly TrainingDummyConfig dummyConfig;

        /// <param name="playerConfig">Kept by reference, so changing its values (live tuning) takes effect next tick.</param>
        /// <param name="dummyConfig">The training dummy, or null for no dummy.</param>
        public GameSimulation(LevelGeometry level, FighterConfig playerConfig, InputFeelConfig inputFeel, TrainingDummyConfig dummyConfig = null)
        {
            this.level = level;
            this.playerConfig = playerConfig;
            this.inputFeel = inputFeel;
            this.dummyConfig = dummyConfig;
        }

        /// <summary>The state at the very start of play: the player standing at the respawn point facing right, and the dummy in place.</summary>
        public GameState CreateInitialState()
        {
            var state = new GameState();
            state.Player.Facing = 1;
            PlaceAtRespawn(ref state.Player);

            if (dummyConfig != null)
            {
                state.Dummy.Exists = true;
                state.Dummy.Position = dummyConfig.Position;
            }
            return state;
        }

        public void Step(GameState state, InputFrame input)
        {
            ref FighterState player = ref state.Player;

            InputEvents events = FighterInput.Read(ref player, input, inputFeel);
            FighterMovement.UpdateBeforeMove(ref player, events, playerConfig, level);
            KinematicMover.Move(ref player, playerConfig, level);
            CombatSystem.Step(ref player, playerConfig, ref state.Dummy, dummyConfig);
            FighterMovement.UpdateAfterMove(ref player);

            if (level.TouchesFallZone(KinematicMover.BodyBox(player, playerConfig)))
            {
                PlaceAtRespawn(ref player);
            }

            player.PreviousInput = input;
            state.Tick++;
        }

        private void PlaceAtRespawn(ref FighterState fighter)
        {
            fighter.Position = level.RespawnPoint;
            KinematicMover.UpdateGround(ref fighter, playerConfig, level);
            FighterMovement.ResetAfterTeleport(ref fighter);
        }
    }
}
