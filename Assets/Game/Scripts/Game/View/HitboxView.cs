using DreamsOfEverus.Simulation;
using UnityEngine;

namespace DreamsOfEverus.Game
{
    /// <summary>
    /// Shows the player's attack hitbox as a see-through red box on the frames it's out.
    /// Reads the simulation's hitbox, so what you see is exactly what can hit. Press F2 to show or hide.
    /// </summary>
    public class HitboxView : MonoBehaviour
    {
        [SerializeField] private SimulationRunner runner;
        [SerializeField] private Renderer box;
        [SerializeField] private bool visible = true;

        private void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current?.f2Key.wasPressedThisFrame == true) visible = !visible;
        }

        private void LateUpdate()
        {
            if (box == null) return;

            bool show = visible && runner != null && runner.State != null
                && CombatSystem.TryGetActiveHitbox(runner.State.Player, runner.PlayerConfig, out FixedBox hitbox);
            box.enabled = show;
            if (!show) return;

            CombatSystem.TryGetActiveHitbox(runner.State.Player, runner.PlayerConfig, out hitbox);
            Vector2 min = new Vector2(hitbox.Min.X.ToFloat(), hitbox.Min.Y.ToFloat());
            Vector2 max = new Vector2(hitbox.Max.X.ToFloat(), hitbox.Max.Y.ToFloat());

            // Slightly in front of everything (toward the camera) so it's never hidden.
            box.transform.position = new Vector3((min.x + max.x) * 0.5f, (min.y + max.y) * 0.5f, -1.2f);
            box.transform.localScale = new Vector3(max.x - min.x, max.y - min.y, 0.05f);
        }
    }
}
