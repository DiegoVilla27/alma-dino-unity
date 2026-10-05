using UnityEngine;

namespace AlmaGame.Player
{
    // Roar feedback: three sound-wave arcs leaving Alma's mouth with the same opening and reach as
    // the gameplay cone, a light camera shake and, on the ground, dust pushed forward. Arcs are a
    // pool of three sprites and the dust one particle system (max 10); nothing renders between roars.
    [DisallowMultipleComponent, RequireComponent(typeof(AlmaMotor2D), typeof(CapsuleCollider2D), typeof(SpriteRenderer))]
    public sealed class AlmaRoarFx : MonoBehaviour
    {
        [Header("Sound-wave arcs")]
        [SerializeField] private Vector2 _mouthOffset = new Vector2(0.9f, 0.4f);
        [SerializeField, Min(0.02f)] private float _arcInterval = 0.07f;
        [SerializeField, Min(0.05f)] private float _arcDuration = 0.35f;
        [SerializeField] private Color _arcColor = new Color(1f, 0.95f, 0.8f, 0.8f);

        [Header("Dust (ground only)")]
        [SerializeField, Range(0, 10)] private int _dustPuffs = 8;
        [SerializeField] private Color _dustColor = new Color(0.9f, 0.84f, 0.72f, 0.7f);

        [Header("Screen shake")]
        [SerializeField, Min(0f)] private float _shakeAmplitude = 0.07f;
        [SerializeField, Min(0f)] private float _shakeDuration = 0.2f;

        private const int ArcCount = 3;
        // The ring sits at 80 % of the sprite's half-width, so its radius is 0.4 × scale.
        private const float ArcRadiusPerScale = 0.4f;
        private static Sprite s_arcSprite;
        private static float s_arcSpriteHalfAngle = -1f;

        private AlmaMotor2D _motor;
        private AlmaCameraFollow _camera;
        private readonly SpriteRenderer[] _arcs = new SpriteRenderer[ArcCount];
        private readonly float[] _arcStartedAt = new float[ArcCount];
        private readonly bool[] _arcActive = new bool[ArcCount];
        private float _roarStartedAt;
        private int _arcsSpawned = ArcCount;
        private bool _wasRoaring;
        private ParticleSystem _dust;
        private Material _dustMaterial;
        private float _feetY;

        private void Awake()
        {
            _motor = GetComponent<AlmaMotor2D>();
            var capsule = GetComponent<CapsuleCollider2D>();
            var sprite = GetComponent<SpriteRenderer>();
            _feetY = capsule.offset.y - capsule.size.y * 0.5f + 0.05f;

            // Arcs live in world space: each stays where it left Alma's mouth.
            Sprite arcSprite = ArcSprite(_motor.Settings.RoarHalfAngle);
            for (int i = 0; i < ArcCount; i++)
            {
                var arc = new GameObject("RoarArc").AddComponent<SpriteRenderer>();
                arc.sprite = arcSprite;
                arc.sharedMaterial = sprite.sharedMaterial;
                arc.sortingLayerID = sprite.sortingLayerID;
                arc.sortingOrder = sprite.sortingOrder + 1;
                arc.enabled = false;
                _arcs[i] = arc;
            }

            var dustObject = new GameObject("RoarDust");
            dustObject.transform.SetParent(transform, false);
            _dust = dustObject.AddComponent<ParticleSystem>();
            _dust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = _dust.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.6f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.45f);
            main.startColor = _dustColor;
            main.gravityModifier = 0.1f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 10;

            var emission = _dust.emission;
            emission.enabled = false; // Only manual Emit() when the roar starts.

            var shape = _dust.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(0.6f, 0.1f, 0f);

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

        private void LateUpdate()
        {
            bool roaring = _motor.IsRoaring;
            if (roaring && !_wasRoaring) BeginRoar();
            _wasRoaring = roaring;

            // Staggered arcs: one immediately, the rest every _arcInterval.
            while (_arcsSpawned < ArcCount && Time.time >= _roarStartedAt + _arcsSpawned * _arcInterval)
                SpawnArc(_arcsSpawned++);

            float reach = _motor.Settings.RoarRange;
            for (int i = 0; i < ArcCount; i++)
            {
                if (!_arcActive[i]) continue;
                float t = (Time.time - _arcStartedAt[i]) / _arcDuration;
                if (t >= 1f)
                {
                    _arcActive[i] = false;
                    _arcs[i].enabled = false;
                    continue;
                }
                float eased = 1f - (1f - t) * (1f - t);
                float radius = Mathf.Lerp(0.2f, reach, eased);
                _arcs[i].transform.localScale = Vector3.one * (radius / ArcRadiusPerScale);
                Color arcColor = _arcColor;
                arcColor.a *= 1f - t;
                _arcs[i].color = arcColor;
            }
        }

        private void BeginRoar()
        {
            _roarStartedAt = Time.time;
            _arcsSpawned = 0;

            int facing = _motor.FacingDirection;
            if (_motor.IsGrounded && _dustPuffs > 0)
            {
                _dust.transform.localPosition = new Vector3(0.6f * facing, _feetY, 0f);
                var velocity = _dust.velocityOverLifetime;
                velocity.enabled = true;
                velocity.space = ParticleSystemSimulationSpace.World;
                velocity.x = new ParticleSystem.MinMaxCurve(2f * facing, 4f * facing);
                velocity.y = new ParticleSystem.MinMaxCurve(0.2f, 0.6f);
                velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
                _dust.Emit(_dustPuffs);
            }
            if (_camera != null) _camera.Shake(_shakeAmplitude, _shakeDuration);
        }

        // Arcs keep the facing locked at roar start, like the gameplay cone.
        private void SpawnArc(int index)
        {
            int facing = _motor.FacingDirection;
            SpriteRenderer arc = _arcs[index];
            arc.transform.position = transform.position + new Vector3(_mouthOffset.x * facing, _mouthOffset.y, 0f);
            arc.flipX = facing < 0;
            arc.transform.localScale = Vector3.one * (0.2f / ArcRadiusPerScale);
            arc.color = _arcColor;
            arc.enabled = true;
            _arcStartedAt[index] = Time.time;
            _arcActive[index] = true;
        }

        private void OnDestroy()
        {
            foreach (var arc in _arcs)
                if (arc != null) Destroy(arc.gameObject);
            if (_dustMaterial != null) Destroy(_dustMaterial);
        }

        // Soft ring segment opening to +X with the roar's half-angle; pivot at the ring's centre.
        private static Sprite ArcSprite(float halfAngle)
        {
            if (s_arcSprite != null && Mathf.Approximately(s_arcSpriteHalfAngle, halfAngle)) return s_arcSprite;
            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "AlmaRoarArc",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            var pixels = new Color32[size * size];
            float radius = size * 0.5f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                Vector2 offset = new Vector2(x + 0.5f - radius, y + 0.5f - radius);
                float distance = offset.magnitude / radius;
                float ring = Mathf.Clamp01(1f - Mathf.Abs(distance - 0.8f) / 0.07f);
                float angle = Mathf.Abs(Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg);
                float cone = Mathf.Clamp01((halfAngle - angle) / 10f);
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(ring * ring * cone * 255f));
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            s_arcSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
            s_arcSprite.name = "AlmaRoarArc";
            s_arcSpriteHalfAngle = halfAngle;
            return s_arcSprite;
        }
    }
}
