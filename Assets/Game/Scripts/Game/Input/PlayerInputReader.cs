using DreamsOfEverus.Simulation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DreamsOfEverus.Game
{
    /// <summary>
    /// Reads keyboard and controller through the named actions in GameplayControls
    /// and hands the SimulationRunner one InputFrame per tick.
    /// Put it on the same GameObject as the SimulationRunner.
    /// Runs before other scripts each frame so presses are noted before ticks run.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class PlayerInputReader : MonoBehaviour, IInputSource
    {
        public const string MapName = "Gameplay";
        public const string MoveAction = "Move";
        public const string JumpAction = "Jump";
        public const string AttackAction = "Attack";
        public const string WalkAction = "Walk";

        [Tooltip("The GameplayControls input actions asset (Assets/Game/Input).")]
        [SerializeField] private InputActionAsset controls;

        [Tooltip("The Input Feel asset (Assets/Game/Data). Sets how far the walk key tilts.")]
        [SerializeField] private InputFeelData inputFeel;

        private InputActionMap gameplayMap;
        private InputAction move;
        private InputAction jump;
        private InputAction attack;
        private InputAction walk;

        private readonly ButtonLatch jumpLatch = new ButtonLatch();
        private readonly ButtonLatch attackLatch = new ButtonLatch();

        private void Awake()
        {
            if (controls == null || inputFeel == null)
            {
                Debug.LogError("PlayerInputReader: missing assets. Assign the Controls and Input Feel fields.", this);
                enabled = false;
                return;
            }

            gameplayMap = controls.FindActionMap(MapName, throwIfNotFound: true);
            move = gameplayMap.FindAction(MoveAction, throwIfNotFound: true);
            jump = gameplayMap.FindAction(JumpAction, throwIfNotFound: true);
            attack = gameplayMap.FindAction(AttackAction, throwIfNotFound: true);
            walk = gameplayMap.FindAction(WalkAction, throwIfNotFound: true);
        }

        private void OnEnable() => gameplayMap?.Enable();
        private void OnDisable() => gameplayMap?.Disable();

        private void Update()
        {
            if (jump.WasPressedThisFrame()) jumpLatch.NotePress();
            if (attack.WasPressedThisFrame()) attackLatch.NotePress();
        }

        public InputFrame ReadInput()
        {
            if (!enabled) return default;

            Vector2 stick = InputFrameBuilder.ApplyWalkKey(move.ReadValue<Vector2>(), walk.IsPressed(), inputFeel.keyboardWalkTilt);

            return InputFrameBuilder.Build(
                stick,
                jumpLatch.ConsumeForTick(jump.IsPressed()),
                attackLatch.ConsumeForTick(attack.IsPressed()));
        }
    }
}
