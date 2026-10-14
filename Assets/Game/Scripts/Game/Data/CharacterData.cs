using DreamsOfEverus.Simulation;
using UnityEngine;

namespace DreamsOfEverus.Game
{
    /// <summary>
    /// One character's tunable values, edited in the Inspector.
    /// Values are typed as normal decimals here and converted to fixed-point when play starts.
    /// The numbers below are only the defaults for a brand-new asset; the asset itself is what the game uses.
    /// </summary>
    [CreateAssetMenu(menuName = "Dreams of Everus/Character Data", fileName = "CharacterData")]
    public class CharacterData : ScriptableObject
    {
        [Header("Body (PLACEHOLDER values, tune freely)")]
        [Tooltip("Width of the collision box, in world units (1 unit = 1 Unity grid square).")]
        [Min(0.1f)] public float bodyWidth = 0.8f;

        [Tooltip("Height of the collision box, from the feet up, in world units.")]
        [Min(0.1f)] public float bodyHeight = 1.8f;

        public FighterConfig ToFighterConfig()
        {
            return new FighterConfig
            {
                BodyWidth = Fixed.FromFloat(bodyWidth),
                BodyHeight = Fixed.FromFloat(bodyHeight),
            };
        }
    }
}
