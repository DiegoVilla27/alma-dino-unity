using System.Collections.Generic;
using AlmaGame.Player;
using UnityEngine;

namespace AlmaGame.Systems
{
    // Root of the in-level HUD (only in level and boss scenes). It is drawn with sprites and TextMeshPro text
    // that follow the camera view (without its shake), above everything else, and is responsive: elements are
    // anchored to the device's safe area and scaled with UiScale (see "responsive layout"). It fades out while a cinematic plays (CinematicState).
    [DisallowMultipleComponent]
    public sealed class Hud2D : MonoBehaviour
    {
        [Tooltip("Sorting order of the HUD: above the level, the backdrops and the foreground effects.")]
        [SerializeField] private int _sortingOrder = 1000;
        [SerializeField, Min(0.01f)] private float _fadeTime = 0.35f;
        [Tooltip("Smallest real height (mm) of body-text capitals on devices that report their DPI; all HUD text scales from it.")]
        [SerializeField, Min(0f)] private float _minTextMillimetres = 2.4f;

        public static Hud2D Instance { get; private set; }

        private Camera _camera;
        private AlmaCameraFollow _follow;
        private readonly List<SpriteRenderer> _renderers = new List<SpriteRenderer>();
        private readonly List<float> _baseAlpha = new List<float>();
        private float _visibility = 1f;

        public int SortingOrder => _sortingOrder;
        public Camera Camera => _camera;
        // 0..1 multiplier the elements apply to their own alpha (cinematic fade).
        public float Visibility => _visibility;
        // World units per HUD unit: the HUD is designed for a camera of size 8 and scales with it.
        public float Scale => _camera != null ? _camera.orthographicSize / 8f : 1f;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            CinematicState.Reset();
            FindCamera();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void FindCamera()
        {
            _camera = Camera.main;
            _follow = _camera != null ? _camera.GetComponent<AlmaCameraFollow>() : null;
        }

        private void LateUpdate()
        {
            if (_camera == null) FindCamera();
            if (_camera == null) return;
            Vector3 view = _follow != null ? _follow.ViewPosition : _camera.transform.position;
            // Just in front of the camera's near plane, so nothing in the level (sprites or 3D meshes) covers it.
            transform.position = new Vector3(view.x, view.y, view.z + _camera.nearClipPlane + 0.5f);
            transform.localScale = Vector3.one * Scale;
            float target = CinematicState.IsPlaying ? 0f : 1f;
            _visibility = Mathf.MoveTowards(_visibility, target, Time.unscaledDeltaTime / _fadeTime);
        }

        // ---------- responsive layout ----------
        // The HUD is designed for a 16:9 landscape screen 16 HUD units tall (camera size 8). On any device it
        // lays out inside the screen's safe area (notches, rounded corners) and shrinks uniformly (UiScale) when
        // the safe area is narrower or shorter than that design, so nothing ever leaves the screen.

        public const float DesignHeight = 16f, DesignWidth = 16f * 16f / 9f;

        // Normalized (0..1) safe area to use instead of the device's one (tests, previews).
        public static Rect? SafeAreaOverride { get; set; }
        // Pixels per inch of the camera's image, instead of Screen.dpi (tests that render to a texture).
        public static float? DpiOverride { get; set; }

        // Cap height of body text in design units (HudText sizes are relative to it).
        public const float BodyCap = 0.5f;

        // Scale for texts and text-led elements (banners, ability presentation): UiScale, but never so small that body
        // text falls under _minTextMillimetres on the real screen, and never so big that a banner can't fit.
        public float TextScale
        {
            get
            {
                Rect safe = Safe;
                float max = Mathf.Min(1.8f, safe.width / 13f, safe.height / 9f);
                return Mathf.Clamp(Mathf.Max(UiScale, ScaleForPhysicalSize(BodyCap, _minTextMillimetres)), 0.3f, Mathf.Max(0.3f, max));
            }
        }

        // Safe area in HUD-local units.
        public Rect Safe
        {
            get
            {
                float halfH = 8f, halfW = 8f * Aspect;
                Rect n = NormalizedSafeArea();
                return Rect.MinMaxRect(-halfW + n.xMin * 2f * halfW, -halfH + n.yMin * 2f * halfH,
                                       -halfW + n.xMax * 2f * halfW, -halfH + n.yMax * 2f * halfH);
            }
        }

        // Uniform scale for HUD elements: 1 on a 16:9 (or wider) screen, smaller on narrow / short safe areas.
        public float UiScale
        {
            get
            {
                Rect safe = Safe;
                return Mathf.Clamp(Mathf.Min(safe.width / DesignWidth, safe.height / DesignHeight), 0.4f, 1f);
            }
        }

        // Smallest scale that makes `sizeUnits` (HUD units, before scaling) at least `millimetres` on the real screen.
        // 0 when the device doesn't report its DPI (then only UiScale applies).
        public float ScaleForPhysicalSize(float sizeUnits, float millimetres)
        {
            float dpi = DpiOverride ?? Screen.dpi;
            if (dpi <= 0f || _camera == null || sizeUnits <= 0f) return 0f;
            float pixelsPerUnit = _camera.pixelHeight / DesignHeight;
            float pixels = millimetres / 25.4f * dpi;
            return pixels / (sizeUnits * pixelsPerUnit);
        }

        // Point of the safe area at `anchor` (0..1 on each axis) moved by `offset` design units (scaled by UiScale).
        public Vector2 Anchor(Vector2 anchor, Vector2 offset)
        {
            Rect safe = Safe;
            return new Vector2(safe.xMin + anchor.x * safe.width, safe.yMin + anchor.y * safe.height) + offset * UiScale;
        }

        // Point at `inset` design units from the top-left corner of the safe area.
        public Vector2 TopLeft(Vector2 inset) => Anchor(new Vector2(0f, 1f), new Vector2(inset.x, -inset.y));

        // Point at `inset` design units from the bottom-right corner of the safe area.
        public Vector2 BottomRight(Vector2 inset) => Anchor(new Vector2(1f, 0f), new Vector2(-inset.x, inset.y));

        private float Aspect => _camera != null ? _camera.aspect : 16f / 9f;

        private Rect NormalizedSafeArea()
        {
            if (SafeAreaOverride.HasValue) return SafeAreaOverride.Value;
            if (Screen.width <= 0 || Screen.height <= 0) return new Rect(0f, 0f, 1f, 1f);
            Rect a = Screen.safeArea;
            return Rect.MinMaxRect(Mathf.Clamp01(a.xMin / Screen.width), Mathf.Clamp01(a.yMin / Screen.height),
                                   Mathf.Clamp01(a.xMax / Screen.width), Mathf.Clamp01(a.yMax / Screen.height));
        }

        // HUD-local position under a screen point (touch, mouse).
        public Vector2 ScreenToLocal(Vector2 screen)
        {
            if (_camera == null) return Vector2.zero;
            Vector3 world = _camera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -_camera.transform.position.z));
            return transform.InverseTransformPoint(world);
        }

        // World position of a HUD-local point, as it is this frame.
        public Vector3 ToWorld(Vector2 local) => transform.TransformPoint(local);
    }
}
