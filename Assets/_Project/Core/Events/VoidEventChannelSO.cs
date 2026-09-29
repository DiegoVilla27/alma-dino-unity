using System;
using System.Collections.Generic;
using UnityEngine;

namespace AlmaDino.Core.Events
{
    [CreateAssetMenu(fileName = "NewVoidEventChannel", menuName = "AlmaDino/Events/Void Event Channel")]
    public class VoidEventChannelSO : ScriptableObject
    {
        private readonly HashSet<Action> _listeners = new();

        public void Register(Action listener)
        {
            if (listener != null)
                _listeners.Add(listener);
        }

        public void Unregister(Action listener)
        {
            if (listener != null)
                _listeners.Remove(listener);
        }

        public void Raise()
        {
            foreach (var listener in _listeners)
            {
                listener?.Invoke();
            }
        }
    }
}
