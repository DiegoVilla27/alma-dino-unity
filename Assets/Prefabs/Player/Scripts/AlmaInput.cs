using UnityEngine;

namespace AlmaGame.Player
{
    [DisallowMultipleComponent, RequireComponent(typeof(AlmaMotor2D))]
    public sealed class AlmaInput : MonoBehaviour
    {
        private AlmaMotor2D _motor;
        private bool _downHeld;
        private void Awake() => _motor = GetComponent<AlmaMotor2D>();

        private void Update()
        {
            if (AlmaTouchControls.InputLocked)
            {
                _motor.SetInput(0f, false, false);
                _downHeld = Input.GetAxisRaw("Vertical") < -0.5f;
                return;
            }
            // Uses the Input Manager already configured in this project, plus the on-screen touch buttons.
            float move = Input.GetAxisRaw("Horizontal");
            if (Mathf.Abs(move) < 0.15f) move = AlmaTouchControls.Move;
            bool touchJump = AlmaTouchControls.ConsumeJump();
            _motor.SetInput(Mathf.Abs(move) < 0.15f ? 0f : move,
                Input.GetButtonDown("Jump") || touchJump, Input.GetButton("Jump") || AlmaTouchControls.JumpHeld);

            // Ground pound: S / down arrow (Vertical axis) or C, on press only.
            bool down = Input.GetAxisRaw("Vertical") < -0.5f;
            if ((down && !_downHeld) || Input.GetKeyDown(KeyCode.C) || AlmaTouchControls.ConsumePound()) _motor.RequestGroundPound();
            _downHeld = down;

            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift) || AlmaTouchControls.ConsumeDash()) _motor.RequestDash();
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.F) || AlmaTouchControls.ConsumeRoar()) _motor.RequestRoar();
        }

        private void OnDisable()
        {
            if (_motor != null) _motor.SetInput(0f, false, false);
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused && _motor != null) _motor.SetInput(0f, false, false);
        }
    }
}
