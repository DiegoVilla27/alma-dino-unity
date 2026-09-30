using System.Collections;
using AlmaDino.Core.Events;
using AlmaDino.Core.Interfaces;
using UnityEngine;

namespace AlmaDino.Features.Environment
{
    [RequireComponent(typeof(Collider2D))]
    public class AbilityRelic2D : MonoBehaviour
    {
        [Header("Ability Setting")]
        [SerializeField] private AbilityType _abilityToUnlock = AbilityType.DoubleJump;
        [SerializeField] private AbilityUnlockedEventChannelSO _eventChannel;

        [Header("Lore & Narrative")]
        [SerializeField] private string _relicTitle = "Gema de Energía Materna";
        [TextArea(2, 4)]
        [SerializeField] private string _loreDescription = "El dolor de la pérdida despierta tus alas. Presiona SALTO en el aire para un segundo impulso.";

        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private float _floatAmplitude = 0.25f;
        [SerializeField] private float _floatFrequency = 2.5f;
        [SerializeField] private float _rotationSpeed = 30f;
        [SerializeField] private Color _glowColor = new Color(0.2f, 1f, 0.4f, 1f);

        private Vector3 _startPosition;
        private bool _isCollected;
        private Collider2D _collider;

        public AbilityType AbilityToUnlock => _abilityToUnlock;
        public string RelicTitle => _relicTitle;
        public string LoreDescription => _loreDescription;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _collider.isTrigger = true;

            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            _startPosition = transform.position;
        }

        private void Update()
        {
            if (_isCollected) return;

            // Suave oscilación senoidal vertical
            float newY = _startPosition.y + Mathf.Sin(Time.time * _floatFrequency) * _floatAmplitude;
            transform.position = new Vector3(_startPosition.x, newY, _startPosition.z);

            // Rotación suave para efecto místico
            transform.Rotate(0f, 0f, _rotationSpeed * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (_isCollected) return;

            if (collision.TryGetComponent<IAbilityUnlockable>(out var unlockable))
            {
                CollectRelic(unlockable);
            }
        }

        private void CollectRelic(IAbilityUnlockable target)
        {
            _isCollected = true;
            if (_collider != null) _collider.enabled = false;

            target.UnlockAbility(_abilityToUnlock);

            if (_eventChannel != null)
            {
                _eventChannel.Raise(_abilityToUnlock);
            }

            Debug.Log($"[AbilityRelic2D] Recogida: {_relicTitle} -> Desbloqueado: {_abilityToUnlock}");

            // Notificar al NarrativeBanner si existe en la escena
            if (NarrativeBannerUI.Instance != null)
            {
                NarrativeBannerUI.Instance.ShowRelicUnlock(_relicTitle, _loreDescription, _glowColor);
            }

            StartCoroutine(CollectAnimationRoutine());
        }

        private IEnumerator CollectAnimationRoutine()
        {
            float elapsed = 0f;
            float duration = 0.4f;
            Vector3 initialScale = transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(initialScale, initialScale * 1.5f, t);

                if (_spriteRenderer != null)
                {
                    Color c = _spriteRenderer.color;
                    c.a = Mathf.Lerp(1f, 0f, t);
                    _spriteRenderer.color = c;
                }

                yield return null;
            }

            gameObject.SetActive(false);
        }
    }
}
