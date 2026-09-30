using System.Collections;
using AlmaDino.Core.Interfaces;
using UnityEngine;

namespace AlmaDino.Features.Environment
{
    /// <summary>
    /// Secuencia narrativa en gameplay: El Mono Ladrón aparece visible sosteniendo el huevo robado.
    /// Al acercarse Alma, el mono huye con acrobacias hacia las copas superiores, guiando el camino.
    /// </summary>
    public class ThiefMonkeyTeaser2D : MonoBehaviour
    {
        [Header("Trigger Detection")]
        [Tooltip("Distancia horizontal en metros para que el mono se percate de Alma y huya")]
        [SerializeField] private float _triggerDistance = 7.0f;
        [SerializeField] private string _monkeyTaunt = "¡Kikiki! ¡Nunca alcanzarás la copa del árbol!";

        [Header("Flee Arc Movement")]
        [SerializeField] private Vector2 _fleeDirection = new Vector2(9.0f, 6.0f);
        [SerializeField] private float _fleeDuration = 1.0f;

        [Header("Visuals")]
        [SerializeField] private SpriteRenderer _monkeyRenderer;
        [SerializeField] private SpriteRenderer _eggRenderer;

        private Transform _playerTransform;
        private bool _hasFled = false;

        private void Start()
        {
            var player = Object.FindAnyObjectByType<MonoBehaviour>() as IPlayerRespawnable;
            if (player != null)
            {
                _playerTransform = (player as Component).transform;
            }
        }

        private void Update()
        {
            if (_hasFled) return;

            if (_playerTransform == null)
            {
                var p = GameObject.Find("Alma (Player)");
                if (p != null) _playerTransform = p.transform;
                if (_playerTransform == null) return;
            }

            float dist = Vector2.Distance(_playerTransform.position, transform.position);
            if (dist <= _triggerDistance)
            {
                _hasFled = true;
                StartCoroutine(FleeRoutine());
            }
        }

        private IEnumerator FleeRoutine()
        {
            Debug.Log($"[ThiefMonkeyTeaser2D] El Mono Ladrón huye hacia las copas: {_monkeyTaunt}");
            if (NarrativeBannerUI.Instance != null)
            {
                NarrativeBannerUI.Instance.ShowBanner("¡MONO LADRÓN AVISTADO!", _monkeyTaunt, new Color(1.0f, 0.7f, 0.2f), 3.5f);
            }

            Vector3 startPos = transform.position;
            Vector3 endPos = startPos + (Vector3)_fleeDirection;
            float elapsed = 0f;

            while (elapsed < _fleeDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / _fleeDuration;

                // Movimiento parabólico de salto
                float arcY = Mathf.Sin(progress * Mathf.PI) * 2.5f;
                Vector3 current = Vector3.Lerp(startPos, endPos, progress);
                current.y += arcY;
                transform.position = current;

                // Giro acrobático
                transform.rotation = Quaternion.Euler(0f, 0f, -progress * 360f);

                yield return null;
            }

            // Desaparecer en el follaje
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
