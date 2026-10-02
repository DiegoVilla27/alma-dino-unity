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
    public static class LevelBuilderBoss_3
    {
        public const string ScenePath = "Assets/Scenes/World_3_Swamp/Boss_3.unity";
        [MenuItem("Tools/Alma/Construir Jefe 3 - Pterodáctilo Alfa")]
        public static void BuildBoss3()
        {
            if (EditorApplication.isPlaying) return;
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
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
            SceneSetupValidator.SetupAlmaVisuals(player.gameObject);
            SceneSetupValidator.SetupTouchControls();
            var branches = new GameObject[4];
            float[] positions = { 0f, -6f, 6f, 12f };
            float[] heights = { 0f, .6f, .6f, 1.2f };
            for (int i = 0; i < branches.Length; i++)
            {
                branches[i] = f.Platform("Arena_Branch_" + i, new Vector2(positions[i], heights[i] - .3f), new Vector2(i == 0 ? 6f : 4f, .6f));
                branches[i].GetComponent<SpriteRenderer>().color = new Color(.28f, .22f, .13f);
                branches[i].transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(.4f, .57f, .2f);
                f.Visual("Branch_Root", branches[i].transform, new Vector2(0f, -1.5f), new Vector2(.12f, 3f), new Color(.2f, .26f, .14f));
            }
            f.Checkpoint("Arena_Checkpoint", 0f, 0f);
            f.Hazard("Swamp_Abyss", new Vector2(3f, -6f), new Vector2(60f, 2f));
            for (int i = 0; i < 6; i++)
            {
                var ridge = f.Visual("Distant_Volcanic_Ridge", root.transform, new Vector2(-15f + i * 7f, -5f), new Vector2(10f, 10f), new Color(.15f, .07f, .13f), true);
                ridge.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
                ridge.GetComponent<SpriteRenderer>().sortingOrder = -20;
            }
            var portal = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Universal/Level_Exit_Portal.prefab"));
            portal.name = "Portal_Victory_To_World_4";
            portal.transform.SetParent(root.transform, false);
            portal.transform.position = new Vector2(0f, 1.4f);
            var exit = portal.GetComponent<LevelExit2D>();
            CaveLevelSceneFactory.Set(exit, "_nextSceneName", "Level_4_1");
            CaveLevelSceneFactory.Set(exit, "_levelTitle", "PANTANO DE VIENTO COMPLETADO");
            CaveLevelSceneFactory.Set(exit, "_victoryMessage", "El Pterodáctilo huye. La Cima Volcánica te espera.");
            portal.SetActive(false);
            var go = new GameObject("Boss_Alpha_Pterodactyl");
            go.transform.SetParent(root.transform, false);
            go.transform.position = new Vector2(5f, 6f);
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var boss = go.AddComponent<PterodactylBoss2D>();
            go.AddComponent<CircleCollider2D>().radius = .65f;
            go.GetComponent<CircleCollider2D>().isTrigger = true;
            var head = go.AddComponent<PterodactylHead2D>();
            CaveLevelSceneFactory.Set(head, "_boss", boss);
            var visual = new GameObject("Pterodactyl_Visual");
            visual.transform.SetParent(go.transform, false);
            var circle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Circle.png");
            var skull = f.Visual("Head", visual.transform, Vector2.zero, new Vector2(1.5f, 1.2f), new Color(.37f, .5f, .36f), true);
            skull.GetComponent<SpriteRenderer>().sprite = circle;
            skull.GetComponent<SpriteRenderer>().sortingOrder = 3;
            var eye = f.Visual("Eye", visual.transform, new Vector2(-.3f, .25f), new Vector2(.3f, .3f), Color.black, true);
            eye.GetComponent<SpriteRenderer>().sprite = circle;
            eye.GetComponent<SpriteRenderer>().sortingOrder = 4;
            var pupil = f.Visual("Eye_Glint", visual.transform, new Vector2(-.34f, .26f), new Vector2(.13f, .13f), Color.yellow, true);
            pupil.GetComponent<SpriteRenderer>().sprite = circle;
            pupil.GetComponent<SpriteRenderer>().sortingOrder = 5;
            f.Visual("Beak", visual.transform, new Vector2(-.9f, -.1f), new Vector2(.9f, .25f), new Color(.75f, .68f, .23f), true);
            var crest = f.Visual("Cyan_Crest", visual.transform, new Vector2(.15f, .75f), new Vector2(.3f, 1f), Color.cyan, true);
            crest.transform.localRotation = Quaternion.Euler(0f, 0f, -30f);
            var torso = f.Visual("Body_Hazard", visual.transform, new Vector2(2.2f, 0f), new Vector2(2.1f, 1.2f), new Color(.26f, .36f, .28f), true);
            torso.GetComponent<SpriteRenderer>().sprite = circle;
            // Visual scale is baked into the collider by local size, distinct from the head.
            torso.AddComponent<BoxCollider2D>().isTrigger = true;
            CaveLevelSceneFactory.Set(torso.AddComponent<PterodactylBody2D>(), "_boss", boss);
            var wings = new Transform[2];
            for (int side = -1; side <= 1; side += 2)
            {
                var wing = f.Visual("Purple_Wing", visual.transform, new Vector2(2f, side * 1.3f), new Vector2(3.4f, .8f), new Color(.235f, .035f, .42f), true);
                f.Visual("Wing_Bone", wing.transform, Vector2.zero, new Vector2(.95f, .06f), new Color(.45f, .18f, .61f), true);
                wings[side < 0 ? 0 : 1] = wing.transform;
                wing.transform.localRotation = Quaternion.Euler(0f, 0f, side * 30f);
            }
            var streaks = new SpriteRenderer[7];
            for (int i = 0; i < streaks.Length; i++)
                streaks[i] = f.Visual("Wind_Streak", root.transform, new Vector2(i * 3f - 10f, 1f + i % 4), new Vector2(1.1f, .05f), new Color(.85f, .9f, .95f, .55f), true).GetComponent<SpriteRenderer>();
            var marker = f.Visual("Air_Dash_Target_Height", root.transform, new Vector2(0f, 3.5f), new Vector2(1.8f, .08f), Color.cyan, true);
            marker.SetActive(false);
            var hint = new GameObject("Boss_Attack_Hint");
            hint.transform.SetParent(root.transform, false);
            var text = hint.AddComponent<TextMesh>();
            text.fontSize = 40; text.characterSize = .06f; text.anchor = TextAnchor.MiddleCenter;
            var particles = new GameObject("Dash_Impact_Sparks");
            particles.transform.SetParent(go.transform, false);
            var sparks = particles.AddComponent<ParticleSystem>();
            var main = sparks.main;
            main.playOnAwake = false; main.loop = false; main.duration = .25f; main.startLifetime = .4f; main.startSpeed = 4f; main.startSize = .12f; main.startColor = Color.cyan;
            var emission = sparks.emission; emission.rateOverTime = 0f; emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 20) });
            sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var config = AssetDatabase.LoadAssetAtPath<PterodactylBossConfigSO>("Assets/_Project/ScriptableObjects/PterodactylBossConfig.asset");
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<PterodactylBossConfigSO>();
                AssetDatabase.CreateAsset(config, "Assets/_Project/ScriptableObjects/PterodactylBossConfig.asset");
            }
            CaveLevelSceneFactory.Set(boss, "_config", config);
            CaveLevelSceneFactory.Set(boss, "_playerSource", player);
            CaveLevelSceneFactory.Set(boss, "_visual", visual.transform);
            CaveLevelSceneFactory.Set(boss, "_crest", crest.GetComponent<SpriteRenderer>());
            CaveLevelSceneFactory.Set(boss, "_hint", text);
            CaveLevelSceneFactory.Set(boss, "_exit", portal);
            CaveLevelSceneFactory.Set(boss, "_sparks", sparks);
            CaveLevelSceneFactory.Set(boss, "_shake", AssetDatabase.LoadAssetAtPath<CameraShakeEventChannelSO>("Assets/_Project/Core/Events/CameraShakeChannel.asset"));
            var data = new SerializedObject(boss);
            var windEntries = data.FindProperty("_windStreaks"); windEntries.arraySize = streaks.Length;
            for (int i = 0; i < streaks.Length; i++) windEntries.GetArrayElementAtIndex(i).objectReferenceValue = streaks[i];
            var wingEntries = data.FindProperty("_wings"); wingEntries.arraySize = wings.Length;
            for (int i = 0; i < wings.Length; i++) wingEntries.GetArrayElementAtIndex(i).objectReferenceValue = wings[i];
            data.FindProperty("_attackMarker").objectReferenceValue = marker.transform;
            var entries = data.FindProperty("_branches"); entries.arraySize = branches.Length;
            for (int i = 0; i < branches.Length; i++) entries.GetArrayElementAtIndex(i).objectReferenceValue = branches[i];
            data.ApplyModifiedPropertiesWithoutUndo();
            var camera = Camera.main;
            camera.orthographicSize = 6f;
            camera.backgroundColor = new Color(.09f, .1f, .16f);
            var follow = camera.GetComponent<Camera2DFollow>();
            follow.SetTarget(player.transform); follow.SetBounds(new Vector2(-8f, 1f), new Vector2(14f, 8f));
            CaveLevelSceneFactory.Set(follow, "_offset", new Vector2(0f, 1.5f));
            CaveLevelSceneFactory.Set(follow, "_lookAheadDistance", 1.25f);
            camera.transform.position = new Vector3(0f, 2.2f, -10f);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            var scenes = EditorBuildSettings.scenes.ToList();
            scenes.RemoveAll(s => s.path == ScenePath);
            scenes.Insert(scenes.FindIndex(s => s.path == LevelBuilder3_4.ScenePath) + 1, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
        }
        [MenuItem("Alma/📂 Cargar Arena Jefe 3")]
        public static void LoadBoss3()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(ScenePath);
        }
    }
}
#endif
