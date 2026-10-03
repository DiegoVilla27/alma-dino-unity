using System;
using AlmaDino.Features.Player.Controllers;
using AlmaDino.Features.Player.Models;
using UnityEngine;

namespace AlmaDino.Features.Player.Components
{
    /// <summary>
    /// Controlador de animación por sprites para Alma.
    /// Administra secuencias de frames (Idle, Correr, Salto, etc.) y actualiza el SpriteRenderer.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerSpriteAnimator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        [Header("Idle Animation")]
        [SerializeField] private Sprite[] _idleFrames;
        [SerializeField] private float _idleFps = 10f;

        [Header("Run Animation")]
        [SerializeField] private Sprite[] _runFrames;
        [SerializeField] private float _runFps = 10f;

        [Header("Airborne Animation")]
        [SerializeField] private Sprite[] _jumpFrames;
        [SerializeField] private float _jumpFps = 12f;
        [SerializeField] private Sprite[] _fallFrames;
        [SerializeField] private float _fallFps = 10f;

        [Header("Other Animations")]
        [SerializeField] private Sprite[] _walkFrames;
        [SerializeField] private float _walkFps = 10f;
        [SerializeField] private Sprite[] _deadFrames;
        [SerializeField] private float _deadFps = 10f;

        private float _animTimer = 0f;
        private PlayerStateEnum _currentState = PlayerStateEnum.Idle;

        public SpriteRenderer SpriteRenderer => _spriteRenderer;
        public Sprite[] IdleFrames => _idleFrames;
        public Sprite[] RunFrames => _runFrames;
        public Sprite[] JumpFrames => _jumpFrames;
        public Sprite[] FallFrames => _fallFrames;
        public Sprite[] WalkFrames => _walkFrames;
        public Sprite[] DeadFrames => _deadFrames;
        public float IdleFps => _idleFps;
        public float RunFps => _runFps;
        public float JumpFps => _jumpFps;
        public float FallFps => _fallFps;

        private void Awake()
        {
            if (_playerController == null)
            {
                _playerController = GetComponent<PlayerController>() ?? GetComponentInParent<PlayerController>();
            }

            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            CleanLegacyVisuals();
        }

        private void OnEnable()
        {
            if (_playerController != null)
            {
                _playerController.OnStateChanged += HandleStateChanged;
            }
        }

        private void OnDisable()
        {
            if (_playerController != null)
            {
                _playerController.OnStateChanged -= HandleStateChanged;
            }
        }

        private void HandleStateChanged(PlayerStateEnum newState)
        {
            if (_currentState != newState)
            {
                _currentState = newState;
                _animTimer = 0f; // Reiniciar ciclo en cambio de estado
            }
        }

        private void Update()
        {
            if (_spriteRenderer == null) return;

            PlayerStateEnum state = _playerController != null && _playerController.StateMachine != null
                ? _playerController.StateMachine.CurrentStateType
                : _currentState;

            switch (state)
            {
                case PlayerStateEnum.Idle:
                    PlaySequence(_idleFrames, _idleFps);
                    break;

                case PlayerStateEnum.Run:
                    if (_runFrames != null && _runFrames.Length > 0)
                    {
                        PlaySequence(_runFrames, _runFps);
                    }
                    else
                    {
                        PlayFallbackFrame();
                    }
                    break;

                case PlayerStateEnum.Jump:
                case PlayerStateEnum.DoubleJump:
                    if (_jumpFrames != null && _jumpFrames.Length > 0)
                    {
                        PlaySequence(_jumpFrames, _jumpFps);
                    }
                    else
                    {
                        PlayFallbackFrame();
                    }
                    break;

                case PlayerStateEnum.Fall:
                    if (_fallFrames != null && _fallFrames.Length > 0)
                    {
                        PlaySequence(_fallFrames, _fallFps);
                    }
                    else if (_jumpFrames != null && _jumpFrames.Length > 0)
                    {
                        // Fallback al frame de caída/apex de la animación de salto
                        int fallIdx = Mathf.Clamp(7, 0, _jumpFrames.Length - 1);
                        _spriteRenderer.sprite = _jumpFrames[fallIdx];
                    }
                    else
                    {
                        PlayFallbackFrame();
                    }
                    break;

                default:
                    PlayFallbackFrame();
                    break;
            }
        }

        private void PlaySequence(Sprite[] frames, float fps)
        {
            if (frames == null || frames.Length == 0)
            {
                PlayFallbackFrame();
                return;
            }

            _animTimer += Time.deltaTime;
            int frameIndex = Mathf.FloorToInt(_animTimer * fps) % frames.Length;
            if (frames[frameIndex] != null)
                _spriteRenderer.sprite = frames[frameIndex];
            else
                PlayFallbackFrame();
        }

        private void PlayFallbackFrame()
        {
            if (_idleFrames == null) return;
            foreach (var frame in _idleFrames)
            {
                if (frame == null) continue;
                _spriteRenderer.sprite = frame;
                return;
            }
        }

        /// <summary>
        /// Oculta o desactiva primitivas temporales de prototipo (ojos y pupilas de cubos verdes)
        /// y normaliza el SpriteRenderer.
        /// </summary>
        public void CleanLegacyVisuals()
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = Color.white;
                _spriteRenderer.drawMode = SpriteDrawMode.Simple;

                Transform visualTr = _spriteRenderer.transform;
                for (int i = visualTr.childCount - 1; i >= 0; i--)
                {
                    Transform child = visualTr.GetChild(i);
                    string lowerName = child.name.ToLowerInvariant();
                    if (lowerName.Contains("eye") || lowerName.Contains("pupil") || lowerName.Contains("placeholder"))
                    {
                        child.gameObject.SetActive(false);
                    }
                }

                if (_idleFrames != null && _idleFrames.Length > 0 && _spriteRenderer.sprite == null)
                {
                    _spriteRenderer.sprite = _idleFrames[0];
                }
            }
        }

        public void SetIdleFrames(Sprite[] frames, float fps = 8f)
        {
            _idleFrames = frames;
            _idleFps = fps;
            CleanLegacyVisuals();
        }

        public void SetRunFrames(Sprite[] frames, float fps = 10f)
        {
            _runFrames = frames;
            _runFps = fps;
        }

        public void SetJumpFrames(Sprite[] frames, float fps = 12f)
        {
            _jumpFrames = frames;
            _jumpFps = fps;
        }

        public void SetFallFrames(Sprite[] frames, float fps = 10f)
        {
            _fallFrames = frames;
            _fallFps = fps;
        }

        public void SetWalkFrames(Sprite[] frames, float fps = 10f)
        {
            _walkFrames = frames;
            _walkFps = fps;
        }

        public void SetDeadFrames(Sprite[] frames, float fps = 10f)
        {
            _deadFrames = frames;
            _deadFps = fps;
        }

        public void SetSpriteRenderer(SpriteRenderer sr)
        {
            _spriteRenderer = sr;
            CleanLegacyVisuals();
        }
    }
}
