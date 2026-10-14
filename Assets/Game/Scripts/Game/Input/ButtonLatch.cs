namespace DreamsOfEverus.Game
{
    /// <summary>
    /// Remembers a button press until the next tick reads it.
    /// Unity can draw several frames between two ticks (e.g. on a 144 Hz screen). Without this,
    /// a very quick tap that starts and ends between two ticks would never reach the simulation.
    /// </summary>
    public sealed class ButtonLatch
    {
        private bool pressedSinceLastTick;

        /// <summary>Call every Unity frame in which the button went down.</summary>
        public void NotePress()
        {
            pressedSinceLastTick = true;
        }

        /// <summary>Whether the button counts as held for this tick. Clears the remembered press.</summary>
        public bool ConsumeForTick(bool isHeldNow)
        {
            bool held = isHeldNow || pressedSinceLastTick;
            pressedSinceLastTick = false;
            return held;
        }
    }
}
