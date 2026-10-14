using UnityEngine;

namespace DreamsOfEverus.Game
{
    /// <summary>Makes the training dummy flash and shake while it's reacting to a hit. Visual only.</summary>
    public class TrainingDummyView : MonoBehaviour
    {
        [SerializeField] private SimulationRunner runner;
        [SerializeField] private Transform body;
        [SerializeField] private Renderer bodyRenderer;

        [Header("Look (visual only)")]
        [SerializeField] private Color normalColor = new Color(0.85f, 0.85f, 0.8f);
        [SerializeField] private Color hitColor = new Color(1f, 0.25f, 0.2f);
        [Tooltip("How far the dummy shakes sideways when hit, in units.")]
        [SerializeField] private float shakeAmount = 0.12f;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private MaterialPropertyBlock colors;
        private Vector3 bodyRestPosition;

        private void Start()
        {
            colors = new MaterialPropertyBlock();
            if (body != null) bodyRestPosition = body.localPosition;
        }

        private void LateUpdate()
        {
            if (runner == null || runner.State == null || runner.DummyConfig == null || body == null) return;

            int ticksLeft = runner.State.Dummy.HitReactTicksLeft;
            float reaction = (float)ticksLeft / runner.DummyConfig.HitReactTicks; // 1 right after the hit, fading to 0

            // Shake side to side, getting smaller as the reaction fades.
            float shake = ticksLeft > 0 ? Mathf.Sin(ticksLeft * 2.5f) * shakeAmount * reaction : 0f;
            body.localPosition = bodyRestPosition + new Vector3(shake, 0f, 0f);

            if (bodyRenderer != null)
            {
                colors.SetColor(BaseColorId, Color.Lerp(normalColor, hitColor, reaction));
                bodyRenderer.SetPropertyBlock(colors);
            }
        }
    }
}
