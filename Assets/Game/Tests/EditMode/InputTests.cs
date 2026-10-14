using System.Linq;
using DreamsOfEverus.Game;
using DreamsOfEverus.Simulation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DreamsOfEverus.Tests
{
    public class InputTests
    {
        private const string ControlsPath = "Assets/Game/Input/GameplayControls.inputactions";

        [Test]
        public void Build_RoundsStickToWholeNumbers()
        {
            InputFrame frame = InputFrameBuilder.Build(new Vector2(0.5f, -1f), false, false);
            Assert.AreEqual(50, frame.MoveX);
            Assert.AreEqual(-100, frame.MoveY);
        }

        [Test]
        public void Build_ClampsOutOfRangeStick()
        {
            InputFrame frame = InputFrameBuilder.Build(new Vector2(1.5f, -2f), false, false);
            Assert.AreEqual(100, frame.MoveX);
            Assert.AreEqual(-100, frame.MoveY);
        }

        [Test]
        public void Build_SetsButtons()
        {
            InputFrame frame = InputFrameBuilder.Build(Vector2.zero, true, false);
            Assert.IsTrue(frame.IsHeld(InputButtons.Jump));
            Assert.IsFalse(frame.IsHeld(InputButtons.Attack));
        }

        [Test]
        public void Latch_KeepsTapThatEndedBetweenTicks()
        {
            var latch = new ButtonLatch();
            latch.NotePress();                          // pressed and released between two ticks
            Assert.IsTrue(latch.ConsumeForTick(false)); // the next tick still sees it
            Assert.IsFalse(latch.ConsumeForTick(false)); // and only once
        }

        [Test]
        public void Latch_PassesThroughHeldButton()
        {
            var latch = new ButtonLatch();
            Assert.IsTrue(latch.ConsumeForTick(true));
            Assert.IsFalse(latch.ConsumeForTick(false));
        }

        [Test]
        public void ControlsAsset_HasNamedActionsAndBindings()
        {
            var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ControlsPath);
            Assert.IsNotNull(asset, "GameplayControls asset missing");

            InputActionMap map = asset.FindActionMap(PlayerInputReader.MapName);
            Assert.IsNotNull(map);

            AssertHasBinding(map, PlayerInputReader.MoveAction, "<Gamepad>/leftStick");
            AssertHasBinding(map, PlayerInputReader.JumpAction, "<Gamepad>/buttonNorth"); // Triangle
            AssertHasBinding(map, PlayerInputReader.JumpAction, "<Keyboard>/space");
            AssertHasBinding(map, PlayerInputReader.AttackAction, "<Gamepad>/buttonWest"); // Square
            AssertHasBinding(map, PlayerInputReader.AttackAction, "<Keyboard>/j");
        }

        private static void AssertHasBinding(InputActionMap map, string actionName, string path)
        {
            InputAction action = map.FindAction(actionName);
            Assert.IsNotNull(action, $"Missing action {actionName}");
            Assert.IsTrue(action.bindings.Any(b => b.path == path), $"{actionName} has no binding {path}");
        }
    }
}
