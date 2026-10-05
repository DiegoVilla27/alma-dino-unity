using System.Collections;
using UnityEngine;

namespace AlmaGame.Player
{
    // Death and respawn sequence built from Alma's existing sprite plus generated textures:
    // freeze → red tint and squash → "poof" into dust and stars → a light orb flies to the respawn
    // point (Alma's transform carries it, so the camera follows) → ring burst and bouncy pop-in.
    // Control returns only when the sequence calls AlmaMotor2D.Respawn().
    [DisallowMultipleComponent, RequireComponent(typeof(AlmaMotor2D), typeof(SpriteRenderer), typeof(Animator))]
    public sealed class AlmaDeathFx : MonoBehaviour
    {
        [Header("Hit")]
        [SerializeField, Min(0f)] private float _freezeTime = 0.08f;
        [SerializeField, Min(0.01f)] private float _squashTime = 0.15f;
        [SerializeField] private Color _hitTint = new Color(1f, 0.55f, 0.55f, 1f);

        [Header("Poof")]
        [SerializeField, Min(0.01f)] private float _poofTime = 0.12f;
        [SerializeField, Range(0, 16)] private int _dustPuffs = 12;
        [SerializeField, Range(0, 8)] private int _stars = 5;
        [SerializeField] private Color _dustColor = new Color(0.92f, 0.88f, 0.8f, 0.85f);
        [SerializeField] private Color _starColor = new Color(1f, 0.93f, 0.55f, 1f);

        [Header("Light orb")]
        [SerializeField] private Color _orbColor = new Color(1f, 0.88f, 0.55f, 1f);
        [SerializeField, Min(1f)] private float _travelSpeed = 14f;
        [SerializeField] private Vector2 _travelTimeRange = new Vector2(0.45f, 1f);

        [Header("Reappear")]
        [SerializeField, Min(0.01f)] private float _popTime = 0.22f;
        [SerializeField, Min(0.05f)] private float _ringTime = 0.35f;

        [Header("Screen shake")]
        [SerializeField, Min(0f)] private float _shakeAmplitude = 0.15f;
        [SerializeField, Min(0f)] private float _shakeDuration = 0.2f;

        private static Texture2D s_starTexture;
        private static Sprite s_orbSprite;

        private AlmaMotor2D _motor;
        private SpriteRenderer _sprite;
        private Animator _animator;
        private AlmaCameraFollow _camera;
        private Vector3 _baseScale;
        private Coroutine _sequence;

        private SpriteRenderer _orb;
        private ParticleSystem _orbTrail;
        private ParticleSystem _dustBurst;
        private ParticleSystem _starBurst;
        private SpriteRenderer _ring;
        private float _ringStartedAt;
        private bool _ringActive;
        private readonly Material[] _materials = new Material[3];

        private void Awake()
        {
            _motor = GetComponent<AlmaMotor2D>();
            _sprite = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();
            _baseScale = transform.localScale;
            Material spriteMaterial = _sprite.sharedMaterial;
            int layer = _sprite.sortingLayerID;
            int order = _sprite.sortingOrder;

            // The orb rides on Alma's transform while her sprite is hidden.
            var orbObject = new GameObject("DeathOrb");
            orbObject.transform.SetParent(transform, false);
            _orb = orbObject.AddComponent<SpriteRenderer>();
            _orb.sprite = OrbSprite();
            _orb.color = _orbColor;
            _orb.sortingLayerID = layer;
            _orb.sortingOrder = order + 2;
            _orb.enabled = false;
            _materials[0] = new Material(spriteMaterial) { mainTexture = AlmaRunDust.PuffTexture() };
            _orbTrail = CreateParticles("DeathOrbTrail", orbObject.transform, _materials[0], _orbColor,
                new Vector2(0.25f, 0.35f), Vector2.zero, new Vector2(0.25f, 0.35f), 0f, 0f, 40, layer, order + 1);
            var trailEmission = _orbTrail.emission;
            trailEmission.rateOverDistance = 14f;
            _orbTrail.Play();

            // Bursts and the ring live in world space so they stay where they happened.
            _dustBurst = CreateParticles("DeathDust", null, _materials[0], _dustColor,
                new Vector2(0.4f, 0.6f), new Vector2(1f, 2.5f), new Vector2(0.4f, 0.7f), 1.5f, -0.1f, 16, layer, order + 1);
            _materials[1] = new Material(spriteMaterial) { mainTexture = StarTexture() };
            _starBurst = CreateParticles("DeathStars", null, _materials[1], _starColor,
                new Vector2(0.45f, 0.7f), new Vector2(2f, 3.5f), new Vector2(0.25f, 0.4f), 0f, 0.4f, 12, layer, order + 2);

            _ring = new GameObject("RespawnRing").AddComponent<SpriteRenderer>();
            _ring.sprite = AlmaGroundPoundFx.RingSprite();
            _ring.sortingLayerID = layer;
            _ring.sortingOrder = order + 1;
            _ring.enabled = false;
        }

        private void Start() => _camera = FindAnyObjectByType<AlmaCameraFollow>();

        private void OnEnable() => _motor.Died += OnDied;

        private void OnDisable()
        {
            _motor.Died -= OnDied;
            if (_sequence == null) return;
            // Interrupted mid-sequence: never leave Alma hidden or without physics.
            StopCoroutine(_sequence);
            _sequence = null;
            RestoreVisuals();
            if (_motor.IsDead) _motor.Respawn();
        }

        private void OnDied()
        {
            if (_sequence != null) StopCoroutine(_sequence);
            _sequence = StartCoroutine(Sequence());
        }

        private IEnumerator Sequence()
        {
            _animator.speed = 0f;
            if (_camera != null) _camera.Shake(_shakeAmplitude, _shakeDuration);
            yield return new WaitForSeconds(_freezeTime);

            // Hit: red tint while squashing flat.
            var squashed = new Vector3(1.2f, 0.75f, 1f);
            for (float t = 0f; t < 1f; t += Time.deltaTime / _squashTime)
            {
                transform.localScale = Vector3.Scale(_baseScale, Vector3.Lerp(Vector3.one, squashed, t));
                _sprite.color = Color.Lerp(Color.white, _hitTint, t);
                yield return null;
            }

            // Poof: snap down to nothing, then dust and stars from where she was.
            Vector3 center = transform.position;
            for (float t = 0f; t < 1f; t += Time.deltaTime / _poofTime)
            {
                transform.localScale = Vector3.Scale(_baseScale, Vector3.Lerp(squashed, new Vector3(0.1f, 1.3f, 1f), t * t));
                yield return null;
            }
            RestoreVisuals();
            _sprite.enabled = false;
            Emit(_dustBurst, center, _dustPuffs);
            Emit(_starBurst, center, _stars);

            // Light orb: starts from the bottom of the screen if she fell into a pit.
            Vector3 start = center;
            Camera view = Camera.main;
            if (view != null && view.orthographic)
                start.y = Mathf.Max(start.y, view.transform.position.y - view.orthographicSize + 0.6f);
            Vector3 end = _motor.RespawnPosition;
            end.z = start.z;
            transform.position = start;
            _orbTrail.Clear();
            _orb.enabled = true;
            var trailEmission = _orbTrail.emission;
            trailEmission.enabled = true;

            float distance = Vector3.Distance(start, end);
            float travelTime = Mathf.Clamp(distance / _travelSpeed, _travelTimeRange.x, _travelTimeRange.y);
            Vector3 control = (start + end) * 0.5f + Vector3.up * (1.5f + distance * 0.15f);
            for (float t = 0f; t < 1f; t += Time.deltaTime / travelTime)
            {
                float e = t * t * (3f - 2f * t);
                float u = 1f - e;
                transform.position = u * u * start + 2f * u * e * control + e * e * end;
                _orb.transform.localScale = Vector3.one * (0.55f + 0.08f * Mathf.Sin(Time.time * 30f));
                yield return null;
            }
            transform.position = end;
            trailEmission.enabled = false;
            _orb.enabled = false;

            // Reappear: ring and sparkle, then a bouncy pop to full size.
            PlayRing(end);
            Emit(_starBurst, end, Mathf.Min(3, _stars));
            _sprite.enabled = true;
            _animator.speed = 1f;
            for (float t = 0f; t < 1f; t += Time.deltaTime / _popTime)
            {
                float scale = t < 0.6f ? Mathf.Lerp(0f, 1.15f, t / 0.6f) : Mathf.Lerp(1.15f, 1f, (t - 0.6f) / 0.4f);
                transform.localScale = _baseScale * scale;
                yield return null;
            }
            transform.localScale = _baseScale;
            _sequence = null;
            _motor.Respawn();
        }

        private void Update()
        {
            if (!_ringActive) return;
            float t = (Time.time - _ringStartedAt) / _ringTime;
            if (t >= 1f)
            {
                _ringActive = false;
                _ring.enabled = false;
                return;
            }
            float eased = 1f - (1f - t) * (1f - t);
            _ring.transform.localScale = Vector3.one * Mathf.Lerp(0.5f, 3f, eased);
            Color ringColor = _orbColor;
            ringColor.a *= 1f - t;
            _ring.color = ringColor;
        }

        private void PlayRing(Vector3 position)
        {
            _ring.transform.position = position;
            _ring.transform.localScale = Vector3.one * 0.5f;
            _ring.color = _orbColor;
            _ring.enabled = true;
            _ringStartedAt = Time.time;
            _ringActive = true;
        }

        private void RestoreVisuals()
        {
            transform.localScale = _baseScale;
            _sprite.color = Color.white;
            _sprite.enabled = true;
            _animator.speed = 1f;
            _orb.enabled = false;
            var trailEmission = _orbTrail.emission;
            trailEmission.enabled = false;
        }

        private static void Emit(ParticleSystem system, Vector3 position, int count)
        {
            if (count <= 0) return;
            system.transform.position = position;
            system.Emit(count);
        }

        // Small world-space system: radial spread from a circle, scaled over life to endSize, fading out.
        private static ParticleSystem CreateParticles(string name, Transform parent, Material material, Color color,
            Vector2 lifetime, Vector2 speed, Vector2 size, float endSize, float gravity, int max, int layer, int order)
        {
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, false);
            var system = go.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = system.main;
            main.loop = true;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime.x, lifetime.y);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speed.x, speed.y);
            main.startSize = new ParticleSystem.MinMaxCurve(size.x, size.y);
            main.startColor = color;
            main.gravityModifier = gravity;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = max;

            var emission = system.emission;
            emission.rateOverTime = 0f;
            emission.enabled = false;

            var shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.2f;

            var sizeOverLife = system.sizeOverLifetime;
            sizeOverLife.enabled = true;
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, endSize));

            var fade = new Gradient();
            fade.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            var colorOverLife = system.colorOverLifetime;
            colorOverLife.enabled = true;
            colorOverLife.color = fade;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.sortingLayerID = layer;
            renderer.sortingOrder = order;
            return system;
        }

        private void OnDestroy()
        {
            if (_dustBurst != null) Destroy(_dustBurst.gameObject);
            if (_starBurst != null) Destroy(_starBurst.gameObject);
            if (_ring != null) Destroy(_ring.gameObject);
            foreach (var material in _materials)
                if (material != null) Destroy(material);
        }

        // Glowing orb sprite from the shared soft puff texture, one world unit wide.
        private static Sprite OrbSprite()
        {
            if (s_orbSprite != null) return s_orbSprite;
            Texture2D texture = AlmaRunDust.PuffTexture();
            s_orbSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), texture.width);
            s_orbSprite.name = "AlmaDeathOrb";
            return s_orbSprite;
        }

        // Four-point sparkle: two thin soft beams crossing over a small glowing core. Generated once.
        private static Texture2D StarTexture()
        {
            if (s_starTexture != null) return s_starTexture;
            const int size = 32;
            s_starTexture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "AlmaDeathStar",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            var pixels = new Color32[size * size];
            float half = size * 0.5f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x + 0.5f - half) / half;
                float dy = Mathf.Abs(y + 0.5f - half) / half;
                float beams = Mathf.Max(Mathf.Clamp01(1f - dx * 7f) * (1f - dy), Mathf.Clamp01(1f - dy * 7f) * (1f - dx));
                float core = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy) * 3f);
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(beams + core) * 255f));
            }
            s_starTexture.SetPixels32(pixels);
            s_starTexture.Apply(false, true);
            return s_starTexture;
        }
    }
}
