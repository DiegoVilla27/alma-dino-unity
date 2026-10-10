using AlmaGame.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AlmaGame.Level
{
    // Cartoon iris opening for every playable scene (levels "Level_*" and bosses "Boss_*"): the screen starts
    // black, a small circle pops open on Alma (or on `Focus`, e.g. the boss), holds a moment, then grows until
    // the whole screen is visible. Drawn on top of everything (HUD included). Automatic, nothing to place in
    // the scenes; pairs with the exit portal's fade to black.
    public sealed class LevelFadeIn : MonoBehaviour
    {
        // Timeline (seconds, after the scene runs smoothly): black, pop open, hold on the target, expand.
        public const float Black = 0.3f, Pop = 0.45f, HoldSmall = 0.45f, Expand = 0.8f;
        // Radius of the small circle, as a fraction of the screen height.
        public const float SmallRadius = 0.17f;

        // World point the circle opens on; when not set, Alma.
        public static Vector3? Focus;

        private RectTransform _hole, _top, _bottom, _left, _right;
        private float _start = -1f, _waited;
        private int _smooth;
        private Transform _alma;
        private static LevelFadeIn s_current;
        private static Sprite s_holeSprite;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        // Pressing Play with a level already open: make sure that first scene gets it too.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void FirstScene() => OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);

        private static bool IsPlayable(string scene) => scene.StartsWith("Level_") || scene.StartsWith("Boss_");

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode != LoadSceneMode.Single || !IsPlayable(scene.name)) return;
            if (s_current != null && s_current.gameObject.scene == scene) return;   // already opening this scene
            Focus = null;
            var go = new GameObject("LevelFadeIn");
            SceneManager.MoveGameObjectToScene(go, scene);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32767;
            s_current = go.AddComponent<LevelFadeIn>();
            s_current.Build();
        }

        // A black square with a round transparent hole, plus four black bars that cover the rest of the screen.
        private void Build()
        {
            _hole = Part("Hole", HoleSprite());
            _top = Part("Top", null); _bottom = Part("Bottom", null); _left = Part("Left", null); _right = Part("Right", null);
            Layout(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f), 0f);
        }

        private RectTransform Part(string name, Sprite sprite)
        {
            var img = new GameObject(name).AddComponent<Image>();
            img.transform.SetParent(transform, false);
            img.sprite = sprite; img.color = Color.black; img.raycastTarget = false;
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = Vector2.zero; rt.pivot = new Vector2(0.5f, 0.5f);
            return rt;
        }

        // Places the hole (centre and radius in screen pixels) and the bars around it.
        private void Layout(Vector2 c, float r)
        {
            float w = Screen.width, h = Screen.height, pad = 4f;
            float x0 = c.x - r, x1 = c.x + r, y0 = c.y - r, y1 = c.y + r;
            Rect(_hole, x0, y0, x1, y1);
            Rect(_top, -pad, y1, w + pad, Mathf.Max(y1, h + pad));
            Rect(_bottom, -pad, Mathf.Min(y0, -pad), w + pad, y0);
            Rect(_left, Mathf.Min(x0, -pad), y0, x0, y1);
            Rect(_right, x1, y0, Mathf.Max(x1, w + pad), y1);
            _hole.gameObject.SetActive(r > 0.5f);
        }

        private static void Rect(RectTransform rt, float xMin, float yMin, float xMax, float yMax)
        {
            rt.anchoredPosition = new Vector2((xMin + xMax) * 0.5f, (yMin + yMax) * 0.5f);
            rt.sizeDelta = new Vector2(Mathf.Max(0f, xMax - xMin), Mathf.Max(0f, yMax - yMin));
        }

        private Vector2 Center()
        {
            var cam = Camera.main;
            Vector3 world;
            if (Focus.HasValue) world = Focus.Value;
            else
            {
                if (_alma == null) { var a = FindAnyObjectByType<AlmaMotor2D>(); if (a != null) _alma = a.transform; }
                if (_alma == null || cam == null) return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
                world = _alma.position + Vector3.up * 1.1f;
            }
            Vector3 s = cam.WorldToScreenPoint(world);
            float m = Screen.height * SmallRadius;
            return new Vector2(Mathf.Clamp(s.x, m, Screen.width - m), Mathf.Clamp(s.y, m, Screen.height - m));
        }

        private void Update()
        {
            // The first frames after a load (and after pressing Play in the editor) are long and hitchy: stay
            // black until several frames in a row run smoothly (at most 3 s), then start.
            Vector2 c = Center();
            if (_start < 0f)
            {
                _waited += Time.unscaledDeltaTime;
                _smooth = Time.unscaledDeltaTime < 0.05f ? _smooth + 1 : 0;
                if (_smooth >= 8 || _waited >= 3f) _start = Time.unscaledTime;
                Layout(c, 0f);
                return;
            }
            float t = Time.unscaledTime - _start, small = Screen.height * SmallRadius, r;
            if (t < Black) r = 0f;
            else if (t < Black + Pop)
            {
                // Pops open with a little overshoot.
                float u = (t - Black) / Pop; const float s = 2.2f;
                float e = 1f + (s + 1f) * Mathf.Pow(u - 1f, 3f) + s * Mathf.Pow(u - 1f, 2f);
                r = small * e;
            }
            else if (t < Black + Pop + HoldSmall)
            {
                // Holds on the target, breathing a little.
                float u = (t - Black - Pop) / HoldSmall;
                r = small * (1f + 0.04f * Mathf.Sin(u * Mathf.PI));
            }
            else
            {
                // Grows past the farthest corner.
                float u = Mathf.Clamp01((t - Black - Pop - HoldSmall) / Expand);
                float far = 0f;
                foreach (var k in new[] { Vector2.zero, new Vector2(Screen.width, 0f), new Vector2(0f, Screen.height), new Vector2(Screen.width, Screen.height) })
                    far = Mathf.Max(far, Vector2.Distance(c, k));
                r = Mathf.Lerp(small, far + 8f, u * u * u);
                if (u >= 1f) { Destroy(gameObject); return; }
            }
            Layout(c, r);
        }

        // 512 px black square with an anti-aliased transparent circle touching its edges.
        private static Sprite HoleSprite()
        {
            if (s_holeSprite != null) return s_holeSprite;
            const int n = 512; var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "IrisHole" };
            var px = new Color32[n * n]; float c = (n - 1) * 0.5f, rad = n * 0.5f - 1f;
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
                    byte a = (byte)(Mathf.Clamp01(d - rad + 1f) * 255f);
                    px[y * n + x] = new Color32(0, 0, 0, a);
                }
            tex.SetPixels32(px); tex.Apply();
            s_holeSprite = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100f);
            return s_holeSprite;
        }
    }
}
