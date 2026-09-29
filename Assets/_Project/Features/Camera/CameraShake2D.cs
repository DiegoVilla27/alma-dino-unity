using AlmaDino.Core.Events;
using UnityEngine;

namespace AlmaDino.Features.Camera
{
    public class CameraShake2D : MonoBehaviour
    {
        [Header("Event Channel")]
        [SerializeField] private CameraShakeEventChannelSO _shakeChannel;

        [Header("Shake Config")]
        [SerializeField] private float _frequency = 25f;

        private float _shakeIntensity;
        private float _shakeDuration;
        private float _shakeTimer;
        private Vector3 _lastShakeOffset;

        private void OnEnable()
        {
            if (_shakeChannel != null)
                _shakeChannel.Register(Shake);
        }

        private void OnDisable()
        {
            if (_shakeChannel != null)
                _shakeChannel.Unregister(Shake);
        }

        public void Shake(float intensity, float duration)
        {
            _shakeIntensity = intensity;
            _shakeDuration = duration;
            _shakeTimer = 0f;
        }

        private void LateUpdate()
        {
            transform.localPosition -= _lastShakeOffset;
            _lastShakeOffset = Vector3.zero;

            if (_shakeTimer >= _shakeDuration) return;

            _shakeTimer += Time.deltaTime;
            float decay = 1f - Mathf.Clamp01(_shakeTimer / Mathf.Max(0.001f, _shakeDuration));

            float offsetX = (Mathf.PerlinNoise(Time.time * _frequency, 0f) - 0.5f) * 2f;
            float offsetY = (Mathf.PerlinNoise(0f, Time.time * _frequency) - 0.5f) * 2f;

            _lastShakeOffset = new Vector3(offsetX, offsetY, 0f) * (_shakeIntensity * decay);
            transform.localPosition += _lastShakeOffset;
        }
    }
}
