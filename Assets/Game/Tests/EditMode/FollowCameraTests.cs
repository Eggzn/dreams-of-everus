using DreamsOfEverus.Game;
using NUnit.Framework;
using UnityEngine;

namespace DreamsOfEverus.Tests
{
    public class FollowCameraTests
    {
        private FollowCameraData settings;

        [SetUp]
        public void SetUp()
        {
            settings = ScriptableObject.CreateInstance<FollowCameraData>();
            settings.distance = 10f;
            settings.heightOffset = 1f;
            settings.pitch = 0f;
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(settings);

        [Test]
        public void SideOn_SitsStraightBackFromTheCharacter()
        {
            Vector3 position = FollowCamera.DesiredPosition(new Vector3(3f, 2f, 0f), 0f, settings);

            Assert.AreEqual(3f, position.x, 0.0001f);
            Assert.AreEqual(3f, position.y, 0.0001f); // feet + height offset
            Assert.AreEqual(-10f, position.z, 0.0001f);
        }

        [Test]
        public void LookAhead_ShiftsTowardFacing()
        {
            Vector3 right = FollowCamera.DesiredPosition(Vector3.zero, 1.5f, settings);
            Vector3 left = FollowCamera.DesiredPosition(Vector3.zero, -1.5f, settings);

            Assert.Greater(right.x, 0f);
            Assert.Less(left.x, 0f);
        }

        [Test]
        public void Pitch_LiftsCameraToLookDown()
        {
            settings.pitch = 10f;
            Vector3 position = FollowCamera.DesiredPosition(Vector3.zero, 0f, settings);

            Assert.Greater(position.y, settings.heightOffset);
            Assert.AreEqual(settings.distance, Vector3.Distance(position, new Vector3(0f, settings.heightOffset, 0f)), 0.0001f);
        }
    }
}
