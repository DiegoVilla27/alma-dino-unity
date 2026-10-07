using System.Collections.Generic;
using AlmaGame.Player;
using UnityEngine;

namespace AlmaGame.Level
{
    // Solid gate that opens while ALL its linked rune switches are active at the same time. It slides up
    // to open (collider off) and slides back down when any rune runs out — but never while Alma is in the
    // doorway, so it can't crush her. Runes are linked in the Inspector; if none are, it links every rune
    // within `Auto Link Radius`. Gizmo lines show the links.
    [DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public sealed class TimedRuneGate2D : MonoBehaviour
    {
        [SerializeField] private Vector2 _size = new Vector2(1f, 4f);
        [SerializeField] private RuneSwitch2D[] _runes = new RuneSwitch2D[0];
        [SerializeField, Min(0f)] private float _autoLinkRadius = 20f;
        [SerializeField, Min(0.05f)] private float _slideTime = 0.25f;
        [SerializeField] private string _label = "Compuerta rúnica";
        [SerializeField] private bool _showLabel = true;

        private readonly Collider2D[] _overlaps = new Collider2D[8];
        private SpriteRenderer _renderer;
        private BoxCollider2D _collider;
        private Vector3 _closedPosition;
        private bool _open;
        private float _openAmount;

        public bool IsOpen => _open;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<BoxCollider2D>();
            LevelPieceUtility.ApplySize(_renderer, _collider, _size);
            _collider.isTrigger = false;
            _closedPosition = transform.position;
            if (_showLabel)
                HazardZone2D.CreatePlaceholderLabel(transform, _label, new Vector3(0f, _size.y * 0.5f + 0.3f, 0f), _renderer);
        }

        private void Start()
        {
            if (_runes != null && _runes.Length > 0) return;
            var found = new List<RuneSwitch2D>();
            foreach (var rune in FindObjectsByType<RuneSwitch2D>(FindObjectsSortMode.None))
                if (Vector2.Distance(rune.transform.position, transform.position) <= _autoLinkRadius) found.Add(rune);
            _runes = found.ToArray();
            if (_runes.Length == 0) Debug.LogWarning("Rune gate has no rune switches linked or nearby.", this);
        }

        private void OnValidate() => HazardFx.DeferInEditor(this,
            () => LevelPieceUtility.ApplySize(GetComponent<SpriteRenderer>(), GetComponent<BoxCollider2D>(), _size));

        private void Update()
        {
            bool allActive = _runes.Length > 0;
            foreach (var rune in _runes)
                if (rune == null || !rune.IsActive) allActive = false;

            if (allActive && !_open)
            {
                _open = true;
                _collider.enabled = false;
            }
            else if (!allActive && _open && !AlmaInDoorway())
            {
                _open = false;
            }

            // Slide: up by its own height when open; collider back on only once fully closed.
            _openAmount = Mathf.MoveTowards(_openAmount, _open ? 1f : 0f, Time.deltaTime / _slideTime);
            transform.position = _closedPosition + Vector3.up * (_size.y * _openAmount);
            if (!_open && _openAmount <= 0f) _collider.enabled = true;
        }

        private bool AlmaInDoorway()
        {
            int count = Physics2D.OverlapBox(_closedPosition, _size, 0f, ContactFilter2D.noFilter, _overlaps);
            for (int i = 0; i < count; i++)
                if (LevelPieceUtility.IsAlma(_overlaps[i], out AlmaMotor2D _)) return true;
            return false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.45f, 1f, 0.6f, 0.8f);
            if (_runes != null)
                foreach (var rune in _runes)
                    if (rune != null) Gizmos.DrawLine(transform.position, rune.transform.position);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + new Vector3(0f, _size.y * 0.5f + 0.5f, 0f), _label);
#endif
        }
    }
}
