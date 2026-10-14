namespace DreamsOfEverus.Simulation
{
    /// <summary>
    /// Advances the game by exactly one tick. Same state + same input always gives the same result.
    /// There is no clock in here: one call to Step is always 1/60th of a second of game time.
    /// </summary>
    public sealed class GameSimulation
    {
        public const int TicksPerSecond = 60;

        public void Step(GameState state, InputFrame input)
        {
            // Movement, collision and attacks are added here in later Milestone 1 steps.

            state.Player.PreviousInput = input;
            state.Tick++;
        }
    }
}
