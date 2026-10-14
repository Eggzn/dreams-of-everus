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

        /// <param name="playerConfig">Kept by reference, so changing its values (live tuning) takes effect next tick.</param>
        public GameSimulation(LevelGeometry level, FighterConfig playerConfig, InputFeelConfig inputFeel)
        {
            this.level = level;
            this.playerConfig = playerConfig;
            this.inputFeel = inputFeel;
        }

        /// <summary>The state at the very start of play: the player standing at the respawn point, facing right.</summary>
        public GameState CreateInitialState()
        {
            var state = new GameState();
            state.Player.Facing = 1;
            PlaceAtRespawn(ref state.Player);
            return state;
        }

        public void Step(GameState state, InputFrame input)
        {
            ref FighterState player = ref state.Player;

            InputEvents events = FighterInput.Read(ref player, input, inputFeel);
            FighterMovement.UpdateBeforeMove(ref player, events, playerConfig);
            KinematicMover.Move(ref player, playerConfig, level);
            FighterMovement.UpdateAfterMove(ref player);

            if (level.TouchesFallZone(KinematicMover.BodyBox(player.Position, playerConfig)))
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
