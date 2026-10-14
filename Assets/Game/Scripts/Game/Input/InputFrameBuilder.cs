using DreamsOfEverus.Simulation;
using UnityEngine;

namespace DreamsOfEverus.Game
{
    /// <summary>
    /// Turns raw device values into an exact InputFrame. This is the one place where
    /// float stick values are allowed near gameplay: they're rounded to whole numbers here,
    /// and the simulation only ever sees those whole numbers.
    /// </summary>
    public static class InputFrameBuilder
    {
        public static InputFrame Build(Vector2 move, bool jumpHeld, bool attackHeld)
        {
            var buttons = InputButtons.None;
            if (jumpHeld) buttons |= InputButtons.Jump;
            if (attackHeld) buttons |= InputButtons.Attack;

            return new InputFrame
            {
                MoveX = ToStickValue(move.x),
                MoveY = ToStickValue(move.y),
                Held = buttons,
            };
        }

        private static sbyte ToStickValue(float axis)
        {
            return (sbyte)Mathf.RoundToInt(Mathf.Clamp(axis, -1f, 1f) * InputFrame.StickMax);
        }
    }
}
