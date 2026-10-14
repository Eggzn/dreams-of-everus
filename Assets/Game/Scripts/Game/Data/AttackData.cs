using DreamsOfEverus.Simulation;
using UnityEngine;

namespace DreamsOfEverus.Game
{
    /// <summary>
    /// One attack's frame data and hitbox, edited in the Inspector. Each character points to their own attacks.
    /// Frames are at 60 per second. ALL VALUES ARE PLACEHOLDERS until tuned by feel.
    /// </summary>
    [CreateAssetMenu(menuName = "Dreams of Everus/Attack Data", fileName = "AttackData")]
    public class AttackData : ScriptableObject
    {
        [Header("Frame data (PLACEHOLDER)")]
        [Tooltip("Frames of windup before the hitbox comes out.")]
        [Min(0)] public int startupFrames = 3;
        [Tooltip("Frames the hitbox is out and can hit.")]
        [Min(1)] public int activeFrames = 3;
        [Tooltip("Frames after the hitbox is gone before the character can act again.")]
        [Min(0)] public int recoveryFrames = 12;

        [Header("Hitbox (PLACEHOLDER)")]
        [Tooltip("Center of the hitbox measured from the character's feet, in units. X is forward (it flips when facing left), Y is up.")]
        public Vector2 hitboxOffset = new Vector2(0.75f, 1.1f);
        [Tooltip("Width and height of the hitbox, in units.")]
        public Vector2 hitboxSize = new Vector2(0.8f, 0.5f);

        public int TotalFrames => startupFrames + activeFrames + recoveryFrames;

        /// <summary>Copies these values into a config. Used at the start of play and for live tuning in the Editor.</summary>
        public void ApplyTo(AttackConfig config)
        {
            config.StartupTicks = startupFrames;
            config.ActiveTicks = activeFrames;
            config.RecoveryTicks = recoveryFrames;
            config.HitboxOffset = new FixedVector2(Fixed.FromFloat(hitboxOffset.x), Fixed.FromFloat(hitboxOffset.y));
            config.HitboxSize = new FixedVector2(Fixed.FromFloat(hitboxSize.x), Fixed.FromFloat(hitboxSize.y));
        }
    }
}
