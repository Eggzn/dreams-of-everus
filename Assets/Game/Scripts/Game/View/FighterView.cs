using DreamsOfEverus.Simulation;
using UnityEngine;

namespace DreamsOfEverus.Game
{
    /// <summary>
    /// Shows the player where the simulation says they are, and which way they face.
    /// Reads the state, never changes it. Smooths between the last two ticks so movement
    /// looks fluid on screens faster than 60 Hz.
    /// </summary>
    public class FighterView : MonoBehaviour
    {
        [SerializeField] private SimulationRunner runner;

        [Tooltip("Turns to show facing. The body and nose are its children.")]
        [SerializeField] private Transform visualRoot;

        [Tooltip("The placeholder body (a capsule). Sized to match the character's collision box.")]
        [SerializeField] private Transform body;

        private void Start()
        {
            if (runner == null || runner.PlayerCharacter == null || body == null) return;

            // Unity's capsule is 1 wide and 2 tall at scale 1, with its pivot in the middle.
            CharacterData data = runner.PlayerCharacter;
            body.localScale = new Vector3(data.bodyWidth, data.bodyHeight * 0.5f, data.bodyWidth);
            body.localPosition = new Vector3(0f, data.bodyHeight * 0.5f, 0f);
        }

        private void LateUpdate()
        {
            if (runner == null || runner.State == null) return;

            Vector2 previous = ToVector(runner.PreviousState.Player.Position);
            Vector2 current = ToVector(runner.State.Player.Position);
            Vector2 shown = Vector2.Lerp(previous, current, runner.InterpolationAlpha);
            transform.position = new Vector3(shown.x, shown.y, 0f);

            if (visualRoot != null) visualRoot.localRotation = Quaternion.Euler(0f, FacingAngle(runner.State.Player), 0f);
        }

        // 0 degrees faces right, 180 faces left. During a run turnaround the body swings around slowly.
        private float FacingAngle(FighterState player)
        {
            float angle = player.Facing > 0 ? 0f : 180f;
            if (player.Action == FighterAction.RunTurnaround && runner.PlayerConfig != null)
            {
                float progress = Mathf.Clamp01((float)player.ActionTick / runner.PlayerConfig.RunTurnaroundTicks);
                angle += 180f * progress;
            }
            return angle;
        }

        private static Vector2 ToVector(FixedVector2 v) => new Vector2(v.X.ToFloat(), v.Y.ToFloat());
    }
}
