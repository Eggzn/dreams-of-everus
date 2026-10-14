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

        [Tooltip("The small block that shows which way the character faces.")]
        [SerializeField] private Transform nose;

        private void LateUpdate()
        {
            if (runner == null || runner.State == null) return;
            FighterState player = runner.State.Player;

            Vector2 previous = ToVector(runner.PreviousState.Player.Position);
            Vector2 current = ToVector(player.Position);
            Vector2 shown = Vector2.Lerp(previous, current, runner.InterpolationAlpha);
            transform.position = new Vector3(shown.x, shown.y, 0f);

            if (visualRoot != null) visualRoot.localRotation = Quaternion.Euler(0f, FacingAngle(player), 0f);
            MatchBodySize(player);
        }

        // Sizes the placeholder capsule to the collision box: shorter while crouching or crawling.
        private void MatchBodySize(FighterState player)
        {
            if (body == null || runner.PlayerConfig == null) return;

            float width = runner.PlayerConfig.BodyWidth.ToFloat();
            float height = runner.PlayerConfig.HeightFor(player).ToFloat();

            // Unity's capsule is 1 wide and 2 tall at scale 1, with its pivot in the middle.
            body.localScale = new Vector3(width, height * 0.5f, width);
            body.localPosition = new Vector3(0f, height * 0.5f, 0f);
            if (nose != null) nose.localPosition = new Vector3(width * 0.5f, height * 0.75f, 0f);
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
