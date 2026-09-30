using AlmaDino.Core.Events;
using AlmaDino.Core.Interfaces;
using UnityEngine;

namespace AlmaDino.Features.MobileUI.Controllers
{
    public class MobileHUDContextualController : MonoBehaviour
    {
        [Header("Button References")]
        [SerializeField] private GameObject _jumpButtonObject;
        [SerializeField] private GameObject _dashButtonObject;
        [SerializeField] private GameObject _poundButtonObject;
        [SerializeField] private GameObject _roarButtonObject;

        [Header("Event Listening")]
        [SerializeField] private AbilityUnlockedEventChannelSO _abilityUnlockedChannel;

        private IAbilityUnlockable _player;

        private void Awake()
        {
            FindPlayer();
            RefreshButtons();
        }

        private void OnEnable()
        {
            if (_abilityUnlockedChannel != null)
            {
                _abilityUnlockedChannel.OnAbilityUnlocked += HandleAbilityUnlocked;
            }
            RefreshButtons();
        }

        private void Start()
        {
            FindPlayer();
            RefreshButtons();
        }

        private void FindPlayer()
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                _player = playerObj.GetComponent<IAbilityUnlockable>();
            }

            if (_player == null)
            {
                var allMBs = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
                foreach (var mb in allMBs)
                {
                    if (mb is IAbilityUnlockable unlockable)
                    {
                        _player = unlockable;
                        break;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (_abilityUnlockedChannel != null)
            {
                _abilityUnlockedChannel.OnAbilityUnlocked -= HandleAbilityUnlocked;
            }
        }

        private void HandleAbilityUnlocked(AbilityType abilityType)
        {
            RefreshButtons();
        }

        public void RefreshButtons()
        {
            bool hasDash = _player != null && _player.IsDashUnlocked;
            bool hasPound = _player != null && _player.IsGroundPoundUnlocked;
            bool hasRoar = _player != null && _player.IsRoarUnlocked;

            if (_dashButtonObject != null) _dashButtonObject.SetActive(hasDash);
            if (_poundButtonObject != null) _poundButtonObject.SetActive(hasPound);
            if (_roarButtonObject != null) _roarButtonObject.SetActive(hasRoar);
        }
    }
}
