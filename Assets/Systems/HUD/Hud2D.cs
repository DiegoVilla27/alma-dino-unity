using System.Collections.Generic;
using AlmaGame.Player;
using UnityEngine;

namespace AlmaGame.Systems
{
    // Root of the in-level HUD (only in level and boss scenes). It is drawn with sprites that follow the camera
    // view (without its shake), above everything else, and lays its elements out against the screen corners
    // whatever the device aspect. It fades out while a cinematic plays (CinematicState).
    [DisallowMultipleComponent]
    public sealed class Hud2D : MonoBehaviour
    {
        [Tooltip("Sorting order of the HUD: above the level, the backdrops and the foreground effects.")]
        [SerializeField] private int _sortingOrder = 1000;
        [SerializeField, Min(0.01f)] private float _fadeTime = 0.35f;

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
            transform.position = new Vector3(view.x, view.y, 0f);
            transform.localScale = Vector3.one * Scale;
            float target = CinematicState.IsPlaying ? 0f : 1f;
            _visibility = Mathf.MoveTowards(_visibility, target, Time.unscaledDeltaTime / _fadeTime);
        }

        // Local position (HUD units, camera size 8) of a point at `inset` units from the top-left corner.
        public Vector2 TopLeft(Vector2 inset)
        {
            float halfH = 8f, halfW = 8f * (_camera != null ? _camera.aspect : 16f / 9f);
            return new Vector2(-halfW + inset.x, halfH - inset.y);
        }

        // World position of a HUD-local point, as it is this frame.
        public Vector3 ToWorld(Vector2 local) => transform.TransformPoint(local);
    }
}
