using UnityEngine;

namespace AlmaGame.Player
{
    // Pisotón impact feedback: a flat shockwave ring on the ground, a sideways dust burst and a
    // short camera shake. Everything is built at runtime from generated textures; the ring is one
    // sprite and the dust one particle system (max 16 particles), so the cost is two draw calls
    // only while the effect is visible.
    [DisallowMultipleComponent, RequireComponent(typeof(AlmaMotor2D), typeof(CapsuleCollider2D), typeof(SpriteRenderer))]
    public sealed class AlmaGroundPoundFx : MonoBehaviour
    {
        [Header("Shockwave")]
        [SerializeField, Min(0.05f)] private float _waveDuration = 0.4f;
        [SerializeField, Min(0.1f)] private float _waveWidth = 6f;
        [SerializeField, Range(0.1f, 1f)] private float _waveFlatten = 0.3f;
        [SerializeField] private Color _waveColor = new Color(1f, 0.96f, 0.85f, 0.85f);

        [Header("Dust")]
        [SerializeField, Range(0, 16)] private int _dustPuffs = 10;
        [SerializeField] private Color _dustColor = new Color(0.9f, 0.84f, 0.72f, 0.8f);

        [Header("Screen shake")]
        [SerializeField, Min(0f)] private float _shakeAmplitude = 0.6f;
        [SerializeField, Min(0f)] private float _shakeDuration = 0.4f;

        private static Sprite s_ringSprite;
        private AlmaMotor2D _motor;
        private AlmaCameraFollow _camera;
        private SpriteRenderer _wave;
        private ParticleSystem _dust;
        private Material _dustMaterial;
        private Vector3 _feetOffset;
        private float _waveStartedAt;
        private bool _waveActive;

        private void Awake()
        {
            _motor = GetComponent<AlmaMotor2D>();
            var capsule = GetComponent<CapsuleCollider2D>();
            var sprite = GetComponent<SpriteRenderer>();
            _feetOffset = new Vector3(0f, capsule.offset.y - capsule.size.y * 0.5f + 0.05f, 0f);

            // The ring lives in world space so it stays where Alma landed if she runs off.
            var waveObject = new GameObject("GroundPoundWave");
            _wave = waveObject.AddComponent<SpriteRenderer>();
            _wave.sprite = RingSprite();
            _wave.sortingLayerID = sprite.sortingLayerID;
            _wave.sortingOrder = sprite.sortingOrder + 1;
            _wave.enabled = false;

            var dustObject = new GameObject("GroundPoundDust");
            dustObject.transform.SetParent(transform, false);
            dustObject.transform.localPosition = _feetOffset;
            _dust = dustObject.AddComponent<ParticleSystem>();
            _dust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = _dust.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.55f);
            main.startColor = _dustColor;
            main.gravityModifier = 0.3f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 16;

            var emission = _dust.emission;
            emission.enabled = false; // Only manual Emit() on impact.

            // Upper half-circle: puffs fan out sideways and up, never into the floor.
            var shape = _dust.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.15f;
            shape.arc = 180f;

            var size = _dust.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.7f, 1f, 1.4f));

            var fade = new Gradient();
            fade.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            var color = _dust.colorOverLifetime;
            color.enabled = true;
            color.color = fade;

            _dustMaterial = new Material(sprite.sharedMaterial) { mainTexture = AlmaRunDust.PuffTexture() };
            var particleRenderer = dustObject.GetComponent<ParticleSystemRenderer>();
            particleRenderer.sharedMaterial = _dustMaterial;
            particleRenderer.sortingLayerID = sprite.sortingLayerID;
            particleRenderer.sortingOrder = sprite.sortingOrder + 1;
        }

        private void Start() => _camera = FindAnyObjectByType<AlmaCameraFollow>();

        private void OnEnable() => _motor.GroundPoundLanded += PlayImpact;

        private void OnDisable() => _motor.GroundPoundLanded -= PlayImpact;

        private void PlayImpact()
        {
            _wave.transform.position = transform.position + _feetOffset;
            _waveStartedAt = Time.time;
            _waveActive = true;
            _wave.enabled = true;
            UpdateWave(0f);

            if (_dustPuffs > 0) _dust.Emit(_dustPuffs);
            if (_camera != null) _camera.Shake(_shakeAmplitude, _shakeDuration);
        }

        private void Update()
        {
            if (!_waveActive) return;
            float t = (Time.time - _waveStartedAt) / _waveDuration;
            if (t >= 1f)
            {
                _waveActive = false;
                _wave.enabled = false;
                return;
            }
            UpdateWave(t);
        }

        // Ease-out expansion from a small ellipse to full width while fading out.
        private void UpdateWave(float t)
        {
            float eased = 1f - (1f - t) * (1f - t);
            float width = Mathf.Lerp(0.6f, _waveWidth, eased);
            _wave.transform.localScale = new Vector3(width, width * _waveFlatten, 1f);
            Color waveColor = _waveColor;
            waveColor.a *= 1f - t;
            _wave.color = waveColor;
        }

        private void OnDestroy()
        {
            if (_wave != null) Destroy(_wave.gameObject);
            if (_dustMaterial != null) Destroy(_dustMaterial);
        }

        // Soft ring, one world unit wide, generated once and shared by every instance.
        private static Sprite RingSprite()
        {
            if (s_ringSprite != null) return s_ringSprite;
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "AlmaGroundPoundRing",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            var pixels = new Color32[size * size];
            float radius = size * 0.5f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(radius, radius)) / radius;
                    float ring = Mathf.Clamp01(1f - Mathf.Abs(distance - 0.8f) / 0.18f);
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(ring * ring * 255f));
                }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            s_ringSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
            s_ringSprite.name = "AlmaGroundPoundRing";
            return s_ringSprite;
        }
    }
}
