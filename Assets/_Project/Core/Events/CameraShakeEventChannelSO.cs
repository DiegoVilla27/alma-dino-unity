using System;
using System.Collections.Generic;
using UnityEngine;

namespace AlmaDino.Core.Events
{
    [CreateAssetMenu(fileName = "NewCameraShakeChannel", menuName = "AlmaDino/Events/Camera Shake Channel")]
    public class CameraShakeEventChannelSO : ScriptableObject
    {
        private readonly HashSet<Action<float, float>> _listeners = new();

        public void Register(Action<float, float> listener)
        {
            if (listener != null)
                _listeners.Add(listener);
        }

        public void Unregister(Action<float, float> listener)
        {
            if (listener != null)
                _listeners.Remove(listener);
        }

        public void Raise(float intensity, float duration)
        {
            foreach (var listener in _listeners)
            {
                listener?.Invoke(intensity, duration);
            }
        }
    }
}
