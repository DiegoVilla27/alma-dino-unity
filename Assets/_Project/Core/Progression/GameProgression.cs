using System;
using System.Collections.Generic;
using AlmaDino.Core.Interfaces;
using UnityEngine;

namespace AlmaDino.Core.Progression
{
    public enum EggType
    {
        GreenEgg = 1,
        BlueEgg = 2,
        PurpleEgg = 3,
        RedEgg = 4
    }

    /// <summary>
    /// Servicio de persistencia y progresión global de habilidades y rescate de huevos de Alma.
    /// Garantiza que una vez que el jugador despierta una mecánica o rescata a una cría,
    /// esta permanezca disponible de forma permanente en todos los niveles subsiguientes.
    /// </summary>
    public static class GameProgression
    {
        private const string PREF_KEY_PREFIX = "AlmaDino_AbilityUnlocked_";
        private const string PREF_EGG_PREFIX = "AlmaDino_EggRescued_";
        private const string PREF_WORLD_PREFIX = "AlmaDino_WorldCompleted_";
        private static readonly HashSet<AbilityType> _unlockedAbilities = new();
        private static readonly HashSet<EggType> _rescuedEggs = new();
        private static readonly HashSet<int> _completedWorlds = new();
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

        public static void RescueEgg(EggType egg)
        {
            if (!_isInitialized) Initialize();

            if (_rescuedEggs.Add(egg))
            {
                PlayerPrefs.SetInt(PREF_EGG_PREFIX + egg, 1);
                PlayerPrefs.Save();
                Debug.Log($"<color=#00F5D4><b>[GameProgression]</b> ¡Huevo '{egg}' rescatado y guardado en la partida!</color>");
            }
        }

        public static bool IsEggRescued(EggType egg)
        {
            if (!_isInitialized) Initialize();
            return _rescuedEggs.Contains(egg);
        }

        public static int RescuedEggCount
        {
            get
            {
                if (!_isInitialized) Initialize();
                return _rescuedEggs.Count;
            }
        }

        public static void CompleteWorld(int worldIndex)
        {
            if (!_isInitialized) Initialize();

            if (_completedWorlds.Add(worldIndex))
            {
                PlayerPrefs.SetInt(PREF_WORLD_PREFIX + worldIndex, 1);
                PlayerPrefs.Save();
                Debug.Log($"<color=#FFD700><b>[GameProgression]</b> ¡Mundo {worldIndex} completado con éxito!</color>");
            }
        }

        public static bool IsWorldCompleted(int worldIndex)
        {
            if (!_isInitialized) Initialize();
            return _completedWorlds.Contains(worldIndex);
        }

        /// <summary>
        /// Asegura que el nivel actual tenga activas las mecánicas mínimas que el jugador ya debería poseer
        /// por diseño del mundo si se entra directamente a dicho nivel.
        /// </summary>
        public static void EnsureLevelBaseline(string sceneName)
        {
            if (!_isInitialized) Initialize();

            // Tras el altar del 2-1, conservar Doble Salto y Pisotón también al entrar al pantano.
            if (sceneName == "Level_2_2" || sceneName == "Level_2_3" || sceneName == "Level_2_4"
                || sceneName == "Boss_2" || sceneName == "Level_3_1")
            {
                UnlockAbility(AbilityType.DoubleJump);
                UnlockAbility(AbilityType.GroundPound);
            }

            if (sceneName == "Level_3_2" || sceneName == "Level_3_3" || sceneName == "Level_3_4" || sceneName == "Boss_3" || sceneName == "Level_4_1" || sceneName == "Level_4_2" || sceneName == "Level_4_3" || sceneName == "Level_4_4")
            {
                UnlockAbility(AbilityType.DoubleJump);
                UnlockAbility(AbilityType.GroundPound);
                UnlockAbility(AbilityType.Dash);
            }

            if (sceneName == "Level_4_2" || sceneName == "Level_4_3" || sceneName == "Level_4_4") UnlockAbility(AbilityType.Roar);

            if (sceneName == "Level_1_2" || sceneName == "Level_1_3" || sceneName == "Level_1_4" || sceneName == "Boss_1"
                || sceneName == "Level_2_1")
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

            _rescuedEggs.Clear();
            foreach (EggType egg in Enum.GetValues(typeof(EggType)))
            {
                if (PlayerPrefs.GetInt(PREF_EGG_PREFIX + egg, 0) == 1
                    || (egg == EggType.PurpleEgg && PlayerPrefs.GetInt(PREF_EGG_PREFIX + "YellowEgg", 0) == 1))
                {
                    _rescuedEggs.Add(egg);
                }
            }

            _completedWorlds.Clear();
            for (int w = 1; w <= 4; w++)
            {
                if (PlayerPrefs.GetInt(PREF_WORLD_PREFIX + w, 0) == 1)
                {
                    _completedWorlds.Add(w);
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

            _rescuedEggs.Clear();
            foreach (EggType egg in Enum.GetValues(typeof(EggType)))
            {
                PlayerPrefs.DeleteKey(PREF_EGG_PREFIX + egg);
            }

            PlayerPrefs.DeleteKey(PREF_EGG_PREFIX + "YellowEgg");
            _completedWorlds.Clear();
            for (int w = 1; w <= 4; w++)
            {
                PlayerPrefs.DeleteKey(PREF_WORLD_PREFIX + w);
            }

            PlayerPrefs.Save();
            _isInitialized = true;
            Debug.Log("<color=#FFCC00><b>[GameProgression]</b> Progresión de habilidades, huevos y mundos reseteada a estado inicial.</color>");
        }
    }
}
