using AlmaDino.Core.Interfaces;
using UnityEngine;
namespace AlmaDino.Features.Environment
{
    public sealed class DashRefillRespawnReset2D : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour _playerSource;
        [SerializeField] private DashRefillPickup2D[] _spores;
        private IPlayerRespawnable _player;
        private void Start()
        {
            _player = _playerSource as IPlayerRespawnable;
            if (_player != null) _player.OnRespawned += RestoreSpores;
        }
        private void OnDestroy()
        {
            if (_player != null) _player.OnRespawned -= RestoreSpores;
        }
        private void RestoreSpores(Vector2 position)
        {
            foreach (var spore in _spores) if (spore != null) spore.Restore();
        }
    }
}
