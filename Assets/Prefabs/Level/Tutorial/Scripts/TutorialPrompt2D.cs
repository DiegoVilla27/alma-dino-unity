using AlmaGame.Player;
using AlmaGame.Systems;
using UnityEngine;

namespace AlmaGame.Level
{
    public enum TutorialAction { Jump, DoubleJump, Dash, GroundPound, Roar }

    // Invisible tutorial zone. When Alma is in it (and in the right state for the move — on the ground for a
    // jump, in the air for a double jump or a ground pound) the HUD pauses the game in slow motion, frames the
    // obstacle, draws the move's arc to `Landing`, lights up the button to press and shows the title and hint
    // (HudTutorial). Pressing that button resumes the game and the move happens at once. Shown once per scene
    // load (not again after dying). Move the box over the edge before the gap; drag the landing gizmo.
    [DisallowMultipleComponent, RequireComponent(typeof(BoxCollider2D))]
    public sealed class TutorialPrompt2D : MonoBehaviour
    {
        [Tooltip("Jump: on the ground. DoubleJump: in the air (pauses near the top of the jump). GroundPound: in the air.")]
        [SerializeField] private TutorialAction _action = TutorialAction.Jump;
        [SerializeField] private string _title = "¡Salta!";
        [SerializeField, TextArea(1, 3)] private string _text = "Toca el botón SALTAR para cruzar al otro lado.";
        [SerializeField] private Color _accent = new Color(0.55f, 1f, 0.4f);
        [Header("Shot")]
        [Tooltip("Where the move should land, relative to this object (draw it on the far side of the gap).")]
        [SerializeField] private Vector2 _landing = new Vector2(4.5f, 0f);
        [SerializeField, Min(0.3f)] private float _arcHeight = 1.5f;
        [SerializeField] private bool _showArc = true;
        [Tooltip("Orthographic size of the close-up (the game uses 8).")]
        [SerializeField, Range(3f, 8f)] private float _zoom = 6f;
        [Tooltip("Camera framing, relative to the midpoint between Alma and the landing.")]
        [SerializeField] private Vector2 _focusOffset = new Vector2(0f, 1.2f);
        [Tooltip("After the press, keep Alma moving towards the landing until she lands (useful on touch).")]
        [SerializeField] private bool _assistMove = true;
        [SerializeField] private bool _onlyOnce = true;

        private bool _done;

        private void Reset()
        {
            var box = GetComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(1.2f, 3f);
            box.offset = new Vector2(0f, 1.5f);
        }

        private void Awake() => GetComponent<BoxCollider2D>().isTrigger = true;

        private AlmaAbility Button
        {
            get
            {
                switch (_action)
                {
                    case TutorialAction.Dash: return AlmaAbility.Dash;
                    case TutorialAction.GroundPound: return AlmaAbility.GroundPound;
                    case TutorialAction.Roar: return AlmaAbility.Roar;
                    default: return AlmaAbility.DoubleJump;   // the Jump button
                }
            }
        }

        private void OnTriggerStay2D(Collider2D other) => Try(other);
        private void OnTriggerEnter2D(Collider2D other) => Try(other);

        private void Try(Collider2D other)
        {
            if (_done && _onlyOnce) return;
            var body = other.attachedRigidbody;
            if (body == null || !body.TryGetComponent(out AlmaMotor2D alma) || alma.IsDead) return;
            var tutorial = HudTutorial.Instance;
            if (tutorial == null || tutorial.IsShowing) return;
            if (!StateMatches(alma)) return;
            Vector3 from = alma.transform.position;
            Vector3 to = transform.TransformPoint(_landing);
            Vector2 mid = (Vector2)(from + to) * 0.5f + _focusOffset;
            bool shown = tutorial.Show(new TutorialRequest
            {
                Action = Button, Title = _title, Text = _text, Accent = _accent,
                ArcFrom = from, ArcTo = to, ArcHeight = _arcHeight, ShowArc = _showArc,
                Focus = mid, Zoom = _zoom, AssistMove = _assistMove,
            });
            if (shown) _done = true;
        }

        private bool StateMatches(AlmaMotor2D alma)
        {
            if (_action == TutorialAction.Roar) return true;
            if (_action == TutorialAction.DoubleJump && !alma.IsUnlocked(AlmaAbility.DoubleJump)) return false;
            bool air = _action == TutorialAction.DoubleJump || _action == TutorialAction.GroundPound || _action == TutorialAction.Dash;
            if (air) return !alma.IsGrounded && alma.Velocity.y < 1.5f;
            return alma.IsGrounded;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            var box = GetComponent<BoxCollider2D>();
            Gizmos.color = new Color(0.4f, 1f, 0.5f, 0.35f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.offset, box.size);
            Gizmos.matrix = Matrix4x4.identity;
            Vector3 a = transform.position, b = transform.TransformPoint(_landing);
            float apex = Mathf.Max(a.y, b.y) + _arcHeight;
            Vector3 mid = new Vector3((a.x + b.x) * 0.5f, 2f * apex - (a.y + b.y) * 0.5f, 0f);
            Gizmos.color = new Color(1f, 1f, 1f, 0.9f);
            Vector3 prev = a;
            for (int i = 1; i <= 20; i++)
            {
                float u = i / 20f; Vector3 p = (1 - u) * (1 - u) * a + 2 * (1 - u) * u * mid + u * u * b;
                Gizmos.DrawLine(prev, p); prev = p;
            }
            Gizmos.DrawWireSphere(b, 0.3f);
            UnityEditor.Handles.Label(transform.position + Vector3.up * 3.3f, "Tutorial: " + _title);
        }
#endif
    }
}
