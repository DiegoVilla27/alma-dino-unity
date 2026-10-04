using UnityEngine;

namespace AlmaGame.Player
{
    [DisallowMultipleComponent, RequireComponent(typeof(AlmaMotor2D))]
    public sealed class AlmaInput : MonoBehaviour
    {
        private AlmaMotor2D _motor;
        private void Awake() => _motor = GetComponent<AlmaMotor2D>();

        private void Update()
        {
            // Uses the Input Manager already configured in this project.
            float move = Input.GetAxisRaw("Horizontal");
            _motor.SetInput(Mathf.Abs(move) < 0.15f ? 0f : move,
                Input.GetButtonDown("Jump"), Input.GetButton("Jump"));
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
