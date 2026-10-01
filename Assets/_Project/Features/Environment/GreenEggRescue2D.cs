using System;
using System.Collections;
using AlmaDino.Core.Interfaces;
using AlmaDino.Core.Progression;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace AlmaDino.Features.Environment
{
    [RequireComponent(typeof(Collider2D))]
    public class GreenEggRescue2D : MonoBehaviour
    {
        [Header("Rescue Settings")]
        [SerializeField] private EggType _eggType = EggType.GreenEgg;
        [SerializeField] private string _rescueTitle = "¡PRIMER RESCATE: EL HUEVO VERDE!";
        [TextArea(3, 6)]
        [SerializeField] private string _rescueText = "«Aún estás tibio...\nMamá llegó a tiempo. Ya estás a salvo.»\n(1 de 4 rescatados)\n\n¡Un rugido colosal sacude la copa del Gran Árbol!\nEl Mono Ladrón Gigante aguarda furioso más adelante...";
        [SerializeField] private Color _glowColor = new Color(0.0f, 0.96f, 0.83f, 1f); // #00F5D4

        [Header("References & Feedback")]
        [SerializeField] private Light2D _pointLight;
        [SerializeField] private LevelExit2D _linkedPortal;
        [SerializeField] private SpriteRenderer _eggRenderer;

        private bool _isRescued = false;
        private Vector3 _baseScale;
        private float _pulseTimer = 0f;

        public event Action<EggType> OnEggRescued;
        public bool IsRescued => _isRescued;
        public EggType RescuedEggType => _eggType;

        private void Awake()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;

            if (_eggRenderer == null)
            {
                _eggRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (_pointLight == null)
            {
                _pointLight = GetComponentInChildren<Light2D>();
            }

            _baseScale = transform.localScale;
        }

        private void Start()
        {
            // If already rescued in progression, maintain glowing rescued aura
            if (GameProgression.IsEggRescued(_eggType))
            {
                _isRescued = true;
                if (_linkedPortal != null) _linkedPortal.gameObject.SetActive(true);
                if (_pointLight != null)
                {
                    _pointLight.intensity = 1.6f;
                    _pointLight.color = _glowColor;
                }
            }
        }

        private void Update()
        {
            // Rhythmic heartbeat pulsing effect
            _pulseTimer += Time.deltaTime * (_isRescued ? 3.5f : 2.0f);
            float pulse = Mathf.Sin(_pulseTimer);
            float scaleMultiplier = 1f + (pulse * 0.06f);
            transform.localScale = _baseScale * scaleMultiplier;

            if (_pointLight != null)
            {
                float baseIntensity = _isRescued ? 1.4f : 0.9f;
                _pointLight.intensity = baseIntensity + (pulse * 0.35f);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (_isRescued) return;

            if (collision.GetComponent<IPlayerRespawnable>() != null)
            {
                TriggerRescue();
            }
        }

        public void TriggerRescue()
        {
            if (_isRescued) return;
            _isRescued = true;

            // 1. Persist rescue in global progression
            GameProgression.RescueEgg(_eggType);

            // 2. Visual & Audio highlight
            if (_pointLight != null)
            {
                _pointLight.color = _glowColor;
                _pointLight.intensity = 2.0f;
            }

            if (_eggRenderer != null)
            {
                _eggRenderer.color = Color.white;
            }

            // 3. Show diegetic emotional narrative banner
            if (NarrativeBannerUI.Instance != null)
            {
                NarrativeBannerUI.Instance.ShowBanner(_rescueTitle, _rescueText, _glowColor, 7.0f);
            }

            // 4. Activate linked exit portal if present
            if (_linkedPortal != null)
            {
                _linkedPortal.gameObject.SetActive(true);
            }

            OnEggRescued?.Invoke(_eggType);
            Debug.Log($"<color=#00F5D4><b>[GreenEggRescue2D]</b> ¡Huevo {_eggType} rescatado con éxito!</color>");
        }

        public void Configure(EggType eggType, string title, string text, Color color, LevelExit2D portal = null)
        {
            _eggType = eggType;
            _rescueTitle = title;
            _rescueText = text;
            _glowColor = color;
            _linkedPortal = portal;
        }
    }
}
