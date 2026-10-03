#if UNITY_EDITOR
using System.IO;
using System.Linq;
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
    public static class LevelBuilder4_2
    {
        public const string ScenePath = "Assets/Scenes/World_4_Volcano/Level_4_2.unity";
        [MenuItem("Tools/Alma/Construir Nivel 4-2 - Las Campanas de Basalto")]
        public static void BuildLevel4_2()
        {
            if (EditorApplication.isPlaying) return;
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!File.Exists(ScenePath)) AssetDatabase.CopyAsset(LevelBuilder4_1.ScenePath, ScenePath);
            var scene = EditorSceneManager.OpenScene(ScenePath);
            Object.DestroyImmediate(GameObject.Find("--- LEVEL ---"));
            var root = new GameObject("--- LEVEL ---");
            var f = new CaveLevelSceneFactory(root.transform);
            var player = GameObject.Find("Alma (Player)").GetComponent<PlayerController>();
            player.transform.position = new Vector2(0f, .7f);
            CaveLevelSceneFactory.Set(player, "_doubleJumpUnlocked", true); CaveLevelSceneFactory.Set(player, "_groundPoundUnlocked", true);
            CaveLevelSceneFactory.Set(player, "_dashUnlocked", true); CaveLevelSceneFactory.Set(player, "_roarUnlocked", true);
            CaveLevelSceneFactory.Set(player, "_fallDeathY", -8f);
            SceneSetupValidator.SetupAlmaVisuals(player.gameObject); SceneSetupValidator.SetupTouchControls();
            LevelBuilder4_1.Floor(f, "Entrance_Bell_Island", -6f, 20f);
            LevelBuilder4_1.Floor(f, "Aerial_Bell_Island", 28f, 50f);
            LevelBuilder4_1.Floor(f, "Chain_Entry_Island", 58f, 70f);
            LevelBuilder4_1.Floor(f, "Chain_Middle_Island", 78f, 84f);
            LevelBuilder4_1.Floor(f, "Chain_Last_Island", 92f, 98f);
            LevelBuilder4_1.Floor(f, "Exit_Island", 106f, 122f);
            f.Platform("Entrance_Basalt_Boundary", new Vector2(-5.5f, 3f), new Vector2(1f, 8f));
            f.Platform("Exit_Basalt_Boundary", new Vector2(123f, 3f), new Vector2(1f, 8f));
            foreach (float x in new[] { 20f, 50f, 70f, 84f, 98f }) LevelBuilder4_1.Lava(f, root.transform, "Bell_Lava_" + x, x, x + 8f);
            f.Checkpoint("Checkpoint_First_Bell", 30f, 0f); f.Checkpoint("Checkpoint_Bell_Chain", 62f, 0f);
            var config = AssetDatabase.LoadAssetAtPath<ResonanceBellConfigSO>("Assets/_Project/ScriptableObjects/ResonanceBellConfig.asset");
            if (config == null) { config = ScriptableObject.CreateInstance<ResonanceBellConfigSO>(); AssetDatabase.CreateAsset(config, "Assets/_Project/ScriptableObjects/ResonanceBellConfig.asset"); }
            Vector2[] bells = { new Vector2(12f, 5.2f), new Vector2(45.5f, 6.5f), new Vector2(68f, 5.2f), new Vector2(85.5f, 4.6f), new Vector2(99.5f, 4.6f) };
            float[] gates = { 16f, 48f, 68f, 82f, 96f };
            for (int i = 0; i < bells.Length; i++)
            {
                var bell = Bell(f, root.transform, player, config, i, bells[i]);
                Door(f, root.transform, bell, i, gates[i]);
            }
            LevelBuilder4_1.Hint(root.transform, "First_Bell_Hint", new Vector2(6f, 2.4f), "ROAR (E/F) → CAMPANA\n5 SEGUNDOS PARA CRUZAR", Color.yellow);
            LevelBuilder4_1.Hint(root.transform, "Aerial_Bell_Hint", new Vector2(40f, 2f), "DOBLE SALTO + ROAR EN EL AIRE ↗\nATERRIZA Y CORRE HACIA EL DASH", Color.yellow);
            LevelBuilder4_1.Hint(root.transform, "Chain_Bell_Hint", new Vector2(62f, 2.5f), "TRES CAMPANAS, TRES PUERTAS\nRUGE DESDE CADA ISLA", Color.yellow);
            var waveGo = new GameObject("Resonance_Roar_Wave"); waveGo.transform.SetParent(root.transform, false);
            var wave = waveGo.AddComponent<RoarWaveVisual2D>();
            var line = waveGo.GetComponent<LineRenderer>(); line.startWidth = .07f; line.endWidth = .07f; line.sortingOrder = 8;
            line.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat");
            CaveLevelSceneFactory.Set(wave, "_playerSource", player); CaveLevelSceneFactory.Set(wave, "_duration", player.Config.RoarDuration);
            CaveLevelSceneFactory.Set(wave, "_radius", player.Config.RoarResonanceRange); CaveLevelSceneFactory.Set(wave, "_halfAngle", player.Config.RoarHalfAngle);
            var exitGo = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Resources/Resource_LevelExitPortal_Universal.prefab"));
            exitGo.name = "Portal_Exit_To_4_3"; exitGo.transform.SetParent(root.transform, false); exitGo.transform.position = new Vector2(118f, 1.5f);
            CaveLevelSceneFactory.Set(exitGo.GetComponent<LevelExit2D>(), "_nextSceneName", "Level_4_3");
            CaveLevelSceneFactory.Set(exitGo.GetComponent<LevelExit2D>(), "_levelTitle", "LAS CAMPANAS DE BASALTO COMPLETADAS");
            CaveLevelSceneFactory.Set(exitGo.GetComponent<LevelExit2D>(), "_victoryMessage", "Tu rugido domina el fuego.\nEl camino al cráter exige combinar todas tus habilidades.");
            var camera = Camera.main; camera.orthographicSize = 6f; camera.backgroundColor = new Color(.12f, .065f, .07f);
            camera.transform.position = new Vector3(0f, 2f, -10f);
            var follow = camera.GetComponent<Camera2DFollow>(); follow.SetTarget(player.transform); follow.SetBounds(new Vector2(0f, 1f), new Vector2(119f, 9f));
            CaveLevelSceneFactory.Set(follow, "_offset", new Vector2(0f, 1.5f)); CaveLevelSceneFactory.Set(follow, "_lookAheadDistance", 1.25f);
            LevelBuilder4_1.Atmosphere(f, root.transform, camera.transform);
            var prologue = new GameObject("Bell_Sanctuary_Prologue"); prologue.transform.SetParent(root.transform, false); prologue.transform.position = new Vector2(1f, 1f);
            prologue.AddComponent<BoxCollider2D>().isTrigger = true;
            prologue.AddComponent<NarrativePrologueTrigger>().Configure("LAS CAMPANAS DE BASALTO", "Mi voz llega hasta la piedra colgada.\nQue su eco apague el fuego... antes de que vuelva a encenderse.", Color.yellow, 5f);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene, ScenePath);
            var scenes = EditorBuildSettings.scenes.ToList(); scenes.RemoveAll(s => s.path == ScenePath);
            scenes.Insert(scenes.FindIndex(s => s.path == LevelBuilder4_1.ScenePath) + 1, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray(); AssetDatabase.SaveAssets();
        }
        private static ResonanceBell2D Bell(CaveLevelSceneFactory f, Transform root, PlayerController player, ResonanceBellConfigSO config, int index, Vector2 position)
        {
            var go = new GameObject("Resonance_Bell_" + index); go.transform.SetParent(root, false); go.transform.position = position;
            go.AddComponent<CircleCollider2D>().radius = .7f; go.GetComponent<CircleCollider2D>().isTrigger = true;
            var visual = new GameObject("Bell_Visual"); visual.transform.SetParent(go.transform, false);
            var body = f.Visual("Basalt_Bell_Body", visual.transform, Vector2.zero, new Vector2(1.6f, 1.4f), new Color(.13f, .145f, .16f), true);
            body.GetComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Circle.png");
            var rim = f.Visual("Golden_Bell_Rim", visual.transform, new Vector2(0f, -.55f), new Vector2(1.8f, .18f), new Color(.87f, .63f, .37f), true);
            f.Visual("Bell_Clapper", visual.transform, new Vector2(0f, -.82f), new Vector2(.17f, .5f), new Color(.87f, .63f, .37f), true);
            f.Visual("Hanging_Chain", go.transform, new Vector2(0f, 1.5f), new Vector2(.09f, 1.8f), new Color(.4f, .3f, .23f), true);
            var label = LevelBuilder4_1.Hint(go.transform, "Bell_Countdown", position + Vector2.up * 1.1f, "ROAR → CAMPANA", Color.white);
            var bell = go.AddComponent<ResonanceBell2D>();
            CaveLevelSceneFactory.Set(bell, "_config", config); CaveLevelSceneFactory.Set(bell, "_playerSource", player);
            CaveLevelSceneFactory.Set(bell, "_visual", visual.transform); CaveLevelSceneFactory.Set(bell, "_rim", rim.GetComponent<SpriteRenderer>()); CaveLevelSceneFactory.Set(bell, "_countdown", label);
            return bell;
        }
        private static void Door(CaveLevelSceneFactory f, Transform root, ResonanceBell2D bell, int index, float x)
        {
            var go = new GameObject("Bell_Flame_Door_" + index); go.transform.SetParent(root, false); go.transform.position = new Vector2(x, 3f);
            var collider = go.AddComponent<BoxCollider2D>(); collider.size = new Vector2(1.2f, 6f); collider.isTrigger = true;
            var flames = new GameObject("Methane_Flames"); flames.transform.SetParent(go.transform, false);
            f.Visual("Blue_Outer_Flame", flames.transform, Vector2.zero, collider.size, new Color(0f, .7f, .85f, .75f), true);
            f.Visual("Orange_Core_Flame", flames.transform, new Vector2(.1f, -.3f), new Vector2(.6f, 5.4f), new Color(1f, .48f, 0f, .8f), true);
            f.Visual("Gas_Burner", go.transform, new Vector2(0f, -2.9f), new Vector2(1.8f, .2f), new Color(.2f, .2f, .22f), true);
            CaveLevelSceneFactory.Light(go.transform, Vector2.zero, Color.cyan, 4f, .8f);
            var status = LevelBuilder4_1.Hint(root, "Door_Countdown_" + index, new Vector2(x, 6.5f), "¡FUEGO!", Color.yellow);
            var door = go.AddComponent<BellFlameDoor2D>();
            CaveLevelSceneFactory.Set(door, "_bell", bell); CaveLevelSceneFactory.Set(door, "_flameRoot", flames);
            CaveLevelSceneFactory.Set(door, "_light", go.GetComponentInChildren<Light2D>()); CaveLevelSceneFactory.Set(door, "_status", status);
        }
        [MenuItem("Alma/📂 Cargar Nivel 4-2")]
        public static void LoadLevel() { if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(ScenePath); }
    }
}
#endif
