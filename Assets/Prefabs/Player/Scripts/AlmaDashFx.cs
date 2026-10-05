using UnityEngine;

namespace AlmaGame.Player
{
    // Dash feedback: tinted afterimages of Alma left along the path plus thin wind lines streaming
    // behind her. Afterimages are a small pool of sprites reused every dash; the wind lines are one
    // particle system (max 8) with a generated texture. Nothing renders while Alma isn't dashing.
    [DisallowMultipleComponent, RequireComponent(typeof(AlmaMotor2D), typeof(SpriteRenderer))]
    public sealed class AlmaDashFx : MonoBehaviour
    {
        [Header("Afterimages")]
        [SerializeField, Range(1, 8)] private int _ghostCount = 5;
        [SerializeField, Min(0.02f)] private float _ghostInterval = 0.09f;
        [SerializeField, Min(0.05f)] private float _ghostFade = 0.25f;
        [SerializeField] private Color _ghostColor = new Color(0.75f, 0.9f, 1f, 0.55f);

        [Header("Wind lines")]
        [SerializeField, Min(0f)] private float _linesPerSecond = 16f;
        [SerializeField] private Vector2 _lineLength = new Vector2(0.8f, 1.5f);
        [SerializeField] private Color _lineColor = new Color(0.92f, 0.97f, 1f, 0.7f);
        [SerializeField, Min(0f)] private float _lineBackOffset = 0.6f;

        private static Texture2D s_lineTexture;
        private AlmaMotor2D _motor;
        private SpriteRenderer _sprite;
        private SpriteRenderer[] _ghosts;
        private float[] _ghostStartedAt;
        private int _nextGhost;
        private float _nextGhostAt;
        private bool _wasDashing;
        private ParticleSystem _lines;
        private Material _lineMaterial;
        private int _facing;

        private void Awake()
        {
            _motor = GetComponent<AlmaMotor2D>();
            _sprite = GetComponent<SpriteRenderer>();

            // Afterimages live in world space so they stay where Alma left them.
            Transform fxRoot = AlmaFxRoot.For(gameObject);
            _ghosts = new SpriteRenderer[_ghostCount];
            _ghostStartedAt = new float[_ghostCount];
            for (int i = 0; i < _ghostCount; i++)
            {
                var ghost = new GameObject("DashGhost").AddComponent<SpriteRenderer>();
                ghost.transform.SetParent(fxRoot, false);
                ghost.sharedMaterial = _sprite.sharedMaterial;
                ghost.sortingLayerID = _sprite.sortingLayerID;
                ghost.sortingOrder = _sprite.sortingOrder - 1;
                ghost.enabled = false;
                _ghosts[i] = ghost;
            }

            var linesObject = new GameObject("DashWindLines");
            linesObject.transform.SetParent(transform, false);
            _lines = linesObject.AddComponent<ParticleSystem>();
            _lines.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = _lines.main;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.18f, 0.28f);
            main.startSpeed = 0f;
            main.startSize3D = true;
            main.startSizeX = new ParticleSystem.MinMaxCurve(_lineLength.x, _lineLength.y);
            main.startSizeY = new ParticleSystem.MinMaxCurve(0.04f, 0.08f);
            main.startSizeZ = 1f;
            main.startColor = _lineColor;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 8;

            var emission = _lines.emission;
            emission.rateOverTime = _linesPerSecond;
            emission.enabled = false;

            // Spawn along Alma's body height, slightly behind her centre.
            var shape = _lines.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(0.2f, 1.6f, 0f);

            var velocity = _lines.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(-1.5f);
            velocity.y = new ParticleSystem.MinMaxCurve(0f);
            velocity.z = new ParticleSystem.MinMaxCurve(0f);
            _facing = 0; // Forces LateUpdate to set direction and offset on the first frame.

            var fade = new Gradient();
            fade.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            var color = _lines.colorOverLifetime;
            color.enabled = true;
            color.color = fade;

            _lineMaterial = new Material(_sprite.sharedMaterial) { mainTexture = LineTexture() };
            var particleRenderer = linesObject.GetComponent<ParticleSystemRenderer>();
            particleRenderer.sharedMaterial = _lineMaterial;
            particleRenderer.sortingLayerID = _sprite.sortingLayerID;
            particleRenderer.sortingOrder = _sprite.sortingOrder - 2;

            _lines.Play();
        }

        private void LateUpdate()
        {
            bool dashing = _motor.IsDashing;
            if (dashing && (!_wasDashing || Time.time >= _nextGhostAt))
            {
                SpawnGhost();
                _nextGhostAt = Time.time + _ghostInterval;
            }
            _wasDashing = dashing;

            var emission = _lines.emission;
            if (emission.enabled != dashing) emission.enabled = dashing;

            if (_facing != _motor.FacingDirection)
            {
                _facing = _motor.FacingDirection;
                var velocity = _lines.velocityOverLifetime;
                velocity.x = new ParticleSystem.MinMaxCurve(-1.5f * _facing);
                _lines.transform.localPosition = new Vector3(-_lineBackOffset * _facing, 0f, 0f);
            }

            for (int i = 0; i < _ghosts.Length; i++)
            {
                if (!_ghosts[i].enabled) continue;
                float t = (Time.time - _ghostStartedAt[i]) / _ghostFade;
                if (t >= 1f)
                {
                    _ghosts[i].enabled = false;
                    continue;
                }
                Color ghostColor = _ghostColor;
                ghostColor.a *= 1f - t;
                _ghosts[i].color = ghostColor;
            }
        }

        // Copies Alma's current frame and facing; the oldest ghost is reused when all are visible.
        private void SpawnGhost()
        {
            SpriteRenderer ghost = _ghosts[_nextGhost];
            _ghostStartedAt[_nextGhost] = Time.time;
            _nextGhost = (_nextGhost + 1) % _ghosts.Length;
            ghost.sprite = _sprite.sprite;
            ghost.flipX = _sprite.flipX;
            ghost.transform.SetPositionAndRotation(transform.position, transform.rotation);
            ghost.transform.localScale = transform.lossyScale;
            ghost.color = _ghostColor;
            ghost.enabled = true;
        }

        private void OnDestroy()
        {
            if (_ghosts != null)
                foreach (var ghost in _ghosts)
                    if (ghost != null) Destroy(ghost.gameObject);
            if (_lineMaterial != null) Destroy(_lineMaterial);
        }

        // Thin horizontal streak: soft top/bottom edges, fading towards both ends. Generated once.
        private static Texture2D LineTexture()
        {
            if (s_lineTexture != null) return s_lineTexture;
            const int width = 64;
            const int height = 8;
            s_lineTexture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = "AlmaDashWindLine",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            var pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                float across = 1f - Mathf.Abs((y + 0.5f) / height * 2f - 1f);
                float along = Mathf.Sin((x + 0.5f) / width * Mathf.PI);
                pixels[y * width + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(across * along) * 255f));
            }
            s_lineTexture.SetPixels32(pixels);
            s_lineTexture.Apply(false, true);
            return s_lineTexture;
        }
    }
}
