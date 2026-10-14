using DreamsOfEverus.Simulation;
using UnityEngine;

namespace DreamsOfEverus.Game
{
    /// <summary>
    /// Marks a cube as level collision. Its position and scale (X and Y) become a box in the simulation
    /// when play starts. Keep these cubes unrotated: rotation is ignored.
    /// </summary>
    [DisallowMultipleComponent]
    public class LevelBox : MonoBehaviour
    {
        [Tooltip("Solid blocks from every side. Pass Through can be jumped up through and dropped through with a hard Down. Fall Zone respawns the player.")]
        [SerializeField] private LevelBlockKind kind = LevelBlockKind.Solid;

        public LevelBlockKind Kind => kind;

        public LevelBlock ToBlock()
        {
            Vector3 center = transform.position;
            Vector3 halfSize = transform.lossyScale * 0.5f;

            var min = new FixedVector2(Fixed.FromFloat(center.x - halfSize.x), Fixed.FromFloat(center.y - halfSize.y));
            var max = new FixedVector2(Fixed.FromFloat(center.x + halfSize.x), Fixed.FromFloat(center.y + halfSize.y));
            return new LevelBlock(new FixedBox(min, max), kind);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = kind switch
            {
                LevelBlockKind.PassThrough => new Color(0.3f, 0.7f, 1f),
                LevelBlockKind.FallZone => new Color(1f, 0.2f, 0.2f),
                _ => Color.white,
            };
            Vector3 scale = transform.lossyScale;
            Gizmos.DrawWireCube(transform.position, new Vector3(scale.x, scale.y, 0.01f));
        }
    }
}
