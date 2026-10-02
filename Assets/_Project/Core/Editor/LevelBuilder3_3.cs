#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using AlmaDino.Features.Enemies.Controllers;
using AlmaDino.Features.Enemies.ScriptableObjects;
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
    public static class LevelBuilder3_3
    {
        public const string ScenePath = "Assets/Scenes/World_3_Swamp/Level_3_3.unity";
        private static readonly Color Slate = new Color(0.25f, 0.35f, 0.46f);
        

        [MenuItem("Alma/📂 Cargar Nivel 3-3")]
        public static void LoadLevel()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(ScenePath);
        }

        [MenuItem("Tools/Alma/Construir Nivel 3-3 - El Vuelo de las Esporas")]
        public static void BuildLevel3_3()
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
            Rock(factory, "Entrance_Root_Boundary", -5.5f, 3f, 1f, 8f);
            Rock(factory, "Entrance_Runway", 2f, -0.75f, 16f, 1.5f);
            // A cracked floor opens the route under an unjumpable root on solid ground.
            Rock(factory, "Pound_Approach", 28f, -0.75f, 4f, 1.5f);
            Rock(factory, "Pound_Tunnel_Floor", 34f, -3.75f, 16f, 1.5f);
            Rock(factory, "Pound_Root_Gate", 36.5f, 5f, 5f, 10f);
            Rock(factory, "Tunnel_Exit_Step", 41f, -1.75f, 2f, 1.5f);
            Rock(factory, "Refuge_One", 44f, -0.75f, 4f, 1.5f);
            Rock(factory, "Tunnel_Exit_Cover", 42f, -0.2f, 0.6f, 1.6f);
            var crackedFloor = factory.BreakableFloor("Cracked_Root_Floor", new Vector2(32f, -0.4f), new Vector2(4f, 0.8f));
            Rock(factory, "Refuge_Two", 72f, -0.75f, 20f, 1.5f);
            Rock(factory, "Exit_Refuge", 117f, -0.75f, 14f, 1.5f);
            Mud(factory, root.transform, "Spore_Lake_One", 10f, 26f);
            Mud(factory, root.transform, "Spore_Lake_Two", 46f, 62f);
            Mud(factory, root.transform, "Spore_Lake_Final", 82f, 110f);
            factory.Checkpoint("Checkpoint_Roots_One", 28f, 0f);
            factory.Checkpoint("Checkpoint_Roots_Two", 76f, 0f);
            Rock(factory, "Checkpoint_Two_Cover", 74.5f, 0.8f, 0.6f, 1.6f);
            var reeds = factory.Visual("Reed_Dash_Gate", root.transform, new Vector2(65f, 3.5f),
                new Vector2(0.45f, 7f), new Color(0.36f, 0.3f, 0.24f), true);
            reeds.AddComponent<BoxCollider2D>();
            reeds.AddComponent<DashBreakableBarrier2D>();
            for (int i = 0; i < 2; i++)
                factory.Visual("Reed_Crack", reeds.transform, new Vector2(0f, -0.2f + i * 0.4f),
                    new Vector2(0.8f, 0.035f), Color.cyan, true);
            var spores = new List<DashRefillPickup2D>();
            Vector2[] positions =
            {
                new Vector2(14f, 2.7f), new Vector2(20f, 2.7f),
                new Vector2(50f, 2.7f), new Vector2(56f, 2.7f),
                new Vector2(86f, 2.7f), new Vector2(92f, 2.7f),
                new Vector2(98f, 2.7f), new Vector2(104f, 2.7f)
            };
            foreach (var position in positions) spores.Add(Spore(factory, root.transform, position));
            Portal(root.transform);
            var player = GameObject.Find("Alma (Player)").GetComponent<PlayerController>();
            player.transform.position = new Vector3(0f, 0.8f, 0f);
            CaveLevelSceneFactory.Set(player, "_doubleJumpUnlocked", true);
            CaveLevelSceneFactory.Set(player, "_groundPoundUnlocked", true);
            CaveLevelSceneFactory.Set(player, "_dashUnlocked", true);
            CaveLevelSceneFactory.Set(player, "_roarUnlocked", false);
            CaveLevelSceneFactory.Set(player, "_fallDeathY", -8f);
            SceneSetupValidator.SetupAlmaVisuals(player.gameObject);
            SceneSetupValidator.SetupTouchControls();
            var reset = root.AddComponent<DashRefillRespawnReset2D>();
            CaveLevelSceneFactory.Set(reset, "_playerSource", player);
            var serializedReset = new SerializedObject(reset);
            var entries = serializedReset.FindProperty("_spores");
            entries.arraySize = spores.Count;
            for (int i = 0; i < spores.Count; i++) entries.GetArrayElementAtIndex(i).objectReferenceValue = spores[i];
            serializedReset.ApplyModifiedPropertiesWithoutUndo();
            var groundReset = root.AddComponent<BreakableGroundRespawnReset2D>();
            CaveLevelSceneFactory.Set(groundReset, "_playerSource", player);
            var groundEntries = new SerializedObject(groundReset);
            groundEntries.FindProperty("_grounds").arraySize = 1;
            groundEntries.FindProperty("_grounds").GetArrayElementAtIndex(0).objectReferenceValue = crackedFloor;
            groundEntries.ApplyModifiedPropertiesWithoutUndo();
            var config = ToadConfig();
            var bubble = BubblePrefab(factory, root.transform);
            Toad(factory, root.transform, player, config, bubble, "Poison_Toad_One", new Vector2(43.5f, 0.65f), -1f);
            Toad(factory, root.transform, player, config, bubble, "Poison_Toad_Two", new Vector2(72.5f, 0.65f), 1f);

            var camera = Camera.main;
            camera.orthographicSize = 6f;
            camera.backgroundColor = new Color(0.12f, 0.14f, 0.19f);
            camera.transform.position = new Vector3(0f, 2f, -10f);
            var follow = camera.GetComponent<Camera2DFollow>();
            follow.SetTarget(player.transform);
            follow.SetBounds(new Vector2(0f, -2f), new Vector2(121f, 5f));
            CaveLevelSceneFactory.Set(follow, "_offset", new Vector2(0f, 1.2f));
            CaveLevelSceneFactory.Set(follow, "_smoothTime", 0.12f);
            var global = Object.FindObjectsByType<Light2D>().First(light => light.lightType == Light2D.LightType.Global);
            global.intensity = 0.7f;
            global.color = new Color(0.82f, 0.9f, 0.96f);
            Atmosphere(factory, root.transform, camera.transform);
            var tutorial = new GameObject("Spore_Tutorial");
            tutorial.transform.SetParent(root.transform);
            tutorial.transform.position = new Vector2(2f, 1f);
            tutorial.AddComponent<BoxCollider2D>().isTrigger = true;
            tutorial.AddComponent<NarrativePrologueTrigger>().Configure("EL VUELO DE LAS ESPORAS",
                "Salta y encadena DASH hacia la derecha.\nLas esporas recuperan tu impulso.\nEn tierra, rompe suelos con POUND y cañas con DASH.", Color.cyan, 6f);
            Hint(root.transform, "Pound_Hint", new Vector2(29f, 1f), "ABRE EL PASO",
                "Salta sobre el suelo agrietado y pulsa ABAJO o POUND.\nPasa bajo la raíz y usa Doble Salto para salir.");
            Hint(root.transform, "Dash_Gate_Hint", new Vector2(63f, 1f), "CAÑAS Y VENENO",
                "Rompe las cañas con DASH.\nSalta sobre los sapos y sus disparos; el aviso ! anuncia el ataque.");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            var scenes = EditorBuildSettings.scenes.ToList();
            scenes.RemoveAll(entry => entry.path == ScenePath);
            int previousIndex = scenes.FindIndex(entry => entry.path.EndsWith("Level_3_2.unity"));
            scenes.Insert(previousIndex + 1, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("[AlmaDino] Level_3_3 built: eight horizontal refill spores, a Ground Pound tunnel, a Dash gate and two poison toads.");
        }

        private static void Hint(Transform root, string name, Vector2 position, string title, string message)
        {
            var hint = new GameObject(name);
            hint.transform.SetParent(root);
            hint.transform.position = position;
            var trigger = hint.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(1.5f, 2f);
            hint.AddComponent<NarrativePrologueTrigger>().Configure(title, message, Color.cyan, 5f, false);
        }

        private static void Rock(CaveLevelSceneFactory factory, string name, float x, float y, float width, float height)
        {
            var rock = factory.Platform(name, new Vector2(x, y), new Vector2(width, height));
            rock.GetComponent<SpriteRenderer>().color = Slate;
            rock.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(0.47f, 0.55f, 0.55f);
        }

        private static void Mud(CaveLevelSceneFactory factory, Transform root, string name, float left, float right)
        {
            var mud = factory.Visual(name, root, new Vector2((left + right) * 0.5f, -2f),
                new Vector2(right - left, 3f), new Color(0.12f, 0.22f, 0.17f), true);
            mud.AddComponent<BoxCollider2D>().isTrigger = true;
            mud.AddComponent<HazardTrigger2D>();
        }

        private static DashRefillPickup2D Spore(CaveLevelSceneFactory factory, Transform root, Vector2 position)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/World_3_Swamp/DashRefillSpore_Swamp.prefab"));
            go.name = "Refill_Spore_" + position.x;
            go.transform.SetParent(root);
            go.transform.position = position;
            var pickup = go.GetComponent<DashRefillPickup2D>();
            CaveLevelSceneFactory.Set(pickup, "_activeColor", new Color(0.44f, 0.88f, 0f));
            CaveLevelSceneFactory.Set(pickup, "_consumedColor", new Color(0.44f, 0.88f, 0f, 0.15f));
            CaveLevelSceneFactory.Light(go.transform, Vector2.zero, new Color(0.44f, 0.88f, 0f), 2f, 0.5f);
            CaveLevelSceneFactory.Set(pickup, "_glowLight", go.GetComponentInChildren<Light2D>());
            return pickup;
        }

        private static PoisonToadConfigSO ToadConfig()
        {
            const string path = "Assets/_Project/ScriptableObjects/PoisonToadConfig.asset";
            var config = AssetDatabase.LoadAssetAtPath<PoisonToadConfigSO>(path);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<PoisonToadConfigSO>();
                AssetDatabase.CreateAsset(config, path);
            }
            return config;
        }

        private static PoisonBubble2D BubblePrefab(CaveLevelSceneFactory factory, Transform root)
        {
            const string path = "Assets/_Project/Prefabs/World_3_Swamp/Poison_Bubble.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing.GetComponent<PoisonBubble2D>();
            var go = factory.Visual("Poison_Bubble", root, Vector2.zero, new Vector2(0.4f, 0.4f),
                new Color(0.76f, 0.87f, 0.19f), true);
            go.GetComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Circle.png");
            go.GetComponent<SpriteRenderer>().sortingOrder = 3;
            var body = go.AddComponent<Rigidbody2D>();
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            go.AddComponent<CircleCollider2D>().isTrigger = true;
            go.AddComponent<PoisonBubble2D>();
            go.SetActive(false);
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab.GetComponent<PoisonBubble2D>();
        }

        private static void Toad(CaveLevelSceneFactory factory, Transform root, PlayerController player,
            PoisonToadConfigSO config, PoisonBubble2D bubble, string name, Vector2 position, float direction)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root);
            go.transform.position = position;
            var collider = go.AddComponent<CircleCollider2D>();
            collider.radius = 0.6f;
            collider.isTrigger = true;
            var visual = factory.Visual("Toad_Body", go.transform, Vector2.zero, new Vector2(1.3f, 0.9f),
                new Color(0.35f, 0.09f, 0.6f), true);
            visual.GetComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Circle.png");
            for (int i = 0; i < 2; i++)
                factory.Visual("Yellow_Eye", go.transform, new Vector2(-0.25f + i * 0.5f, 0.35f),
                    new Vector2(0.16f, 0.16f), Color.yellow, true);
            var warning = new GameObject("Shot_Warning");
            warning.transform.SetParent(go.transform, false);
            warning.transform.localPosition = new Vector2(0f, 1f);
            var label = warning.AddComponent<TextMesh>();
            label.text = "!";
            label.characterSize = 0.5f;
            label.anchor = TextAnchor.MiddleCenter;
            label.color = Color.yellow;
            warning.SetActive(false);
            var toad = go.AddComponent<PoisonToad2D>();
            CaveLevelSceneFactory.Set(toad, "_config", config);
            CaveLevelSceneFactory.Set(toad, "_projectilePrefab", bubble);
            CaveLevelSceneFactory.Set(toad, "_playerSource", player);
            CaveLevelSceneFactory.Set(toad, "_visual", visual.GetComponent<SpriteRenderer>());
            CaveLevelSceneFactory.Set(toad, "_warningSign", warning);
            CaveLevelSceneFactory.Set(toad, "_shotDirection", direction);
        }

        private static void Portal(Transform root)
        {
            var portal = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Universal/Level_Exit_Portal.prefab"));
            portal.name = "Portal_Exit_To_3_4";
            portal.transform.SetParent(root);
            portal.transform.position = new Vector2(122f, 1.5f);
            var exit = portal.GetComponent<LevelExit2D>();
            CaveLevelSceneFactory.Set(exit, "_nextSceneName", "Level_3_4");
            CaveLevelSceneFactory.Set(exit, "_levelTitle", "EL VUELO DE LAS ESPORAS COMPLETADO");
            CaveLevelSceneFactory.Set(exit, "_victoryMessage", "Entre la niebla distingo un brillo morado.\nEl sauce ancestral aguarda: nivel 3-4.");
        }

        private static void Atmosphere(CaveLevelSceneFactory factory, Transform root, Transform camera)
        {
            for (int layer = 0; layer < 3; layer++)
            {
                var group = new GameObject("Spore_Lake_Parallax_" + layer);
                group.transform.SetParent(root, false);
                var parallax = group.AddComponent<ParallaxLayer2D>();
                CaveLevelSceneFactory.Set(parallax, "_camera", camera);
                CaveLevelSceneFactory.Set(parallax, "_cameraMotionFactor", new Vector2(0.75f - layer * 0.3f, 0.1f));
                for (int i = 0; i < 22; i++)
                {
                    var silhouette = factory.Visual(layer == 1 ? "Leaning_Willow" : "Mist_Bank", group.transform,
                        new Vector2(-15f + i * 8f, 5f), new Vector2(layer == 1 ? 0.4f : 12f, layer == 1 ? 18f : 3f),
                        new Color(0.18f, 0.26f, 0.3f, 0.25f + layer * 0.1f), true);
                    silhouette.transform.localRotation = Quaternion.Euler(0f, 0f, layer == 1 ? 18f : -8f);
                    silhouette.GetComponent<SpriteRenderer>().sortingOrder = -30 + layer * 8;
                }
            }
        }
    }
}
#endif
