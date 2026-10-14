namespace DreamsOfEverus.Simulation
{
    /// <summary>What this tick's input means for a fighter: holds and hard presses.</summary>
    public struct InputEvents
    {
        public int StickX;            // -100..100
        public int HeldDirectionX;    // -1, 0 or 1: which way the stick is held (outside the deadzone)
        public int HardPressX;        // -1, 0 or 1: a fast, hard sideways press this tick (dash, dash dance)
        public bool HardPressDown;    // A fast, hard press of Down this tick (drop-through, fast-fall)
        public bool HeldDown;         // Stick held down far enough to crouch
        public bool JumpHeld;
        public bool JumpPressed;      // Jump went down this tick
    }

    /// <summary>
    /// Turns raw input plus the fighter's input history into InputEvents, and updates that history.
    /// A "hard press" (like a smash input in Smash) is the stick reaching the press threshold
    /// within a few ticks of starting to push that way. Easing the stick over slowly is just a tilt.
    /// </summary>
    public static class FighterInput
    {
        public static InputEvents Read(ref FighterState fighter, InputFrame input, InputFeelConfig feel)
        {
            InputFrame previous = fighter.PreviousInput;
            var events = new InputEvents
            {
                StickX = input.MoveX,
                HeldDirectionX = input.MoveX > feel.StickDeadzone ? 1 : (input.MoveX < -feel.StickDeadzone ? -1 : 0),
                HeldDown = input.MoveY <= -feel.CrouchThreshold,
                JumpHeld = input.IsHeld(InputButtons.Jump),
            };
            events.JumpPressed = events.JumpHeld && !previous.IsHeld(InputButtons.Jump);

            // Each check uses how long the stick had been pushed that way BEFORE this tick.
            if (IsHardPress(input.MoveX, previous.MoveX, fighter.TicksPushedRight, feel)) events.HardPressX = 1;
            else if (IsHardPress(-input.MoveX, -previous.MoveX, fighter.TicksPushedLeft, feel)) events.HardPressX = -1;
            events.HardPressDown = IsHardPress(-input.MoveY, -previous.MoveY, fighter.TicksPushedDown, feel);

            fighter.TicksPushedRight = CountPush(fighter.TicksPushedRight, input.MoveX, feel);
            fighter.TicksPushedLeft = CountPush(fighter.TicksPushedLeft, -input.MoveX, feel);
            fighter.TicksPushedDown = CountPush(fighter.TicksPushedDown, -input.MoveY, feel);

            return events;
        }

        /// <param name="amount">How far the stick is pushed in the direction being checked (0..100).</param>
        private static bool IsHardPress(int amount, int previousAmount, byte ticksPushedBefore, InputFeelConfig feel)
        {
            return amount >= feel.PressThreshold          // far enough now
                && previousAmount < feel.PressThreshold   // and it only just got there
                && ticksPushedBefore < feel.HardPressWindowTicks; // and it got there quickly
        }

        // Counts ticks in a row the stick is pushed this way; back to 0 as soon as it isn't.
        private static byte CountPush(byte ticks, int amount, InputFeelConfig feel)
        {
            if (amount <= feel.StickDeadzone) return 0;
            return ticks < byte.MaxValue ? (byte)(ticks + 1) : ticks;
        }
    }
}
