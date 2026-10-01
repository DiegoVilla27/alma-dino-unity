using UnityEngine;

namespace AlmaDino.Features.Environment
{
    public class ParallaxLayer2D : MonoBehaviour
    {
        [SerializeField] private Transform _camera;
        [SerializeField] private Vector2 _cameraMotionFactor = new Vector2(0.6f, 0.6f);
        private Vector3 _startPosition;
        private Vector3 _cameraStartPosition;

        private void Start()
        {
            _startPosition = transform.position;
            if (_camera != null) _cameraStartPosition = _camera.position;
        }

        private void LateUpdate()
        {
            if (_camera == null) return;
            var movement = _camera.position - _cameraStartPosition;
            transform.position = _startPosition + new Vector3(
                movement.x * _cameraMotionFactor.x, movement.y * _cameraMotionFactor.y, 0f);
        }
    }
}
