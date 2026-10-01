using AlmaDino.Core.Interfaces;
using UnityEngine;

namespace AlmaDino.Features.Environment
{
    public class BreakableGroundRespawnReset2D : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour _playerSource;
        [SerializeField] private BreakableGround2D[] _grounds;
        private IPlayerRespawnable _player;

        private void Start()
        {
            _player = _playerSource as IPlayerRespawnable;
            if (_player != null) _player.OnRespawned += RestoreGrounds;
        }

        private void OnDestroy()
        {
            if (_player != null) _player.OnRespawned -= RestoreGrounds;
        }

        private void RestoreGrounds(Vector2 position)
        {
            foreach (var ground in _grounds)
                if (ground != null) ground.Restore();
        }
    }
}
