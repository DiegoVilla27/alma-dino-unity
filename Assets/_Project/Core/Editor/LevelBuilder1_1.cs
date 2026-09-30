#if UNITY_EDITOR
using System.IO;
using AlmaDino.Core.Interfaces;
using AlmaDino.Features.Environment;
using AlmaDino.Features.MobileUI.Controllers;
using AlmaDino.Features.Player.Components;
using AlmaDino.Features.Player.Controllers;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;

namespace AlmaDino.Core.Editor
{
    [InitializeOnLoad]
    public static class LevelBuilder1_1
    {
        private const string SCENE_PATH = "Assets/Scenes/World_1_Jungle/Level_1_1.unity";
        private const string PREFAB_CHECKPOINT = "Assets/_Project/Prefabs/Universal/Checkpoint_Nest.prefab";
        private const string PREFAB_PORTAL = "Assets/_Project/Prefabs/Universal/Level_Exit_Portal.prefab";
        private const string PREFAB_ALTAR = "Assets/_Project/Prefabs/Universal/Ability_Relic_Altar.prefab";
        private const string PREFAB_SPIKES = "Assets/_Project/Prefabs/World_1_Jungle/Hazard_Spikes_Jungle.prefab";
        private const string PREFAB_MUSHROOM = "Assets/_Project/Prefabs/World_1_Jungle/BouncyMushroom_Jungle.prefab";
        private const string PREFAB_LEAF = "Assets/_Project/Prefabs/World_1_Jungle/CrumblingLeaf_Jungle.prefab";

        private const string SPRITE_SQUARE = "Assets/Sprites/Square.png";
        private const string SPRITE_CIRCLE = "Assets/Sprites/Circle.png";
        private const string UNLIT_MAT_PATH = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";

        static LevelBuilder1_1()
        {
            EditorApplication.delayCall += AutoBuildOnce;
        }

        private static void AutoBuildOnce()
        {
            if (!EditorPrefs.GetBool("Alma_Level_1_1_Prefabs_Rebuilt_v1", false))
            {
                BuildLevel1_1WithPrefabs();
                EditorPrefs.SetBool("Alma_Level_1_1_Prefabs_Rebuilt_v1", true);
            }
        }

        [MenuItem("Alma/🏗️ Reestructurar Nivel 1-1 con Prefabs")]
        public static void BuildLevel1_1WithPrefabs()
        {
            if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;

            // Asegurar que los prefabs existan
            if (!File.Exists(PREFAB_CHECKPOINT))
            {
                EnvironmentPrefabFactory.GenerateAllPrefabs();
            }

            var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);

            Sprite square = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_SQUARE);
            Sprite circle = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_CIRCLE);
            Material unlitMat = AssetDatabase.LoadAssetAtPath<Material>(UNLIT_MAT_PATH);
            if (unlitMat == null)
            {
                Shader s = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit");
                if (s != null) unlitMat = new Material(s);
            }

            // 1. Configurar o limpiar contenedor del nivel
            GameObject levelRoot = GameObject.Find("--- LEVEL ---");
            if (levelRoot != null)
            {
                Object.DestroyImmediate(levelRoot);
            }
            levelRoot = new GameObject("--- LEVEL ---");

            // 2. Colores de la paleta Jungla Esmeralda (según Level_1_1.md)
            Color mossGreen = new Color(0.30f, 0.48f, 0.22f); // #4E7A38
            Color soilBrown = new Color(0.29f, 0.20f, 0.10f); // #4A3319
            Color darkStone = new Color(0.18f, 0.18f, 0.16f); // #2E2E2A

            // Cargar prefabs para instanciación
            GameObject prefabCheckpoint = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_CHECKPOINT);
            GameObject prefabPortal = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PORTAL);
            GameObject prefabAltar = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_ALTAR);
            GameObject prefabSpikes = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_SPIKES);
            GameObject prefabMushroom = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_MUSHROOM);
            GameObject prefabLeaf = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_LEAF);

            // ==========================================
            // ZONA 0: El Despertar en el Nido (X = -4 a 10)
            // ==========================================
            CreateGround("Floor_Nest_Zone0", levelRoot.transform, new Vector3(3f, -1f, 0f), new Vector2(14f, 2f), mossGreen, square, unlitMat);
            CreateGround("Wall_Left_Boundary", levelRoot.transform, new Vector3(-4.5f, 5f, 0f), new Vector2(1f, 12f), darkStone, square, unlitMat);

            // Cuna de inicio con prefab Checkpoint_Nest
            if (prefabCheckpoint != null)
            {
                var nestGo = (GameObject)PrefabUtility.InstantiatePrefab(prefabCheckpoint);
                nestGo.name = "Nest_Cradle_Spawn";
                nestGo.transform.SetParent(levelRoot.transform);
                nestGo.transform.position = new Vector3(0f, 0.3f, 0f);
            }

            // ==========================================
            // ZONA 1: Primer Salto y Desnivel (X = 10 a 22)
            // ==========================================
            CreateGround("Step_1_Rock", levelRoot.transform, new Vector3(12f, 0.4f, 0f), new Vector2(3f, 0.8f), darkStone, square, unlitMat);
            CreateGround("Pit_Floor", levelRoot.transform, new Vector3(16f, -1.8f, 0f), new Vector2(4f, 1f), soilBrown, square, unlitMat);

            // Pinchos en foso con prefab Hazard_Spikes_Jungle
            if (prefabSpikes != null)
            {
                var spikes1 = (GameObject)PrefabUtility.InstantiatePrefab(prefabSpikes);
                spikes1.name = "Spikes_Gap_Pit";
                spikes1.transform.SetParent(levelRoot.transform);
                spikes1.transform.position = new Vector3(16f, -0.9f, 0f);
            }

            CreateGround("Step_2_Rock", levelRoot.transform, new Vector3(20f, 1.2f, 0f), new Vector2(3.5f, 0.8f), darkStone, square, unlitMat);

            // ==========================================
            // ZONA 2: El Gran Abismo & Altar Materno (X = 22 a 36)
            // ==========================================
            CreateGround("Cliff_Edge_Zone2", levelRoot.transform, new Vector3(23.5f, 2.0f, 0f), new Vector2(3f, 0.8f), mossGreen, square, unlitMat);
            CreateGround("Altar_Grotto_Floor", levelRoot.transform, new Vector3(28.5f, 0.0f, 0f), new Vector2(9f, 1.5f), darkStone, square, unlitMat);

            // Altar Materno con prefab Ability_Relic_Altar
            if (prefabAltar != null)
            {
                var altarGo = (GameObject)PrefabUtility.InstantiatePrefab(prefabAltar);
                altarGo.name = "Altar_Maternal_Gem";
                altarGo.transform.SetParent(levelRoot.transform);
                altarGo.transform.position = new Vector3(27f, 1.4f, 0f);

                var relic = altarGo.GetComponent<AbilityRelic2D>();
                if (relic != null)
                {
                    // Se asegura que otorga el Doble Salto
                    var field = typeof(AbilityRelic2D).GetField("_abilityToUnlock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null) field.SetValue(relic, AbilityType.DoubleJump);
                }
            }

            // Hongo elástico de retorno con prefab BouncyMushroom_Jungle
            if (prefabMushroom != null)
            {
                var mushGo = (GameObject)PrefabUtility.InstantiatePrefab(prefabMushroom);
                mushGo.name = "Mushroom_Grotto_Return";
                mushGo.transform.SetParent(levelRoot.transform);
                mushGo.transform.position = new Vector3(31.5f, 1.0f, 0f);
            }

            // ==========================================
            // ZONA 3: Gran Abismo de Evaluación & Nido 2 (X = 36 a 55)
            // ==========================================
            CreateGround("Chasm_Floor", levelRoot.transform, new Vector3(39f, -3.5f, 0f), new Vector2(10f, 1.5f), soilBrown, square, unlitMat);
            if (prefabSpikes != null)
            {
                var chasmSpikes = (GameObject)PrefabUtility.InstantiatePrefab(prefabSpikes);
                chasmSpikes.name = "Spikes_Great_Chasm";
                chasmSpikes.transform.SetParent(levelRoot.transform);
                chasmSpikes.transform.position = new Vector3(39f, -2.3f, 0f);
                chasmSpikes.transform.localScale = new Vector3(6f, 1f, 1f);
            }

            // Cornisa de aterrizaje de Doble Salto
            CreateGround("Landing_Cliff_Zone3", levelRoot.transform, new Vector3(45.5f, 2.8f, 0f), new Vector2(4f, 0.8f), mossGreen, square, unlitMat);
            CreateGround("Rest_Platform_Zone3", levelRoot.transform, new Vector3(50.5f, 3.8f, 0f), new Vector2(5f, 0.8f), darkStone, square, unlitMat);

            // Nido Checkpoint 1 con prefab Checkpoint_Nest
            if (prefabCheckpoint != null)
            {
                var cp1 = (GameObject)PrefabUtility.InstantiatePrefab(prefabCheckpoint);
                cp1.name = "Nest_Checkpoint_1";
                cp1.transform.SetParent(levelRoot.transform);
                cp1.transform.position = new Vector3(51f, 4.5f, 0f);
            }

            // ==========================================
            // ZONA 4: Hojas Quebradizas sobre Zarzas (X = 55 a 75)
            // ==========================================
            CreateGround("Brambles_Floor_Zone4", levelRoot.transform, new Vector3(65f, -2.5f, 0f), new Vector2(22f, 1.5f), soilBrown, square, unlitMat);
            if (prefabSpikes != null)
            {
                var leafSpikes = (GameObject)PrefabUtility.InstantiatePrefab(prefabSpikes);
                leafSpikes.name = "Spikes_Leaves_Undergrowth";
                leafSpikes.transform.SetParent(levelRoot.transform);
                leafSpikes.transform.position = new Vector3(65f, -1.3f, 0f);
                leafSpikes.transform.localScale = new Vector3(8f, 1.2f, 1f);
            }

            // Hojas Quebradizas con prefab CrumblingLeaf_Jungle
            if (prefabLeaf != null)
            {
                var leaf1 = (GameObject)PrefabUtility.InstantiatePrefab(prefabLeaf);
                leaf1.name = "CrumblingLeaf_1";
                leaf1.transform.SetParent(levelRoot.transform);
                leaf1.transform.position = new Vector3(58.5f, 4.4f, 0f);

                var leaf2 = (GameObject)PrefabUtility.InstantiatePrefab(prefabLeaf);
                leaf2.name = "CrumblingLeaf_2";
                leaf2.transform.SetParent(levelRoot.transform);
                leaf2.transform.position = new Vector3(65.5f, 4.8f, 0f);

                var leaf3 = (GameObject)PrefabUtility.InstantiatePrefab(prefabLeaf);
                leaf3.name = "CrumblingLeaf_3";
                leaf3.transform.SetParent(levelRoot.transform);
                leaf3.transform.position = new Vector3(71.5f, 5.2f, 0f);
            }

            // ==========================================
            // ZONA 5: Salida del Nivel hacia el 1-2 (X = 76 a 86)
            // ==========================================
            CreateGround("Exit_Cliff_Zone5", levelRoot.transform, new Vector3(80f, 5.0f, 0f), new Vector2(8f, 1.2f), mossGreen, square, unlitMat);
            CreateGround("Wall_Right_Boundary", levelRoot.transform, new Vector3(84.5f, 9.0f, 0f), new Vector2(1f, 10f), darkStone, square, unlitMat);

            // Portal de salida con prefab Level_Exit_Portal
            if (prefabPortal != null)
            {
                var portalGo = (GameObject)PrefabUtility.InstantiatePrefab(prefabPortal);
                portalGo.name = "Portal_Exit_To_1_2";
                portalGo.transform.SetParent(levelRoot.transform);
                portalGo.transform.position = new Vector3(82f, 7.0f, 0f);

                var exit = portalGo.GetComponent<LevelExit2D>();
                if (exit != null)
                {
                    var nextSceneField = typeof(LevelExit2D).GetField("_nextSceneName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (nextSceneField != null) nextSceneField.SetValue(exit, "Level_1_2");

                    var titleField = typeof(LevelExit2D).GetField("_levelTitle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (titleField != null) titleField.SetValue(exit, "NIVEL 1-1 COMPLETADO");

                    var msgField = typeof(LevelExit2D).GetField("_victoryMessage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (msgField != null) msgField.SetValue(exit, "Has despertado y dominado el Aleteo Materno.\nEl rastro de tus pequeños se adentra en las copas de la Jungla Esmeralda (1-2).");
                }
            }

            // Zona de muerte profunda (DeadZone KillFloor)
            CreateDeadZone(levelRoot.transform, new Vector3(45f, -9f, 0f), new Vector2(120f, 2f));

            // ==========================================
            // 3. Configuración del Jugador (Alma)
            // ==========================================
            var alma = GameObject.Find("Alma (Player)");
            if (alma != null)
            {
                alma.transform.position = new Vector3(0f, 1.2f, 0f);

                var pc = alma.GetComponent<PlayerController>();
                if (pc != null)
                {
                    // En Nivel 1-1 el jugador empieza SIN Doble Salto hasta alcanzar el Altar Materno
                    var djField = typeof(PlayerController).GetField("_doubleJumpUnlocked", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (djField != null) djField.SetValue(pc, false);
                    EditorUtility.SetDirty(pc);
                }

                var rb = alma.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    EditorUtility.SetDirty(rb);
                }
            }

            // 4. Configurar Cámara y Follow
            var cam = Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = 6.0f;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.12f, 0.16f, 0.22f, 1f); // Deep twilight emerald
                if (alma != null)
                {
                    cam.transform.position = new Vector3(alma.transform.position.x, alma.transform.position.y + 1.5f, -10f);
                }
                EditorUtility.SetDirty(cam);
            }

            // 5. Configurar Iluminación 2D URP
            var globalLight = Object.FindAnyObjectByType<Light2D>();
            if (globalLight != null)
            {
                globalLight.lightType = Light2D.LightType.Global;
                globalLight.intensity = 0.75f;
                globalLight.color = new Color(1.0f, 0.95f, 0.88f); // Warm sunrise light
                EditorUtility.SetDirty(globalLight);
            }

            // 6. Configurar UI Móvil y Controles
            var joystickBase = GameObject.Find("VirtualJoystick_Base");
            if (joystickBase != null)
            {
                var vj = joystickBase.GetComponent<VirtualJoystick>();
                if (vj == null) vj = joystickBase.AddComponent<VirtualJoystick>();
                EditorUtility.SetDirty(joystickBase);
            }

            SetupVirtualButton("Button_Jump", VirtualButtonType.Jump);
            SetupVirtualButton("Button_Dash", VirtualButtonType.Dash);
            SetupVirtualButton("Button_Pound", VirtualButtonType.GroundPound);
            SetupVirtualButton("Button_Roar", VirtualButtonType.Roar);

            // 7. EventSystem
            var eventSystem = Object.FindAnyObjectByType<EventSystem>(FindObjectsInactive.Include);
            if (eventSystem != null)
            {
                var inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
                if (inputModule != null) Object.DestroyImmediate(inputModule);

                var standalone = eventSystem.GetComponent<StandaloneInputModule>();
                if (standalone == null) standalone = eventSystem.gameObject.AddComponent<StandaloneInputModule>();
                EditorUtility.SetDirty(eventSystem);
            }

            // 8. Configurar Banner Narrativo de Prólogo Inicial
            EnsurePrologueNarrativeTrigger(levelRoot.transform);

            // 9. Guardar la escena
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("<color=#00FF88><b>[AlmaDino]</b> ¡Nivel 1-1 ('Despertar en el Nido') reestructurado exitosamente con la biblioteca de Prefabs!</color>");
        }

        private static void CreateGround(string name, Transform parent, Vector3 position, Vector2 size, Color color, Sprite sprite, Material material)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sharedMaterial = material;
            sr.color = color;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;
        }

        private static void CreateDeadZone(Transform parent, Vector3 position, Vector2 size)
        {
            var go = new GameObject("DeadZone_KillFloor");
            go.transform.SetParent(parent);
            go.transform.position = position;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = size;
            col.isTrigger = true;

            go.AddComponent<HazardTrigger2D>();
        }

        private static void SetupVirtualButton(string gameObjectName, VirtualButtonType buttonType)
        {
            var btnGo = GameObject.Find(gameObjectName);
            if (btnGo != null)
            {
                var vb = btnGo.GetComponent<VirtualTouchButton>();
                if (vb == null) vb = btnGo.AddComponent<VirtualTouchButton>();
                vb.ButtonType = buttonType;
                EditorUtility.SetDirty(btnGo);
            }
        }

        private static void EnsurePrologueNarrativeTrigger(Transform parent)
        {
            var triggerGo = GameObject.Find("Prologue_Narrative_Trigger");
            if (triggerGo == null)
            {
                triggerGo = new GameObject("Prologue_Narrative_Trigger");
                triggerGo.transform.SetParent(parent);
                triggerGo.transform.position = new Vector3(0.5f, 1.0f, 0f);

                var col = triggerGo.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(3f, 3f);

                var narrativeTrigger = triggerGo.AddComponent<NarrativePrologueTrigger>();
            }
        }
    }
}
#endif
