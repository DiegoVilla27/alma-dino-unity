using System;
using UnityEngine;

namespace AlmaDino.Core.Interfaces
{
    public interface IPlayerRespawnable
    {
        event Action<Vector2> OnRespawned;
        void SetCheckpoint(Vector2 position);
        void RespawnAt(Vector2 position);
        void KillAndRespawn();
    }
}
