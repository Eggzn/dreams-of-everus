using DreamsOfEverus.Simulation;
using UnityEngine;

namespace DreamsOfEverus.Game
{
    /// <summary>
    /// How stick and button input is read, shared by all characters. Stick values run from -100 to 100.
    /// ALL VALUES ARE PLACEHOLDERS until tuned by feel.
    /// </summary>
    [CreateAssetMenu(menuName = "Dreams of Everus/Input Feel", fileName = "InputFeel")]
    public class InputFeelData : ScriptableObject
    {
        [Header("Stick (PLACEHOLDER)")]
        [Tooltip("Stick values smaller than this (out of 100) count as centered.")]
        [Range(0, 99)] public int stickDeadzone = 25;
        [Tooltip("How far (out of 100) the stick must go to count as a press.")]
        [Range(1, 100)] public int pressThreshold = 80;

        [Header("Hard press (PLACEHOLDER)")]
        [Tooltip("A press counts as 'hard', like a smash input (dash, dash dance, drop-through, fast-fall), if the stick gets from not pushed to the press threshold within this many frames. Easing it over slower is just a tilt.")]
        [Min(1)] public int hardPressWindowFrames = 2;

        [Header("Keyboard (PLACEHOLDER)")]
        [Tooltip("While the walk key (Shift) is held, keyboard movement only tilts this far (out of 100). Keep it below the press threshold so it walks instead of dashing. Also how keyboard players moonwalk.")]
        [Range(1, 100)] public int keyboardWalkTilt = 65;

        [Header("Crouch (PLACEHOLDER)")]
        [Tooltip("How far down (out of 100) the stick must be held to crouch.")]
        [Range(1, 100)] public int crouchThreshold = 50;

        public InputFeelConfig ToConfig()
        {
            var config = new InputFeelConfig();
            ApplyTo(config);
            return config;
        }

        public void ApplyTo(InputFeelConfig config)
        {
            config.StickDeadzone = stickDeadzone;
            config.PressThreshold = pressThreshold;
            config.HardPressWindowTicks = hardPressWindowFrames;
            config.CrouchThreshold = crouchThreshold;
        }
    }
}
