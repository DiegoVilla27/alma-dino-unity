using UnityEngine;

namespace AlmaGame.Player
{
    [DisallowMultipleComponent, RequireComponent(typeof(AlmaMotor2D), typeof(Animator), typeof(SpriteRenderer))]
    public sealed class AlmaAnimation : MonoBehaviour
    {
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int Grounded = Animator.StringToHash("Grounded");
        private static readonly int VerticalSpeed = Animator.StringToHash("VerticalSpeed");
        private static readonly int RunRate = Animator.StringToHash("RunRate");
        private AlmaMotor2D _motor;
        private Animator _animator;
        private SpriteRenderer _renderer;

        private void Awake()
        {
            _motor = GetComponent<AlmaMotor2D>();
            _animator = GetComponent<Animator>();
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            float speed = Mathf.Abs(_motor.Velocity.x);
            _renderer.flipX = _motor.FacingDirection < 0;
            _animator.SetFloat(Speed, speed);
            _animator.SetBool(Grounded, _motor.IsGrounded);
            _animator.SetFloat(VerticalSpeed, _motor.Velocity.y);
            _animator.SetFloat(RunRate, Mathf.Clamp(speed / _motor.Settings.MoveSpeed, 0.5f, 1.2f));
        }
    }
}
