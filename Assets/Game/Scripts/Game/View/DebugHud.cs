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
                AttackLine(p) + "\n" +
                $"Dummy hits: {runner.State.Dummy.TimesHit}\n" +
                $"Tick: {runner.State.Tick}   (F1 hides this, F2 hides hitboxes)";

            GUI.Label(new Rect(12, 12, 700, 300), text, style);
        }

        // "Light attack: frame 5 / 18 (HITBOX OUT)" while attacking.
        private string AttackLine(FighterState p)
        {
            AttackConfig attack = runner.PlayerConfig?.LightAttack;
            if (p.Action != FighterAction.LightAttack || attack == null) return "Light attack: -";

            // After a tick, ActionTick has already moved on, so the frame that just played is ActionTick - 1
            // (shown counting from 1, like frame data: "frame 1" is the first frame).
            int framePlayed = p.ActionTick - 1;
            string phase = attack.IsActiveOn(framePlayed) ? "HITBOX OUT"
                : (framePlayed < attack.StartupTicks ? "startup" : "recovery");
            return $"Light attack: frame {framePlayed + 1} / {attack.TotalTicks} ({phase}){(p.AttackHasHit ? "  HIT" : "")}";
        }

        // Sliding backward while still dashing forward.
        private static bool IsMoonwalking(FighterState p)
        {
            return p.Action == FighterAction.Dash && Fixed.Sign(p.Velocity.X) == -p.Facing;
        }
    }
}
