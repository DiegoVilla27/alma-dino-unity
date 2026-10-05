using UnityEngine;

namespace AlmaGame.Player
{
    // Double jump feedback: a thin flat air ring where Alma "steps" on the air, a few air puffs
    // pushed down and sideways, and a quick stretch of her sprite. One ring sprite and one particle
    // system (max 8) using the shared generated textures; nothing renders between double jumps.
    [DisallowMultipleComponent, RequireComponent(typeof(AlmaMotor2D), typeof(CapsuleCollider2D), typeof(SpriteRenderer))]
    public sealed class AlmaDoubleJumpFx : MonoBehaviour
    {
        [Header("Air ring")]
        [SerializeField, Min(0.05f)] private float _ringDuration = 0.25f;
        [SerializeField, Min(0.1f)] private float _ringWidth = 1.8f;
        [SerializeField, Range(0.1f, 1f)] private float _ringFlatten = 0.35f;
        [SerializeField] private Color _ringColor = new Color(0.85f, 0.95f, 1f, 0.85f);

        [Header("Air puffs")]
        [SerializeField, Range(0, 8)] private int _puffs = 5;
        [SerializeField] private Color _puffColor = new Color(0.92f, 0.97f, 1f, 0.75f);

        [Header("Stretch")]
        [SerializeField] private Vector2 _stretch = new Vector2(0.9f, 1.1f);
        [SerializeField, Min(0.01f)] private float _stretchTime = 0.12f;

        private AlmaMotor2D _motor;
        private SpriteRenderer _ring;
        private ParticleSystem _airPuffs;
        private Material _puffMaterial;
        private Vector3 _feetOffset;
        private Vector3 _baseScale;
        private float _ringStartedAt;
        private bool _ringActive;
        private float _stretchStartedAt;
        private bool _stretching;

        private void Awake()
        {
            _motor = GetComponent<AlmaMotor2D>();
            var capsule = GetComponent<CapsuleCollider2D>();
            var sprite = GetComponent<SpriteRenderer>();
            _feetOffset = new Vector3(0f, capsule.offset.y - capsule.size.y * 0.5f + 0.05f, 0f);
            _baseScale = transform.localScale;

            // The ring stays in the air where the jump happened.
            _ring = new GameObject("DoubleJumpRing").AddComponent<SpriteRenderer>();
            _ring.sprite = AlmaGroundPoundFx.RingSprite();
            _ring.sortingLayerID = sprite.sortingLayerID;
            _ring.sortingOrder = sprite.sortingOrder + 1;
            _ring.enabled = false;

            var puffObject = new GameObject("DoubleJumpPuffs");
            puffObject.transform.SetParent(transform, false);
            puffObject.transform.localPosition = _feetOffset;
            _airPuffs = puffObject.AddComponent<ParticleSystem>();
            _airPuffs.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = _airPuffs.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.4f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.35f);
            main.startColor = _puffColor;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 8;

            var emission = _airPuffs.emission;
            emission.enabled = false; // Only manual Emit() on each double jump.

            // Lower half-circle: puffs push down and sideways, away from the jump.
            var shape = _airPuffs.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.15f;
            shape.arc = 180f;
            shape.rotation = new Vector3(0f, 0f, 180f);

            var size = _airPuffs.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 1.3f));

            var fade = new Gradient();
            fade.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            var color = _airPuffs.colorOverLifetime;
            color.enabled = true;
            color.color = fade;

            _puffMaterial = new Material(sprite.sharedMaterial) { mainTexture = AlmaRunDust.PuffTexture() };
            var particleRenderer = puffObject.GetComponent<ParticleSystemRenderer>();
            particleRenderer.sharedMaterial = _puffMaterial;
            particleRenderer.sortingLayerID = sprite.sortingLayerID;
            particleRenderer.sortingOrder = sprite.sortingOrder + 1;
        }

        private void OnEnable() => _motor.DoubleJumped += PlayEffect;

        private void OnDisable()
        {
            _motor.DoubleJumped -= PlayEffect;
            if (_stretching) EndStretch();
        }

        private void PlayEffect()
        {
            _ring.transform.position = transform.position + _feetOffset;
            _ringStartedAt = Time.time;
            _ringActive = true;
            _ring.enabled = true;
            UpdateRing(0f);

            if (_puffs > 0) _airPuffs.Emit(_puffs);

            _stretchStartedAt = Time.time;
            _stretching = true;
        }

        private void Update()
        {
            if (_ringActive)
            {
                float t = (Time.time - _ringStartedAt) / _ringDuration;
                if (t >= 1f)
                {
                    _ringActive = false;
                    _ring.enabled = false;
                }
                else UpdateRing(t);
            }

            if (!_stretching) return;
            // The death sequence owns Alma's scale while she is dead.
            if (_motor.IsDead)
            {
                _stretching = false;
                return;
            }
            float s = (Time.time - _stretchStartedAt) / _stretchTime;
            if (s >= 1f)
            {
                EndStretch();
                return;
            }
            // Snap to the stretch, then ease back to normal.
            float back = s * s;
            transform.localScale = Vector3.Scale(_baseScale,
                new Vector3(Mathf.Lerp(_stretch.x, 1f, back), Mathf.Lerp(_stretch.y, 1f, back), 1f));
        }

        private void EndStretch()
        {
            _stretching = false;
            transform.localScale = _baseScale;
        }

        // Ease-out expansion of a flat ellipse while fading out.
        private void UpdateRing(float t)
        {
            float eased = 1f - (1f - t) * (1f - t);
            float width = Mathf.Lerp(0.4f, _ringWidth, eased);
            _ring.transform.localScale = new Vector3(width, width * _ringFlatten, 1f);
            Color ringColor = _ringColor;
            ringColor.a *= 1f - t;
            _ring.color = ringColor;
        }

        private void OnDestroy()
        {
            if (_ring != null) Destroy(_ring.gameObject);
            if (_puffMaterial != null) Destroy(_puffMaterial);
        }
    }
}
