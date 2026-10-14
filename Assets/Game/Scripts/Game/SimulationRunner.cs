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

        [Tooltip("If the game falls behind (e.g. a lag spike), at most this many ticks run in one frame.")]
        [SerializeField] private int maxTicksPerFrame = 5;

        public CharacterData PlayerCharacter => playerCharacter;
        public GameState State { get; private set; }
        public GameState PreviousState { get; private set; }

        /// <summary>0..1: how far real time is between PreviousState and State. For visuals only.</summary>
        public float InterpolationAlpha { get; private set; }

        private const double TickDuration = 1.0 / GameSimulation.TicksPerSecond;

        private GameSimulation simulation;
        private IInputSource inputSource;
        private double accumulatedTime;

        private void Awake()
        {
            if (playerCharacter == null)
            {
                Debug.LogError("SimulationRunner: no Player Character data assigned.", this);
                enabled = false;
                return;
            }

            LevelGeometry level = LevelBaker.BakeScene();
            simulation = new GameSimulation(level, playerCharacter.ToFighterConfig());

            State = simulation.CreateInitialState();
            PreviousState = State.Clone();
            inputSource = GetComponent<IInputSource>();
        }

        private void Update()
        {
            // Real time only decides HOW MANY ticks to run. The ticks themselves never see it.
            accumulatedTime += Time.unscaledDeltaTime;

            int ticksThisFrame = 0;
            while (accumulatedTime >= TickDuration && ticksThisFrame < maxTicksPerFrame)
            {
                InputFrame input = inputSource != null ? inputSource.ReadInput() : default;
                PreviousState.CopyFrom(State);
                simulation.Step(State, input);

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
    }
}
