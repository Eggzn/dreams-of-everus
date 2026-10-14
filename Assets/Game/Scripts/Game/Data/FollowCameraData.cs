using UnityEngine;

namespace DreamsOfEverus.Game
{
    /// <summary>
    /// Settings for the side-on follow camera. Visual only: the camera never affects gameplay,
    /// so it can use normal decimals and time. ALL VALUES ARE PLACEHOLDERS until tuned by eye.
    /// </summary>
    [CreateAssetMenu(menuName = "Dreams of Everus/Follow Camera", fileName = "FollowCamera")]
    public class FollowCameraData : ScriptableObject
    {
        [Header("Framing (PLACEHOLDER)")]
        [Tooltip("How far the camera sits from the point it looks at, in units. Bigger = more of the room in view.")]
        [Min(1f)] public float distance = 16f;
        [Tooltip("The camera looks at a point this far above the character's feet, in units.")]
        public float heightOffset = 1.5f;
        [Tooltip("Tilts the camera to look slightly down, in degrees. 0 = perfectly side-on.")]
        [Range(-30f, 30f)] public float pitch = 6f;
        [Tooltip("Field of view, in degrees. Lower = flatter, more 2D-looking perspective.")]
        [Range(10f, 90f)] public float fieldOfView = 40f;

        [Header("Look-ahead (PLACEHOLDER)")]
        [Tooltip("How far ahead of the character (in the direction they face) the camera leans, in units.")]
        [Min(0f)] public float lookAheadDistance = 1.5f;
        [Tooltip("How long the look-ahead takes to swing to the other side after turning, in seconds.")]
        [Min(0.01f)] public float lookAheadSmoothTime = 0.5f;

        [Header("Smoothing (PLACEHOLDER)")]
        [Tooltip("How long the camera takes to catch up sideways, in seconds. Smaller = tighter.")]
        [Min(0.01f)] public float horizontalSmoothTime = 0.15f;
        [Tooltip("How long the camera takes to catch up vertically, in seconds. Lazier than sideways so jumps don't jerk the view.")]
        [Min(0.01f)] public float verticalSmoothTime = 0.3f;
        [Tooltip("If the character suddenly moves further than this (e.g. respawning), the camera jumps straight there, in units.")]
        [Min(1f)] public float snapDistance = 8f;
    }
}
