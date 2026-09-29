using AlmaDino.Core.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AlmaDino.Features.MobileUI.Controllers
{
    public enum VirtualButtonType
    {
        Jump,
        Dash,
        GroundPound,
        Roar
    }

    public class VirtualTouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private VirtualButtonType _buttonType;

        public VirtualButtonType ButtonType
        {
            get => _buttonType;
            set => _buttonType = value;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            switch (_buttonType)
            {
                case VirtualButtonType.Jump:
                    VirtualInputBridge.TriggerJump();
                    break;
                case VirtualButtonType.Dash:
                    VirtualInputBridge.TriggerDash();
                    break;
                case VirtualButtonType.GroundPound:
                    VirtualInputBridge.TriggerGroundPound();
                    break;
                case VirtualButtonType.Roar:
                    VirtualInputBridge.TriggerRoar();
                    break;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_buttonType == VirtualButtonType.Jump)
            {
                VirtualInputBridge.ReleaseJump();
            }
        }
    }
}
