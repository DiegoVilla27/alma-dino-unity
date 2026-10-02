using AlmaDino.Core.Interfaces;
using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace AlmaDino.Features.Environment.Controllers
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class BellFlameDoor2D : MonoBehaviour, IConditionalHazard2D
    {
        [SerializeField] private ResonanceBell2D _bell;
        [SerializeField] private GameObject _flameRoot;
        [SerializeField] private Light2D _light;
        [SerializeField] private TextMesh _status;
        private Collider2D _collider;
        public bool IsDangerous => _bell == null || !_bell.IsOpen;
        private void Awake() { _collider = GetComponent<Collider2D>(); _collider.isTrigger = true; }
        private void FixedUpdate() => _collider.enabled = IsDangerous;
        private void Update()
        {
            _flameRoot.SetActive(IsDangerous);
            _light.intensity = IsDangerous ? .8f : .08f;
            _status.text = IsDangerous ? "¡FUEGO!" : "PASA: " + _bell.Remaining.ToString("0.0") + " s";
            _status.color = IsDangerous ? Color.yellow : Color.cyan;
        }
        public void OnHazardTouch() { }
    }
}
