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
        [Tooltip("How far (out of 100) the stick must go to count as a press: for double-taps and hard presses.")]
        [Range(1, 100)] public int pressThreshold = 70;

        [Header("Timing (PLACEHOLDER)")]
        [Tooltip("A press of Down counts as 'hard' (drop-through, fast-fall) if the stick gets from centered to the press threshold within this many frames.")]
        [Min(1)] public int hardPressWindowFrames = 3;
        [Tooltip("Most frames allowed between the two presses of a double-tap (dash).")]
        [Min(1)] public int doubleTapWindowFrames = 12;

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
            config.DoubleTapWindowTicks = doubleTapWindowFrames;
        }
    }
}
