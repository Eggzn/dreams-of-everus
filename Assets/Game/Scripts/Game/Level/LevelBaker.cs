using DreamsOfEverus.Simulation;
using UnityEngine;

namespace DreamsOfEverus.Game
{
    /// <summary>Reads every LevelBox and the RespawnPoint in the open scene and turns them into simulation data.</summary>
    public static class LevelBaker
    {
        public static LevelGeometry BakeScene()
        {
            var level = new LevelGeometry();

            foreach (LevelBox box in Object.FindObjectsByType<LevelBox>())
            {
                level.Add(box.ToBlock().Box, box.Kind);
            }

            RespawnPoint respawn = Object.FindAnyObjectByType<RespawnPoint>();
            if (respawn != null)
            {
                Vector3 p = respawn.transform.position;
                level.RespawnPoint = new FixedVector2(Fixed.FromFloat(p.x), Fixed.FromFloat(p.y));
            }
            else
            {
                Debug.LogWarning("LevelBaker: no RespawnPoint in the scene, using (0, 0).");
            }

            return level;
        }
    }
}
