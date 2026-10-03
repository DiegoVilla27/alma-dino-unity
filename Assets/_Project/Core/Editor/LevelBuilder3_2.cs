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
    public static class LevelBuilder3_2
    {
        public const string ScenePath = "Assets/Scenes/World_3_Swamp/Level_3_2.unity";
        private static readonly Color Slate = new Color(0.25f, 0.35f, 0.46f);
        private static readonly Color Gale = new Color(0.88f, 0.88f, 0.87f, 0.4f);

        [MenuItem("Alma/📂 Cargar Nivel 3-2")]
        public static void LoadLevel()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(ScenePath);
        }

        [MenuItem("Tools/Alma/Construir Nivel 3-2 - El Cañón de las Ráfagas")]
        public static void BuildLevel3_2()
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
            Rock(factory, "Entrance_Runway", 5f, -0.75f, 22f, 1.5f);
            Rock(factory, "Refuge_One", 32.5f, -0.75f, 15f, 1.5f);
            Rock(factory, "Refuge_Two", 59f, -0.75f, 18f, 1.5f);
            Rock(factory, "Exit_Refuge", 86f, -0.75f, 14f, 1.5f);
            Mud(factory, root.transform, "Toxic_Gap_One", 16f, 25f);
            Mud(factory, root.transform, "Toxic_Gap_Two", 40f, 50f);
            Mud(factory, root.transform, "Toxic_Gap_Final", 68f, 79f);
            Wind(factory, root.transform, "Wind_Tutorial", 4f, 14f);
            Wind(factory, root.transform, "Wind_Gap_One", 14f, 25f);
            Wind(factory, root.transform, "Wind_Gap_Two", 38f, 50f);
            Wind(factory, root.transform, "Wind_Gap_Final", 66f, 79f);
            // Teach breaking a wall over solid ground before combining it with a canyon crossing.
            Barrier(factory, root.transform, "Reed_Barrier_Tutorial", 10f);
            Barrier(factory, root.transform, "Reed_Barrier_Two", 45f);
            Barrier(factory, root.transform, "Reed_Barrier_Final", 74.5f);
            factory.Checkpoint("Checkpoint_Refuge_One", 28f, 0f);
            factory.Checkpoint("Checkpoint_Refuge_Two", 64f, 0f);
            Rock(factory, "Refuge_One_Canopy", 28f, 6f, 7f, 0.7f);
            Rock(factory, "Refuge_Two_Canopy", 64f, 6f, 7f, 0.7f);
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
            var camera = Camera.main;
            camera.orthographicSize = 6f;
            camera.backgroundColor = new Color(0.25f, 0.33f, 0.39f);
            camera.transform.position = new Vector3(0f, 2f, -10f);
            var follow = camera.GetComponent<Camera2DFollow>();
            follow.SetTarget(player.transform);
            follow.SetBounds(new Vector2(0f, 1f), new Vector2(90f, 5f));
            CaveLevelSceneFactory.Set(follow, "_offset", new Vector2(0f, 1.2f));
            CaveLevelSceneFactory.Set(follow, "_smoothTime", 0.12f);
            var global = Object.FindObjectsByType<Light2D>().First(light => light.lightType == Light2D.LightType.Global);
            global.intensity = 0.7f;
            global.color = new Color(0.82f, 0.9f, 0.96f);
            Atmosphere(factory, root.transform, camera.transform);
            var tutorial = new GameObject("Gale_Tutorial");
            tutorial.transform.SetParent(root.transform);
            tutorial.transform.position = new Vector2(2f, 1f);
            tutorial.AddComponent<BoxCollider2D>().isTrigger = true;
            tutorial.AddComponent<NarrativePrologueTrigger>().Configure("EL CAÑÓN DE LAS RÁFAGAS",
                "El viento empuja, pero mis alas lo cortan.\nSalta y usa DASH contra las cañas. Aterriza para recuperar el impulso.", Color.cyan, 6f);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            var scenes = EditorBuildSettings.scenes.ToList();
            scenes.RemoveAll(entry => entry.path == ScenePath);
            int previousIndex = scenes.FindIndex(entry => entry.path.EndsWith("Level_3_1.unity"));
            scenes.Insert(previousIndex + 1, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("[AlmaDino] Level_3_2 built: four wind zones, three Dash barriers and two sheltered checkpoints.");
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

        private static void Barrier(CaveLevelSceneFactory factory, Transform root, string name, float x)
        {
            var wall = factory.Visual(name, root, new Vector2(x, 2.5f), new Vector2(0.45f, 7f),
                new Color(0.36f, 0.3f, 0.24f), true);
            wall.AddComponent<BoxCollider2D>();
            wall.AddComponent<DashBreakableBarrier2D>();
            for (int i = 0; i < 2; i++)
            {
                var crack = factory.Visual("Reed_Crack", wall.transform, new Vector2(0f, -0.2f + i * 0.4f),
                    new Vector2(0.8f, 0.035f), Color.cyan, true);
                crack.transform.localRotation = Quaternion.Euler(0f, 0f, i == 0 ? 20f : -20f);
            }
        }

        private static void Wind(CaveLevelSceneFactory factory, Transform root, string name, float left, float right)
        {
            var zone = new GameObject(name);
            zone.transform.SetParent(root);
            zone.transform.position = new Vector2((left + right) * 0.5f, 2.5f);
            var trigger = zone.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(right - left, 7f);
            var wind = zone.AddComponent<WindCurrentZone2D>();
            CaveLevelSceneFactory.Set(wind, "_direction", Vector2.left);
            CaveLevelSceneFactory.Set(wind, "_windStrength", 12f);
            CaveLevelSceneFactory.Set(wind, "_counteractGravity", false);
            for (int i = 0; i < 12; i++)
            {
                var streak = factory.Visual("Gale_Streak", zone.transform,
                    new Vector2(-(right - left) * 0.4f + (i % 4) * (right - left) * 0.25f, -1.2f + (i / 4) * 1.4f),
                    new Vector2(1.5f, 0.045f), Gale, true);
                streak.GetComponent<SpriteRenderer>().sortingOrder = -1;
            }
            var particles = new GameObject("Leftward_Gale_Particles");
            particles.transform.SetParent(zone.transform, false);
            particles.transform.localPosition = new Vector2((right - left) * 0.5f, 0f);
            var system = particles.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = system.main;
            main.startLifetime = (right - left) / 8f;
            main.startSpeed = 0f;
            main.startSize = 0.07f;
            main.startColor = Gale;
            main.maxParticles = 100;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = system.emission;
            emission.rateOverTime = 15f;
            var shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(0.1f, 5f, 0.1f);
            var velocity = system.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = -8f;
            var renderer = system.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                "Packages/com.unity.render-pipelines.universal/Runtime/Materials/ParticlesUnlit.mat");
            renderer.sortingOrder = -1;
        }

        private static void Portal(Transform root)
        {
            var portal = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Resources/Resource_LevelExitPortal_Universal.prefab"));
            portal.name = "Portal_Exit_To_3_3";
            portal.transform.SetParent(root);
            portal.transform.position = new Vector2(90f, 1.5f);
            var exit = portal.GetComponent<LevelExit2D>();
            CaveLevelSceneFactory.Set(exit, "_nextSceneName", "Level_3_3");
            CaveLevelSceneFactory.Set(exit, "_levelTitle", "EL CAÑÓN DE LAS RÁFAGAS COMPLETADO");
            CaveLevelSceneFactory.Set(exit, "_victoryMessage", "Ningún vendaval nos separará.\nLas esporas del pantano anuncian el siguiente camino: nivel 3-3.");
        }

        private static void Atmosphere(CaveLevelSceneFactory factory, Transform root, Transform camera)
        {
            for (int layer = 0; layer < 3; layer++)
            {
                var group = new GameObject("Canyon_Parallax_" + layer);
                group.transform.SetParent(root, false);
                var parallax = group.AddComponent<ParallaxLayer2D>();
                CaveLevelSceneFactory.Set(parallax, "_camera", camera);
                CaveLevelSceneFactory.Set(parallax, "_cameraMotionFactor", new Vector2(0.75f - layer * 0.3f, 0.1f));
                for (int i = 0; i < 18; i++)
                {
                    var silhouette = factory.Visual(layer == 1 ? "Leaning_Willow" : "Canyon_Cliff", group.transform,
                        new Vector2(-15f + i * 8f, 5f), new Vector2(layer == 1 ? 0.4f : 5f, 18f),
                        new Color(0.18f, 0.26f, 0.3f, 0.25f + layer * 0.1f), true);
                    silhouette.transform.localRotation = Quaternion.Euler(0f, 0f, layer == 1 ? 18f : -8f);
                    silhouette.GetComponent<SpriteRenderer>().sortingOrder = -30 + layer * 8;
                }
            }
        }
    }
}
#endif
