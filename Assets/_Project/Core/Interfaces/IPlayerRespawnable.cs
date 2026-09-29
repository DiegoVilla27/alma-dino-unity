using UnityEngine;

namespace AlmaDino.Core.Interfaces
{
    public interface IPlayerRespawnable
    {
        void SetCheckpoint(Vector2 position);
        void RespawnAt(Vector2 position);
        void KillAndRespawn();
    }
}
