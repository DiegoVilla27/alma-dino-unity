using System.Collections;
using AlmaGame.Player;
using AlmaGame.Systems;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AlmaGame.Level
{
    // End of every level. When Alma enters it, her control stops, the portal bursts, the level is marked
    // as completed and the save moves on to the next level (GameProgress.CompleteLevel), then the screen
    // fades to black and `Next Scene` loads. The next scene must be in Build Settings; if it isn't, the
    // progress is still saved, a warning is logged and the screen fades back in with control restored.
    [DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public sealed class LevelExitPortal2D : MonoBehaviour
    {
        [SerializeField] private string _nextScene = string.Empty;
        [SerializeField] private Vector2 _size = new Vector2(1.2f, 2.4f);
        [SerializeField] private Color _swirlColorA = new Color(0.6f, 0.8f, 1f, 0.9f);
        [SerializeField] private Color _swirlColorB = new Color(0.8f, 0.55f, 1f, 0.9f);
        [SerializeField, Min(0.1f)] private float _fadeTime = 0.8f;
        [SerializeField] private string _label = "Portal de salida";
        [SerializeField] private bool _showLabel = true;

        private static Sprite s_pixel;

        private SpriteRenderer _renderer;
        private BoxCollider2D _trigger;
        private SpriteRenderer _core;
        private ParticleSystem _swirl;
        private ParticleSystem _burst;
        private bool _used;

        public string NextScene => _nextScene;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _trigger = GetComponent<BoxCollider2D>();
            ApplyLayout();
            int layer = _renderer.sortingLayerID;
            int order = _renderer.sortingOrder;
            Material material = _renderer.sharedMaterial;

            // Bright core that breathes.
            _core = new GameObject("Core").AddComponent<SpriteRenderer>();
            _core.transform.SetParent(transform, false);
            _core.sprite = HazardFx.Glow();
            _core.color = Color.Lerp(_swirlColorA, Color.white, 0.4f);
            _core.sortingLayerID = layer;
            _core.sortingOrder = order + 1;

            // Swirl: sparks orbiting and drifting inward over the portal's height.
            _swirl = HazardFx.CreateParticles("Swirl", transform, material, HazardFx.Puff(), 40, layer, order + 2, false);
            var main = _swirl.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.2f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
            main.startColor = new ParticleSystem.MinMaxGradient(_swirlColorA, _swirlColorB);
            var shape = _swirl.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = _size.x * 0.5f;
            shape.radiusThickness = 0f;
            shape.scale = new Vector3(1f, _size.y / _size.x, 1f);
            var velocity = _swirl.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.x = new ParticleSystem.MinMaxCurve(0f);
            velocity.y = new ParticleSystem.MinMaxCurve(0f);
            velocity.z = new ParticleSystem.MinMaxCurve(0f);
            velocity.orbitalZ = new ParticleSystem.MinMaxCurve(2.5f);
            velocity.radial = new ParticleSystem.MinMaxCurve(-0.4f);
            var emission = _swirl.emission;
            emission.enabled = true;
            emission.rateOverTime = 30f;

            _burst = HazardFx.CreateParticles("Burst", transform, material, HazardFx.Puff(), 30, layer, order + 3);
            var burstMain = _burst.main;
            burstMain.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.9f);
            burstMain.startSpeed = new ParticleSystem.MinMaxCurve(2f, 4.5f);
            burstMain.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.3f);
            burstMain.startColor = new ParticleSystem.MinMaxGradient(_swirlColorA, Color.white);
            var burstShape = _burst.shape;
            burstShape.shapeType = ParticleSystemShapeType.Circle;
            burstShape.radius = 0.3f;
            HazardFx.SetSizeOverLifetime(_burst, 1f, 0.1f);

            if (_showLabel)
                HazardZone2D.CreatePlaceholderLabel(transform, _label, new Vector3(0f, _size.y * 0.5f + 0.3f, 0f), _renderer);
        }

        private void OnDestroy()
        {
            HazardFx.DestroyMaterial(_swirl);
            HazardFx.DestroyMaterial(_burst);
        }

        private void OnValidate() => HazardFx.DeferInEditor(this, ApplyLayout);

        private void ApplyLayout()
        {
            var renderer = GetComponent<SpriteRenderer>();
            var trigger = GetComponent<BoxCollider2D>();
            if (renderer != null)
            {
                renderer.drawMode = SpriteDrawMode.Tiled;
                renderer.size = _size;
            }
            if (trigger != null)
            {
                trigger.isTrigger = true;
                trigger.size = _size;
                trigger.offset = Vector2.zero;
            }
        }

        private void Update()
        {
            float pulse = 1f + 0.12f * Mathf.Sin(Time.time * 4f);
            _core.transform.localScale = new Vector3(_size.x * 0.8f * pulse, _size.y * 0.7f * pulse, 1f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_used || !LevelPieceUtility.IsAlma(other, out AlmaMotor2D alma) || alma.IsDead) return;
            _used = true;
            StartCoroutine(Exit(alma));
        }

        private IEnumerator Exit(AlmaMotor2D alma)
        {
            var input = alma.GetComponent<AlmaInput>();
            if (input != null) input.enabled = false;
            alma.SetInput(0f, false, false);
            _burst.Emit(26);
            if (GameProgress.Instance != null) GameProgress.Instance.CompleteLevel(_nextScene);
            else Debug.LogWarning("Level completed but not saved: no System_GameProgress in the scene.", this);

            SpriteRenderer fade = CreateFade();
            yield return Fade(fade, 0f, 1f);

            if (!string.IsNullOrEmpty(_nextScene) && Application.CanStreamedLevelBeLoaded(_nextScene))
            {
                SceneManager.LoadScene(_nextScene);
                yield break;
            }

            // Nowhere to go: say why and give control back so the game isn't stuck on black.
            Debug.LogWarning(string.IsNullOrEmpty(_nextScene)
                ? "Exit portal has no Next Scene set."
                : $"Exit portal: scene '{_nextScene}' isn't in Build Settings.", this);
            yield return Fade(fade, 1f, 0f);
            Destroy(fade.gameObject);
            if (input != null) input.enabled = true;
            _used = false;
        }

        private IEnumerator Fade(SpriteRenderer fade, float from, float to)
        {
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / _fadeTime)
            {
                fade.color = new Color(0f, 0f, 0f, Mathf.Lerp(from, to, t));
                yield return null;
            }
            fade.color = new Color(0f, 0f, 0f, to);
        }

        // Black sprite in front of the camera, big enough to cover the whole view.
        private SpriteRenderer CreateFade()
        {
            Camera view = Camera.main;
            var fade = new GameObject("ExitFade").AddComponent<SpriteRenderer>();
            fade.sprite = Pixel();
            fade.color = new Color(0f, 0f, 0f, 0f);
            fade.sortingOrder = 32000;
            if (view != null)
            {
                fade.transform.SetParent(view.transform, false);
                fade.transform.localPosition = new Vector3(0f, 0f, view.nearClipPlane + 0.5f);
                float height = view.orthographic ? view.orthographicSize * 2f : 50f;
                fade.transform.localScale = new Vector3(height * view.aspect * 1.5f, height * 1.5f, 1f);
            }
            else
            {
                fade.transform.position = transform.position;
                fade.transform.localScale = new Vector3(200f, 200f, 1f);
            }
            return fade;
        }

        private static Sprite Pixel()
        {
            if (s_pixel != null) return s_pixel;
            Texture2D white = Texture2D.whiteTexture;
            s_pixel = Sprite.Create(white, new Rect(0f, 0f, white.width, white.height), new Vector2(0.5f, 0.5f), white.width);
            s_pixel.name = "ExitFadePixel";
            return s_pixel;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.7f, 0.6f, 1f, 0.9f);
            Gizmos.DrawWireCube(transform.position, _size);
#if UNITY_EDITOR
            string target = string.IsNullOrEmpty(_nextScene) ? "(sin escena siguiente)" : "→ " + _nextScene;
            UnityEditor.Handles.Label(transform.position + new Vector3(0f, _size.y * 0.5f + 0.5f, 0f), _label + " " + target);
#endif
        }
    }
}
