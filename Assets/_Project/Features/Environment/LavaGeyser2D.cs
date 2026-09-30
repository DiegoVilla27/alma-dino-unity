using System.Collections;
using AlmaDino.Core.Interfaces;
using UnityEngine;

namespace AlmaDino.Features.Environment
{
    /// <summary>
    /// Géiser volcánico intermitente que alterna en un ciclo rítmico:
    /// Inactivo -> Advertencia (chispas / humo) -> Erupción ardiente (letal) -> Enfriamiento.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class LavaGeyser2D : MonoBehaviour, IHazard2D
    {
        [Header("Cycle Timings")]
        [SerializeField] private float _dormantDuration = 2.2f;
        [SerializeField] private float _warningDuration = 0.8f;
        [SerializeField] private float _eruptingDuration = 1.2f;

        [Header("Visual References")]
        [SerializeField] private Transform _flamePillarRoot;
        [SerializeField] private SpriteRenderer _flameRenderer;
        [SerializeField] private SpriteRenderer _ventBaseRenderer;

        [Header("Colors")]
        [SerializeField] private Color _dormantVentColor = new Color(0.2f, 0.15f, 0.15f, 1f);
        [SerializeField] private Color _warningVentColor = new Color(1f, 0.6f, 0.1f, 1f);
        [SerializeField] private Color _eruptingFlameColor = new Color(1f, 0.25f, 0.05f, 0.9f);

        private Collider2D _flameCollider;
        private bool _isErupting;

        public bool IsErupting => _isErupting;

        private void Awake()
        {
            _flameCollider = GetComponent<Collider2D>();
            _flameCollider.isTrigger = true;

            if (_flameRenderer == null && _flamePillarRoot != null)
                _flameRenderer = _flamePillarRoot.GetComponentInChildren<SpriteRenderer>();

            SetErupting(false);
        }

        private void OnEnable()
        {
            StartCoroutine(GeyserCycleRoutine());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            SetErupting(false);
        }

        private IEnumerator GeyserCycleRoutine()
        {
            while (true)
            {
                // 1. Inactivo
                SetVentColor(_dormantVentColor);
                SetErupting(false);
                yield return new WaitForSeconds(_dormantDuration);

                // 2. Advertencia
                float warningElapsed = 0f;
                while (warningElapsed < _warningDuration)
                {
                    warningElapsed += Time.deltaTime;
                    float flash = Mathf.PingPong(warningElapsed * 8f, 1f);
                    SetVentColor(Color.Lerp(_dormantVentColor, _warningVentColor, flash));
                    yield return null;
                }

                // 3. Erupción ardiente
                SetVentColor(_warningVentColor);
                SetErupting(true);
                yield return new WaitForSeconds(_eruptingDuration);

                // 4. Fin de erupción
                SetErupting(false);
            }
        }

        private void SetErupting(bool erupting)
        {
            _isErupting = erupting;
            _flameCollider.enabled = erupting;

            if (_flamePillarRoot != null)
            {
                _flamePillarRoot.gameObject.SetActive(erupting);
            }
            else if (_flameRenderer != null)
            {
                _flameRenderer.enabled = erupting;
                _flameRenderer.color = _eruptingFlameColor;
            }
        }

        private void SetVentColor(Color color)
        {
            if (_ventBaseRenderer != null)
            {
                _ventBaseRenderer.color = color;
            }
        }

        public void OnHazardTouch()
        {
            Debug.Log($"<color=#FF3300><b>[LavaGeyser2D]</b> Jugador alcanzado por el chorro de magma de {gameObject.name}.</color>");
        }
    }
}
