using System.Collections.Generic;
using DreamsOfEverus.Simulation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DreamsOfEverus.Game
{
    /// <summary>
    /// Debug keys that prove the game is rollback-ready:
    /// F5 saves a snapshot, F9 restores it, F10 rewinds to it in the background, replays every input
    /// since, and checks the result matches the live game exactly. Debug only; never part of gameplay.
    /// </summary>
    public class SnapshotDebugger : MonoBehaviour
    {
        [SerializeField] private SimulationRunner runner;

        private GameState savedSnapshot;
        private readonly List<InputFrame> inputsSinceSave = new List<InputFrame>();
        private string message = "F5 save snapshot   F9 load snapshot   F10 rollback check";
        private Color messageColor = Color.white;
        private GUIStyle style;

        private void OnEnable()
        {
            if (runner != null) runner.TickRan += RecordInput;
        }

        private void OnDisable()
        {
            if (runner != null) runner.TickRan -= RecordInput;
        }

        private void RecordInput(InputFrame input)
        {
            if (savedSnapshot != null) inputsSinceSave.Add(input);
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || runner == null || runner.State == null) return;

            if (keyboard.f5Key.wasPressedThisFrame) Save();
            if (keyboard.f9Key.wasPressedThisFrame) Load();
            if (keyboard.f10Key.wasPressedThisFrame) RollbackCheck();
        }

        private void Save()
        {
            savedSnapshot = runner.State.Clone();
            inputsSinceSave.Clear();
            Show($"Snapshot saved at tick {savedSnapshot.Tick}.", Color.white);
        }

        private void Load()
        {
            if (savedSnapshot == null)
            {
                Show("No snapshot yet. Press F5 first.", Color.yellow);
                return;
            }

            runner.RestoreState(savedSnapshot);
            inputsSinceSave.Clear(); // the game is back at the snapshot, so nothing has happened since
            Show($"Snapshot loaded: back to tick {savedSnapshot.Tick}.", Color.white);
        }

        private void RollbackCheck()
        {
            if (savedSnapshot == null)
            {
                Show("No snapshot yet. Press F5 first.", Color.yellow);
                return;
            }

            GameState replayed = runner.Replay(savedSnapshot, inputsSinceSave);
            if (replayed.SameAs(runner.State))
            {
                Show($"Rollback check MATCH: replayed {inputsSinceSave.Count} ticks from the snapshot and got exactly the same game.", Color.green);
            }
            else
            {
                // In the Editor, changing tuning values after saving will also cause this.
                Show($"Rollback check MISMATCH after {inputsSinceSave.Count} ticks. (Changed any values since saving?)", Color.red);
            }
        }

        private void Show(string text, Color color)
        {
            message = text;
            messageColor = color;
        }

        private void OnGUI()
        {
            style ??= new GUIStyle(GUI.skin.label) { fontSize = 18 };
            style.normal.textColor = messageColor;
            GUI.Label(new Rect(12, Screen.height - 40, 1200, 30), message, style);
        }
    }
}
