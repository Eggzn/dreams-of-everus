namespace DreamsOfEverus.Simulation
{
    /// <summary>
    /// A character's tuning values in fixed-point, ready for the simulation.
    /// Built from the character's data asset (CharacterData) when play starts.
    /// </summary>
    public sealed class FighterConfig
    {
        // Collision box. Its bottom-center is the fighter's position (the feet).
        public Fixed BodyWidth;
        public Fixed BodyHeight;

        public Fixed HalfWidth => BodyWidth / 2;
    }
}
