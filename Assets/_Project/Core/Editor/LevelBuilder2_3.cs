#if UNITY_EDITOR
using System.IO;
using System.Linq;
using AlmaDino.Features.Camera;
using AlmaDino.Features.Enemies;
using AlmaDino.Features.Environment;
using AlmaDino.Features.Player.Controllers;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace AlmaDino.Core.Editor
{
    public static class LevelBuilder2_3
    {
        public const string ScenePath = "Assets/Scenes/World_2_Caves/Level_2_3.unity";
        private const string ConfigPath = "Assets/_Project/ScriptableObjects/CrystalEnemyConfig.asset";

        [MenuItem("Tools/Alma/Construir Nivel 2-3 - El Filo Resonante")]
        public static void BuildLevel2_3()
        {
            if (EditorApplication.isPlaying) return;
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!File.Exists(ScenePath)) AssetDatabase.CopyAsset(LevelBuilder2_2.ScenePath, ScenePath);
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var previous = GameObject.Find("--- LEVEL ---");
            if (previous != null) Object.DestroyImmediate(previous);
            var config = AssetDatabase.LoadAssetAtPath<CrystalEnemyConfigSO>(ConfigPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<CrystalEnemyConfigSO>();
                AssetDatabase.CreateAsset(config, ConfigPath);
                AssetDatabase.SaveAssets();
            }
            var root = new GameObject("--- LEVEL ---");
            var f = new CaveLevelSceneFactory(root.transform);
            var player = GameObject.Find("Alma (Player)").GetComponent<PlayerController>();
            player.transform.position = new Vector2(0f, 0.7f);
            CaveLevelSceneFactory.Set(player, "_doubleJumpUnlocked", true);
            CaveLevelSceneFactory.Set(player, "_groundPoundUnlocked", true);
            CaveLevelSceneFactory.Set(player, "_dashUnlocked", false);
            CaveLevelSceneFactory.Set(player, "_roarUnlocked", false);
            CaveLevelSceneFactory.Set(player, "_fallDeathY", -10f);
            SceneSetupValidator.SetupAlmaVisuals(player.gameObject);
            SceneSetupValidator.SetupTouchControls();
            var enemies = new ResonantEnemySceneFactory(f, root.transform, config, player);
            f.Platform("Beetle_Gallery_Floor", new Vector2(12.5f, -0.5f), new Vector2(31f, 1f));
            f.Platform("Left_Boundary", new Vector2(-3.5f, 4f), new Vector2(1f, 10f));
            f.Platform("First_Low_Ceiling", new Vector2(12f, 3f), new Vector2(8f, 1.2f));
            f.Platform("Second_Low_Ceiling", new Vector2(21f, 3f), new Vector2(7f, 1.2f));
            enemies.Beetle("Beetle_Tutorial", new Vector2(12f, 0.45f), new Vector2(11.4f, 12.6f));
            enemies.Beetle("Beetle_Gallery", new Vector2(21f, 0.45f), new Vector2(20.4f, 21.6f));
            f.Checkpoint("Checkpoint_After_Beetles", 26f, 0f);
            f.Platform("Puzzle_Step_1", new Vector2(30.5f, 0.7f), new Vector2(3f, 1f));
            f.Platform("Puzzle_Step_2", new Vector2(34.5f, 1.9f), new Vector2(3f, 1f));
            f.Platform("Puzzle_Approach", new Vector2(38f, 3.1f), new Vector2(3.2f, 1f));
            var slab = f.BreakableFloor("Cracked_Beetle_Floor", new Vector2(42.05f, 3.2f), new Vector2(4.9f, 0.8f));
            f.Platform("Puzzle_Safe_Catch", new Vector2(40.35f, -0.5f), new Vector2(1.9f, 1f));
            f.Platform("Beetle_Stone_Perch", new Vector2(42.2f, -0.2f), new Vector2(1.1f, 1f));
            f.Platform("Puzzle_Upper_Seal", new Vector2(44.8f, 9.3f), new Vector2(0.7f, 11.4f));
            f.Hazard("Puzzle_Stalagmites", new Vector2(44.4f, -0.9f), new Vector2(6.2f, 0.7f));
            enemies.Beetle("Beetle_Fragile_Puzzle", new Vector2(42f, 4.05f), new Vector2(41.8f, 42.1f));
            f.Platform("Puzzle_Exit_And_Checkpoint", new Vector2(54f, 1f), new Vector2(13f, 1f));
            f.Checkpoint("Checkpoint_Before_Bats", 58f, 1.5f);
            var reset = root.AddComponent<BreakableGroundRespawnReset2D>();
            CaveLevelSceneFactory.Set(reset, "_playerSource", player);
            var serialized = new SerializedObject(reset);
            serialized.FindProperty("_grounds").arraySize = 1;
            serialized.FindProperty("_grounds").GetArrayElementAtIndex(0).objectReferenceValue = slab;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            BuildBatAscent(f, enemies);
            f.Platform("Right_Exit_Boundary", new Vector2(101f, 11f), new Vector2(1f, 8f));
            LevelBuilder2_2.BuildAtmosphere(f, root.transform);
            ConfigureCamera(player);
            root.AddComponent<BoxCollider2D>().isTrigger = true;
            var intro = root.AddComponent<NarrativePrologueTrigger>();
            intro.Configure("EL FILO RESONANTE", "El cristal protege sus caparazones.\nPisotea la roca cercana para voltearlos durante 3,5 segundos.\nAnte los murciélagos, espera el vuelo y usa el Doble Salto.", new Color(0f, 0.96f, 0.83f), 7f);
            var portal = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Universal/Level_Exit_Portal.prefab"));
            portal.name = "Portal_Exit_To_2_4";
            portal.transform.SetParent(root.transform, false);
            portal.transform.position = new Vector2(97f, 9f);
            CaveLevelSceneFactory.Set(portal.GetComponent<LevelExit2D>(), "_nextSceneName", "Level_2_4");
            CaveLevelSceneFactory.Set(portal.GetComponent<LevelExit2D>(), "_levelTitle", "EL FILO RESONANTE COMPLETADO");
            CaveLevelSceneFactory.Set(portal.GetComponent<LevelExit2D>(), "_victoryMessage", "Has aprendido a leer el cristal y sus guardianes.\nEl camino continúa en el nivel 2-4.");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            var scenes = EditorBuildSettings.scenes.ToList();
            scenes.RemoveAll(entry => entry.path == ScenePath);
            scenes.Insert(scenes.FindIndex(entry => entry.path == LevelBuilder2_2.ScenePath) + 1, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("[AlmaDino] Level_2_3 built: three beetles, three telegraphed bats, seismic bridge and two checkpoints.");
        }

        private static void BuildBatAscent(CaveLevelSceneFactory f, ResonantEnemySceneFactory enemies)
        {
            f.Platform("Bat_Ledge_1", new Vector2(63.5f, 2.2f), new Vector2(3f, 1f));
            f.Platform("Bat_Ledge_2", new Vector2(69f, 3.4f), new Vector2(4f, 1f));
            f.Platform("Bat_Ledge_3", new Vector2(75.5f, 4.6f), new Vector2(4f, 1f));
            f.Platform("Bat_Ledge_4", new Vector2(82f, 5.8f), new Vector2(4f, 1f));
            f.Platform("Exit_High_Ledge", new Vector2(93f, 7f), new Vector2(14f, 1f));
            f.Hazard("Bat_Tunnel_Stalagmites", new Vector2(74f, -3f), new Vector2(27f, 1f));
            f.Hazard("Abyss_Death_Zone", new Vector2(50f, -11f), new Vector2(110f, 2f));
            enemies.Bat("Bat_Entrance", new Vector2(64f, 6.8f), 2.6f);
            enemies.Bat("Bat_Middle", new Vector2(74f, 8.5f), 3.2f);
            enemies.Bat("Bat_Upper", new Vector2(81f, 10f), 3.1f);
        }

        private static void ConfigureCamera(PlayerController player)
        {
            var camera = Camera.main;
            camera.orthographicSize = 5.2f;
            camera.transform.position = new Vector3(1f, 2.2f, -10f);
            var follow = camera.GetComponent<Camera2DFollow>();
            follow.SetTarget(player.transform);
            follow.SetBounds(new Vector2(1f, 2f), new Vector2(95f, 12f));
            CaveLevelSceneFactory.Set(follow, "_offset", new Vector2(1f, 1.5f));
            foreach (var light in Object.FindObjectsByType<Light2D>())
            {
                light.color = new Color(0.1f, 0.85f, 1f);
                if (light.lightType == Light2D.LightType.Global) light.intensity = 0.25f;
            }
        }
    }
}
#endif
