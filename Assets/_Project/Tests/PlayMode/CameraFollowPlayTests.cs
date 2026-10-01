using System.Collections;
using AlmaDino.Features.Camera;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AlmaDino.Tests.PlayMode
{
    public class CameraFollowPlayTests
    {
        private GameObject _target;
        private GameObject _cameraObject;
        private Rigidbody2D _body;
        private Camera _camera;

        [SetUp]
        public void SetUp()
        {
            _target = new GameObject("Camera_Test_Target");
            _body = _target.AddComponent<Rigidbody2D>();
            _body.gravityScale = 0f;
            _cameraObject = new GameObject("Camera_Test");
            _camera = _cameraObject.AddComponent<Camera>();
            _cameraObject.transform.position = new Vector3(0f, 1.5f, -10f);
            var follow = _cameraObject.AddComponent<Camera2DFollow>();
            follow.SetTarget(_target.transform);
            follow.SetBounds(new Vector2(-100f, -100f), new Vector2(100f, 100f));
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_target);
            Object.Destroy(_cameraObject);
        }

        [UnityTest]
        public IEnumerator CameraLooksAheadInBothDirections()
        {
            _body.linearVelocity = Vector2.right * 7f;
            yield return new WaitForSeconds(1.5f);
            Assert.That(_camera.transform.position.x - _target.transform.position.x,
                Is.InRange(.15f, 1.5f), "Rightward movement must reveal the right path.");
            _body.linearVelocity = Vector2.left * 7f;
            yield return new WaitForSeconds(1.5f);
            Assert.That(_camera.transform.position.x - _target.transform.position.x,
                Is.InRange(-1.5f, -.15f), "Leftward movement must reveal the left path.");
        }

        [UnityTest]
        public IEnumerator ZoomRemainsSixInPortraitAndLandscape()
        {
            _camera.aspect = 9f / 16f;
            yield return null;
            Assert.That(_camera.orthographicSize, Is.EqualTo(6f));
            _camera.aspect = 16f / 9f;
            yield return null;
            Assert.That(_camera.orthographicSize, Is.EqualTo(6f));
        }
    }
}
