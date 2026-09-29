using AlmaDino.Core.Utilities;
using AlmaDino.Features.Player.Models;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AlmaDino.Features.Player.Controllers
{
    public class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private InputActionAsset _inputActions;

        private InputActionMap _playerMap;
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _dashAction;
        private InputAction _groundPoundAction;
        private InputAction _roarAction;

        private PlayerFrameInput _currentInput;

        public PlayerFrameInput CurrentInput => _currentInput;

        private void Awake()
        {
            InitializeActions();
        }

        private void OnEnable()
        {
            EnableActions();
        }

        private void OnDisable()
        {
            DisableActions();
        }

        public void SetInputAsset(InputActionAsset asset)
        {
            _inputActions = asset;
            InitializeActions();
            EnableActions();
        }

        private void InitializeActions()
        {
            if (_inputActions == null) return;

            _playerMap = _inputActions.FindActionMap("Player");
            if (_playerMap != null)
            {
                _moveAction = _playerMap.FindAction("Move");
                _jumpAction = _playerMap.FindAction("Jump");
                _dashAction = _playerMap.FindAction("Sprint");
                _groundPoundAction = _playerMap.FindAction("Crouch");
                _roarAction = _playerMap.FindAction("Attack");
            }
        }

        public void EnableActions()
        {
            _playerMap?.Enable();
            _moveAction?.Enable();
            _jumpAction?.Enable();
            _dashAction?.Enable();
            _groundPoundAction?.Enable();
            _roarAction?.Enable();
        }

        public void DisableActions()
        {
            _playerMap?.Disable();
            _moveAction?.Disable();
            _jumpAction?.Disable();
            _dashAction?.Disable();
            _groundPoundAction?.Disable();
            _roarAction?.Disable();
        }

        private void Update()
        {
            Vector2 move = Vector2.zero;
            bool jumpDown = false;
            bool jumpHeld = false;
            bool jumpUp = false;
            bool dashDown = false;
            bool groundPoundDown = false;
            bool roarDown = false;

            // 1. Read from Input Actions (Asset-based)
            if (_moveAction != null) move = _moveAction.ReadValue<Vector2>();
            if (_jumpAction != null)
            {
                jumpDown |= _jumpAction.WasPressedThisFrame();
                jumpHeld |= _jumpAction.IsPressed();
                jumpUp |= _jumpAction.WasReleasedThisFrame();
            }
            if (_dashAction != null) dashDown |= _dashAction.WasPressedThisFrame();
            if (_groundPoundAction != null) groundPoundDown |= _groundPoundAction.WasPressedThisFrame();
            if (_roarAction != null) roarDown |= _roarAction.WasPressedThisFrame();

            // 2. Direct Keyboard Hardware Fallback
            var kb = Keyboard.current;
            if (kb != null)
            {
                float kx = 0f;
                float ky = 0f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) kx -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) kx += 1f;
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) ky += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) ky -= 1f;

                if (Mathf.Abs(kx) > 0.01f || Mathf.Abs(ky) > 0.01f)
                {
                    move = new Vector2(kx, ky);
                }

                jumpDown |= kb.spaceKey.wasPressedThisFrame;
                jumpHeld |= kb.spaceKey.isPressed;
                jumpUp |= kb.spaceKey.wasReleasedThisFrame;

                dashDown |= kb.leftShiftKey.wasPressedThisFrame || kb.rightShiftKey.wasPressedThisFrame;
                groundPoundDown |= kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame || kb.cKey.wasPressedThisFrame;
                roarDown |= kb.fKey.wasPressedThisFrame || kb.eKey.wasPressedThisFrame;
            }

            // 2b. Universal Legacy Keyboard Fallback (activeInputHandler: Both)
            try
            {
                float legX = UnityEngine.Input.GetAxisRaw("Horizontal");
                float legY = UnityEngine.Input.GetAxisRaw("Vertical");
                if (Mathf.Abs(legX) > 0.05f || Mathf.Abs(legY) > 0.05f)
                {
                    move = new Vector2(legX, legY);
                }
                jumpDown |= UnityEngine.Input.GetKeyDown(KeyCode.Space);
                jumpHeld |= UnityEngine.Input.GetKey(KeyCode.Space);
                jumpUp |= UnityEngine.Input.GetKeyUp(KeyCode.Space);
                dashDown |= UnityEngine.Input.GetKeyDown(KeyCode.LeftShift) || UnityEngine.Input.GetKeyDown(KeyCode.RightShift);
                groundPoundDown |= UnityEngine.Input.GetKeyDown(KeyCode.S) || UnityEngine.Input.GetKeyDown(KeyCode.DownArrow) || UnityEngine.Input.GetKeyDown(KeyCode.C);
                roarDown |= UnityEngine.Input.GetKeyDown(KeyCode.F) || UnityEngine.Input.GetKeyDown(KeyCode.E);
            }
            catch { }

            // 3. Direct Gamepad Hardware Fallback
            var pad = Gamepad.current;
            if (pad != null)
            {
                Vector2 stick = pad.leftStick.ReadValue();
                Vector2 dpad = pad.dpad.ReadValue();
                if (stick.sqrMagnitude > 0.04f) move = stick;
                else if (dpad.sqrMagnitude > 0.04f) move = dpad;

                jumpDown |= pad.buttonSouth.wasPressedThisFrame;
                jumpHeld |= pad.buttonSouth.isPressed;
                jumpUp |= pad.buttonSouth.wasReleasedThisFrame;

                dashDown |= pad.rightTrigger.wasPressedThisFrame || pad.rightShoulder.wasPressedThisFrame;
                groundPoundDown |= pad.buttonEast.wasPressedThisFrame || pad.buttonNorth.wasPressedThisFrame;
                roarDown |= pad.buttonWest.wasPressedThisFrame;
            }

            // 4. Virtual Input Bridge (From On-Screen Mobile UI Joystick & Buttons)
            if (VirtualInputBridge.MoveVector.sqrMagnitude > 0.01f)
            {
                move = VirtualInputBridge.MoveVector;
            }
            jumpDown |= VirtualInputBridge.JumpDown;
            jumpHeld |= VirtualInputBridge.JumpHeld;
            jumpUp |= VirtualInputBridge.JumpUp;
            dashDown |= VirtualInputBridge.DashDown;
            groundPoundDown |= VirtualInputBridge.GroundPoundDown;
            roarDown |= VirtualInputBridge.RoarDown;

            // Reset single-frame triggers from the bridge
            VirtualInputBridge.ConsumeFrameTriggers();

            // Populate current frame input
            _currentInput.MoveVector = move;
            _currentInput.JumpDown = jumpDown;
            _currentInput.JumpHeld = jumpHeld;
            _currentInput.JumpUp = jumpUp;
            _currentInput.DashDown = dashDown;
            _currentInput.GroundPoundDown = groundPoundDown || (move.y < -0.6f && jumpDown);
            _currentInput.RoarDown = roarDown;
        }
    }
}
