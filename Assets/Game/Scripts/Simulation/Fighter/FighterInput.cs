namespace DreamsOfEverus.Simulation
{
    /// <summary>What this tick's input means for a fighter: presses, double-taps, hard presses.</summary>
    public struct InputEvents
    {
        public int StickX;            // -100..100
        public int HeldDirectionX;    // -1, 0 or 1: which way the stick is held (outside the deadzone)
        public int PressedDirectionX; // -1, 0 or 1: a sideways press that started this tick
        public bool DoubleTapX;       // That press was the second of a double-tap in the same direction
        public bool HardPressDown;    // A fast, hard press of Down (drop-through, fast-fall)
        public bool JumpHeld;
        public bool JumpPressed;      // Jump went down this tick
    }

    /// <summary>Turns raw input plus the fighter's input history into InputEvents. Updates that history.</summary>
    public static class FighterInput
    {
        public static InputEvents Read(ref FighterState fighter, InputFrame input, InputFeelConfig feel)
        {
            InputFrame previous = fighter.PreviousInput;
            var events = new InputEvents
            {
                StickX = input.MoveX,
                HeldDirectionX = DirectionOf(input.MoveX, feel.StickDeadzone),
                JumpHeld = input.IsHeld(InputButtons.Jump),
            };
            events.JumpPressed = events.JumpHeld && !previous.IsHeld(InputButtons.Jump);

            // Sideways press: the stick crossed the press threshold this tick.
            if (input.MoveX >= feel.PressThreshold && previous.MoveX < feel.PressThreshold) events.PressedDirectionX = 1;
            else if (input.MoveX <= -feel.PressThreshold && previous.MoveX > -feel.PressThreshold) events.PressedDirectionX = -1;

            if (events.PressedDirectionX != 0)
            {
                // Same direction as the last press, and soon enough after it: that's a double-tap.
                events.DoubleTapX = fighter.LastPressDirectionX == events.PressedDirectionX
                    && fighter.TicksSinceLastPressX <= feel.DoubleTapWindowTicks;
                fighter.LastPressDirectionX = (sbyte)events.PressedDirectionX;
                fighter.TicksSinceLastPressX = 0;
            }
            else if (fighter.TicksSinceLastPressX < byte.MaxValue)
            {
                fighter.TicksSinceLastPressX++;
            }

            // Hard press of Down: the stick reached the threshold this tick, and it was centered only a moment ago.
            // A slow tilt down takes too long and doesn't count.
            int downThreshold = -feel.PressThreshold;
            events.HardPressDown = input.MoveY <= downThreshold
                && previous.MoveY > downThreshold
                && fighter.TicksSinceStickYNeutral < feel.HardPressWindowTicks;

            if (input.MoveY > -feel.StickDeadzone) fighter.TicksSinceStickYNeutral = 0;
            else if (fighter.TicksSinceStickYNeutral < byte.MaxValue) fighter.TicksSinceStickYNeutral++;

            return events;
        }

        private static int DirectionOf(int stick, int deadzone)
        {
            if (stick > deadzone) return 1;
            if (stick < -deadzone) return -1;
            return 0;
        }
    }
}
