using System;
using AlmaDino.Core.Interfaces;
using UnityEngine;

namespace AlmaDino.Core.Events
{
    [CreateAssetMenu(fileName = "AbilityUnlockedEventChannel", menuName = "AlmaDino/Events/Ability Unlocked Event Channel")]
    public class AbilityUnlockedEventChannelSO : ScriptableObject
    {
        public event Action<AbilityType> OnAbilityUnlocked;

        public void Raise(AbilityType abilityType)
        {
            OnAbilityUnlocked?.Invoke(abilityType);
        }
    }
}
