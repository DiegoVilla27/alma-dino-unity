using UnityEngine;

namespace AlmaGame.Player
{
    // Subtle cartoon dust at Alma's feet while she runs on the ground.
    // One small particle system (max 20 particles, one draw call) built at runtime; it emits
    // per metre travelled, so it naturally thins out at low speed and stops when she stops.
    [DisallowMultipleComponent, RequireComponent(typeof(AlmaMotor2D), typeof(CapsuleCollider2D), typeof(SpriteRenderer))]
    public sealed class AlmaRunDust : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float _minSpeedRatio = 0.5f;
        [SerializeField, Min(0f)] private float _puffsPerMeter = 2.5f;
        [SerializeField] private Vector2 _sizeRange = new Vector2(0.35f, 0.6f);
        [SerializeField] private Color _color = new Color(0.9f, 0.84f, 0.72f, 0.75f);
        // Emitter sits this far behind Alma's centre so the body doesn't hide the puffs.
        [SerializeField, Min(0f)] private float _backOffset = 0.35f;

        private static Texture2D s_puffTexture;
        private AlmaMotor2D _motor;
        private ParticleSystem _dust;
        private Material _material;
        private int _facing;

        private void Awake()
        {
            _motor = GetComponent<AlmaMotor2D>();
            var capsule = GetComponent<CapsuleCollider2D>();
            var sprite = GetComponent<SpriteRenderer>();

            var dustObject = new GameObject("RunDust");
            dustObject.transform.SetParent(transform, false);
            dustObject.transform.localPosition =
                new Vector3(0f, capsule.offset.y - capsule.size.y * 0.5f + 0.05f, 0f);
            _dust = dustObject.AddComponent<ParticleSystem>();
            _dust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = _dust.main;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.6f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(_sizeRange.x, _sizeRange.y);
            main.startColor = _color;
            main.gravityModifier = -0.03f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 20;

            var emission = _dust.emission;
            emission.rateOverTime = 0f;
            emission.rateOverDistance = _puffsPerMeter;
            emission.enabled = false;

            var shape = _dust.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.12f;

            // Drift backwards and slightly up; x is flipped with Alma's facing in LateUpdate.
            var velocity = _dust.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.6f);
            velocity.y = new ParticleSystem.MinMaxCurve(0.35f);
            velocity.z = new ParticleSystem.MinMaxCurve(0f);
            _facing = 0; // Forces LateUpdate to place the emitter and set the drift on the first frame.

            var size = _dust.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.6f, 1f, 1.3f));

            var fade = new Gradient();
            fade.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            var color = _dust.colorOverLifetime;
            color.enabled = true;
            color.color = fade;

            // Reuses the sprite shader Alma already renders with, so nothing extra ships in the build.
            _material = new Material(sprite.sharedMaterial) { mainTexture = PuffTexture() };
            var particleRenderer = dustObject.GetComponent<ParticleSystemRenderer>();
            particleRenderer.sharedMaterial = _material;
            particleRenderer.sortingLayerID = sprite.sortingLayerID;
            particleRenderer.sortingOrder = sprite.sortingOrder - 1;

            _dust.Play();
        }

        private void LateUpdate()
        {
            bool running = _motor.IsGrounded
                && Mathf.Abs(_motor.Velocity.x) >= _motor.Settings.MoveSpeed * _minSpeedRatio;
            var emission = _dust.emission;
            if (emission.enabled != running) emission.enabled = running;

            if (_facing != _motor.FacingDirection)
            {
                _facing = _motor.FacingDirection;
                var velocity = _dust.velocityOverLifetime;
                velocity.x = new ParticleSystem.MinMaxCurve(-0.6f * _facing);
                Vector3 position = _dust.transform.localPosition;
                position.x = -_backOffset * _facing;
                _dust.transform.localPosition = position;
            }
        }

        private void OnDestroy()
        {
            if (_material != null) Destroy(_material);
        }

        // Soft white circle generated once and shared by every instance.
        private static Texture2D PuffTexture()
        {
            if (s_puffTexture != null) return s_puffTexture;
            const int size = 32;
            s_puffTexture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "AlmaRunDustPuff",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            var pixels = new Color32[size * size];
            float radius = size * 0.5f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(radius, radius)) / radius;
                byte alpha = (byte)(Mathf.Clamp01(1f - distance * distance) * 255f);
                pixels[y * size + x] = new Color32(255, 255, 255, alpha);
            }
            s_puffTexture.SetPixels32(pixels);
            s_puffTexture.Apply(false, true);
            return s_puffTexture;
        }
    }
}
