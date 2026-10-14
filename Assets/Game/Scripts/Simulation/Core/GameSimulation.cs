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

        public GameSimulation(LevelGeometry level, FighterConfig playerConfig)
        {
            this.level = level;
            this.playerConfig = playerConfig;
        }

        /// <summary>The state at the very start of play: the player standing at the respawn point.</summary>
        public GameState CreateInitialState()
        {
            var state = new GameState();
            PlaceAtRespawn(ref state.Player);
            return state;
        }

        public void Step(GameState state, InputFrame input)
        {
            ref FighterState player = ref state.Player;

            // Running, jumping and fast-fall will set the player's velocity here (Milestone 1, step 5).

            KinematicMover.Move(ref player, playerConfig, level);

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
            fighter.Velocity = FixedVector2.Zero;
            KinematicMover.UpdateGround(ref fighter, playerConfig, level);
        }
    }
}
