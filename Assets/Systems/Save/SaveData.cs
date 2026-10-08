using System;
using System.Collections.Generic;

namespace AlmaGame.Systems
{
    // Everything the game keeps between sessions (GDD 9.3). Serialized with JsonUtility, so only
    // public fields of supported types. Bump `Version` if the layout changes in an incompatible way.
    [Serializable]
    public sealed class SaveData
    {
        public const int CurrentVersion = 1;

        public int Version = CurrentVersion;
        public string Level = string.Empty;
        public bool HasCheckpoint;
        public float CheckpointX;
        public float CheckpointY;
        public List<string> RescuedEggs = new List<string>();
        public List<string> CompletedLevels = new List<string>();
        public bool DoubleJump;
        public bool GroundPound;
        public bool Dash;
        public bool Roar;
        // Abilities unlocked in `Level` since it was last started from its beginning (names of AlmaAbility).
        // Resuming at a checkpoint of that level gives them back; the flags above are the overall record.
        public List<string> LevelUnlocks = new List<string>();
        public float PlayTimeSeconds;
        public string SavedAtUtc = string.Empty;
    }
}
