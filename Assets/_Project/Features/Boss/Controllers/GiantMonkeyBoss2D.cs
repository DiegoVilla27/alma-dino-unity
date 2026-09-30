using System;
using System.Collections;
using AlmaDino.Core.Events;
using AlmaDino.Core.Interfaces;
using AlmaDino.Core.Progression;
using AlmaDino.Features.Boss.Models;
using AlmaDino.Features.Boss.Projectiles;
using UnityEngine;

namespace AlmaDino.Features.Boss.Controllers
{
    public class GiantMonkeyBoss2D : MonoBehaviour
    {
        [Header("Boss Parameters")]
        [SerializeField] private int _maxHits = 3;
        [SerializeField] private float[] _exhaustionDurations = new float[] { 3.2f, 2.8f, 2.4f };
        [SerializeField] private Vector3 _centerHangingPos = new Vector3(0f, 6.0f, 0f);
        [SerializeField] private Vector3 _leftHangingPos = new Vector3(-4.5f, 6.0f, 0f);
        [SerializeField] private Vector3 _rightHangingPos = new Vector3(4.5f, 6.0f, 0f);
        [SerializeField] private Vector3 _tiredPosition = new Vector3(0f, 1.2f, 0f);

        [Header("References")]
        [SerializeField] private GameObject _fruitPrefab;
        [SerializeField] private Transform _projectileSpawnPoint;
        [SerializeField] private BossHeadHurtbox2D _headHurtbox;
        [SerializeField] private BossBodyHazard2D _bodyHazard;
        [SerializeField] private GameObject _dizzyIndicator;
        [SerializeField] private SpriteRenderer _bodyRenderer;
        [SerializeField] private GameObject _linkedExitPortal;
        [SerializeField] private CameraShakeEventChannelSO _shakeChannel;

        private BossStateEnum _currentState = BossStateEnum.Intro;
        private int _currentHits = 0;
        private Coroutine _currentBehaviorRoutine;
        private Color _originalBodyColor = Color.white;

        public event Action<int> OnBossDamaged;
        public event Action OnBossDefeated;

        public BossStateEnum CurrentState => _currentState;
        public int CurrentHits => _currentHits;
        public int MaxHits => _maxHits;
        public bool IsDefeated => _currentState == BossStateEnum.Defeated;

        public float CurrentExhaustionDuration
        {
            get
            {
                int index = Mathf.Clamp(_currentHits, 0, _exhaustionDurations.Length - 1);
                return _exhaustionDurations[index];
            }
        }

        private void Awake()
        {
            if (_headHurtbox == null) _headHurtbox = GetComponentInChildren<BossHeadHurtbox2D>();
            if (_bodyHazard == null) _bodyHazard = GetComponentInChildren<BossBodyHazard2D>();
            if (_bodyRenderer == null) _bodyRenderer = GetComponentInChildren<SpriteRenderer>();
            if (_bodyRenderer != null) _originalBodyColor = _bodyRenderer.color;

            if (_headHurtbox != null) _headHurtbox.SetHurtboxActive(false);
            if (_bodyHazard != null) _bodyHazard.SetHazardActive(true);
            if (_dizzyIndicator != null) _dizzyIndicator.SetActive(false);
        }

        private IPlayerRespawnable _cachedPlayer;

        private void OnEnable()
        {
            var playerGo = GameObject.FindWithTag("Player");
            if (playerGo != null && playerGo.TryGetComponent<IPlayerRespawnable>(out var respawnable))
            {
                _cachedPlayer = respawnable;
                _cachedPlayer.OnRespawned += HandlePlayerRespawned;
            }
        }

        private void OnDisable()
        {
            if (_cachedPlayer != null)
            {
                _cachedPlayer.OnRespawned -= HandlePlayerRespawned;
                _cachedPlayer = null;
            }
        }

        private void HandlePlayerRespawned(Vector2 respawnPos)
        {
            ClearActiveProjectiles();
        }

        public void ClearActiveProjectiles()
        {
            var fruits = FindObjectsByType<RollingFruitProjectile2D>(FindObjectsInactive.Exclude);
            foreach (var fruit in fruits)
            {
                if (fruit != null) Destroy(fruit.gameObject);
            }
        }

        private void Start()
        {
            StartCoroutine(IntroSequenceRoutine());
        }

        private IEnumerator IntroSequenceRoutine()
        {
            _currentState = BossStateEnum.Intro;
            transform.position = _centerHangingPos;

            yield return new WaitForSeconds(0.6f);

            NarrativeBannerEvents.RequestBanner(
                "¡JEFE DE MUNDO: MONO LADRÓN GIGANTE!",
                "El rey de la copa protege las reliquias. Esquiva sus ráfagas aéreas y terrestres.",
                new Color(1.0f, 0.60f, 0.0f),
                4.0f
            );

            if (_shakeChannel != null)
            {
                _shakeChannel.Raise(0.35f, 0.4f);
            }

            yield return new WaitForSeconds(1.8f);

            TransitionToState(BossStateEnum.HangingAttack);
        }

        public void TransitionToState(BossStateEnum newState)
        {
            if (_currentState == BossStateEnum.Defeated) return;

            if (_currentBehaviorRoutine != null)
            {
                StopCoroutine(_currentBehaviorRoutine);
                _currentBehaviorRoutine = null;
            }

            _currentState = newState;

            switch (newState)
            {
                case BossStateEnum.HangingAttack:
                    _currentBehaviorRoutine = StartCoroutine(HangingAttackRoutine());
                    break;
                case BossStateEnum.TiredDescent:
                    _currentBehaviorRoutine = StartCoroutine(TiredDescentRoutine());
                    break;
                case BossStateEnum.HurtEnrage:
                    _currentBehaviorRoutine = StartCoroutine(HurtEnrageRoutine());
                    break;
                case BossStateEnum.Defeated:
                    _currentBehaviorRoutine = StartCoroutine(DefeatedRoutine());
                    break;
            }
        }

        private IEnumerator HangingAttackRoutine()
        {
            if (_headHurtbox != null) _headHurtbox.SetHurtboxActive(false);
            if (_bodyHazard != null) _bodyHazard.SetHazardActive(true);
            if (_dizzyIndicator != null) _dizzyIndicator.SetActive(false);

            // Retornar a la copa
            yield return StartCoroutine(MoveToPositionRoutine(_centerHangingPos, 0.5f));
            yield return new WaitForSeconds(0.3f);

            switch (_currentHits)
            {
                case 0:
                    yield return StartCoroutine(AttackWavePhase1Routine());
                    break;
                case 1:
                    yield return StartCoroutine(AttackWavePhase2Routine());
                    break;
                default:
                    yield return StartCoroutine(AttackWavePhase3Routine());
                    break;
            }

            yield return new WaitForSeconds(0.4f);

            // Al terminar la oleada, cae exhausto
            TransitionToState(BossStateEnum.TiredDescent);
        }

        // FASE 1: 4 frutos, vel 6.5, intervalo 1.3s. Anti-camping en ramas elevadas.
        private IEnumerator AttackWavePhase1Routine()
        {
            float speed = 6.5f;
            float interval = 1.3f;

            for (int i = 0; i < 4; i++)
            {
                var player = GetPlayerTransform();
                bool playerOnHighBranch = player != null && player.position.y > 1.8f;

                // Telegraph
                yield return StartCoroutine(TelegraphAttackRoutine(0.2f));

                if (playerOnHighBranch || i == 2)
                {
                    // Tiro parabólico / rebotante hacia la posición del jugador
                    float dir = player != null ? Mathf.Sign(player.position.x - transform.position.x) : 1f;
                    if (Mathf.Abs(dir) < 0.1f) dir = 1f;
                    ThrowLobbedFruit(dir, speed * 0.9f, 6.2f, 2);
                }
                else
                {
                    // Fruto rodante estándar
                    float dir = player != null ? Mathf.Sign(player.position.x - transform.position.x) : (i % 2 == 0 ? 1f : -1f);
                    if (Mathf.Abs(dir) < 0.1f) dir = (i % 2 == 0 ? 1f : -1f);
                    ThrowRollingFruit(dir, speed);
                }

                // En el tiro 2, balancearse hacia un lado
                if (i == 1)
                {
                    Vector3 sidePos = (UnityEngine.Random.value > 0.5f) ? _leftHangingPos : _rightHangingPos;
                    yield return StartCoroutine(MoveToPositionRoutine(sidePos, 0.4f));
                }

                yield return new WaitForSeconds(interval);
            }
        }

        // FASE 2: 5 frutos, vel 8.0, intervalo 1.0s. Balanceo entre 3 anclas y tiros cruzados.
        private IEnumerator AttackWavePhase2Routine()
        {
            float speed = 8.0f;
            float interval = 1.0f;
            Vector3[] swingPositions = new Vector3[] { _leftHangingPos, _centerHangingPos, _rightHangingPos };

            for (int i = 0; i < 5; i++)
            {
                // Balancearse a una nueva posición antes de disparar
                Vector3 targetPos = swingPositions[i % swingPositions.Length];
                yield return StartCoroutine(MoveToPositionRoutine(targetPos, 0.35f));

                yield return StartCoroutine(TelegraphAttackRoutine(0.2f));

                var player = GetPlayerTransform();
                float dirToPlayer = player != null ? Mathf.Sign(player.position.x - transform.position.x) : (transform.position.x < 0 ? 1f : -1f);
                if (Mathf.Abs(dirToPlayer) < 0.1f) dirToPlayer = (transform.position.x < 0 ? 1f : -1f);

                if (i % 2 == 1 || (player != null && player.position.y > 1.8f))
                {
                    // Tiro aéreo rebotante
                    ThrowLobbedFruit(dirToPlayer, speed * 0.95f, 7.0f, 3);
                }
                else
                {
                    // Fruto rodante de alta velocidad
                    ThrowRollingFruit(dirToPlayer, speed);
                }

                yield return new WaitForSeconds(interval);
            }
        }

        // FASE 3: FRENESÍ (7 frutos en ráfagas dobles coordinadas, vel 9.5, intervalo 0.75s).
        private IEnumerator AttackWavePhase3Routine()
        {
            float speed = 9.5f;
            float interval = 0.75f;

            // Ráfaga 1: Doble lanzamiento simultáneo (izquierda y derecha a ras de suelo)
            yield return StartCoroutine(MoveToPositionRoutine(_centerHangingPos, 0.3f));
            yield return StartCoroutine(TelegraphAttackRoutine(0.22f));
            ThrowRollingFruit(-1f, speed);
            ThrowRollingFruit(1f, speed);
            yield return new WaitForSeconds(interval + 0.2f);

            // Ráfaga 2: Balanceo a la izquierda y tiro aéreo rebotante cruzado
            yield return StartCoroutine(MoveToPositionRoutine(_leftHangingPos, 0.3f));
            yield return StartCoroutine(TelegraphAttackRoutine(0.18f));
            ThrowLobbedFruit(1f, speed * 0.95f, 7.5f, 3);
            yield return new WaitForSeconds(interval);

            // Ráfaga 3: Fruto rodante veloz
            ThrowRollingFruit(1f, speed);
            yield return new WaitForSeconds(interval);

            // Ráfaga 4: Balanceo a la derecha y doble ataque (suelo + aire)
            yield return StartCoroutine(MoveToPositionRoutine(_rightHangingPos, 0.35f));
            yield return StartCoroutine(TelegraphAttackRoutine(0.2f));
            ThrowRollingFruit(-1f, speed);
            ThrowLobbedFruit(-1f, speed * 0.85f, 8.0f, 2);
            yield return new WaitForSeconds(interval + 0.15f);

            // Ráfaga 5: Regreso al centro y tiro parabólico final
            yield return StartCoroutine(MoveToPositionRoutine(_centerHangingPos, 0.25f));
            yield return StartCoroutine(TelegraphAttackRoutine(0.18f));
            var player = GetPlayerTransform();
            float dir = player != null ? Mathf.Sign(player.position.x - transform.position.x) : 1f;
            ThrowLobbedFruit(dir, speed, 7.0f, 2);
            yield return new WaitForSeconds(0.6f);
        }

        private IEnumerator TelegraphAttackRoutine(float duration)
        {
            if (_bodyRenderer != null)
            {
                _bodyRenderer.color = new Color(1.0f, 0.55f, 0.1f); // Resplandor ámbar de anticipación
            }
            yield return new WaitForSeconds(duration);
            if (_bodyRenderer != null)
            {
                _bodyRenderer.color = _originalBodyColor;
            }
        }

        private IEnumerator MoveToPositionRoutine(Vector3 targetPos, float duration)
        {
            float elapsed = 0f;
            Vector3 startPos = transform.position;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }
            transform.position = targetPos;
        }

        private void ThrowRollingFruit(float dir, float speed)
        {
            if (_fruitPrefab == null) return;
            Vector3 spawnPos = _projectileSpawnPoint != null ? _projectileSpawnPoint.position : transform.position;

            var fruitGo = Instantiate(_fruitPrefab, spawnPos, Quaternion.identity);
            var proj = fruitGo.GetComponent<RollingFruitProjectile2D>();
            if (proj != null)
            {
                proj.InitializeRolling(dir, speed);
            }
        }

        private void ThrowLobbedFruit(float dir, float hSpeed, float vImpulse, int maxBounces)
        {
            if (_fruitPrefab == null) return;
            Vector3 spawnPos = _projectileSpawnPoint != null ? _projectileSpawnPoint.position : transform.position;

            var fruitGo = Instantiate(_fruitPrefab, spawnPos, Quaternion.identity);
            var proj = fruitGo.GetComponent<RollingFruitProjectile2D>();
            if (proj != null)
            {
                proj.InitializeLobbed(dir, hSpeed, vImpulse, maxBounces);
            }
        }

        private Transform GetPlayerTransform()
        {
            var player = FindAnyObjectByType<MonoBehaviour>() as IPlayerRespawnable;
            if (player is MonoBehaviour mb)
            {
                return mb.transform;
            }
            return null;
        }

        private IEnumerator TiredDescentRoutine()
        {
            // Caída pesada de aterrizaje hacia la posición de suelo
            float dropElapsed = 0f;
            Vector3 startPos = transform.position;
            Vector3 targetFloorPos = new Vector3(Mathf.Clamp(startPos.x, -2.5f, 2.5f), _tiredPosition.y, 0f);

            while (dropElapsed < 0.35f)
            {
                dropElapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(startPos, targetFloorPos, dropElapsed / 0.35f);
                yield return null;
            }
            transform.position = targetFloorPos;

            // Impacto sísmico (Ground Slam) al caer al suelo
            if (_shakeChannel != null)
            {
                _shakeChannel.Raise(0.35f, 0.3f);
            }

            // Exponer la cabeza vulnerable
            if (_headHurtbox != null) _headHurtbox.SetHurtboxActive(true);
            if (_bodyHazard != null) _bodyHazard.SetHazardActive(true); // El torso sigue siendo peligroso al contacto horizontal
            if (_dizzyIndicator != null) _dizzyIndicator.SetActive(true);

            // Duración de vulnerabilidad escalonada
            float exhaustionTime = CurrentExhaustionDuration;
            yield return new WaitForSeconds(exhaustionTime);

            // Si el tiempo expira sin golpe: se recupera
            if (_headHurtbox != null) _headHurtbox.SetHurtboxActive(false);
            if (_dizzyIndicator != null) _dizzyIndicator.SetActive(false);

            TransitionToState(BossStateEnum.HangingAttack);
        }

        public void OnHeadStomped()
        {
            if (_currentState != BossStateEnum.TiredDescent) return;

            _currentHits++;
            OnBossDamaged?.Invoke(_currentHits);

            if (_headHurtbox != null) _headHurtbox.SetHurtboxActive(false);
            if (_dizzyIndicator != null) _dizzyIndicator.SetActive(false);

            if (_shakeChannel != null)
            {
                _shakeChannel.Raise(0.5f, 0.45f);
            }

            if (_currentHits >= _maxHits)
            {
                TransitionToState(BossStateEnum.Defeated);
            }
            else
            {
                TransitionToState(BossStateEnum.HurtEnrage);
            }
        }

        private IEnumerator HurtEnrageRoutine()
        {
            // Parpadeo de daño
            if (_bodyRenderer != null)
            {
                for (int f = 0; f < 3; f++)
                {
                    _bodyRenderer.color = new Color(1f, 0.2f, 0.2f);
                    yield return new WaitForSeconds(0.08f);
                    _bodyRenderer.color = _originalBodyColor;
                    yield return new WaitForSeconds(0.08f);
                }
            }

            // Checkpoint en la arena
            var player = FindAnyObjectByType<MonoBehaviour>() as IPlayerRespawnable;
            if (player != null)
            {
                player.SetCheckpoint(new Vector2(-8.0f, 1.0f));
            }

            NarrativeBannerEvents.RequestBanner(
                $"¡IMPACTO RECIBIDO! ({_currentHits}/{_maxHits})",
                _currentHits == 2
                    ? "¡EL REY DE LA COPA ENTRA EN FRENESÍ TOTAL! Ráfagas dobles inminentes."
                    : "El gran simio ruge enfurecido e intensifica su asedio.",
                new Color(1.0f, 0.3f, 0.2f),
                2.5f
            );

            yield return new WaitForSeconds(0.8f);

            TransitionToState(BossStateEnum.HangingAttack);
        }

        private IEnumerator DefeatedRoutine()
        {
            GameProgression.CompleteWorld(1);

            if (_shakeChannel != null)
            {
                _shakeChannel.Raise(0.5f, 0.6f);
            }

            // El simio cae derrotado hacia el abismo
            float fallElapsed = 0f;
            Vector3 startPos = transform.position;
            Vector3 endPos = startPos + new Vector3(0f, -12f, 0f);
            while (fallElapsed < 1.2f)
            {
                fallElapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(startPos, endPos, fallElapsed / 1.2f);
                yield return null;
            }

            gameObject.SetActive(false);

            NarrativeBannerEvents.RequestBanner(
                "¡MUNDO 1 COMPLETADO: REY DE LA COPA DERROTADO!",
                "El gran simio huye hacia el abismo de las cuevas.\nEl rastro de tus otros tres hijos desciende hacia las profundidades de la tierra.\n(Mundo 1: Jungla Esmeralda Completado)",
                new Color(0.0f, 0.96f, 0.83f),
                7.0f
            );

            if (_linkedExitPortal != null)
            {
                _linkedExitPortal.SetActive(true);
            }

            OnBossDefeated?.Invoke();
            Debug.Log("<color=#FFD700><b>[GiantMonkeyBoss2D]</b> ¡Jefe 1 derrotado con éxito! Mundo 1 completado.</color>");
        }

        public void Configure(int maxHits, float[] exhaustionDurations, GameObject exitPortal, CameraShakeEventChannelSO shake)
        {
            _maxHits = maxHits;
            if (exhaustionDurations != null && exhaustionDurations.Length > 0)
            {
                _exhaustionDurations = exhaustionDurations;
            }
            _linkedExitPortal = exitPortal;
            _shakeChannel = shake;
        }
    }
}
