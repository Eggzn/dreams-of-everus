using DreamsOfEverus.Simulation;
using UnityEngine;

namespace DreamsOfEverus.Game
{
    /// <summary>On-screen readout of the player's state, to help with tuning. Press F1 to show or hide.</summary>
    public class DebugHud : MonoBehaviour
    {
        [SerializeField] private SimulationRunner runner;
        [SerializeField] private bool visible = true;

        private GUIStyle style;

        private void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current?.f1Key.wasPressedThisFrame == true) visible = !visible;
        }

        private void OnGUI()
        {
            if (!visible || runner == null || runner.State == null) return;
            style ??= new GUIStyle(GUI.skin.label) { fontSize = 18 };

            FighterState p = runner.State.Player;
            float speedX = p.Velocity.X.ToFloat() * GameSimulation.TicksPerSecond;
            float speedY = p.Velocity.Y.ToFloat() * GameSimulation.TicksPerSecond;

            string text =
                $"Action: {p.Action} (frame {p.ActionTick}){(IsMoonwalking(p) ? "  MOONWALKING" : "")}\n" +
                $"Facing: {(p.Facing > 0 ? "Right" : "Left")}\n" +
                $"Speed: {speedX:0.0}, {speedY:0.0} units/s\n" +
                $"Ground: {p.Ground}\n" +
                $"Air jumps used: {p.AirJumpsUsed}\n" +
                $"Fast-falling: {(p.IsFastFalling ? "yes" : "no")}\n" +
                $"Tick: {runner.State.Tick}   (F1 hides this)";

            GUI.Label(new Rect(12, 12, 600, 220), text, style);
        }

        // Sliding backward while still dashing forward.
        private static bool IsMoonwalking(FighterState p)
        {
            return p.Action == FighterAction.Dash && Fixed.Sign(p.Velocity.X) == -p.Facing;
        }
    }
}
