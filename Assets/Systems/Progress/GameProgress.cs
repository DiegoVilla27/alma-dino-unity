using System.Collections.Generic;
using AlmaGame.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AlmaGame.Systems
{
    // One per level (System_GameProgress prefab). Bridges the save file, Alma and the progression
    // pieces: on start it loads the save and, if the save is from this level, puts Alma at the saved
    // checkpoint. Abilities belong to the level: Alma gets this level's starting ones (what earlier levels
    // gave) plus those already unlocked in this level when resuming at one of its checkpoints, so starting
    // a level always plays it as designed (an altar is never spent before it is reached). Checkpoints and altars
    // report to it; it autosaves when a checkpoint is reached or an ability is unlocked (GDD 9.3).
    [DisallowMultipleComponent]
    public sealed class GameProgress : MonoBehaviour
    {
        [Header("Abilities available when this level starts (even with no save)")]
        [SerializeField] private bool _startWithDoubleJump;
        [SerializeField] private bool _startWithGroundPound;
        [SerializeField] private bool _startWithDash;
        [SerializeField] private bool _startWithRoar;

        [Header("Loading")]
        [SerializeField] private bool _loadSave = true;
        [SerializeField] private bool _resumeAtSavedCheckpoint = true;

        public static GameProgress Instance { get; private set; }

        private static readonly List<ICheckpoint> s_checkpoints = new List<ICheckpoint>();
        private AlmaMotor2D _alma;
        private SaveData _data;
        private readonly HashSet<AlmaAbility> _levelAbilities = new HashSet<AlmaAbility>();

        public SaveData Data => _data;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Only one GameProgress per scene; this one is ignored.", this);
                enabled = false;
                return;
            }
            Instance = this;
            _alma = FindAnyObjectByType<AlmaMotor2D>();
            _data = (_loadSave ? SaveSystem.Load() : null) ?? new SaveData();

            string level = SceneManager.GetActiveScene().name;
            if (_data.Level != level)
            {
                // Entering a new level: the old checkpoint doesn't apply here.
                _data.Level = level;
                _data.HasCheckpoint = false;
            }
            bool resuming = _resumeAtSavedCheckpoint && _data.HasCheckpoint;
            if (!resuming) _data.LevelUnlocks.Clear(); // starting from the beginning: unlock it all again

            if (_startWithDoubleJump) _levelAbilities.Add(AlmaAbility.DoubleJump);
            if (_startWithGroundPound) _levelAbilities.Add(AlmaAbility.GroundPound);
            if (_startWithDash) _levelAbilities.Add(AlmaAbility.Dash);
            if (_startWithRoar) _levelAbilities.Add(AlmaAbility.Roar);
            foreach (string name in _data.LevelUnlocks)
                if (System.Enum.TryParse(name, out AlmaAbility ability)) _levelAbilities.Add(ability);
            foreach (AlmaAbility ability in _levelAbilities) SetRecord(ability);
        }

        // Start runs after every Awake, so Alma, checkpoints and altars are ready.
        private void Start()
        {
            if (!enabled || _alma == null) return;
            foreach (AlmaAbility ability in System.Enum.GetValues(typeof(AlmaAbility)))
                _alma.SetUnlocked(ability, IsUnlocked(ability));

            if (_resumeAtSavedCheckpoint && _data.HasCheckpoint)
            {
                var position = new Vector2(_data.CheckpointX, _data.CheckpointY);
                _alma.SetRespawnPosition(position);
                _alma.Respawn();
                foreach (var checkpoint in s_checkpoints)
                    checkpoint.SetLit(Vector2.Distance(checkpoint.RespawnPoint, position) < 0.25f, false);
            }
        }

        private void Update()
        {
            if (_data != null) _data.PlayTimeSeconds += Time.unscaledDeltaTime;
        }

        private void OnApplicationQuit()
        {
            if (enabled && _data != null) SaveSystem.Save(_data);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // Whether Alma has the ability in this level (see the class comment).
        public bool IsUnlocked(AlmaAbility ability) => _levelAbilities.Contains(ability);

        public void UnlockAbility(AlmaAbility ability)
        {
            _levelAbilities.Add(ability);
            if (!_data.LevelUnlocks.Contains(ability.ToString())) _data.LevelUnlocks.Add(ability.ToString());
            SetRecord(ability);
            if (_alma != null) _alma.SetUnlocked(ability, true);
            SaveSystem.Save(_data);
        }

        // Overall record of abilities ever obtained (kept in the save, e.g. for a future level select).
        private void SetRecord(AlmaAbility ability)
        {
            switch (ability)
            {
                case AlmaAbility.DoubleJump: _data.DoubleJump = true; break;
                case AlmaAbility.GroundPound: _data.GroundPound = true; break;
                case AlmaAbility.Dash: _data.Dash = true; break;
                case AlmaAbility.Roar: _data.Roar = true; break;
            }
        }

        public void CheckpointReached(Vector2 respawnPoint)
        {
            _data.HasCheckpoint = true;
            _data.CheckpointX = respawnPoint.x;
            _data.CheckpointY = respawnPoint.y;
            SaveSystem.Save(_data);
        }

        public bool IsLevelCompleted(string level) => !string.IsNullOrEmpty(level) && _data.CompletedLevels.Contains(level);

        // Exit portal: marks this level as completed and moves the save on to the next level
        // (without a checkpoint, so the next level starts from its beginning), then saves.
        public void CompleteLevel(string nextLevel)
        {
            string current = SceneManager.GetActiveScene().name;
            if (!_data.CompletedLevels.Contains(current)) _data.CompletedLevels.Add(current);
            if (!string.IsNullOrEmpty(nextLevel))
            {
                _data.Level = nextLevel;
                _data.HasCheckpoint = false;
                _data.LevelUnlocks.Clear();
            }
            SaveSystem.Save(_data);
        }

        public bool IsEggRescued(string eggId) => !string.IsNullOrEmpty(eggId) && _data.RescuedEggs.Contains(eggId);

        public void RescueEgg(string eggId)
        {
            if (string.IsNullOrEmpty(eggId) || _data.RescuedEggs.Contains(eggId)) return;
            _data.RescuedEggs.Add(eggId);
            SaveSystem.Save(_data);
        }

        public static void Register(ICheckpoint checkpoint)
        {
            if (!s_checkpoints.Contains(checkpoint)) s_checkpoints.Add(checkpoint);
        }

        public static void Unregister(ICheckpoint checkpoint) => s_checkpoints.Remove(checkpoint);

        // Only the latest checkpoint burns brightly; the others go back to embers.
        public static void SetCurrentCheckpoint(ICheckpoint current)
        {
            foreach (var checkpoint in s_checkpoints)
                if (checkpoint != current) checkpoint.SetLit(false, false);
        }

        [ContextMenu("Borrar partida guardada")]
        private void DeleteSave()
        {
            SaveSystem.Delete();
            Debug.Log($"Save deleted: {SaveSystem.FilePath}");
        }
    }

    // Implemented by the checkpoint nest so GameProgress can light the saved one on load.
    public interface ICheckpoint
    {
        Vector2 RespawnPoint { get; }
        void SetLit(bool lit, bool celebrate);
    }
}
