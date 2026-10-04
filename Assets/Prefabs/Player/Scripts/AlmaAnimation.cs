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
        private static readonly int GroundPound = Animator.StringToHash("GroundPound");
        private static readonly int Dash = Animator.StringToHash("Dash");
        private static readonly int Roar = Animator.StringToHash("Roar");
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
            // Pound and Roar end in gameplay before their clips do; hold the visual state until the
            // clip has played once. Control is never delayed: the motor has already finished the action.
            _animator.SetBool(GroundPound, _motor.IsGroundPounding
                || (_motor.IsGrounded && IsFinishingClip(GroundPound)));
            _animator.SetBool(Dash, _motor.IsDashing);
            _animator.SetBool(Roar, _motor.IsRoaring
                || (!_motor.IsDashing && !_motor.IsGroundPounding && IsFinishingClip(Roar)));
        }

        // State names match their parameter names, so the parameter hash is also the state hash.
        private bool IsFinishingClip(int stateHash)
        {
            AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);
            return state.shortNameHash == stateHash && state.normalizedTime < 1f;
        }
    }
}
