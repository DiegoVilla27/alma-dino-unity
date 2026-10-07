using UnityEngine;

namespace AlmaGame.Level
{
    // Ambient life for resizable liquids (lava, toxic lake, mud), on top of the LiquidSprite shader:
    // slow bubbles that swell and fade at the surface, embers drifting up, and a splash where Alma
    // dies. Rates scale with the zone width and every system has a small particle cap, so a wide
    // lake costs about the same as a few torches. Emission stops while the liquid is off screen.
    [DisallowMultipleComponent, RequireComponent(typeof(HazardZone2D))]
    public sealed class LiquidFx2D : MonoBehaviour
    {
        [Header("Bubbles (per unit of width and second)")]
        [SerializeField, Min(0f)] private float _bubbleRate = 0.6f;
        [SerializeField] private Vector2 _bubbleSize = new Vector2(0.2f, 0.36f);
        [SerializeField] private Color _bubbleColor = new Color(1f, 0.92f, 0.6f, 1f);
        [SerializeField, Min(0f)] private float _bubbleDepth = 0.14f;
        [Tooltip("Ring-shaped bubbles with a highlight instead of soft glows; reads better on bright surfaces.")]
        [SerializeField] private bool _ringBubbles;

        [Header("Embers (per unit of width and second)")]
        [SerializeField, Min(0f)] private float _emberRate = 0.4f;
        [SerializeField] private Vector2 _emberSpeed = new Vector2(0.4f, 0.9f);
        [SerializeField] private Vector2 _emberSize = new Vector2(0.06f, 0.11f);
        [SerializeField] private Color _emberColor = new Color(1f, 0.7f, 0.3f, 1f);

        [Header("Splash when Alma dies")]
        [SerializeField, Min(0)] private int _splashCount = 14;
        [SerializeField] private Color _splashColor = new Color(1f, 0.5f, 0.1f, 1f);

        [Header("Budget")]
        [SerializeField, Min(4)] private int _maxParticlesPerSystem = 24;
        [SerializeField] private Material _particleMaterial;

        private HazardZone2D _zone;
        private ParticleSystem _bubbles;
        private ParticleSystem _embers;
        private ParticleSystem _splash;

        private void Awake()
        {
            _zone = GetComponent<HazardZone2D>();
            var surface = GetComponent<SpriteRenderer>();
            int layer = surface.sortingLayerID;
            int order = surface.sortingOrder;
            Vector2 size = _zone.Size;
            float top = size.y * 0.5f;
            float width = size.x * 0.9f;

            // Bubbles: blobs (or rings) that swell just below the surface line and fade out (no motion).
            _bubbles = HazardFx.CreateParticles("LiquidBubbles", transform, _particleMaterial,
                _ringBubbles ? HazardFx.Bubble() : HazardFx.Puff(),
                Budget(_bubbleRate * size.x, 1.1f), layer, order + 1);
            _bubbles.transform.localPosition = new Vector3(0f, top - _bubbleDepth, 0f);
            var bubbleMain = _bubbles.main;
            bubbleMain.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.1f);
            bubbleMain.startSpeed = 0f;
            bubbleMain.startSize = new ParticleSystem.MinMaxCurve(_bubbleSize.x, _bubbleSize.y);
            bubbleMain.startColor = _bubbleColor;
            SetEdge(_bubbles, width);
            HazardFx.SetSizeOverLifetime(_bubbles, 0.4f, 1.15f);
            SetRate(_bubbles, _bubbleRate * size.x);

            // Embers: tiny bright dots rising slowly from the surface and fading.
            _embers = HazardFx.CreateParticles("LiquidEmbers", transform, _particleMaterial, HazardFx.Puff(),
                Budget(_emberRate * size.x, 2.2f), layer, order + 2);
            _embers.transform.localPosition = new Vector3(0f, top - 0.05f, 0f);
            var emberMain = _embers.main;
            emberMain.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2.2f);
            emberMain.startSpeed = new ParticleSystem.MinMaxCurve(_emberSpeed.x, _emberSpeed.y);
            emberMain.startSize = new ParticleSystem.MinMaxCurve(_emberSize.x, _emberSize.y);
            emberMain.startColor = _emberColor;
            SetEdge(_embers, width);
            HazardFx.SetSizeOverLifetime(_embers, 1f, 0.3f);
            SetRate(_embers, _emberRate * size.x);

            // Splash: droplets thrown up and sideways where Alma fell in, on demand only.
            _splash = HazardFx.CreateParticles("LiquidSplash", transform, _particleMaterial, HazardFx.Puff(),
                Mathf.Max(4, _splashCount), layer, order + 2);
            var splashMain = _splash.main;
            splashMain.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.6f);
            splashMain.startSpeed = new ParticleSystem.MinMaxCurve(2f, 3.2f);
            splashMain.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
            splashMain.startColor = _splashColor;
            splashMain.gravityModifier = 1.2f;
            var splashShape = _splash.shape;
            splashShape.shapeType = ParticleSystemShapeType.Circle;
            splashShape.radius = 0.1f;
            splashShape.arc = 180f;

            _zone.Killed += Splash;
        }

        private void OnDestroy()
        {
            if (_zone != null) _zone.Killed -= Splash;
            HazardFx.DestroyMaterial(_bubbles);
            HazardFx.DestroyMaterial(_embers);
            HazardFx.DestroyMaterial(_splash);
        }

        // Called by Unity for the liquid's own SpriteRenderer.
        private void OnBecameVisible()
        {
            if (_bubbles != null) _bubbles.Play();
            if (_embers != null) _embers.Play();
        }

        private void OnBecameInvisible()
        {
            if (_bubbles != null) _bubbles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (_embers != null) _embers.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void Splash(Vector2 position)
        {
            float top = transform.position.y + _zone.Size.y * 0.5f;
            _splash.transform.position = new Vector3(position.x, top, transform.position.z);
            _splash.Emit(_splashCount);
        }

        // Enough particles for the rate and lifetime, never above the per-system cap.
        private int Budget(float rate, float lifetime) =>
            Mathf.Clamp(Mathf.CeilToInt(rate * lifetime) + 2, 4, _maxParticlesPerSystem);

        private static void SetEdge(ParticleSystem system, float width)
        {
            var shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.SingleSidedEdge;
            shape.radius = width * 0.5f;
        }

        private static void SetRate(ParticleSystem system, float rate)
        {
            var emission = system.emission;
            emission.rateOverTime = rate;
            emission.enabled = rate > 0f;
        }
    }
}
