using UnityEngine;

namespace DreamsOfEverus.Game
{
    /// <summary>Where the player starts and where they come back after touching a fall zone. Marks the feet position.</summary>
    [DisallowMultipleComponent]
    public class RespawnPoint : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Vector3 feet = transform.position;
            Gizmos.DrawWireSphere(feet + Vector3.up * 0.25f, 0.25f);
            Gizmos.DrawLine(feet, feet + Vector3.up * 1.5f);
        }
    }
}
