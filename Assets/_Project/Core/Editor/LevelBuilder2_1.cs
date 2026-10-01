#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AlmaDino.Features.Camera;
using AlmaDino.Features.Environment;
using AlmaDino.Features.Player.Controllers;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace AlmaDino.Core.Editor
{
    public static class LevelBuilder2_1
    {
        public const string ScenePath = "Assets/Scenes/World_2_Caves/Level_2_1.unity";

        [MenuItem("Tools/Alma/Construir Nivel 2-1 - Descenso a la Penumbra")]
        public static void BuildLevel2_1()
        {
            if (EditorApplication.isPlaying) return;
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Directory.CreateDirectory("Assets/Scenes/World_2_Caves");
            AssetDatabase.Refresh();
            if (!File.Exists(ScenePath))
                AssetDatabase.CopyAsset("Assets/Scenes/World_1_Jungle/Level_1_1.unity", ScenePath);
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var previousLevel = GameObject.Find("--- LEVEL ---");
            if (previousLevel != null) Object.DestroyImmediate(previousLevel);
            var root = new GameObject("--- LEVEL ---");
            var factory = new CaveLevelSceneFactory(root.transform);
            var grounds = new List<BreakableGround2D>();
            BuildTutorial(factory, grounds);
            BuildPractice(factory, grounds);
            BuildFinalDescent(factory, grounds);

            var player = GameObject.Find("Alma (Player)").GetComponent<PlayerController>();
            player.transform.position = new Vector3(0f, 13.2f, 0f);
            CaveLevelSceneFactory.Set(player, "_doubleJumpUnlocked", true);
            CaveLevelSceneFactory.Set(player, "_groundPoundUnlocked", false);
            CaveLevelSceneFactory.Set(player, "_dashUnlocked", false);
            CaveLevelSceneFactory.Set(player, "_roarUnlocked", false);
            CaveLevelSceneFactory.Set(player, "_fallDeathY", -27f);
            SceneSetupValidator.SetupAlmaVisuals(player.gameObject);
            SceneSetupValidator.SetupTouchControls();
            var reset = root.AddComponent<BreakableGroundRespawnReset2D>();
            CaveLevelSceneFactory.Set(reset, "_playerSource", player);
            var serialized = new SerializedObject(reset);
            var blocks = serialized.FindProperty("_grounds");
            blocks.arraySize = grounds.Count;
            for (int i = 0; i < grounds.Count; i++) blocks.GetArrayElementAtIndex(i).objectReferenceValue = grounds[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();

            ConfigureCameraAndLight(player);
            CaveLevelVisuals.Build(factory, root.transform, Camera.main.transform);
            var prologue = new GameObject("Cave_Prologue");
            prologue.transform.SetParent(root.transform);
            prologue.transform.position = player.transform.position;
            prologue.AddComponent<BoxCollider2D>().isTrigger = true;
            prologue.AddComponent<NarrativePrologueTrigger>().Configure("DESCENSO A LA PENUMBRA",
                "El rastro de mis pequeños desciende bajo la roca.\nEntre los cristales late una fuerza que aún no conozco.",
                CaveLevelSceneFactory.Crystal, 5f);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            RegisterInBuild();
            AssetDatabase.SaveAssets();
            Debug.Log("[AlmaDino] Level_2_1 built: safe tutorial, four breakable floors, three checkpoints and cave lighting.");
        }

        private static void BuildTutorial(CaveLevelSceneFactory f, List<BreakableGround2D> grounds)
        {
            f.Platform("Entrance_Ledge", new Vector2(-1f, 12f), new Vector2(6f, 1f));
            f.Platform("Shaft_Left_Wall", new Vector2(-2f, 7f), new Vector2(1f, 18f));
            f.Platform("Shaft_Right_Wall", new Vector2(10f, 8.5f), new Vector2(1f, 13f));
            f.Platform("Altar_Safe_Ledge", new Vector2(1f, 4f), new Vector2(4f, 1f));
            f.Platform("Tutorial_Right_Ledge", new Vector2(7.75f, 4f), new Vector2(3.5f, 1f));
            grounds.Add(f.BreakableFloor("Cracked_Floor_Tutorial", new Vector2(4.5f, 4.1f), new Vector2(3f, 0.8f)));
            f.Altar(new Vector2(0.5f, 5.4f));
            f.Checkpoint("Checkpoint_Altar", 1.5f, 4.5f);
            f.Platform("Tutorial_Safe_Landing", new Vector2(8f, -3f), new Vector2(20f, 1f));
            f.Checkpoint("Checkpoint_First_Descent", 14f, -2.5f);
        }

        private static void BuildPractice(CaveLevelSceneFactory f, List<BreakableGround2D> grounds)
        {
            grounds.Add(f.BreakableFloor("Cracked_Floor_Practice", new Vector2(20f, -2.9f), new Vector2(4f, 0.8f)));
            f.Platform("Practice_Sealing_Wall", new Vector2(22.5f, 0f), new Vector2(1f, 12f));
            f.Platform("Practice_Landing", new Vector2(25.5f, -8.5f), new Vector2(15f, 1f));
            f.Checkpoint("Checkpoint_Deep_Gallery", 28f, -8f);
            f.Hazard("Stalagmites_Short_Gap", new Vector2(34.5f, -9.4f), new Vector2(3f, 1.1f));
            f.Platform("Final_Approach_Ledge", new Vector2(39f, -7.7f), new Vector2(6f, 1f));
        }

        private static void BuildFinalDescent(CaveLevelSceneFactory f, List<BreakableGround2D> grounds)
        {
            grounds.Add(f.BreakableFloor("Cracked_Floor_Chain_Upper", new Vector2(44f, -7.6f), new Vector2(4f, 0.8f)));
            grounds.Add(f.BreakableFloor("Cracked_Floor_Chain_Lower", new Vector2(44f, -10.4f), new Vector2(4f, 0.8f)));
            f.Platform("Final_Sealing_Wall", new Vector2(46.5f, -5.5f), new Vector2(1f, 14f));
            f.Platform("Final_Safe_Landing", new Vector2(44.5f, -15f), new Vector2(7f, 1f));
            f.Hazard("Stalagmites_Left_Descent", new Vector2(37.5f, -16.2f), new Vector2(7f, 1f));
            f.Hazard("Stalagmites_Final_Gap", new Vector2(50f, -16.2f), new Vector2(4f, 1f));
            f.Platform("Exit_Geode_Ledge", new Vector2(56.5f, -14f), new Vector2(9f, 1f));
            f.Platform("Cave_Right_Boundary", new Vector2(62f, -9f), new Vector2(1f, 14f));
            f.Hazard("Deep_Fall_Death_Zone", new Vector2(30f, -28f), new Vector2(90f, 2f));
            var portal = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Universal/Level_Exit_Portal.prefab"));
            portal.name = "Portal_Exit_To_2_2";
            portal.transform.SetParent(f.Platform("Exit_Portal_Pedestal", new Vector2(58f, -13.3f), new Vector2(2f, 0.4f)).transform, true);
            portal.transform.position = new Vector2(58f, -12f);
            var exit = portal.GetComponent<LevelExit2D>();
            CaveLevelSceneFactory.Set(exit, "_nextSceneName", "Level_2_2");
            CaveLevelSceneFactory.Set(exit, "_levelTitle", "DESCENSO A LA PENUMBRA COMPLETADO");
            CaveLevelSceneFactory.Set(exit, "_victoryMessage", "Has despertado el Pisotón Sísmico.\nLa Galería de Ecos aguarda entre las geodas: nivel 2-2.");
        }

        private static void ConfigureCameraAndLight(PlayerController player)
        {
            var camera = Camera.main;
            camera.orthographicSize = 5.2f;
            camera.backgroundColor = new Color(0.025f, 0.045f, 0.1f);
            camera.transform.position = new Vector3(1f, 12.5f, -10f);
            var follow = camera.GetComponent<Camera2DFollow>();
            follow.SetTarget(player.transform);
            follow.SetBounds(new Vector2(-0.5f, -17f), new Vector2(58f, 13f));
            CaveLevelSceneFactory.Set(follow, "_offset", new Vector2(0.5f, -0.5f));
            CaveLevelSceneFactory.Set(follow, "_smoothTime", 0.12f);
            var global = Object.FindObjectsByType<Light2D>().First(light => light.lightType == Light2D.LightType.Global);
            global.intensity = 0.2f;
            global.color = new Color(0.68f, 0.76f, 1f);
            var oldLight = player.transform.Find("Cave_Player_Light");
            if (oldLight != null) Object.DestroyImmediate(oldLight.gameObject);
            CaveLevelSceneFactory.Light(player.transform, Vector2.zero, new Color(0.66f, 0.86f, 1f), 3.5f, 0.8f);
            player.transform.GetChild(player.transform.childCount - 1).name = "Cave_Player_Light";
        }

        private static void RegisterInBuild()
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            scenes.RemoveAll(scene => scene.path == ScenePath);
            int boss = scenes.FindIndex(scene => scene.path.EndsWith("Boss_1.unity"));
            scenes.Insert(boss + 1, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
#endif
