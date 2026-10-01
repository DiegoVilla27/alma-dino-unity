#if UNITY_EDITOR
using System.IO;
using System.Linq;
using AlmaDino.Core.Events;
using AlmaDino.Features.Boss.Controllers;
using AlmaDino.Features.Boss.ScriptableObjects;
using AlmaDino.Features.Camera;
using AlmaDino.Features.Environment;
using AlmaDino.Features.Player.Controllers;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AlmaDino.Core.Editor
{
    public static class LevelBuilderBoss_2
    {
        public const string ScenePath = "Assets/Scenes/World_2_Caves/Boss_2.unity";
        [MenuItem("Tools/Alma/Construir Jefe 2 - Armadillo Prehistórico")]
        public static void BuildBoss2()
        {
            if (EditorApplication.isPlaying) return;
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!File.Exists(ScenePath)) AssetDatabase.CopyAsset(LevelBuilder2_4.ScenePath, ScenePath);
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var old = GameObject.Find("--- LEVEL ---");
            if (old != null) Object.DestroyImmediate(old);
            var root = new GameObject("--- LEVEL ---");
            var f = new CaveLevelSceneFactory(root.transform);
            var config = LoadConfig();
            var player = GameObject.Find("Alma (Player)").GetComponent<PlayerController>();
            player.transform.position = new Vector2(-6f, .7f);
            CaveLevelSceneFactory.Set(player, "_doubleJumpUnlocked", true);
            CaveLevelSceneFactory.Set(player, "_groundPoundUnlocked", true);
            CaveLevelSceneFactory.Set(player, "_dashUnlocked", false);
            CaveLevelSceneFactory.Set(player, "_roarUnlocked", false);
            SceneSetupValidator.SetupAlmaVisuals(player.gameObject);
            SceneSetupValidator.SetupTouchControls();
            f.Platform("Arena_Floor", new Vector2(0f, -.5f), new Vector2(22f, 1f));
            f.Platform("Left_Boundary", new Vector2(-11f, 4f), new Vector2(1f, 10f));
            f.Platform("Right_Boundary", new Vector2(11f, 4f), new Vector2(1f, 10f));
            f.Checkpoint("Arena_Checkpoint", -6f, 0f);
            foreach (float x in new[] { -5f, 5f })
            {
                var ledge = f.Platform(x < 0f ? "Left_Refuge" : "Right_Refuge",
                    new Vector2(x, 2f), new Vector2(3f, .3f));
                ledge.GetComponent<BoxCollider2D>().usedByEffector = true;
                var effector = ledge.AddComponent<PlatformEffector2D>();
                effector.useOneWay = true;
                effector.useOneWayGrouping = true;
                var pillar = f.Visual("Impact_Geode", root.transform, new Vector2(Mathf.Sign(x) * 9.8f, 1.5f),
                    new Vector2(1f, 3f), new Color(.6f, .12f, .4f), true);
                CaveLevelSceneFactory.Light(pillar.transform, Vector2.zero, Color.magenta, 3f, .7f);
            }
            var portal = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Universal/Level_Exit_Portal.prefab"));
            portal.name = "Portal_Victory_To_World_3";
            portal.transform.SetParent(root.transform, false);
            portal.transform.position = new Vector2(0f, 1.4f);
            var exit = portal.GetComponent<LevelExit2D>();
            CaveLevelSceneFactory.Set(exit, "_nextSceneName", "Level_3_1");
            CaveLevelSceneFactory.Set(exit, "_levelTitle", "CUEVAS DE CRISTAL COMPLETADAS");
            CaveLevelSceneFactory.Set(exit, "_victoryMessage", "El Armadillo cede el paso. El Pantano de Viento te espera.");
            portal.SetActive(false);
            var bossGo = new GameObject("Boss_Prehistoric_Armadillo");
            bossGo.transform.SetParent(root.transform, false);
            bossGo.transform.position = new Vector2(0f, 1f);
            var body = bossGo.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.useFullKinematicContacts = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            bossGo.AddComponent<BoxCollider2D>().size = new Vector2(2f, 2f);
            var shell = f.Visual("Armored_Shell", bossGo.transform, Vector2.zero,
                new Vector2(2f, 2f), new Color(.22f, .25f, .35f), true);
            shell.GetComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Circle.png");
            for (int i = 0; i < 5; i++)
                f.Visual("Armor_Band", shell.transform, new Vector2(-.4f + i * .2f, 0f),
                    new Vector2(.03f, .8f), CaveLevelSceneFactory.Crystal, true);
            var weak = f.Visual("Exposed_Crown_Fissure", bossGo.transform, new Vector2(0f, 1f),
                new Vector2(1.4f, .2f), Color.yellow, true);
            var boss = bossGo.AddComponent<PrehistoricArmadilloBoss2D>();
            CaveLevelSceneFactory.Set(boss, "_config", config);
            CaveLevelSceneFactory.Set(boss, "_playerSource", player);
            CaveLevelSceneFactory.Set(boss, "_shell", shell.GetComponent<SpriteRenderer>());
            CaveLevelSceneFactory.Set(boss, "_weakPoint", weak);
            CaveLevelSceneFactory.Set(boss, "_exit", portal);
            CaveLevelSceneFactory.Set(boss, "_rockPrefab", BuildRock(f, root.transform));
            var shake = AssetDatabase.LoadAssetAtPath<CameraShakeEventChannelSO>("Assets/_Project/Core/Events/CameraShakeChannel.asset");
            if (shake != null) CaveLevelSceneFactory.Set(boss, "_shake", shake);
            var camera = Camera.main;
            var follow = camera.GetComponent<Camera2DFollow>();
            if (follow != null)
            {
                follow.enabled = true;
                follow.SetTarget(player.transform);
                follow.SetBounds(new Vector2(-8f, 1.5f), new Vector2(8f, 7f));
                CaveLevelSceneFactory.Set(follow, "_offset", new Vector2(0f, 1.5f));
                CaveLevelSceneFactory.Set(follow, "_lookAheadDistance", 1.25f);
            }
            camera.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 1.5f, -10f);
            camera.orthographicSize = 6f;
            camera.backgroundColor = new Color(.03f, .05f, .12f);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            var scenes = EditorBuildSettings.scenes.ToList();
            scenes.RemoveAll(entry => entry.path == ScenePath);
            scenes.Insert(scenes.FindIndex(entry => entry.path == LevelBuilder2_4.ScenePath) + 1,
                new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
        }

        private static ArmadilloBossConfigSO LoadConfig()
        {
            const string path = "Assets/_Project/ScriptableObjects/ArmadilloBossConfig.asset";
            var config = AssetDatabase.LoadAssetAtPath<ArmadilloBossConfigSO>(path);
            if (config != null) return config;
            config = ScriptableObject.CreateInstance<ArmadilloBossConfigSO>();
            AssetDatabase.CreateAsset(config, path);
            return config;
        }

        private static BossFallingCrystal2D BuildRock(CaveLevelSceneFactory f, Transform root)
        {
            var go = f.Visual("Boss_Falling_Crystal", root, Vector2.zero, new Vector2(.45f, .8f), Color.magenta, true);
            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            go.AddComponent<BoxCollider2D>().isTrigger = true;
            var crystal = go.AddComponent<BossFallingCrystal2D>();
            var marker = f.Visual("Impact_Warning", go.transform, new Vector2(0f, -9.8f),
                new Vector2(2.5f, .08f), Color.yellow, true);
            CaveLevelSceneFactory.Set(crystal, "_warningMarker", marker);
            const string path = "Assets/_Project/Prefabs/World_2_Caves/Boss_Falling_Crystal.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab.GetComponent<BossFallingCrystal2D>();
        }

        [MenuItem("Alma/📂 Cargar Arena Jefe 2")]
        public static void LoadBoss2()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(ScenePath);
        }
    }
}
#endif
