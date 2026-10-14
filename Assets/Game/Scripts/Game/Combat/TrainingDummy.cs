using DreamsOfEverus.Simulation;
using UnityEngine;

namespace DreamsOfEverus.Game
{
    /// <summary>
    /// Marks where the training dummy stands (this object's position is its feet) and sets up its hurtbox.
    /// The dummy never moves; it just flashes and shakes when hit.
    /// </summary>
    [DisallowMultipleComponent]
    public class TrainingDummy : MonoBehaviour
    {
        [Header("Hurtbox (PLACEHOLDER)")]
        [Tooltip("Width of the area that can be hit, in units.")]
        [Min(0.1f)] public float hurtboxWidth = 0.8f;
        [Tooltip("Height of the area that can be hit, from the feet up, in units.")]
        [Min(0.1f)] public float hurtboxHeight = 1.8f;

        [Header("Reaction (PLACEHOLDER)")]
        [Tooltip("How many frames the dummy flashes and shakes after being hit.")]
        [Min(1)] public int hitReactionFrames = 20;

        public TrainingDummyConfig ToConfig()
        {
            Vector3 feet = transform.position;
            return new TrainingDummyConfig
            {
                Position = new FixedVector2(Fixed.FromFloat(feet.x), Fixed.FromFloat(feet.y)),
                HurtboxWidth = Fixed.FromFloat(hurtboxWidth),
                HurtboxHeight = Fixed.FromFloat(hurtboxHeight),
                HitReactTicks = hitReactionFrames,
            };
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + Vector3.up * hurtboxHeight * 0.5f, new Vector3(hurtboxWidth, hurtboxHeight, 0.01f));
        }
    }
}
