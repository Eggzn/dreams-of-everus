using System;
using System.Collections.Generic;
using DreamsOfEverus.Simulation;
using UnityEngine;

namespace DreamsOfEverus.Game
{
    /// <summary>Anything that can provide one InputFrame per tick (keyboard/controller, a replay, an AI...).</summary>
    public interface IInputSource
    {
        InputFrame ReadInput();
    }

    /// <summary>
    /// The bridge between Unity's frame loop and the fixed 60-tick simulation.
    /// Unity frames can be any length; this runs as many whole ticks as real time allows.
    /// Visuals read State (and PreviousState, to smooth between ticks) but never change them.
    /// </summary>
    public class SimulationRunner : MonoBehaviour
    {
        [Tooltip("The player character's data asset (body size, movement values...).")]
        [SerializeField] private CharacterData playerCharacter;

        [Tooltip("How stick and button input is read (deadzone, hard press timing...).")]
        [SerializeField] private InputFeelData inputFeel;

        [Tooltip("If the game falls behind (e.g. a lag spike), at most this many ticks run in one frame.")]
        [SerializeField] private int maxTicksPerFrame = 5;

        public CharacterData PlayerCharacter => playerCharacter;
        public FighterConfig PlayerConfig { get; private set; }
        public TrainingDummyConfig DummyConfig { get; private set; }
        public GameState State { get; private set; }
        public GameState PreviousState { get; private set; }

        /// <summary>0..1: how far real time is between PreviousState and State. For visuals only.</summary>
        public float InterpolationAlpha { get; private set; }

        /// <summary>Called after every tick with the input it used. The snapshot debugger records these.</summary>
        public event Action<InputFrame> TickRan;

        private const double TickDuration = 1.0 / GameSimulation.TicksPerSecond;

        private GameSimulation simulation;
        private InputFeelConfig inputFeelConfig;
        private IInputSource inputSource;
        private double accumulatedTime;

        private void Awake()
        {
            if (playerCharacter == null || inputFeel == null)
            {
                Debug.LogError("SimulationRunner: assign both Player Character and Input Feel data.", this);
                enabled = false;
                return;
            }

            PlayerConfig = playerCharacter.ToFighterConfig();
            inputFeelConfig = inputFeel.ToConfig();

            LevelGeometry level = LevelBaker.BakeScene();
            TrainingDummy dummy = FindAnyObjectByType<TrainingDummy>();
            DummyConfig = dummy != null ? dummy.ToConfig() : null;
            simulation = new GameSimulation(level, PlayerConfig, inputFeelConfig, DummyConfig);

            State = simulation.CreateInitialState();
            PreviousState = State.Clone();
            inputSource = GetComponent<IInputSource>();
        }

        private void Update()
        {
#if UNITY_EDITOR
            // Live tuning: pick up any values changed in the Inspector during Play.
            // (Editor only: in a real match both players must use identical values.)
            playerCharacter.ApplyTo(PlayerConfig);
            inputFeel.ApplyTo(inputFeelConfig);
#endif

            // Real time only decides HOW MANY ticks to run. The ticks themselves never see it.
            accumulatedTime += Time.unscaledDeltaTime;

            int ticksThisFrame = 0;
            while (accumulatedTime >= TickDuration && ticksThisFrame < maxTicksPerFrame)
            {
                InputFrame input = inputSource != null ? inputSource.ReadInput() : default;
                PreviousState.CopyFrom(State);
                simulation.Step(State, input);
                TickRan?.Invoke(input);

                accumulatedTime -= TickDuration;
                ticksThisFrame++;
            }

            // Too far behind: drop the backlog instead of trying to catch up forever.
            if (ticksThisFrame == maxTicksPerFrame && accumulatedTime >= TickDuration)
            {
                accumulatedTime = 0;
            }

            InterpolationAlpha = (float)(accumulatedTime / TickDuration);
        }

        /// <summary>Jumps the game back to a saved snapshot (rollback's "rewind").</summary>
        public void RestoreState(GameState snapshot)
        {
            State.CopyFrom(snapshot);
            PreviousState.CopyFrom(snapshot); // no smoothing between the old and restored positions
            accumulatedTime = 0;
        }

        /// <summary>Runs the given inputs from a snapshot, off to the side, without touching the live game.</summary>
        public GameState Replay(GameState snapshot, IReadOnlyList<InputFrame> inputs)
        {
            return SnapshotReplay.Replay(simulation, snapshot, inputs);
        }
    }
}
