#if UNITY_EDITOR
using System.IO;
using System.Linq;
using AlmaDino.Core.Interfaces;
using AlmaDino.Features.Camera;
using AlmaDino.Features.Environment;
using AlmaDino.Features.Environment.Controllers;
using AlmaDino.Features.Environment.ScriptableObjects;
using AlmaDino.Features.Player.Controllers;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace AlmaDino.Core.Editor
{
    public static class LevelBuilder4_1
    {
        public const string ScenePath = "Assets/Scenes/World_4_Volcano/Level_4_1.unity";
        private static readonly Color Basalt = new Color(.105f, .105f, .118f);
        private static readonly Color Magma = new Color(.97f, .48f, 0f);
        [MenuItem("Tools/Alma/Construir Nivel 4-1 - Los Ríos de Ceniza")]
        public static void BuildLevel4_1()
        {
            if (EditorApplication.isPlaying) return;
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Directory.CreateDirectory("Assets/Scenes/World_4_Volcano");
            AssetDatabase.Refresh();
            if (!File.Exists(ScenePath)) AssetDatabase.CopyAsset(LevelBuilder3_4.ScenePath, ScenePath);
            var scene = EditorSceneManager.OpenScene(ScenePath);
            Object.DestroyImmediate(GameObject.Find("--- LEVEL ---"));
            var root = new GameObject("--- LEVEL ---");
            var f = new CaveLevelSceneFactory(root.transform);
            var player = GameObject.Find("Alma (Player)").GetComponent<PlayerController>();
            player.transform.position = new Vector2(0f, .7f);
            CaveLevelSceneFactory.Set(player, "_doubleJumpUnlocked", true);
            CaveLevelSceneFactory.Set(player, "_groundPoundUnlocked", true);
            CaveLevelSceneFactory.Set(player, "_dashUnlocked", true);
            CaveLevelSceneFactory.Set(player, "_roarUnlocked", false);
            CaveLevelSceneFactory.Set(player, "_fallDeathY", -8f);
            SceneSetupValidator.SetupAlmaVisuals(player.gameObject);
            SceneSetupValidator.SetupTouchControls();
            Floor(f, "Entrance_Fumarole_Island", -6f, 16f);
            Floor(f, "Practice_Island", 30f, 48f);
            Floor(f, "Steam_Gorge_Island", 64f, 90f);
            Floor(f, "Exit_Island", 106f, 124f);
            var rear = f.Platform("Entrance_Basalt_Boundary", new Vector2(-5.5f, 3f), new Vector2(1f, 8f));
            rear.GetComponent<SpriteRenderer>().color = Basalt;
            var end = f.Platform("Exit_Basalt_Boundary", new Vector2(125f, 3f), new Vector2(1f, 8f));
            end.GetComponent<SpriteRenderer>().color = Basalt;
            f.Checkpoint("Checkpoint_Fumarole", 10f, 0f);
            f.Checkpoint("Checkpoint_First_Bridge", 34f, 0f);
            f.Checkpoint("Checkpoint_Steam_Gorge", 68f, 0f);
            f.Checkpoint("Checkpoint_Exit", 110f, 0f);
            var config = LoadConfig<BoulderRoarConfigSO>("Assets/_Project/ScriptableObjects/BoulderRoarConfig.asset");
            var prefab = BoulderPrefab(f, root.transform, config);
            float[] rocks = { 14f, 46f, 88f };
            float[] lefts = { 16f, 48f, 90f };
            float[] rights = { 30f, 64f, 106f };
            for (int i = 0; i < rocks.Length; i++)
            {
                var arch = f.Platform("Basalt_Roar_Gorge_" + i, new Vector2(rocks[i], 4.3f), new Vector2(8f, 1.4f));
                arch.GetComponent<SpriteRenderer>().color = Basalt;
                var lava = Lava(f, root.transform, "Lava_River_" + i, lefts[i], rights[i]);
                var boulder = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                boulder.name = "Roar_Boulder_" + i;
                boulder.transform.SetParent(root.transform, false);
                boulder.transform.position = new Vector2(rocks[i], config.Radius);
                CaveLevelSceneFactory.Set(boulder.GetComponent<PushableBoulder2D>(), "_playerSource", player);
                CaveLevelSceneFactory.Set(boulder.GetComponent<PushableBoulder2D>(), "_lava", lava);
                Hint(root.transform, "Roar_Hint_" + i, new Vector2(rocks[i] - 4f, 3.6f), "ROAR → ROCA → PUENTE", Color.yellow);
                Hint(root.transform, "Bridge_Hint_" + i, new Vector2(rocks[i] + 5f, 2.8f), "APÓYATE AQUÍ\nSALTO + DOBLE SALTO + DASH →", Color.cyan);
                var marker = f.Visual("Boulder_Landing_Marker", root.transform, new Vector2(rocks[i] + 5f, -.32f), new Vector2(config.BridgeWidth, .08f), Color.yellow, true);
                marker.GetComponent<SpriteRenderer>().sortingOrder = 2;
            }
            Altar(f, root.transform);
            var geyserConfig = LoadConfig<GeyserConfigSO>("Assets/_Project/ScriptableObjects/GeyserConfig.asset");
            var geyserPrefab = GeyserPrefab(f, root.transform, geyserConfig);
            foreach (float x in new[] { 74f, 82f })
            {
                var geyser = (GameObject)PrefabUtility.InstantiatePrefab(geyserPrefab);
                geyser.name = "Steam_Geyser_" + x;
                geyser.transform.SetParent(root.transform, false);
                geyser.transform.position = new Vector2(x, 2f);
                CaveLevelSceneFactory.Set(geyser.GetComponent<LavaGeyser2D>(), "_playerSource", player);
            }
            Hint(root.transform, "Steam_Timing_Hint", new Vector2(69f, 3.5f), "VAPOR: ESPERA EL AVISO\nCRUZA CUANDO DIGA PASA", Color.yellow);
            var waveGo = new GameObject("Golden_Roar_Wave");
            waveGo.transform.SetParent(root.transform, false);
            var wave = waveGo.AddComponent<RoarWaveVisual2D>();
            var line = waveGo.GetComponent<LineRenderer>();
            line.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat");
            line.startWidth = .08f; line.endWidth = .08f; line.sortingOrder = 8;
            CaveLevelSceneFactory.Set(wave, "_playerSource", player);
            CaveLevelSceneFactory.Set(wave, "_duration", player.Config.RoarDuration);
            CaveLevelSceneFactory.Set(wave, "_radius", player.Config.RoarRadius);
            CaveLevelSceneFactory.Set(wave, "_halfAngle", player.Config.RoarHalfAngle);
            Exit(root.transform);
            var camera = Camera.main;
            camera.orthographicSize = 6f;
            camera.backgroundColor = new Color(.16f, .065f, .055f);
            camera.transform.position = new Vector3(0f, 2f, -10f);
            var follow = camera.GetComponent<Camera2DFollow>();
            follow.SetTarget(player.transform); follow.SetBounds(new Vector2(0f, 1f), new Vector2(121f, 6f));
            CaveLevelSceneFactory.Set(follow, "_offset", new Vector2(0f, 1.5f));
            CaveLevelSceneFactory.Set(follow, "_lookAheadDistance", 1.25f);
            var global = Object.FindObjectsByType<Light2D>().First(light => light.lightType == Light2D.LightType.Global);
            global.color = new Color(1f, .75f, .55f); global.intensity = .85f;
            Atmosphere(f, root.transform, camera.transform);
            var prologue = new GameObject("Volcano_Prologue");
            prologue.transform.SetParent(root.transform, false); prologue.transform.position = new Vector2(1f, 1f);
            prologue.AddComponent<BoxCollider2D>().isTrigger = true;
            prologue.AddComponent<NarrativePrologueTrigger>().Configure("LOS RÍOS DE CENIZA", "Tres pequeños a salvo... uno sigue en el cráter.\nLa piedra no cede a mis pasos. Mi voz tendrá que abrir camino.", Magma, 5f);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene, ScenePath);
            var scenes = EditorBuildSettings.scenes.ToList(); scenes.RemoveAll(s => s.path == ScenePath);
            scenes.Insert(scenes.FindIndex(s => s.path == LevelBuilderBoss_3.ScenePath) + 1, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray(); AssetDatabase.SaveAssets();
        }
        private static T LoadConfig<T>(string path) where T : ScriptableObject
        {
            var config = AssetDatabase.LoadAssetAtPath<T>(path);
            if (config == null) { config = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(config, path); }
            return config;
        }
        internal static void Floor(CaveLevelSceneFactory f, string name, float left, float right)
        {
            var floor = f.Platform(name, new Vector2((left + right) * .5f, -.75f), new Vector2(right - left, 1.5f));
            floor.GetComponent<SpriteRenderer>().color = Basalt;
            floor.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(.4f, .3f, .22f);
        }
        internal static Collider2D Lava(CaveLevelSceneFactory f, Transform root, string name, float left, float right)
        {
            var go = f.Visual(name, root, new Vector2((left + right) * .5f, -1.6f), new Vector2(right - left, 2.4f), Magma, true);
            go.AddComponent<BoxCollider2D>().isTrigger = true; go.AddComponent<HazardTrigger2D>();
            var rim = f.Visual("Magma_Surface", go.transform, new Vector2(0f, .47f), new Vector2(1f, .08f), new Color(1f, .82f, .4f), true);
            rim.GetComponent<SpriteRenderer>().sortingOrder = 0;
            CaveLevelSceneFactory.Light(go.transform, Vector2.zero, Magma, 7f, .7f);
            return go.GetComponent<Collider2D>();
        }
        private static GameObject BoulderPrefab(CaveLevelSceneFactory f, Transform root, BoulderRoarConfigSO config)
        {
            var go = new GameObject("PushableBoulder_Volcano"); go.transform.SetParent(root, false);
            var body = go.AddComponent<Rigidbody2D>(); body.bodyType = RigidbodyType2D.Kinematic; body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.useFullKinematicContacts = true; body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            go.AddComponent<CircleCollider2D>().radius = config.Radius;
            var sprite = f.Visual("Basalt_Boulder", go.transform, Vector2.zero, Vector2.one * config.Radius * 2f, new Color(.23f, .24f, .26f), true);
            sprite.GetComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Circle.png");
            for (int i = 0; i < 3; i++)
            {
                var crack = f.Visual("Glowing_Crack", sprite.transform, new Vector2(-.2f + i * .2f, 0f), new Vector2(.02f, .55f), Magma, true);
                crack.transform.localRotation = Quaternion.Euler(0f, 0f, i % 2 == 0 ? 20f : -20f);
                crack.GetComponent<SpriteRenderer>().sortingOrder = 2;
            }
            var cap = go.AddComponent<BoxCollider2D>(); cap.offset = new Vector2(0f, config.Radius - .2f); cap.size = new Vector2(config.BridgeWidth, .4f); cap.enabled = false;
            var bridge = f.Visual("Solidified_Bridge_Cap", go.transform, cap.offset, cap.size, new Color(.35f, .43f, .48f), true);
            bridge.GetComponent<SpriteRenderer>().sortingOrder = 3; bridge.SetActive(false);
            var boulder = go.AddComponent<PushableBoulder2D>();
            CaveLevelSceneFactory.Set(boulder, "_config", config);
            CaveLevelSceneFactory.Set(boulder, "_boulderRenderer", sprite.GetComponent<SpriteRenderer>());
            CaveLevelSceneFactory.Set(boulder, "_bridgeCollider", cap);
            CaveLevelSceneFactory.Set(boulder, "_bridgeVisual", bridge);
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, "Assets/_Project/Prefabs/Resources/Resource_RoarBoulder_Volcano.prefab");
            Object.DestroyImmediate(go); return prefab;
        }
        private static void Altar(CaveLevelSceneFactory f, Transform root)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Resources/Resource_AbilityAltar_DoubleJump.prefab"));
            go.name = "Altar_Primordial_Fumarole"; go.transform.SetParent(root, false); go.transform.position = new Vector2(6f, 1f);
            var relic = go.GetComponent<AbilityRelic2D>();
            CaveLevelSceneFactory.Set(relic, "_abilityToUnlock", AbilityType.Roar);
            CaveLevelSceneFactory.Set(relic, "_relicTitle", "RUGIDO DE CHOQUE");
            CaveLevelSceneFactory.Set(relic, "_loreDescription", "Acércate a la roca y mira hacia ella.\nPulsa ROAR (E/F): tu voz la empuja hasta la lava y crea un apoyo seguro.");
            CaveLevelSceneFactory.Set(relic, "_glowColor", Color.yellow);
            go.GetComponent<SpriteRenderer>().enabled = false;
            var gem = go.transform.Find("Relic_Gem").GetComponent<SpriteRenderer>(); gem.color = Color.yellow;
            CaveLevelSceneFactory.Set(relic, "_spriteRenderer", gem);
            CaveLevelSceneFactory.Light(go.transform, Vector2.zero, Magma, 4f, 1f);
            f.Visual("Fumarole_Ember", root, new Vector2(6f, .1f), new Vector2(1.5f, .15f), Magma, true);
        }
        private static GameObject GeyserPrefab(CaveLevelSceneFactory f, Transform root, GeyserConfigSO config)
        {
            var go = new GameObject("LavaGeyser_Volcano"); go.transform.SetParent(root, false);
            var collider = go.AddComponent<BoxCollider2D>(); collider.size = new Vector2(1.4f, 4f); collider.isTrigger = true;
            var flame = f.Visual("Hot_Steam", go.transform, Vector2.zero, collider.size, new Color(1f, .8f, .55f, .8f), true);
            var vent = f.Visual("Vent_Base", go.transform, new Vector2(0f, -1.85f), new Vector2(1.7f, .3f), Basalt, true);
            var label = Hint(go.transform, "Steam_State", new Vector2(0f, 2.4f), "PASA", Color.cyan);
            var geyser = go.AddComponent<LavaGeyser2D>();
            CaveLevelSceneFactory.Set(geyser, "_config", config);
            CaveLevelSceneFactory.Set(geyser, "_flamePillarRoot", flame.transform); CaveLevelSceneFactory.Set(geyser, "_ventBaseRenderer", vent.GetComponent<SpriteRenderer>());
            CaveLevelSceneFactory.Set(geyser, "_warningLabel", label);
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, "Assets/_Project/Prefabs/Traps/Trap_FireGeyser_Volcano.prefab");
            Object.DestroyImmediate(go); return prefab;
        }
        internal static TextMesh Hint(Transform root, string name, Vector2 position, string text, Color color)
        {
            var go = new GameObject(name); go.transform.SetParent(root, false); go.transform.position = position;
            var label = go.AddComponent<TextMesh>();
            go.GetComponent<MeshRenderer>().sortingOrder = 10;
            label.text = text; label.color = color;
            label.anchor = TextAnchor.MiddleCenter; label.fontSize = 36; label.characterSize = .06f;
            return label;
        }
        private static void Exit(Transform root)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Resources/Resource_LevelExitPortal_Universal.prefab"));
            go.name = "Portal_Exit_To_4_2"; go.transform.SetParent(root, false); go.transform.position = new Vector2(120f, 1.5f);
            CaveLevelSceneFactory.Set(go.GetComponent<LevelExit2D>(), "_nextSceneName", "Level_4_2");
            CaveLevelSceneFactory.Set(go.GetComponent<LevelExit2D>(), "_levelTitle", "LOS RÍOS DE CENIZA COMPLETADOS");
            CaveLevelSceneFactory.Set(go.GetComponent<LevelExit2D>(), "_victoryMessage", "Tu voz abre camino.\nLas campanas del fuego esperan más arriba en el volcán.");
        }
        internal static void Atmosphere(CaveLevelSceneFactory f, Transform root, Transform camera)
        {
            for (int layer = 0; layer < 4; layer++)
            {
                var group = new GameObject("Volcano_Parallax_" + layer); group.transform.SetParent(root, false);
                var parallax = group.AddComponent<ParallaxLayer2D>(); CaveLevelSceneFactory.Set(parallax, "_camera", camera);
                CaveLevelSceneFactory.Set(parallax, "_cameraMotionFactor", new Vector2(.75f - layer * .25f, .08f));
                for (int i = 0; i < 25; i++)
                {
                    Vector2 pos = new Vector2(-20f + i * 8f, layer == 0 ? -3f : layer == 1 ? 2f : layer == 2 ? -2f : 3f + i % 3);
                    Vector2 size = layer == 0 ? new Vector2(12f, 12f) : layer == 1 ? new Vector2(.3f, 7f) : layer == 2 ? new Vector2(6f, 2f) : new Vector2(.07f, .12f);
                    Color color = layer == 0 ? new Color(.12f, .045f, .045f) : layer == 1 ? new Color(.95f, .25f, .04f, .3f) : layer == 2 ? new Color(.25f, .16f, .16f, .6f) : new Color(1f, .65f, .25f, .5f);
                    var shape = f.Visual(layer == 0 ? "Volcano_Ridge" : layer == 1 ? "Distant_Lava_Fall" : layer == 2 ? "Ash_Bank" : "Ember", group.transform, pos, size, color, true);
                    shape.GetComponent<SpriteRenderer>().sortingOrder = layer == 3 ? 6 : -30 + layer * 10;
                    if (layer == 0) shape.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
                }
            }
        }
        [MenuItem("Alma/📂 Cargar Nivel 4-1")]
        public static void LoadLevel() { if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(ScenePath); }
    }
}
#endif
