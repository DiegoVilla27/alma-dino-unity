using System;
using System.Collections.Generic;
using AlmaDino.Core.Interfaces;
using UnityEngine;

namespace AlmaDino.Core.Progression
{
    /// <summary>
    /// Servicio de persistencia y progresión global de habilidades de Alma.
    /// Garantiza que una vez que el jugador despierta o desbloquea una mecánica,
    /// esta permanezca disponible de forma permanente en todos los niveles subsiguientes.
    /// </summary>
    public static class GameProgression
    {
        private const string PREF_KEY_PREFIX = "AlmaDino_AbilityUnlocked_";
        private static readonly HashSet<AbilityType> _unlockedAbilities = new();
        private static bool _isInitialized = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            if (_isInitialized) return;
            LoadProgression();
            _isInitialized = true;
        }

        public static bool IsAbilityUnlocked(AbilityType ability)
        {
            if (!_isInitialized) Initialize();
            return _unlockedAbilities.Contains(ability);
        }

        public static void UnlockAbility(AbilityType ability)
        {
            if (!_isInitialized) Initialize();

            if (_unlockedAbilities.Add(ability))
            {
                PlayerPrefs.SetInt(PREF_KEY_PREFIX + ability, 1);
                PlayerPrefs.Save();
                Debug.Log($"<color=#00FF88><b>[GameProgression]</b> Habilidad '{ability}' guardada permanentemente en la partida.</color>");
            }
        }

        /// <summary>
        /// Asegura que el nivel actual tenga activas las mecánicas mínimas que el jugador ya debería poseer
        /// por diseño del mundo si se entra directamente a dicho nivel.
        /// </summary>
        public static void EnsureLevelBaseline(string sceneName)
        {
            if (!_isInitialized) Initialize();

            // Nivel 1-2 en adelante: el jugador ya superó el despertar en 1-1, por lo que el Doble Salto es obligatorio
            if (sceneName == "Level_1_2" || sceneName == "Level_1_3" || sceneName == "Level_1_4")
            {
                UnlockAbility(AbilityType.DoubleJump);
            }
        }

        public static void LoadProgression()
        {
            _unlockedAbilities.Clear();
            foreach (AbilityType type in Enum.GetValues(typeof(AbilityType)))
            {
                if (PlayerPrefs.GetInt(PREF_KEY_PREFIX + type, 0) == 1)
                {
                    _unlockedAbilities.Add(type);
                }
            }
        }

        public static void ResetProgression()
        {
            _unlockedAbilities.Clear();
            foreach (AbilityType type in Enum.GetValues(typeof(AbilityType)))
            {
                PlayerPrefs.DeleteKey(PREF_KEY_PREFIX + type);
            }
            PlayerPrefs.Save();
            _isInitialized = true;
            Debug.Log("<color=#FFCC00><b>[GameProgression]</b> Progresión de habilidades reseteada a estado inicial.</color>");
        }
    }
}
