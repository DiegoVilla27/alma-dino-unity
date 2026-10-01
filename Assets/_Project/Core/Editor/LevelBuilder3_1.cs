#if UNITY_EDITOR
using System.IO;
using System.Linq;
using AlmaDino.Core.Interfaces;
using AlmaDino.Features.Camera;
using AlmaDino.Features.Environment;
using AlmaDino.Features.Player.Controllers;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace AlmaDino.Core.Editor
{
    public static class LevelBuilder3_1
    {
        public const string ScenePath = "Assets/Scenes/World_3_Swamp/Level_3_1.unity";
        private static readonly Color Peat = new Color(0.18f, 0.29f, 0.24f);
        private static readonly Color DashGlow = new Color(0.79f, 0.94f, 0.97f);

        [MenuItem("Alma/📂 Cargar Nivel 3-1")]
        public static void LoadLevel()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(ScenePath);
        }

        [MenuItem("Tools/Alma/Construir Nivel 3-1 - Los Fangales Tóxicos")]
        public static void BuildLevel3_1()
        {
            if (EditorApplication.isPlaying) return;
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Directory.CreateDirectory("Assets/Scenes/World_3_Swamp");
            AssetDatabase.Refresh();
            if (!File.Exists(ScenePath))
                AssetDatabase.CopyAsset("Assets/Scenes/World_1_Jungle/Level_1_1.unity", ScenePath);
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var previous = GameObject.Find("--- LEVEL ---");
            if (previous != null) Object.DestroyImmediate(previous);
            var root = new GameObject("--- LEVEL ---");
            var factory = new CaveLevelSceneFactory(root.transform);

            // All islands have the same top height. Each 11m gap exceeds Double Jump alone.
            Island(factory, "Entrance_Altar_Island", 4f, 20f);
            var entranceWall = factory.Platform("Entrance_Root_Boundary", new Vector2(-5.5f, 3f), new Vector2(1f, 8f));
            entranceWall.GetComponent<SpriteRenderer>().color = Peat;
            entranceWall.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(0.42f, 0.56f, 0.35f);
            Island(factory, "Practice_Island_1", 29f, 8f);
            Island(factory, "Practice_Island_2", 48f, 8f);
            Island(factory, "Practice_Island_3", 67f, 8f);
            Island(factory, "Exit_Island", 88f, 12f);
            Mud(factory, root.transform, "Toxic_Mud_Tutorial", 14f, 25f);
            Mud(factory, root.transform, "Toxic_Mud_Practice_1", 33f, 44f);
            Mud(factory, root.transform, "Toxic_Mud_Practice_2", 52f, 63f);
            Mud(factory, root.transform, "Toxic_Mud_Final", 71f, 82f);
            factory.Checkpoint("Checkpoint_Altar", 10f, 0f);
            factory.Checkpoint("Checkpoint_Practice", 29f, 0f);
            factory.Checkpoint("Checkpoint_Final", 67f, 0f);
            Altar(factory, root.transform);
            Exit(root.transform);

            var player = GameObject.Find("Alma (Player)").GetComponent<PlayerController>();
            player.transform.position = new Vector3(0f, 0.8f, 0f);
            CaveLevelSceneFactory.Set(player, "_doubleJumpUnlocked", true);
            CaveLevelSceneFactory.Set(player, "_groundPoundUnlocked", true);
            CaveLevelSceneFactory.Set(player, "_dashUnlocked", false);
            CaveLevelSceneFactory.Set(player, "_roarUnlocked", false);
            CaveLevelSceneFactory.Set(player, "_fallDeathY", -8f);
            SceneSetupValidator.SetupAlmaVisuals(player.gameObject);
            SceneSetupValidator.SetupTouchControls();
            var camera = Camera.main;
            camera.orthographicSize = 6f;
            camera.backgroundColor = new Color(0.22f, 0.3f, 0.33f);
            camera.transform.position = new Vector3(0f, 2f, -10f);
            var follow = camera.GetComponent<Camera2DFollow>();
            follow.SetTarget(player.transform);
            follow.SetBounds(new Vector2(0f, 1f), new Vector2(91f, 5f));
            CaveLevelSceneFactory.Set(follow, "_offset", new Vector2(0f, 1.2f));
            CaveLevelSceneFactory.Set(follow, "_smoothTime", 0.12f);
            var global = Object.FindObjectsByType<Light2D>().First(light => light.lightType == Light2D.LightType.Global);
            global.intensity = 0.7f;
            global.color = new Color(0.8f, 0.9f, 0.95f);
            BuildAtmosphere(factory, root.transform, camera.transform);
            var prologue = new GameObject("Swamp_Prologue");
            prologue.transform.SetParent(root.transform);
            prologue.transform.position = new Vector2(1f, 1f);
            prologue.AddComponent<BoxCollider2D>().isTrigger = true;
            prologue.AddComponent<NarrativePrologueTrigger>().Configure("LOS FANGALES TÓXICOS",
                "Tras el cristal, la niebla.\nEl rastro cruza el lodo... mis alas necesitan cortar el viento.", DashGlow, 5f);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            var scenes = EditorBuildSettings.scenes.ToList();
            scenes.RemoveAll(entry => entry.path == ScenePath);
            int boss = scenes.FindIndex(entry => entry.path.EndsWith("Boss_2.unity"));
            scenes.Insert(boss + 1, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("[AlmaDino] Level_3_1 built: Dash altar, four 11m toxic gaps, three checkpoints and swamp parallax.");
        }

        private static void Island(CaveLevelSceneFactory factory, string name, float center, float width)
        {
            var island = factory.Platform(name, new Vector2(center, -0.75f), new Vector2(width, 1.5f));
            island.GetComponent<SpriteRenderer>().color = Peat;
            island.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(0.42f, 0.56f, 0.35f);
        }

        private static void Mud(CaveLevelSceneFactory factory, Transform root, string name, float left, float right)
        {
            var mud = factory.Visual(name, root, new Vector2((left + right) * 0.5f, -1.6f),
                new Vector2(right - left, 2.4f), new Color(0.12f, 0.22f, 0.16f), true);
            mud.AddComponent<BoxCollider2D>().isTrigger = true;
            mud.AddComponent<HazardTrigger2D>();
            for (int i = 0; i < 8; i++)
            {
                var bubble = factory.Visual("Sulfur_Bubble_" + i, root,
                    new Vector2(left + 0.6f + i * 1.35f, -0.65f), new Vector2(0.3f, 0.15f),
                    new Color(0.58f, 0.68f, 0.24f), true);
                bubble.GetComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Circle.png");
            }
            // Repeated chevrons communicate the horizontal action without covering the flight path.
            for (int i = 0; i < 2; i++)
            {
                for (int segment = 0; segment < 2; segment++)
                {
                    var arrow = factory.Visual("Dash_Path_Cue", root,
                        new Vector2(left - 1f + i * 0.5f, 1.1f + (segment == 0 ? 0.14f : -0.14f)),
                        new Vector2(0.09f, 0.4f), DashGlow, true);
                    arrow.transform.localRotation = Quaternion.Euler(0f, 0f, segment == 0 ? 45f : -45f);
                }
            }
        }

        private static void Altar(CaveLevelSceneFactory factory, Transform root)
        {
            var altar = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Universal/Ability_Relic_Altar.prefab"));
            altar.name = "Altar_Ancestral_Wind_Spore";
            altar.transform.SetParent(root);
            altar.transform.position = new Vector2(7f, 1f);
            var relic = altar.GetComponent<AbilityRelic2D>();
            CaveLevelSceneFactory.Set(relic, "_abilityToUnlock", AbilityType.Dash);
            CaveLevelSceneFactory.Set(relic, "_relicTitle", "DASH AÉREO");
            CaveLevelSceneFactory.Set(relic, "_loreDescription", "Tus alas se afilan con la velocidad del vendaval.\nEn el aire, pulsa SHIFT o DASH. Combina SALTO → DOBLE SALTO → DASH para cruzar el lodo.");
            CaveLevelSceneFactory.Set(relic, "_glowColor", DashGlow);
            altar.GetComponent<SpriteRenderer>().enabled = false;
            var gem = altar.transform.Find("Relic_Gem").GetComponent<SpriteRenderer>();
            gem.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Circle.png");
            gem.color = DashGlow;
            CaveLevelSceneFactory.Set(relic, "_spriteRenderer", gem);
            for (int petal = 0; petal < 6; petal++)
            {
                float angle = petal * Mathf.PI / 3f;
                var flower = factory.Visual("Wind_Flower_Petal", altar.transform,
                    new Vector2(Mathf.Cos(angle) * 0.4f, 1.2f + Mathf.Sin(angle) * 0.5f),
                    new Vector2(0.4f, 0.45f), DashGlow, true);
                flower.GetComponent<SpriteRenderer>().sprite = gem.sprite;
            }
        }

        private static void Exit(Transform root)
        {
            var portal = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Universal/Level_Exit_Portal.prefab"));
            portal.name = "Portal_Exit_To_3_2";
            portal.transform.SetParent(root);
            portal.transform.position = new Vector2(91f, 1.5f);
            var exit = portal.GetComponent<LevelExit2D>();
            CaveLevelSceneFactory.Set(exit, "_nextSceneName", "Level_3_2");
            CaveLevelSceneFactory.Set(exit, "_levelTitle", "LOS FANGALES TÓXICOS COMPLETADOS");
            CaveLevelSceneFactory.Set(exit, "_victoryMessage", "El viento ya acompaña tus alas.\nLas corrientes del pantano aguardan en el nivel 3-2.");
        }

        private static void BuildAtmosphere(CaveLevelSceneFactory factory, Transform root, Transform camera)
        {
            for (int layer = 0; layer < 4; layer++)
            {
                var group = new GameObject("Swamp_Parallax_" + layer);
                group.transform.SetParent(root, false);
                var parallax = group.AddComponent<ParallaxLayer2D>();
                CaveLevelSceneFactory.Set(parallax, "_camera", camera);
                CaveLevelSceneFactory.Set(parallax, "_cameraMotionFactor", new Vector2(0.8f - layer * 0.28f, 0.1f));
                for (int i = 0; i < 18; i++)
                {
                    bool mist = layer == 1;
                    var shape = factory.Visual(mist ? "Mist_Bank" : "Willow_Or_Reed", group.transform,
                        new Vector2(-16f + i * 9f, mist ? 5f + (i % 3) : 4f),
                        mist ? new Vector2(15f, 2f) : new Vector2(layer == 3 ? 0.12f : 0.7f, layer == 3 ? 2f : 13f),
                        mist ? new Color(0.55f, 0.6f, 0.68f, 0.1f) : new Color(0.1f, 0.19f, 0.18f, layer == 3 ? 0.18f : 0.4f), true);
                    shape.GetComponent<SpriteRenderer>().sortingOrder = layer == 3 ? 5 : -30 + layer * 10;
                    if (!mist) shape.transform.localRotation = Quaternion.Euler(0f, 0f, -12f);
                    if (layer == 0 || layer == 2)
                    {
                        var crown = factory.Visual("Willow_Crown", group.transform,
                            new Vector2(-15f + i * 9f, 7f), new Vector2(6f, 4f),
                            new Color(0.15f, 0.26f, 0.23f, 0.45f), true);
                        crown.GetComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Circle.png");
                        crown.GetComponent<SpriteRenderer>().sortingOrder = -30 + layer * 10;
                    }
                }
            }
        }
    }
}
#endif
