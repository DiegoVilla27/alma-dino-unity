using System.Collections.Generic;
using AlmaGame.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AlmaGame.Systems
{
    // One per level (System_GameProgress prefab). Bridges the save file, Alma and the progression
    // pieces: on start it loads the save, applies unlocked abilities (plus this level's starting ones)
    // and, if the save is from this level, puts Alma at the saved checkpoint. Checkpoints and altars
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
            _data.DoubleJump |= _startWithDoubleJump;
            _data.GroundPound |= _startWithGroundPound;
            _data.Dash |= _startWithDash;
            _data.Roar |= _startWithRoar;
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

        public bool IsUnlocked(AlmaAbility ability) => ability switch
        {
            AlmaAbility.DoubleJump => _data.DoubleJump,
            AlmaAbility.GroundPound => _data.GroundPound,
            AlmaAbility.Dash => _data.Dash,
            AlmaAbility.Roar => _data.Roar,
            _ => false,
        };

        public void UnlockAbility(AlmaAbility ability)
        {
            switch (ability)
            {
                case AlmaAbility.DoubleJump: _data.DoubleJump = true; break;
                case AlmaAbility.GroundPound: _data.GroundPound = true; break;
                case AlmaAbility.Dash: _data.Dash = true; break;
                case AlmaAbility.Roar: _data.Roar = true; break;
            }
            if (_alma != null) _alma.SetUnlocked(ability, true);
            SaveSystem.Save(_data);
        }

        public void CheckpointReached(Vector2 respawnPoint)
        {
            _data.HasCheckpoint = true;
            _data.CheckpointX = respawnPoint.x;
            _data.CheckpointY = respawnPoint.y;
            SaveSystem.Save(_data);
        }

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
