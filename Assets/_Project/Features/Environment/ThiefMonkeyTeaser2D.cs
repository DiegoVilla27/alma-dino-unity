using System.Collections;
using AlmaDino.Core.Interfaces;
using UnityEngine;

namespace AlmaDino.Features.Environment
{
    /// <summary>
    /// Secuencia narrativa en gameplay: El Mono Ladrón aparece visible sosteniendo el huevo robado.
    /// Al acercarse Alma, el mono huye con acrobacias hacia las copas superiores, guiando el camino hacia los hongos.
    /// </summary>
    public class ThiefMonkeyTeaser2D : MonoBehaviour
    {
        [Header("Trigger Detection")]
        [Tooltip("Distancia en metros para que el mono se percate de Alma y huya")]
        [SerializeField] private float _triggerDistance = 6.0f;
        [SerializeField] private string _monkeyTaunt = "¡Kikiki! ¡Nunca alcanzarás la copa del gran árbol!";

        [Header("Flee Arc Movement")]
        [SerializeField] private Vector2 _fleeDirection = new Vector2(10.0f, 8.0f);
        [SerializeField] private float _fleeDuration = 1.2f;

        [Header("Visuals")]
        [SerializeField] private SpriteRenderer _monkeyRenderer;
        [SerializeField] private SpriteRenderer _eggRenderer;

        private Transform _playerTransform;
        private bool _hasFled = false;
        private Vector3 _startPosition;

        private void Awake()
        {
            _startPosition = transform.position;
            FindPlayer();
        }

        private void Start()
        {
            FindPlayer();
        }

        private void FindPlayer()
        {
            if (_playerTransform != null) return;

            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                _playerTransform = playerObj.transform;
            }
            else
            {
                var pc = Object.FindAnyObjectByType<MonoBehaviour>() as IPlayerRespawnable;
                if (pc != null)
                {
                    _playerTransform = (pc as Component).transform;
                }
            }
        }

        private float _startDelay = 0.4f;

        private void Update()
        {
            if (_hasFled) return;

            if (_playerTransform == null)
            {
                FindPlayer();
                if (_playerTransform == null) return;
            }

            // Animación idle: el mono da saltitos en su sitio sosteniendo el huevo de Alma
            float hopY = Mathf.Abs(Mathf.Sin(Time.time * 7f)) * 0.25f;
            transform.position = _startPosition + new Vector3(0f, hopY, 0f);

            _startDelay -= Time.deltaTime;
            if (_startDelay > 0f) return;

            float dist = Vector2.Distance(_playerTransform.position, _startPosition);
            // Si el jugador se acerca o si han pasado 1.2 segundos contemplándolo, el mono se burla y huye
            if (dist <= _triggerDistance || _startDelay <= -0.8f)
            {
                _hasFled = true;
                StartCoroutine(FleeRoutine());
            }
        }

        private IEnumerator FleeRoutine()
        {
            Debug.Log($"<color=#FF9900><b>[ThiefMonkeyTeaser2D]</b> ¡Mono Ladrón avistado huyendo hacia las copas!</color>");
            if (NarrativeBannerUI.Instance != null)
            {
                NarrativeBannerUI.Instance.ShowBanner("¡MONO LADRÓN!", "¡Kikiki! ¿Creías que podías alcanzarme, mamá lagarto?\n¡Tus huevos son míos! ¡Sube a buscarme a la cima si te atreves!", new Color(1.0f, 0.7f, 0.2f), 5.5f);
            }

            // Pausa de burla visible: saltos rápidos de risa antes de huir
            float tauntElapsed = 0f;
            Vector3 tauntBasePos = transform.position;
            while (tauntElapsed < 0.75f)
            {
                tauntElapsed += Time.deltaTime;
                float hop = Mathf.Abs(Mathf.Sin(tauntElapsed * 16f)) * 0.35f;
                transform.position = tauntBasePos + new Vector3(0f, hop, 0f);
                yield return null;
            }

            Vector3 startPos = transform.position;
            Vector3 endPos = _startPosition + (Vector3)_fleeDirection;
            float elapsed = 0f;

            while (elapsed < _fleeDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / _fleeDuration;

                // Salto parabólico acrobático alto hacia las copas
                float arcY = Mathf.Sin(progress * Mathf.PI) * 4.5f;
                Vector3 current = Vector3.Lerp(startPos, endPos, progress);
                current.y += arcY;
                transform.position = current;

                // Rotación acrobática en el aire
                transform.rotation = Quaternion.Euler(0f, 0f, -progress * 720f);

                yield return null;
            }

            // Desaparecer en el follaje del árbol
            gameObject.SetActive(false);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _triggerDistance);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)_fleeDirection);
        }
    }
}
