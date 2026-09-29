using AlmaDino.Core.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AlmaDino.Features.MobileUI.Controllers
{
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [SerializeField] private RectTransform _handle;
        [SerializeField] private float _movementRange = 60f;

        private RectTransform _baseRect;
        private Vector2 _inputVector;

        private void Awake()
        {
            _baseRect = GetComponent<RectTransform>();
            if (_handle == null && transform.childCount > 0)
            {
                _handle = transform.GetChild(0).GetComponent<RectTransform>();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_baseRect == null) return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _baseRect,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint))
            {
                _inputVector = Vector2.ClampMagnitude(localPoint, _movementRange);
                if (_handle != null)
                {
                    _handle.anchoredPosition = _inputVector;
                }

                VirtualInputBridge.MoveVector = _inputVector / _movementRange;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _inputVector = Vector2.zero;
            if (_handle != null)
            {
                _handle.anchoredPosition = Vector2.zero;
            }
            VirtualInputBridge.MoveVector = Vector2.zero;
        }

        private void OnDisable()
        {
            OnPointerUp(null);
        }
    }
}
