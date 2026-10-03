#if UNITY_EDITOR
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
    public static class LevelBuilder2_2
    {
        public const string ScenePath = "Assets/Scenes/World_2_Caves/Level_2_2.unity";
        private const string ConfigPath = "Assets/_Project/ScriptableObjects/EchoSeesawConfig.asset";

        [MenuItem("Tools/Alma/Construir Nivel 2-2 - La Galería de Ecos")]
        public static void BuildLevel2_2()
        {
            if (EditorApplication.isPlaying) return;
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Directory.CreateDirectory("Assets/_Project/ScriptableObjects");
            AssetDatabase.Refresh();
            var config = AssetDatabase.LoadAssetAtPath<SeesawConfigSO>(ConfigPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<SeesawConfigSO>();
                AssetDatabase.CreateAsset(config, ConfigPath);
            }
            if (!File.Exists(ScenePath)) AssetDatabase.CopyAsset(LevelBuilder2_1.ScenePath, ScenePath);
            AssetDatabase.SaveAssets();
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            config = AssetDatabase.LoadAssetAtPath<SeesawConfigSO>(ConfigPath);
            if (config == null) throw new System.InvalidOperationException("Missing seesaw configuration.");
            var previous = GameObject.Find("--- LEVEL ---");
            if (previous != null) Object.DestroyImmediate(previous);
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
            var puzzles = new EchoPuzzleSceneFactory(f, root.transform, config, player);
            BuildGallery(f, puzzles);
            ConfigureCamera(player);
            BuildAtmosphere(f, root.transform);
            var prologue = new GameObject("Echo_Prologue");
            prologue.transform.SetParent(root.transform);
            prologue.AddComponent<BoxCollider2D>().isTrigger = true;
            prologue.AddComponent<NarrativePrologueTrigger>().Configure("LA GALERÍA DE ECOS",
                "Los ladrones han movido estas piedras.\nPisotea el extremo marcado: el contrapeso golpeará la runa.\nCorre al extremo elevado para salir catapultada.", new Color(0.32f, 0.72f, 0.53f), 7f);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            var scenes = EditorBuildSettings.scenes.ToList();
            scenes.RemoveAll(entry => entry.path == ScenePath);
            scenes.Insert(scenes.FindIndex(entry => entry.path == LevelBuilder2_1.ScenePath) + 1,
                new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("[AlmaDino] Level_2_2 built: four seesaws, timed runes, high ledge and linked final gate.");
        }

        private static void BuildGallery(CaveLevelSceneFactory f, EchoPuzzleSceneFactory puzzles)
        {
            f.Platform("Gallery_Safe_Floor", new Vector2(47f, -0.5f), new Vector2(98f, 1f));
            f.Platform("Left_Boundary", new Vector2(-3f, 6f), new Vector2(1f, 14f));
            var first = puzzles.Station("Seesaw_Tutorial", 10f);
            puzzles.Gate("Gate_Tutorial", 20f, first);
            f.Checkpoint("Checkpoint_First_Puzzle", 30f, 0f);
            var second = puzzles.Station("Seesaw_High_Ledge", 40f);
            puzzles.Gate("Gate_High_Ledge", 46f, second);
            f.Platform("Catapult_High_Ledge", new Vector2(51f, 5.7f), new Vector2(10f, 1f));
            f.Platform("High_Ledge_Sealing_Pillar", new Vector2(55.5f, 2.6f), new Vector2(1f, 5.2f));
            f.Checkpoint("Checkpoint_Before_Chain", 65f, 0f);
            var third = puzzles.Station("Seesaw_Chain_First", 72f);
            var fourth = puzzles.Station("Seesaw_Chain_Second", 82f);
            puzzles.Gate("Gate_Linked_Runes", 90f, third, fourth);
            f.Hazard("Stalagmites_Exit_Gap", new Vector2(98f, -1.5f), new Vector2(4f, 1f));
            f.Platform("Exit_Safe_Ledge", new Vector2(106f, -0.5f), new Vector2(12f, 1f));
            f.Platform("Right_Boundary", new Vector2(113f, 6f), new Vector2(1f, 14f));
            f.Hazard("Abyss_Death_Zone", new Vector2(55f, -11f), new Vector2(120f, 2f));
            var portal = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Resources/Resource_LevelExitPortal_Universal.prefab"));
            portal.name = "Portal_Exit_To_2_3";
            portal.transform.SetParent(GameObject.Find("--- LEVEL ---").transform, false);
            portal.transform.position = new Vector2(109f, 1.4f);
            CaveLevelSceneFactory.Set(portal.GetComponent<LevelExit2D>(), "_nextSceneName", "Level_2_3");
            CaveLevelSceneFactory.Set(portal.GetComponent<LevelExit2D>(), "_levelTitle", "LA GALERÍA DE ECOS COMPLETADA");
            CaveLevelSceneFactory.Set(portal.GetComponent<LevelExit2D>(), "_victoryMessage",
                "El peso y el impulso abren el camino.\nEl rastro continúa hacia los guardianes de cristal del nivel 2-3.");
        }

        private static void ConfigureCamera(PlayerController player)
        {
            var camera = Camera.main;
            camera.orthographicSize = 6f;
            camera.transform.position = new Vector3(1f, 3.2f, -10f);
            var follow = camera.GetComponent<Camera2DFollow>();
            follow.SetTarget(player.transform);
            follow.SetBounds(new Vector2(1f, 2.5f), new Vector2(107f, 13f));
            CaveLevelSceneFactory.Set(follow, "_offset", new Vector2(0f, 2f));
            CaveLevelSceneFactory.Set(follow, "_smoothTime", 0.1f);
            var global = Object.FindObjectsByType<Light2D>().First(light => light.lightType == Light2D.LightType.Global);
            global.intensity = 0.25f;
            global.color = new Color(0.62f, 0.82f, 0.74f);
        }

        internal static void BuildAtmosphere(CaveLevelSceneFactory f, Transform root)
        {
            var far = new GameObject("Parallax_0_Echo_Void");
            far.transform.SetParent(root, false);
            var farMotion = far.AddComponent<ParallaxLayer2D>();
            CaveLevelSceneFactory.Set(farMotion, "_camera", Camera.main.transform);
            CaveLevelSceneFactory.Set(farMotion, "_cameraMotionFactor", new Vector2(0.85f, 0.85f));
            var voidSprite = f.Visual("Echo_Void", far.transform, new Vector2(55f, 8f), new Vector2(250f, 80f),
                new Color(0.018f, 0.03f, 0.045f), true);
            voidSprite.GetComponent<SpriteRenderer>().sortingOrder = -30;
            var foreground = new GameObject("Parallax_3_Pale_Vines");
            foreground.transform.SetParent(root, false);
            var closeMotion = foreground.AddComponent<ParallaxLayer2D>();
            CaveLevelSceneFactory.Set(closeMotion, "_camera", Camera.main.transform);
            CaveLevelSceneFactory.Set(closeMotion, "_cameraMotionFactor", new Vector2(-0.07f, -0.04f));
            for (int i = 0; i < 14; i++)
            {
                var vine = f.Visual("Pale_Hanging_Vine", foreground.transform,
                    new Vector2(-4f + i * 9f, 12f), new Vector2(0.06f, 7f), new Color(0.6f, 0.75f, 0.65f, 0.15f), true);
                vine.GetComponent<SpriteRenderer>().sortingOrder = 7;
            }
            var background = new GameObject("Parallax_1_Echo_Gallery");
            background.transform.SetParent(root, false);
            var parallax = background.AddComponent<ParallaxLayer2D>();
            CaveLevelSceneFactory.Set(parallax, "_camera", Camera.main.transform);
            CaveLevelSceneFactory.Set(parallax, "_cameraMotionFactor", new Vector2(0.3f, 0.15f));
            for (int i = 0; i < 24; i++)
            {
                var column = f.Visual("Distant_Gallery_Pillar", background.transform, new Vector2(-10f + i * 6f, 6f),
                    new Vector2(2.2f, 35f), new Color(0.045f, 0.07f, 0.09f), true);
                column.GetComponent<SpriteRenderer>().sortingOrder = -20;
                column.transform.rotation = Quaternion.Euler(0f, 0f, i % 2 == 0 ? 8f : -8f);
            }
            for (int i = 0; i < 14; i++)
            {
                Vector2 position = new Vector2(2f + i * 8f, i == 6 ? 7f : 0.7f);
                Color color = i % 2 == 0 ? new Color(0.32f, 0.72f, 0.53f) : new Color(0.95f, 0.6f, 0.1f);
                var geode = f.Visual("Gallery_Geode", root, position, new Vector2(0.7f, 1.1f), color, true);
                geode.transform.rotation = Quaternion.Euler(0f, 0f, 15f);
                CaveLevelSceneFactory.Light(root, position, color, 4f, 0.7f);
            }
        }
    }
}
#endif
