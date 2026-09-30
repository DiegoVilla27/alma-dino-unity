using AlmaDino.Core.Interfaces;
using UnityEngine;

namespace AlmaDino.Features.Boss.Projectiles
{
    public enum FruitTrajectoryMode
    {
        Rolling,
        LobbedBouncing
    }

    [RequireComponent(typeof(Collider2D))]
    public class RollingFruitProjectile2D : MonoBehaviour, IHazard2D
    {
        [Header("Projectile Settings")]
        [SerializeField] private FruitTrajectoryMode _trajectoryMode = FruitTrajectoryMode.Rolling;
        [SerializeField] private float _speed = 6.5f;
        [SerializeField] private float _direction = 1f;
        [SerializeField] private float _lifeTime = 8.0f;
        [SerializeField] private float _rotationSpeed = 420f;
        [SerializeField] private int _maxBounces = 2;
        [SerializeField] private float _bounceVelocity = 6.0f;

        private Rigidbody2D _rb;
        private float _age = 0f;
        private Transform _visualTransform;
        private int _currentBounces = 0;
        private bool _hasHitPlayer = false;

        public FruitTrajectoryMode TrajectoryMode => _trajectoryMode;
        public float Speed => _speed;
        public float Direction => _direction;
        public int CurrentBounces => _currentBounces;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();

            var spriteTransform = transform.Find("Visual");
            _visualTransform = spriteTransform != null ? spriteTransform : transform;
        }

        private void Start()
        {
            IgnorePlayerSolidCollisions();
        }

        public void IgnorePlayerSolidCollisions()
        {
            var playerRespawnable = FindAnyObjectByType<MonoBehaviour>() as IPlayerRespawnable;
            if (playerRespawnable is MonoBehaviour mb)
            {
                var playerColliders = mb.GetComponentsInChildren<Collider2D>();
                var myColliders = GetComponentsInChildren<Collider2D>();
                foreach (var myCol in myColliders)
                {
                    if (!myCol.isTrigger)
                    {
                        foreach (var pCol in playerColliders)
                        {
                            if (!pCol.isTrigger)
                            {
                                Physics2D.IgnoreCollision(myCol, pCol, true);
                            }
                        }
                    }
                }
            }
        }

        public void Initialize(float direction, float speed)
        {
            InitializeRolling(direction, speed);
        }

        public void InitializeRolling(float direction, float speed)
        {
            _trajectoryMode = FruitTrajectoryMode.Rolling;
            _direction = Mathf.Sign(direction);
            _speed = speed;
            _currentBounces = 0;

            if (_rb != null)
            {
                _rb.linearVelocity = new Vector2(_direction * _speed, -1.5f);
            }

            IgnorePlayerSolidCollisions();
        }

        public void InitializeLobbed(float direction, float horizontalSpeed, float verticalImpulse, int maxBounces = 2, float bounceVelocity = 6.0f)
        {
            _trajectoryMode = FruitTrajectoryMode.LobbedBouncing;
            _direction = Mathf.Sign(direction);
            _speed = horizontalSpeed;
            _maxBounces = maxBounces;
            _bounceVelocity = bounceVelocity;
            _currentBounces = 0;

            if (_rb != null)
            {
                _rb.linearVelocity = new Vector2(_direction * _speed, verticalImpulse);
            }

            IgnorePlayerSolidCollisions();
        }

        private void FixedUpdate()
        {
            if (_rb != null)
            {
                if (_trajectoryMode == FruitTrajectoryMode.Rolling)
                {
                    _rb.linearVelocity = new Vector2(_direction * _speed, _rb.linearVelocity.y);
                }
                else
                {
                    _rb.linearVelocity = new Vector2(_direction * _speed, _rb.linearVelocity.y);
                }
            }
            else
            {
                transform.position += new Vector3(_direction * _speed * Time.fixedDeltaTime, 0f, 0f);
            }

            if (_visualTransform != null)
            {
                _visualTransform.Rotate(0f, 0f, -_direction * _rotationSpeed * Time.fixedDeltaTime);
            }
        }

        private void Update()
        {
            _age += Time.deltaTime;
            if (_age >= _lifeTime || transform.position.y < -8.0f)
            {
                Destroy(gameObject);
            }
        }

        public void OnHazardTouch()
        {
            Debug.Log("[RollingFruitProjectile2D] Fruto espinoso impactó contra Alma.");
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_hasHitPlayer) return;

            if (collision.collider.TryGetComponent<IPlayerRespawnable>(out var respawnable) ||
                collision.gameObject.TryGetComponent<IPlayerRespawnable>(out respawnable) ||
                collision.collider.GetComponentInParent<IPlayerRespawnable>() != null)
            {
                if (respawnable == null) respawnable = collision.collider.GetComponentInParent<IPlayerRespawnable>();
                _hasHitPlayer = true;
                OnHazardTouch();
                respawnable?.KillAndRespawn();
                Destroy(gameObject);
                return;
            }

            // Rebote sobre plataformas o suelo en modo LobbedBouncing
            if (_trajectoryMode == FruitTrajectoryMode.LobbedBouncing)
            {
                bool hitFromBelow = false;
                foreach (var contact in collision.contacts)
                {
                    if (contact.normal.y > 0.4f)
                    {
                        hitFromBelow = true;
                        break;
                    }
                }

                if (hitFromBelow)
                {
                    _currentBounces++;
                    if (_currentBounces <= _maxBounces && _rb != null)
                    {
                        float currentBounceForce = _bounceVelocity * Mathf.Max(0.6f, 1f - (_currentBounces * 0.18f));
                        _rb.linearVelocity = new Vector2(_direction * _speed, currentBounceForce);
                    }
                    else
                    {
                        _trajectoryMode = FruitTrajectoryMode.Rolling;
                    }
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (_hasHitPlayer) return;

            if (collision.TryGetComponent<IPlayerRespawnable>(out var respawnable) ||
                collision.gameObject.TryGetComponent<IPlayerRespawnable>(out respawnable) ||
                collision.GetComponentInParent<IPlayerRespawnable>() != null)
            {
                if (respawnable == null) respawnable = collision.GetComponentInParent<IPlayerRespawnable>();
                _hasHitPlayer = true;
                OnHazardTouch();
                respawnable?.KillAndRespawn();
                Destroy(gameObject);
            }
        }
    }
}
