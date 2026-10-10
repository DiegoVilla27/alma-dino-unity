using AlmaGame.Player;
using AlmaGame.Systems;
using UnityEngine;

namespace AlmaGame.Level
{
    public enum StoryTargetExit { Stay, LeapAway, HideAfterShot }

    // Story zone: when Alma enters it, a camera shot travels to `Shot Target` (e.g. the thief monkey, the altar,
    // a view of the path ahead), holds while the target speaks its lines in a comic bubble (and/or a HUD banner
    // tells the moment), the target can leap out of the shot, and the camera travels back. Alma's controls
    // are locked during the shot (the game keeps running). Once per scene load. Any part can be left out:
    // no target = text only; empty text = shot only.
    [DisallowMultipleComponent, RequireComponent(typeof(BoxCollider2D))]
    public sealed class StoryMoment2D : MonoBehaviour
    {
        [Header("Shot")]
        [SerializeField] private Transform _shotTarget;
        [Tooltip("Shot point relative to this object, used when there is no target.")]
        [SerializeField] private Vector2 _shotOffset = new Vector2(8f, 2f);
        [SerializeField] private bool _useShot = true;
        [SerializeField, Range(3f, 12f)] private float _zoom = 6.5f;
        [SerializeField, Min(0.1f)] private float _travelTime = 1.1f;
        [SerializeField, Min(0f)] private float _holdTime = 2.8f;
        [SerializeField] private bool _lockControls = true;
        [Header("Dialogue (speech bubble over the shot target)")]
        [Tooltip("Lines the target says one after another in a comic bubble above it.")]
        [SerializeField, TextArea(1, 3)] private string[] _lines = new string[0];
        [SerializeField] private string _speaker = "Mono ladrón";
        [SerializeField] private Color _speakerColor = new Color(1f, 0.7f, 0.3f);
        [SerializeField] private Sprite _bubbleSprite;
        [SerializeField] private Sprite _bubbleTailSprite;
        [Header("Target exit")]
        [Tooltip("LeapAway: after talking, the target jumps out of the shot (then is disabled). HideAfterShot: disabled once the camera is back on Alma.")]
        [SerializeField] private StoryTargetExit _targetExit = StoryTargetExit.LeapAway;
        [Tooltip("+1 = escapes to the right, -1 = to the left.")]
        [SerializeField] private float _exitDirection = 1f;
        [Header("Text")]
        [SerializeField] private BannerKind _kind = BannerKind.Line;
        [SerializeField] private string _kicker = "";
        [SerializeField] private string _headline = "";
        [SerializeField, TextArea(1, 4)] private string _text = "";
        [SerializeField] private Color _accent = new Color(1f, 0.75f, 0.3f);
        [SerializeField] private Sprite _icon;
        [Tooltip("Seconds after the shot starts before the text appears.")]
        [SerializeField, Min(0f)] private float _textDelay = 0.6f;

        private bool _done, _running;
        private float _startedAt;
        private bool _textShown;
        private AlmaCameraFollow _camera;
        private SpeechBubble2D _bubble;
        private float _speechStart, _speechEnd, _hold;
        private int _line = -1;
        private float[] _lineEnds;
        private Vector3 _exitFrom;
        private bool _exiting;
        private const float ExitTime = 0.9f;

        private void Reset()
        {
            var box = GetComponent<BoxCollider2D>();
            box.isTrigger = true; box.size = new Vector2(1.5f, 4f); box.offset = new Vector2(0f, 2f);
        }

        private void Awake() => GetComponent<BoxCollider2D>().isTrigger = true;

        private Vector2 Point => _shotTarget != null ? (Vector2)_shotTarget.position : (Vector2)transform.TransformPoint(_shotOffset);

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_done) return;
            var body = other.attachedRigidbody;
            if (body == null || !body.TryGetComponent(out AlmaMotor2D alma) || alma.IsDead) return;
            if (HudTutorial.Instance != null && HudTutorial.Instance.IsShowing) return;
            _done = true; _running = true; _textShown = false;
            _startedAt = Time.unscaledTime;
            _camera = FindAnyObjectByType<AlmaCameraFollow>();
            if (_lockControls && _useShot) { AlmaTouchControls.Clear(); AlmaTouchControls.InputLocked = true; }
            PlanDialogue();
        }

        private float Total => _useShot ? _travelTime * 2f + _hold : 0f;
        private bool HasLines => _lines != null && _lines.Length > 0 && _shotTarget != null;

        // Lines start as the camera arrives; each stays long enough to type and read it. The hold grows to fit
        // the dialogue and the exit leap.
        private void PlanDialogue()
        {
            _hold = _holdTime; _line = -1; _exiting = false;
            if (!HasLines) return;
            if (_bubble == null && HudText.Ready) _bubble = SpeechBubble2D.Create(_shotTarget, _bubbleSprite, _bubbleTailSprite, _speaker, _speakerColor);
            _speechStart = _useShot ? _travelTime * 0.75f : 0.2f;
            _lineEnds = new float[_lines.Length];
            float t = _speechStart;
            for (int i = 0; i < _lines.Length; i++)
            {
                string l = _lines[i] ?? "";
                t += 0.15f + l.Length / 32f + Mathf.Max(1.3f, 0.8f + l.Length * 0.045f);
                _lineEnds[i] = t;
            }
            _speechEnd = t;
            float needed = _speechEnd - _travelTime + (_targetExit == StoryTargetExit.LeapAway ? ExitTime + 0.35f : 0.4f);
            _hold = Mathf.Max(_holdTime, needed);
        }

        private void UpdateDialogue(float t)
        {
            if (!HasLines || _bubble == null) return;
            int want = -1;
            for (int i = 0; i < _lineEnds.Length; i++) if (t >= (i == 0 ? _speechStart : _lineEnds[i - 1]) && t < _lineEnds[i]) want = i;
            if (want != _line)
            {
                _line = want;
                if (want >= 0) _bubble.Say(_lines[want]); else _bubble.Hide();
            }
            if (_targetExit == StoryTargetExit.LeapAway && t >= _speechEnd && _shotTarget.gameObject.activeSelf)
            {
                if (!_exiting) { _exiting = true; _exitFrom = _shotTarget.position; _bubble.Hide(); }
                // Escapes in a high arc out of the shot.
                float u = Mathf.Clamp01((t - _speechEnd) / ExitTime);
                float x = _exitDirection * 7f * u, y = 6f * u - 2.5f * u * u;
                _shotTarget.position = _exitFrom + new Vector3(x, y + 2.8f * Mathf.Sin(u * Mathf.PI) * 0.4f, 0f);
                _shotTarget.localRotation = Quaternion.Euler(0f, 0f, -_exitDirection * 360f * u * u);
                if (u >= 1f) _shotTarget.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (!_running) return;
            float t = Time.unscaledTime - _startedAt;
            if (!_textShown && t >= _textDelay && (!string.IsNullOrEmpty(_text) || !string.IsNullOrEmpty(_headline)))
            {
                _textShown = true;
                HudBanners.Show(_kind, _headline, _text, _accent, _kicker, _icon);
            }
            UpdateDialogue(t);
            if (_useShot && _camera != null)
            {
                // During the leap the shot stays where the target stood, so it visibly escapes the frame.
                Vector2 point = _exiting ? (Vector2)_exitFrom : Point;
                float w = t < _travelTime ? t / _travelTime : t < _travelTime + _hold ? 1f : 1f - (t - _travelTime - _hold) / _travelTime;
                _camera.SetShot(point, _zoom, Mathf.Clamp01(w));
            }
            if (t >= Total && (_textShown || (string.IsNullOrEmpty(_text) && string.IsNullOrEmpty(_headline))))
            {
                _running = false;
                if (_camera != null) _camera.ClearShot();
                if (_lockControls && _useShot) AlmaTouchControls.InputLocked = false;
                if (_bubble != null) _bubble.Hide();
                if (_shotTarget != null && _targetExit == StoryTargetExit.HideAfterShot) _shotTarget.gameObject.SetActive(false);
            }
        }

        private void OnDisable()
        {
            if (!_running) return;
            _running = false;
            if (_camera != null) _camera.ClearShot();
            if (_lockControls && _useShot) AlmaTouchControls.InputLocked = false;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            var box = GetComponent<BoxCollider2D>();
            Gizmos.color = new Color(1f, 0.75f, 0.3f, 0.35f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.offset, box.size);
            Gizmos.matrix = Matrix4x4.identity;
            if (_useShot)
            {
                Vector2 p = Point; float h = _zoom, w = _zoom * 16f / 9f;
                Gizmos.color = new Color(1f, 0.8f, 0.3f, 0.9f);
                Gizmos.DrawWireCube(p, new Vector3(w * 2f, h * 2f, 0f));
                Gizmos.DrawLine(transform.position, p);
            }
            UnityEditor.Handles.Label(transform.position + Vector3.up * 4.3f, "Momento: " + (string.IsNullOrEmpty(_headline) ? _text : _headline));
        }
#endif
    }
}
