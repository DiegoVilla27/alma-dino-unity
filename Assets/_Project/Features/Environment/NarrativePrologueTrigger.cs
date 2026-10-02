using AlmaDino.Core.Interfaces;
using UnityEngine;

namespace AlmaDino.Features.Environment
{
    /// <summary>
    /// Componente que dispara el diálogo inicial de prólogo al arrancar un nivel.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class NarrativePrologueTrigger : MonoBehaviour
    {
        [SerializeField] private string _title = "DESPERTAR EN EL NIDO";
        [TextArea(3, 6)]
        [SerializeField] private string _message = "La tierra tembló una sola vez.\nCuando regresé al nido con comida, el silencio era absoluto. No estaban.\nSi tengo que cruzar el continente entero a pie, mis pequeños volverán a sentir el calor de mis plumas.";
        [SerializeField] private Color _bannerColor = new Color(0.95f, 0.75f, 0.2f, 1f);
        [SerializeField] private float _displayDuration = 6.5f;

        [SerializeField] private bool _triggerOnStart = true;

        private bool _hasTriggered;

        private void Start()
        {
            if (_triggerOnStart) Invoke(nameof(TriggerPrologue), 0.35f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_hasTriggered) return;
            if (other.GetComponentInParent<IPlayerRespawnable>() != null || other.CompareTag("Player"))
            {
                TriggerPrologue();
            }
        }

        public void Configure(string title, string message, Color? bannerColor = null, float displayDuration = 6.5f, bool triggerOnStart = true)
        {
            _triggerOnStart = triggerOnStart;
            _title = title;
            _message = message;
            if (bannerColor.HasValue) _bannerColor = bannerColor.Value;
            _displayDuration = displayDuration;
        }

        public void TriggerPrologue()
        {
            if (_hasTriggered) return;
            _hasTriggered = true;

            if (NarrativeBannerUI.Instance != null)
            {
                NarrativeBannerUI.Instance.ShowBanner(_title, _message, _bannerColor, _displayDuration);
            }
        }
    }
}
