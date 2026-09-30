using UnityEngine;

namespace AlmaDino.Features.Environment
{
    /// <summary>
    /// Plataforma de basalto flotante que se hunde lentamente en el magma
    /// cuando el jugador se para sobre ella, y asciende suavemente a su posición
    /// original cuando queda desocupada.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SinkingBasalt2D : MonoBehaviour
    {
        [Header("Sink Dynamics")]
        [Tooltip("Velocidad de descenso en m/s")]
        [SerializeField] private float _sinkSpeed = 1.2f;
        [Tooltip("Profundidad máxima de descenso en metros")]
        [SerializeField] private float _maxSinkDepth = 2.0f;
        [Tooltip("Velocidad de recuperación ascendente en m/s")]
        [SerializeField] private float _riseSpeed = 0.8f;

        private Vector3 _initialPosition;
        private Vector3 _lowestPosition;
        private bool _isSteppedOn;
        private float _lastContactTime;

        public bool IsSteppedOn => _isSteppedOn;
        public float CurrentDepth => _initialPosition.y - transform.position.y;

        private void Awake()
        {
            _initialPosition = transform.position;
            _lowestPosition = _initialPosition - new Vector3(0f, _maxSinkDepth, 0f);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            for (int i = 0; i < collision.contactCount; i++)
            {
                var contact = collision.GetContact(i);
                if (contact.point.y >= transform.position.y - 0.2f && contact.normal.y < -0.3f)
                {
                    _isSteppedOn = true;
                    _lastContactTime = Time.time;
                    return;
                }
            }
        }

        private void Update()
        {
            if (Time.time - _lastContactTime > 0.15f)
            {
                _isSteppedOn = false;
            }

            Vector3 target = _isSteppedOn ? _lowestPosition : _initialPosition;
            float speed = _isSteppedOn ? _sinkSpeed : _riseSpeed;

            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Vector3 start = Application.isPlaying ? _initialPosition : transform.position;
            Vector3 end = start - new Vector3(0f, _maxSinkDepth, 0f);
            Gizmos.DrawLine(start, end);
            Gizmos.DrawWireCube(end, new Vector3(1.5f, 0.2f, 0f));
        }
    }
}
