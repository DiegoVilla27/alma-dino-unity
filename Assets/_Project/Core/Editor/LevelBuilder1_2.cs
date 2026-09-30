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
    public static class LevelBuilder1_2
    {
        private const string SCENE_PATH = "Assets/Scenes/World_1_Jungle/Level_1_2.unity";
        private const string PREFAB_CHECKPOINT = "Assets/_Project/Prefabs/Universal/Checkpoint_Nest.prefab";
        private const string PREFAB_PORTAL = "Assets/_Project/Prefabs/Universal/Level_Exit_Portal.prefab";
        private const string PREFAB_SPIKES = "Assets/_Project/Prefabs/World_1_Jungle/Hazard_Spikes_Jungle.prefab";
        private const string PREFAB_MUSHROOM = "Assets/_Project/Prefabs/World_1_Jungle/BouncyMushroom_Jungle.prefab";
        private const string PREFAB_LEAF = "Assets/_Project/Prefabs/World_1_Jungle/CrumblingLeaf_Jungle.prefab";
        private const string PREFAB_PLANT = "Assets/_Project/Prefabs/World_1_Jungle/CarnivorousPlant_Jungle.prefab";

        private const string SPRITE_SQUARE = "Assets/Sprites/Square.png";
        private const string SPRITE_CIRCLE = "Assets/Sprites/Circle.png";
        private const string UNLIT_MAT_PATH = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";

        static LevelBuilder1_2()
        {
            EditorApplication.delayCall += AutoBuildOnce;
        }

        private static void AutoBuildOnce()
        {
            if (!EditorPrefs.GetBool("Alma_Level_1_2_Prefabs_Rebuilt_v1", false))
            {
                BuildLevel1_2WithPrefabs();
                EditorPrefs.SetBool("Alma_Level_1_2_Prefabs_Rebuilt_v1", true);
            }
        }

        [MenuItem("Alma/🏗️ Reestructurar Nivel 1-2 con Prefabs")]
        public static void BuildLevel1_2WithPrefabs()
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

            // 2. Colores temáticos del Dosel Peligroso
            Color woodBrown = new Color(0.32f, 0.24f, 0.14f);  // #523D24
            Color mossGreen = new Color(0.21f, 0.31f, 0.20f);  // #364E32
            Color darkTrunk = new Color(0.18f, 0.14f, 0.10f);  // #2E241A

            // Cargar prefabs para instanciación
            GameObject prefabCheckpoint = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_CHECKPOINT);
            GameObject prefabPortal = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PORTAL);
            GameObject prefabSpikes = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_SPIKES);
            GameObject prefabMushroom = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_MUSHROOM);
            GameObject prefabLeaf = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_LEAF);
            GameObject prefabPlant = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PLANT);

            // ==========================================
            // ZONA DE SPAWN & ENCUENTRO CON MONO LADRÓN (X = -4 a 6)
            // ==========================================
            CreateGround("Floor_Spawn", levelRoot.transform, new Vector3(3f, -1f, 0f), new Vector2(10f, 2f), mossGreen, square, unlitMat);
            CreateGround("Wall_Left_Boundary", levelRoot.transform, new Vector3(-3.5f, 8f, 0f), new Vector2(1f, 18f), darkTrunk, square, unlitMat);

            // Nido de spawn con prefab Checkpoint_Nest
            if (prefabCheckpoint != null)
            {
                var nestGo = (GameObject)PrefabUtility.InstantiatePrefab(prefabCheckpoint);
                nestGo.name = "Nest_Cradle_Spawn";
                nestGo.transform.SetParent(levelRoot.transform);
                nestGo.transform.position = new Vector3(0f, 0.3f, 0f);
            }

            // Mono Ladrón cinematográfico con el Huevo Dorado robado
            CreateThiefMonkeyEncounter(levelRoot.transform, square, circle, unlitMat);

            // ==========================================
            // DESAFÍO 1: Rebote Guiado bajo Techo de Espinas (X = 6 a 16)
            // ==========================================
            CreateGround("Mushroom_Floor_1", levelRoot.transform, new Vector3(8.5f, -1.5f, 0f), new Vector2(4f, 1.5f), darkTrunk, square, unlitMat);
            
            if (prefabMushroom != null)
            {
                var mush1 = (GameObject)PrefabUtility.InstantiatePrefab(prefabMushroom);
                mush1.name = "Bouncy_Mushroom_1";
                mush1.transform.SetParent(levelRoot.transform);
                mush1.transform.position = new Vector3(8.5f, -0.4f, 0f);
            }

            // Techo de espinas encima del hongo
            CreateGround("Ceiling_Branch_1", levelRoot.transform, new Vector3(8.5f, 7.5f, 0f), new Vector2(5.5f, 0.8f), woodBrown, square, unlitMat);
            if (prefabSpikes != null)
            {
                var spikesCeiling = (GameObject)PrefabUtility.InstantiatePrefab(prefabSpikes);
                spikesCeiling.name = "Thorn_Ceiling_Spikes_1";
                spikesCeiling.transform.SetParent(levelRoot.transform);
                spikesCeiling.transform.position = new Vector3(8.5f, 6.7f, 0f);
                spikesCeiling.transform.localScale = new Vector3(4.5f, -0.8f, 1f); // Invertido hacia abajo
            }

            // Repisa de aterrizaje seguro a la derecha
            CreateGround("Branch_Ledge_1", levelRoot.transform, new Vector3(14.5f, 4.2f, 0f), new Vector2(4.5f, 0.8f), woodBrown, square, unlitMat);

            // ==========================================
            // DESAFÍO 2: Compuerta Rítmica con Planta Carnívora (X = 18 a 28)
            // ==========================================
            CreateGround("Branch_Carnivorous_Gate", levelRoot.transform, new Vector3(21.0f, 5.0f, 0f), new Vector2(5f, 0.8f), woodBrown, square, unlitMat);
            
            // Planta Carnívora con prefab CarnivorousPlant_Jungle
            if (prefabPlant != null)
            {
                var plant1 = (GameObject)PrefabUtility.InstantiatePrefab(prefabPlant);
                plant1.name = "Carnivorous_Plant_1";
                plant1.transform.SetParent(levelRoot.transform);
                plant1.transform.position = new Vector3(21.0f, 6.0f, 0f);
            }

            // Rama de seguridad inferior
            CreateGround("Safety_Branch_1", levelRoot.transform, new Vector3(21.0f, 1.0f, 0f), new Vector2(6f, 0.8f), darkTrunk, square, unlitMat);

            // Rama del segundo hongo
            CreateGround("Mushroom_Branch_2", levelRoot.transform, new Vector3(25.5f, 6.3f, 0f), new Vector2(3.5f, 0.8f), woodBrown, square, unlitMat);
            if (prefabMushroom != null)
            {
                var mush2 = (GameObject)PrefabUtility.InstantiatePrefab(prefabMushroom);
                mush2.name = "Bouncy_Mushroom_2";
                mush2.transform.SetParent(levelRoot.transform);
                mush2.transform.position = new Vector3(25.5f, 7.3f, 0f);
            }

            // ==========================================
            // DESAFÍO 3: Gran Salto de Altura & Checkpoint 1 (X = 28 a 35)
            // ==========================================
            CreateGround("Canopy_Cliff_1", levelRoot.transform, new Vector3(32.5f, 14.5f, 0f), new Vector2(6f, 1.2f), mossGreen, square, unlitMat);

            // Checkpoint 1 con prefab Checkpoint_Nest
            if (prefabCheckpoint != null)
            {
                var cp1 = (GameObject)PrefabUtility.InstantiatePrefab(prefabCheckpoint);
                cp1.name = "Nest_Checkpoint_1";
                cp1.transform.SetParent(levelRoot.transform);
                cp1.transform.position = new Vector3(32.5f, 15.4f, 0f);
            }

            // ==========================================
            // DESAFÍO 4: Hojas Quebradizas sobre Gran Abismo (X = 36 a 53)
            // ==========================================
            CreateGround("Under_Chasm_Floor", levelRoot.transform, new Vector3(44.0f, 4.0f, 0f), new Vector2(16f, 1.5f), darkTrunk, square, unlitMat);
            if (prefabSpikes != null)
            {
                var underSpikes = (GameObject)PrefabUtility.InstantiatePrefab(prefabSpikes);
                underSpikes.name = "Under_Chasm_Spikes";
                underSpikes.transform.SetParent(levelRoot.transform);
                underSpikes.transform.position = new Vector3(44.0f, 5.0f, 0f);
                underSpikes.transform.localScale = new Vector3(14f, 1f, 1f);
            }

            // Hoja 1 con prefab CrumblingLeaf_Jungle
            if (prefabLeaf != null)
            {
                var leaf1 = (GameObject)PrefabUtility.InstantiatePrefab(prefabLeaf);
                leaf1.name = "Crumbling_Leaf_1";
                leaf1.transform.SetParent(levelRoot.transform);
                leaf1.transform.position = new Vector3(39.0f, 16.0f, 0f);
            }

            // Liana espinosa colgante intermedia
            if (prefabSpikes != null)
            {
                var vineSpikes = (GameObject)PrefabUtility.InstantiatePrefab(prefabSpikes);
                vineSpikes.name = "Hanging_Vine_Spikes";
                vineSpikes.transform.SetParent(levelRoot.transform);
                vineSpikes.transform.position = new Vector3(43.5f, 18.5f, 0f);
                vineSpikes.transform.localScale = new Vector3(1.2f, 3.5f, 1f);
            }

            // Hoja 2 con prefab CrumblingLeaf_Jungle
            if (prefabLeaf != null)
            {
                var leaf2 = (GameObject)PrefabUtility.InstantiatePrefab(prefabLeaf);
                leaf2.name = "Crumbling_Leaf_2";
                leaf2.transform.SetParent(levelRoot.transform);
                leaf2.transform.position = new Vector3(47.5f, 16.5f, 0f);
            }

            CreateGround("Rest_Branch_Zone3", levelRoot.transform, new Vector3(52.0f, 18.0f, 0f), new Vector2(4.5f, 0.8f), woodBrown, square, unlitMat);

            // ==========================================
            // DESAFÍO 5: Cadena Aérea de Hongos en el Vacío (X = 55 a 73)
            // ==========================================
            CreateGround("Midair_Stem_1", levelRoot.transform, new Vector3(57.5f, 20.0f, 0f), new Vector2(1.2f, 3.0f), darkTrunk, square, unlitMat);
            if (prefabMushroom != null)
            {
                var midMush1 = (GameObject)PrefabUtility.InstantiatePrefab(prefabMushroom);
                midMush1.name = "Bouncy_Mushroom_Midair_1";
                midMush1.transform.SetParent(levelRoot.transform);
                midMush1.transform.position = new Vector3(57.5f, 21.8f, 0f);
            }

            // Planta carnívora aérea suspendida
            CreateGround("Midair_Plant_Ledge", levelRoot.transform, new Vector3(63.0f, 23.5f, 0f), new Vector2(2.5f, 0.8f), darkTrunk, square, unlitMat);
            if (prefabPlant != null)
            {
                var plant2 = (GameObject)PrefabUtility.InstantiatePrefab(prefabPlant);
                plant2.name = "Carnivorous_Plant_Midair";
                plant2.transform.SetParent(levelRoot.transform);
                plant2.transform.position = new Vector3(63.0f, 24.5f, 0f);
            }

            CreateGround("Midair_Stem_2", levelRoot.transform, new Vector3(68.5f, 25.0f, 0f), new Vector2(1.2f, 3.0f), darkTrunk, square, unlitMat);
            if (prefabMushroom != null)
            {
                var midMush2 = (GameObject)PrefabUtility.InstantiatePrefab(prefabMushroom);
                midMush2.name = "Bouncy_Mushroom_Midair_2";
                midMush2.transform.SetParent(levelRoot.transform);
                midMush2.transform.position = new Vector3(68.5f, 26.8f, 0f);
            }

            // ==========================================
            // LA CIMA DEL GRAN ÁRBOL & PORTAL DE SALIDA (X = 75 a 86)
            // ==========================================
            CreateGround("Great_Canopy_Floor", levelRoot.transform, new Vector3(80.0f, 30.0f, 0f), new Vector2(10f, 1.5f), mossGreen, square, unlitMat);
            CreateGround("Wall_Right_Boundary", levelRoot.transform, new Vector3(85.5f, 35.0f, 0f), new Vector2(1f, 12f), darkTrunk, square, unlitMat);

            // Checkpoint 2 con prefab Checkpoint_Nest
            if (prefabCheckpoint != null)
            {
                var cp2 = (GameObject)PrefabUtility.InstantiatePrefab(prefabCheckpoint);
                cp2.name = "Nest_Checkpoint_2";
                cp2.transform.SetParent(levelRoot.transform);
                cp2.transform.position = new Vector3(77.0f, 31.0f, 0f);
            }

            // Portal de salida hacia el Nivel 1-3 con prefab Level_Exit_Portal
            if (prefabPortal != null)
            {
                var portalGo = (GameObject)PrefabUtility.InstantiatePrefab(prefabPortal);
                portalGo.name = "Portal_Exit_To_1_3";
                portalGo.transform.SetParent(levelRoot.transform);
                portalGo.transform.position = new Vector3(82.5f, 32.2f, 0f);

                var exit = portalGo.GetComponent<LevelExit2D>();
                if (exit != null)
                {
                    var nextSceneField = typeof(LevelExit2D).GetField("_nextSceneName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (nextSceneField != null) nextSceneField.SetValue(exit, "Level_1_3");

                    var titleField = typeof(LevelExit2D).GetField("_levelTitle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (titleField != null) titleField.SetValue(exit, "NIVEL 1-2 COMPLETADO");

                    var msgField = typeof(LevelExit2D).GetField("_victoryMessage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (msgField != null) msgField.SetValue(exit, "Has escalado el dosel y dejado atrás las trampas del mono ladrón.\nLas huellas descienden hacia las Zarzas Profundas (1-3).");
                }
            }

            // Zona de muerte profunda (DeadZone KillFloor)
            CreateDeadZone(levelRoot.transform, new Vector3(45f, -7f, 0f), new Vector2(140f, 2f));

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
                    // En Nivel 1-2 el Doble Salto SIEMPRE está desbloqueado
                    var djField = typeof(PlayerController).GetField("_doubleJumpUnlocked", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (djField != null) djField.SetValue(pc, true);
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
                cam.backgroundColor = new Color(0.12f, 0.16f, 0.24f, 1f); // Deep canopy sky
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
                globalLight.color = new Color(1.0f, 0.95f, 0.88f);
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

            // 8. Guardar la escena
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("<color=#00FF88><b>[AlmaDino]</b> ¡Nivel 1-2 ('El Dosel Peligroso') reestructurado exitosamente con la biblioteca de Prefabs!</color>");
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

        private static void CreateThiefMonkeyEncounter(Transform parent, Sprite square, Sprite circle, Material material)
        {
            var monkeyGo = new GameObject("Thief_Monkey_Teaser");
            monkeyGo.transform.SetParent(parent);
            monkeyGo.transform.position = new Vector3(3.2f, 0.6f, 0f);

            var sr = monkeyGo.AddComponent<SpriteRenderer>();
            sr.sprite = square;
            sr.sharedMaterial = material;
            sr.color = new Color(0.6f, 0.35f, 0.15f); // Brown monkey fur
            monkeyGo.transform.localScale = new Vector3(0.9f, 1.2f, 1f);

            var col = monkeyGo.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(2.5f, 2f);

            monkeyGo.AddComponent<ThiefMonkeyTeaser2D>();

            // Huevo Dorado Robado sostenido por el mono
            var eggGo = new GameObject("Stolen_Golden_Egg");
            eggGo.transform.SetParent(monkeyGo.transform);
            eggGo.transform.localPosition = new Vector3(0.4f, 0.0f, 0f);
            eggGo.transform.localScale = new Vector3(0.55f, 0.7f, 1f);

            var eggSr = eggGo.AddComponent<SpriteRenderer>();
            eggSr.sprite = circle;
            eggSr.sharedMaterial = material;
            eggSr.color = new Color(1f, 0.84f, 0f); // Bright golden egg
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
    }
}
#endif
