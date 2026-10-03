#if UNITY_EDITOR
using System.IO;
using System.Linq;
using AlmaDino.Core.Progression;
using AlmaDino.Features.Camera;
using AlmaDino.Features.Enemies.Controllers;
using AlmaDino.Features.Enemies.ScriptableObjects;
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
    public static class LevelBuilder3_4
    {
        public const string ScenePath = "Assets/Scenes/World_3_Swamp/Level_3_4.unity";
        private static readonly Color Wood = new Color(0.42f, 0.44f, 0.35f);
        private static readonly Color Purple = new Color(0.62f, 0.31f, 0.87f);

        [MenuItem("Alma/📂 Cargar Nivel 3-4")]
        public static void LoadLevel()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(ScenePath);
        }

        [MenuItem("Tools/Alma/Construir Nivel 3-4 - El Sauce Ancestral")]
        public static void BuildLevel3_4()
        {
            if (EditorApplication.isPlaying) return;
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!File.Exists(ScenePath)) AssetDatabase.CopyAsset(LevelBuilder3_3.ScenePath, ScenePath);
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var previous = GameObject.Find("--- LEVEL ---");
            if (previous != null) Object.DestroyImmediate(previous);
            var root = new GameObject("--- LEVEL ---");
            var f = new CaveLevelSceneFactory(root.transform);
            var player = GameObject.Find("Alma (Player)").GetComponent<PlayerController>();
            player.transform.position = new Vector2(0f, 0.7f);
            CaveLevelSceneFactory.Set(player, "_doubleJumpUnlocked", true);
            CaveLevelSceneFactory.Set(player, "_groundPoundUnlocked", true);
            CaveLevelSceneFactory.Set(player, "_dashUnlocked", true);
            CaveLevelSceneFactory.Set(player, "_roarUnlocked", false);
            CaveLevelSceneFactory.Set(player, "_fallDeathY", -8f);
            SceneSetupValidator.SetupAlmaVisuals(player.gameObject);
            SceneSetupValidator.SetupTouchControls();

            Branch(f, "Entrance_Root_Boundary", new Vector2(-5.5f, 4f), new Vector2(1f, 10f));
            Floor(f, "Entrance_Roots", 3f, 18f, 0f);
            Floor(f, "Branch_One", 16f, 4f, 1.5f);
            Floor(f, "Branch_Two", 22f, 4f, 3f);
            Floor(f, "Branch_Three", 28f, 4f, 4.5f);
            Floor(f, "Branch_Four", 34f, 4f, 6f);
            Floor(f, "Checkpoint_One_Branch", 40f, 8f, 7.5f);
            f.Checkpoint("Checkpoint_Lower_Crown", 40f, 7.5f);
            Floor(f, "Dash_Gate_Landing", 64f, 8f, 7.5f);
            Barrier(f, root.transform);
            Floor(f, "Branch_Five", 72f, 4f, 9f);
            Floor(f, "Branch_Six", 78f, 4f, 10.5f);
            Floor(f, "Checkpoint_Two_Branch", 84f, 8f, 12f);
            f.Checkpoint("Checkpoint_Upper_Crown", 84f, 12f);
            Floor(f, "Catapult_Approach", 92f, 8f, 12f);
            Floor(f, "Crown_Landing", 101f, 6f, 19.5f);
            Floor(f, "Egg_Sanctuary", 115f, 22f, 19.5f);
            Branch(f, "Crown_Right_Boundary", new Vector2(127f, 25f), new Vector2(1f, 16f));
            f.Hazard("Abyss_Death_Zone", new Vector2(60f, -9f), new Vector2(140f, 2f));

            var spores = new[] { Spore(root.transform, new Vector2(48f, 10.2f)), Spore(root.transform, new Vector2(54f, 10.2f)) };
            var sporeReset = root.AddComponent<DashRefillRespawnReset2D>();
            CaveLevelSceneFactory.Set(sporeReset, "_playerSource", player);
            var resetData = new SerializedObject(sporeReset);
            var entries = resetData.FindProperty("_spores");
            entries.arraySize = spores.Length;
            for (int i = 0; i < spores.Length; i++) entries.GetArrayElementAtIndex(i).objectReferenceValue = spores[i];
            resetData.ApplyModifiedPropertiesWithoutUndo();
            Catapult(f, root.transform, player, "Root_Catapult_Shortcut", 8f, 0f);
            Catapult(f, root.transform, player, "Crown_Catapult", 92f, 12f);
            var toadConfig = AssetDatabase.LoadAssetAtPath<PoisonToadConfigSO>("Assets/_Project/ScriptableObjects/PoisonToadConfig.asset");
            var bubble = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Projectiles/Projectile_PoisonBubble_Swamp.prefab").GetComponent<PoisonBubble2D>();
            LevelBuilder3_3.Toad(f, root.transform, player, toadConfig, bubble, "Willow_Toad_Lower", new Vector2(18f, 2.15f));
            LevelBuilder3_3.Toad(f, root.transform, player, toadConfig, bubble, "Willow_Toad_Middle", new Vector2(65.5f, 8.15f));
            LevelBuilder3_3.Toad(f, root.transform, player, toadConfig, bubble, "Willow_Toad_Crown", new Vector2(114.5f, 20.15f));
            Branch(f, "Lower_Poison_Cover", new Vector2(14.4f, 2.4f), new Vector2(0.6f, 1.8f));
            Branch(f, "Crown_Poison_Cover", new Vector2(110.5f, 20.3f), new Vector2(0.6f, 1.6f));
            var egg = Sanctuary(f, root.transform);
            Gas(f, root.transform, player, egg);
            Atmosphere(f, root.transform);

            var camera = Camera.main;
            camera.orthographicSize = 6f;
            camera.backgroundColor = new Color(0.18f, 0.23f, 0.24f);
            camera.transform.position = new Vector3(0f, 2f, -10f);
            var follow = camera.GetComponent<Camera2DFollow>();
            follow.SetTarget(player.transform);
            follow.SetBounds(new Vector2(0f, 1f), new Vector2(121f, 24f));
            CaveLevelSceneFactory.Set(follow, "_offset", new Vector2(0f, 1.5f));
            CaveLevelSceneFactory.Set(follow, "_smoothTime", 0.12f);
            foreach (var light in Object.FindObjectsByType<Light2D>())
                if (light.lightType == Light2D.LightType.Global) { light.intensity = 0.65f; light.color = new Color(0.8f, 0.9f, 0.82f); }
            Hint(root.transform, "Willow_Prologue", new Vector2(2f, 1f), "EL SAUCE ANCESTRAL",
                "El gas sube al dejar cada refugio: sigue las ramas.\nSalta, usa Doble Salto y atraviesa el lago con DASH.\nEl Huevo Morado espera en la copa.", true);
            Hint(root.transform, "Shortcut_Hint", new Vector2(5f, 1f), "UN IMPULSO EXTRA",
                "Atajo opcional: pisa el extremo izquierdo con POUND.\nCorre hacia el derecho para salir impulsada.");
            Hint(root.transform, "Toad_Hint", new Vector2(12.5f, 2f), "SAPOS ENTRE LAS RAMAS",
                "El aviso ! anuncia el disparo.\nSalta sobre los sapos y sus burbujas; DASH no protege del veneno.");
            Hint(root.transform, "Spore_Hint", new Vector2(42f, 8.5f), "CRUZA EL LAGO",
                "Las dos esporas están a la misma altura.\nSalto → Doble Salto → DASH; toca la espora y repite DASH.\nRompe las cañas del otro lado con DASH.");
            Hint(root.transform, "Crown_Pound_Hint", new Vector2(88.5f, 13f), "ALCANZA LA COPA",
                "Sube al extremo izquierdo del balancín.\nSalta y pulsa POUND; corre al extremo derecho.\nMantén Salto para subir más y usa Doble Salto si hace falta.");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            var scenes = EditorBuildSettings.scenes.ToList();
            scenes.RemoveAll(entry => entry.path == ScenePath);
            scenes.Insert(scenes.FindIndex(entry => entry.path == LevelBuilder3_3.ScenePath) + 1, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("[AlmaDino] Level_3_4 built: staged rising gas, horizontal spores, Pound catapults, three poison toads and Purple Egg rescue.");
        }

        private static void Floor(CaveLevelSceneFactory f, string name, float x, float width, float top)
            => Branch(f, name, new Vector2(x, top - 0.4f), new Vector2(width, 0.8f));

        private static void Branch(CaveLevelSceneFactory f, string name, Vector2 position, Vector2 size)
        {
            var branch = f.Platform(name, position, size);
            branch.GetComponent<SpriteRenderer>().color = Wood;
            branch.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(0.62f, 0.65f, 0.45f);
        }

        private static DashRefillPickup2D Spore(Transform root, Vector2 position)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Resources/Resource_DashRefillSpore_Swamp.prefab"));
            go.name = "Refill_Spore_" + position.x;
            go.transform.SetParent(root);
            go.transform.position = position;
            var spore = go.GetComponent<DashRefillPickup2D>();
            CaveLevelSceneFactory.Set(spore, "_activeColor", new Color(0.44f, 0.88f, 0f));
            CaveLevelSceneFactory.Light(go.transform, Vector2.zero, new Color(0.44f, 0.88f, 0f), 2f, 0.5f);
            CaveLevelSceneFactory.Set(spore, "_glowLight", go.GetComponentInChildren<Light2D>());
            return spore;
        }

        private static void Barrier(CaveLevelSceneFactory f, Transform root)
        {
            var go = f.Visual("Crown_Reed_Gate", root, new Vector2(64f, 11f), new Vector2(0.45f, 7f), Wood, true);
            go.AddComponent<BoxCollider2D>();
            go.AddComponent<DashBreakableBarrier2D>();
            for (int i = 0; i < 2; i++) f.Visual("Reed_Crack", go.transform, new Vector2(0f, -0.2f + i * 0.4f),
                new Vector2(0.8f, 0.03f), Color.cyan, true);
        }

        private static void Catapult(CaveLevelSceneFactory f, Transform root, PlayerController player, string name, float x, float floor)
        {
            var board = new GameObject(name);
            board.transform.SetParent(root, false);
            board.transform.position = new Vector2(x, floor + 1.2f);
            board.AddComponent<BoxCollider2D>().size = new Vector2(6f, 0.5f);
            var body = board.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            f.Visual("Willow_Lever", board.transform, Vector2.zero, new Vector2(6f, 0.5f), Wood);
            f.Visual("Lever_Rim", board.transform, new Vector2(0f, 0.23f), new Vector2(6f, 0.07f), Color.yellow, true);
            f.Visual("Root_Fulcrum", root, new Vector2(x, floor + 0.45f), new Vector2(0.7f, 0.9f), Wood);
            var lever = board.AddComponent<SeesawPlatform2D>();
            CaveLevelSceneFactory.Set(lever, "_config", AssetDatabase.LoadAssetAtPath<SeesawConfigSO>("Assets/_Project/ScriptableObjects/EchoSeesawConfig.asset"));
            CaveLevelSceneFactory.Set(lever, "_playerSource", player);
            Label(root, name + "_Pound_Sign", new Vector2(x - 2.3f, floor + 3.2f), "POUND ↓");
            Label(root, name + "_Launch_Sign", new Vector2(x + 2.3f, floor + 3.5f), "CORRE →");
        }

        private static GreenEggRescue2D Sanctuary(CaveLevelSceneFactory f, Transform root)
        {
            var portal = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Resources/Resource_LevelExitPortal_Universal.prefab"));
            portal.name = "Portal_Exit_To_Boss_3";
            portal.transform.SetParent(root);
            portal.transform.position = new Vector2(123f, 21f);
            var exit = portal.GetComponent<LevelExit2D>();
            CaveLevelSceneFactory.Set(exit, "_nextSceneName", "Boss_3");
            CaveLevelSceneFactory.Set(exit, "_levelTitle", "EL HUEVO MORADO ESTÁ A SALVO");
            CaveLevelSceneFactory.Set(exit, "_victoryMessage", "Tres de cuatro. Solo nos falta uno.\nEl Pterodáctilo Alfa desciende sobre el sauce...");
            portal.SetActive(false);
            f.Visual("Moss_Nest", root, new Vector2(118f, 19.7f), new Vector2(2.5f, 0.4f), Purple);
            var go = f.Visual("Purple_Egg", root, new Vector2(118f, 20.6f), new Vector2(0.8f, 1.1f), Purple, true);
            go.GetComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Circle.png");
            go.AddComponent<CircleCollider2D>().isTrigger = true;
            CaveLevelSceneFactory.Light(go.transform, Vector2.zero, Purple, 4f, 1f);
            var egg = go.AddComponent<GreenEggRescue2D>();
            egg.Configure(EggType.PurpleEgg, "¡TERCER RESCATE: EL HUEVO MORADO!",
                "El cascarón tiembla...\nFalta muy poco para que rompas a cantar.\nSolo nos falta uno.\n(3 de 4 rescatados)", Purple, exit);
            var guardian = new GameObject("Pterodactyl_Guardian_Teaser");
            guardian.transform.SetParent(root);
            guardian.transform.position = new Vector2(122f, 23.5f);
            var torso = f.Visual("Pterodactyl_Body", guardian.transform, Vector2.zero, new Vector2(1.2f, 1.2f), new Color(0.3f, 0.22f, 0.38f), true);
            torso.GetComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Circle.png");
            for (int i = 0; i < 2; i++)
            {
                var wing = f.Visual("Pterodactyl_Wing", guardian.transform, new Vector2(i == 0 ? -1.8f : 1.8f, 0.2f),
                    new Vector2(3f, 0.55f), new Color(0.35f, 0.24f, 0.4f), true);
                wing.transform.localRotation = Quaternion.Euler(0f, 0f, i == 0 ? 18f : -18f);
            }
            f.Visual("Pterodactyl_Eye", guardian.transform, new Vector2(-0.2f, 0.2f), Vector2.one * 0.14f, Color.yellow, true);
            guardian.SetActive(false);
            var sanctuary = root.gameObject.AddComponent<PurpleEggSanctuary2D>();
            CaveLevelSceneFactory.Set(sanctuary, "_egg", egg);
            CaveLevelSceneFactory.Set(sanctuary, "_guardian", guardian);
            return egg;
        }

        private static void Gas(CaveLevelSceneFactory f, Transform root, PlayerController player, GreenEggRescue2D egg)
        {
            const string path = "Assets/_Project/ScriptableObjects/RisingGasConfig.asset";
            var config = AssetDatabase.LoadAssetAtPath<RisingGasConfigSO>(path);
            if (config == null) { config = ScriptableObject.CreateInstance<RisingGasConfigSO>(); AssetDatabase.CreateAsset(config, path); }
            var go = new GameObject("Rising_Toxic_Gas");
            go.transform.SetParent(root);
            go.transform.position = new Vector2(60f, -29f);
            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(140f, 50f);
            collider.isTrigger = true;
            var cloud = f.Visual("Toxic_Gas_Cloud", go.transform, Vector2.zero, new Vector2(140f, 50f), new Color(0.22f, 0.69f, 0f, 0.35f), true);
            cloud.GetComponent<SpriteRenderer>().sortingOrder = 2;
            var surface = f.Visual("Toxic_Gas_Surface", go.transform, new Vector2(0f, 25f), new Vector2(140f, 0.08f), Color.yellow, true);
            surface.GetComponent<SpriteRenderer>().sortingOrder = 3;
            var warning = Label(root, "Gas_Warning", new Vector2(10f, 3.5f), "¡EL GAS VA A SUBIR!");
            var gas = go.AddComponent<RisingHazardFloor2D>();
            CaveLevelSceneFactory.Set(gas, "_config", config);
            CaveLevelSceneFactory.Set(gas, "_playerSource", player);
            CaveLevelSceneFactory.Set(gas, "_egg", egg);
            CaveLevelSceneFactory.Set(gas, "_surface", surface.GetComponent<SpriteRenderer>());
            CaveLevelSceneFactory.Set(gas, "_warningLabel", warning);
            var data = new SerializedObject(gas);
            var stages = data.FindProperty("_stages");
            stages.arraySize = 3;
            float[,] layout = { { 12f, 39f, 0f, 7.5f }, { 44f, 83f, 7.5f, 12f }, { 88f, 109f, 12f, 19.5f } };
            for (int i = 0; i < 3; i++)
            {
                var stage = stages.GetArrayElementAtIndex(i);
                stage.FindPropertyRelative("StartX").floatValue = layout[i, 0];
                stage.FindPropertyRelative("EndX").floatValue = layout[i, 1];
                stage.FindPropertyRelative("FloorY").floatValue = layout[i, 2];
                stage.FindPropertyRelative("EndFloorY").floatValue = layout[i, 3];
            }
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static TextMesh Label(Transform root, string name, Vector2 position, string text)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root);
            go.transform.position = position;
            var label = go.AddComponent<TextMesh>();
            label.text = text;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 40;
            label.characterSize = 0.07f;
            label.anchor = TextAnchor.MiddleCenter;
            label.color = Color.yellow;
            go.GetComponent<MeshRenderer>().sharedMaterial = label.font.material;
            go.GetComponent<MeshRenderer>().sortingOrder = 8;
            return label;
        }

        private static void Hint(Transform root, string name, Vector2 position, string title, string message, bool onStart = false)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root);
            go.transform.position = position;
            var collider = go.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(1.5f, 2f);
            go.AddComponent<NarrativePrologueTrigger>().Configure(title, message, Purple, 6f, onStart);
        }

        private static void Atmosphere(CaveLevelSceneFactory f, Transform root)
        {
            for (int i = 0; i < 20; i++)
            {
                var trunk = f.Visual("Willow_Trunk", root, new Vector2(-10f + i * 8f, 10f), new Vector2(0.55f, 32f),
                    new Color(0.24f, 0.31f, 0.24f), true);
                trunk.GetComponent<SpriteRenderer>().sortingOrder = -15;
                for (int j = 0; j < 3; j++)
                {
                    var leaf = f.Visual("Hanging_Willow_Leaf", trunk.transform, new Vector2(-2f + j * 2f, 0.2f),
                        new Vector2(0.5f, 0.25f), new Color(0.32f, 0.42f, 0.28f), true);
                    leaf.GetComponent<SpriteRenderer>().sortingOrder = -14;
                }
            }
            CaveLevelSceneFactory.Light(root, new Vector2(118f, 23f), new Color(1f, 0.83f, 0.4f), 8f, 1f);
        }
    }
}
#endif
