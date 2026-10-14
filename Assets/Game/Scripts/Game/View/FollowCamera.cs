using UnityEngine;

namespace DreamsOfEverus.Game
{
    /// <summary>
    /// Side-on perspective camera that follows a character. Visual only: it reads where the character
    /// is shown and which way they face, and never changes the simulation.
    /// Runs after the character's view has moved for this frame, so it never lags a frame behind.
    /// </summary>
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(Camera))]
    public class FollowCamera : MonoBehaviour
    {
        [SerializeField] private SimulationRunner runner;
        [Tooltip("The character to follow (their view object, which is already smoothed between ticks).")]
        [SerializeField] private Transform target;
        [SerializeField] private FollowCameraData settings;

        private Camera cam;
        private Vector3 lastTargetPosition;
        private float lookAhead;
        private float lookAheadSpeed;
        private float horizontalSpeed;
        private float verticalSpeed;

        private void Start()
        {
            cam = GetComponent<Camera>();
            if (target == null || settings == null) return;

            lookAhead = Facing() * settings.lookAheadDistance;
            SnapToTarget();
        }

        private void LateUpdate()
        {
            if (target == null || settings == null) return;

            cam.fieldOfView = settings.fieldOfView;
            transform.rotation = Quaternion.Euler(settings.pitch, 0f, 0f);

            // A big sudden jump (respawn) snaps the camera instead of sweeping across the level.
            if (Vector3.Distance(target.position, lastTargetPosition) > settings.snapDistance)
            {
                lookAhead = Facing() * settings.lookAheadDistance;
                SnapToTarget();
                return;
            }
            lastTargetPosition = target.position;

            // Lean toward the way the character faces, swinging over smoothly when they turn.
            lookAhead = Mathf.SmoothDamp(lookAhead, Facing() * settings.lookAheadDistance, ref lookAheadSpeed, settings.lookAheadSmoothTime);

            Vector3 desired = DesiredPosition(target.position, lookAhead, settings);
            Vector3 current = transform.position;
            current.x = Mathf.SmoothDamp(current.x, desired.x, ref horizontalSpeed, settings.horizontalSmoothTime);
            current.y = Mathf.SmoothDamp(current.y, desired.y, ref verticalSpeed, settings.verticalSmoothTime);
            current.z = desired.z;
            transform.position = current;
        }

        /// <summary>Where the camera wants to be: pulled back from a point near the character, along its viewing direction.</summary>
        public static Vector3 DesiredPosition(Vector3 targetPosition, float lookAhead, FollowCameraData settings)
        {
            Vector3 focus = targetPosition + new Vector3(lookAhead, settings.heightOffset, 0f);
            Vector3 viewDirection = Quaternion.Euler(settings.pitch, 0f, 0f) * Vector3.forward;
            return focus - viewDirection * settings.distance;
        }

        private void SnapToTarget()
        {
            transform.rotation = Quaternion.Euler(settings.pitch, 0f, 0f);
            transform.position = DesiredPosition(target.position, lookAhead, settings);
            lastTargetPosition = target.position;
            horizontalSpeed = 0f;
            verticalSpeed = 0f;
        }

        private float Facing()
        {
            if (runner == null || runner.State == null) return 1f;
            return runner.State.Player.Facing < 0 ? -1f : 1f;
        }
    }
}
