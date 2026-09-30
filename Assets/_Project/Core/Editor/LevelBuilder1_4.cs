#if UNITY_EDITOR
using System.IO;
using System.Reflection;
using AlmaDino.Core.Interfaces;
using AlmaDino.Core.Progression;
using AlmaDino.Features.Environment;
using AlmaDino.Features.MobileUI.Controllers;
using AlmaDino.Features.Player.Components;
using AlmaDino.Features.Player.Controllers;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;

namespace AlmaDino.Core.Editor
{
    [InitializeOnLoad]
    public static class LevelBuilder1_4
    {
        private const string SCENE_PATH = "Assets/Scenes/World_1_Jungle/Level_1_4.unity";
        private const string PREFAB_CHECKPOINT = "Assets/_Project/Prefabs/Universal/Checkpoint_Nest.prefab";
        private const string PREFAB_PORTAL = "Assets/_Project/Prefabs/Universal/Level_Exit_Portal.prefab";
        private const string PREFAB_SPIKES = "Assets/_Project/Prefabs/World_1_Jungle/Hazard_Spikes_Jungle.prefab";
        private const string PREFAB_MUSHROOM = "Assets/_Project/Prefabs/World_1_Jungle/BouncyMushroom_Jungle.prefab";
        private const string PREFAB_LEAF = "Assets/_Project/Prefabs/World_1_Jungle/CrumblingLeaf_Jungle.prefab";
        private const string PREFAB_PLANT = "Assets/_Project/Prefabs/World_1_Jungle/CarnivorousPlant_Jungle.prefab";

        private const string SPRITE_SQUARE = "Assets/Sprites/Square.png";
        private const string SPRITE_CIRCLE = "Assets/Sprites/Circle.png";
        private const string UNLIT_MAT_PATH = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";

        static LevelBuilder1_4()
        {
            EditorApplication.delayCall += AutoBuildOnce;
        }

        private static void AutoBuildOnce()
        {
            if (!EditorPrefs.GetBool("Alma_Level_1_4_Build_v1", false))
            {
                BuildLevel1_4WithPrefabs();
                EditorPrefs.SetBool("Alma_Level_1_4_Build_v1", true);
            }
        }

        [MenuItem("Alma/🏗️ Reestructurar Nivel 1-4 con Prefabs")]
        public static void BuildLevel1_4WithPrefabs()
        {
            if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;

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

            // 1. Limpiar o configurar contenedor del nivel
            GameObject levelRoot = GameObject.Find("--- LEVEL ---");
            if (levelRoot != null)
            {
                Object.DestroyImmediate(levelRoot);
            }
            levelRoot = new GameObject("--- LEVEL ---");

            // 2. Colores temáticos de La Copa del Gran Árbol (Por encima de las nubes)
            Color goldenWood = new Color(0.38f, 0.22f, 0.12f);    // #61381F Corteza dorada cálida
            Color emeraldCanopy = new Color(0.18f, 0.77f, 0.71f); // #2EC4B6 Follaje esmeralda resplandeciente
            Color goldenLeaves = new Color(1.0f, 0.72f, 0.01f);   // #FFB703 Corona de hojas doradas
            Color darkBranch = new Color(0.24f, 0.16f, 0.10f);    // Rama profunda

            // Cargar prefabs para instanciación
            GameObject prefabCheckpoint = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_CHECKPOINT);
            GameObject prefabPortal = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PORTAL);
            GameObject prefabSpikes = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_SPIKES);
            GameObject prefabMushroom = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_MUSHROOM);
            GameObject prefabLeaf = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_LEAF);
            GameObject prefabPlant = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PLANT);

            // ==========================================
            // ZONA 0: El Tronco Madre (Spawn) (X = -2 a 5)
            // ==========================================
            CreateGround("Spawn_Cliff", levelRoot.transform, new Vector3(0f, 0.0f, 0f), new Vector2(4.0f, 1.5f), goldenWood, square, unlitMat);
            CreateGround("Spawn_Moss", levelRoot.transform, new Vector3(0f, 0.75f, 0f), new Vector2(4.0f, 0.2f), emeraldCanopy, square, unlitMat);
            CreateGround("Wall_Left_Boundary", levelRoot.transform, new Vector3(-2.5f, 6.0f, 0f), new Vector2(1f, 14f), goldenWood, square, unlitMat);

            // Nido de spawn inicial
            if (prefabCheckpoint != null)
            {
                var nestGo = (GameObject)PrefabUtility.InstantiatePrefab(prefabCheckpoint);
                nestGo.name = "Nest_Cradle_Spawn";
                nestGo.transform.SetParent(levelRoot.transform);
                nestGo.transform.position = new Vector3(-0.3f, 1.0f, 0f);
            }

            EnsurePrologueNarrativeTrigger(levelRoot.transform);

            // ==========================================
            // ZONA 1: La Escalera Aérea de Hongos y Hojas Rápidas (X = 5 a 36, Y = 1 a 12)
            // Desafío vertical dinámico:
            // Hongo 1 (16.5 m/s) ➔ Hoja 1 (0.45s) ➔ Arco sobre aguja de espinas ➔ Hoja 2 (0.45s)
            // ➔ Hongo 2 (17.5 m/s) ➔ Hoja 3 alta ➔ Hoja 4 ➔ Doble salto de 5.0m a la cornisa!
            // ==========================================
            // Hongo 1
            CreateGround("Mushroom_Stump_Z1_1", levelRoot.transform, new Vector3(6.5f, 0.5f, 0f), new Vector2(2.2f, 1.0f), goldenWood, square, unlitMat);
            if (prefabMushroom != null)
            {
                var m1 = (GameObject)PrefabUtility.InstantiatePrefab(prefabMushroom);
                m1.name = "Bouncy_Mushroom_Z1_1";
                m1.transform.SetParent(levelRoot.transform);
                m1.transform.position = new Vector3(6.5f, 1.5f, 0f);

                var bp = m1.GetComponent<BouncyPlatform2D>();
                if (bp != null)
                {
                    var field = typeof(BouncyPlatform2D).GetField("_bounceVelocity", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field != null) field.SetValue(bp, 16.5f);
                }
            }

            // Hoja 1 en ascenso diagonal
            CreateLeaf(prefabLeaf, levelRoot.transform, "Crumbling_Leaf_Z1_1", new Vector3(10.5f, 5.5f, 0f), new Vector3(1.3f, 0.35f, 1f), 0.45f);

            // Aguja de espinas 1: se alza entre hoja 1 y hoja 2
            CreateSpikePillar(prefabSpikes, levelRoot.transform, "Spike_Pillar_Z1_A", new Vector3(13.0f, 6.0f, 0f), new Vector2(1.2f, 3.2f), square, darkBranch, unlitMat);

            // Hoja 2: tras saltar y aletear sobre la aguja
            CreateLeaf(prefabLeaf, levelRoot.transform, "Crumbling_Leaf_Z1_2", new Vector3(15.5f, 7.5f, 0f), new Vector3(1.3f, 0.35f, 1f), 0.45f);

            // Hongo 2: a media altura para catapultar a la estratosfera
            CreateGround("Mushroom_Stump_Z1_2", levelRoot.transform, new Vector3(19.5f, 5.0f, 0f), new Vector2(2.0f, 1.0f), goldenWood, square, unlitMat);
            if (prefabMushroom != null)
            {
                var m2 = (GameObject)PrefabUtility.InstantiatePrefab(prefabMushroom);
                m2.name = "Bouncy_Mushroom_Z1_2";
                m2.transform.SetParent(levelRoot.transform);
                m2.transform.position = new Vector3(19.5f, 6.0f, 0f);

                var bp = m2.GetComponent<BouncyPlatform2D>();
                if (bp != null)
                {
                    var field = typeof(BouncyPlatform2D).GetField("_bounceVelocity", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field != null) field.SetValue(bp, 17.5f);
                }
            }

            // Hojas altas en el dosel
            CreateLeaf(prefabLeaf, levelRoot.transform, "Crumbling_Leaf_Z1_3", new Vector3(24.0f, 12.5f, 0f), new Vector3(1.3f, 0.35f, 1f), 0.45f);

            // Aguja de espinas alta
            CreateSpikePillar(prefabSpikes, levelRoot.transform, "Spike_Pillar_Z1_B", new Vector3(27.0f, 12.0f, 0f), new Vector2(1.2f, 3.5f), square, darkBranch, unlitMat);

            CreateLeaf(prefabLeaf, levelRoot.transform, "Crumbling_Leaf_Z1_4", new Vector3(30.0f, 11.5f, 0f), new Vector3(1.3f, 0.35f, 1f), 0.45f);

            // Cornisa de descanso 1 (Salto largo de 5.0m con doble salto!)
            CreateGround("Rest_Ledge_Z1", levelRoot.transform, new Vector3(35.0f, 10.5f, 0f), new Vector2(2.4f, 1.0f), goldenWood, square, unlitMat);
            CreateGround("Rest_Ledge_Z1_Moss", levelRoot.transform, new Vector3(35.0f, 11.05f, 0f), new Vector2(2.4f, 0.15f), emeraldCanopy, square, unlitMat);

            // ==========================================
            // ZONA 2: Checkpoint 1 & El Desfiladero de las Fauces Aéreas (X = 37 a 58, Y = 11 a 15)
            // Desafío sin suelo abajo (mar de nubes):
            // Checkpoint 1 ➔ Hoja 1 ➔ Salto sobre Planta 1 ➔ Hoja Flotante 2 ➔ Salto sobre Planta 2 ➔ Hoja 3!
            // ==========================================
            CreateGround("Checkpoint_Island_1", levelRoot.transform, new Vector3(39.0f, 11.5f, 0f), new Vector2(2.8f, 1.0f), goldenWood, square, unlitMat);
            CreateGround("Checkpoint_Island_1_Moss", levelRoot.transform, new Vector3(39.0f, 12.05f, 0f), new Vector2(2.8f, 0.15f), emeraldCanopy, square, unlitMat);

            if (prefabCheckpoint != null)
            {
                var cp1 = (GameObject)PrefabUtility.InstantiatePrefab(prefabCheckpoint);
                cp1.name = "Nest_Checkpoint_1";
                cp1.transform.SetParent(levelRoot.transform);
                cp1.transform.position = new Vector3(39.0f, 12.55f, 0f);
            }

            // Hoja 1 antes de Planta 1
            CreateLeaf(prefabLeaf, levelRoot.transform, "Crumbling_Leaf_Jaw_1", new Vector3(43.0f, 12.0f, 0f), new Vector3(1.3f, 0.35f, 1f), 0.45f);

            // Planta Carnívora 1
            CreateGround("Carnivore_Pillar_Z2_1", levelRoot.transform, new Vector3(46.5f, 11.0f, 0f), new Vector2(1.8f, 1.6f), goldenWood, square, unlitMat);
            if (prefabPlant != null)
            {
                var plant1 = (GameObject)PrefabUtility.InstantiatePrefab(prefabPlant);
                plant1.name = "Carnivorous_Plant_Z2_1";
                plant1.transform.SetParent(levelRoot.transform);
                plant1.transform.position = new Vector3(46.5f, 12.4f, 0f);
            }

            // Hoja central suspendida entre ambas fauces
            CreateLeaf(prefabLeaf, levelRoot.transform, "Crumbling_Leaf_Jaw_2", new Vector3(50.2f, 12.8f, 0f), new Vector3(1.3f, 0.35f, 1f), 0.45f);

            // Planta Carnívora 2 (ciclo desfasado)
            CreateGround("Carnivore_Pillar_Z2_2", levelRoot.transform, new Vector3(54.0f, 11.5f, 0f), new Vector2(1.8f, 1.6f), goldenWood, square, unlitMat);
            if (prefabPlant != null)
            {
                var plant2 = (GameObject)PrefabUtility.InstantiatePrefab(prefabPlant);
                plant2.name = "Carnivorous_Plant_Z2_2";
                plant2.transform.SetParent(levelRoot.transform);
                plant2.transform.position = new Vector3(54.0f, 12.9f, 0f);

                var cpComp = plant2.GetComponent<CarnivorousPlant2D>();
                if (cpComp != null)
                {
                    var openField = typeof(CarnivorousPlant2D).GetField("_openDuration", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (openField != null) openField.SetValue(cpComp, 2.4f);
                }
            }

            // Hoja 3 de escape
            CreateLeaf(prefabLeaf, levelRoot.transform, "Crumbling_Leaf_Jaw_3", new Vector3(57.5f, 13.5f, 0f), new Vector3(1.3f, 0.35f, 1f), 0.45f);

            // Plataforma de antesala a la bifurcación
            CreateGround("Fork_Entry_Ledge", levelRoot.transform, new Vector3(60.5f, 13.5f, 0f), new Vector2(2.2f, 0.8f), goldenWood, square, unlitMat);
            CreateGround("Fork_Entry_Moss", levelRoot.transform, new Vector3(60.5f, 13.95f, 0f), new Vector2(2.2f, 0.15f), emeraldCanopy, square, unlitMat);

            // ==========================================
            // ZONA 3: La Gran Bifurcación del Dosel (X = 60 a 75, Y = 13 a 20)
            // ==========================================
            // --- RUTA A (Baja): "La Escalera del Vértigo" ---
            // 3 hojas rápidas (0.40s) separadas por dientes verticales de espinas.
            CreateLeaf(prefabLeaf, levelRoot.transform, "Crumbling_Leaf_Vertigo_1", new Vector3(63.5f, 14.0f, 0f), new Vector3(1.2f, 0.35f, 1f), 0.40f);
            CreateSpikePillar(prefabSpikes, levelRoot.transform, "Spike_Tooth_Z3_1", new Vector3(65.7f, 14.5f, 0f), new Vector2(0.9f, 2.2f), square, darkBranch, unlitMat);

            CreateLeaf(prefabLeaf, levelRoot.transform, "Crumbling_Leaf_Vertigo_2", new Vector3(68.0f, 15.0f, 0f), new Vector3(1.2f, 0.35f, 1f), 0.40f);
            CreateSpikePillar(prefabSpikes, levelRoot.transform, "Spike_Tooth_Z3_2", new Vector3(70.2f, 15.5f, 0f), new Vector2(0.9f, 2.2f), square, darkBranch, unlitMat);

            CreateLeaf(prefabLeaf, levelRoot.transform, "Crumbling_Leaf_Vertigo_3", new Vector3(72.5f, 16.0f, 0f), new Vector3(1.2f, 0.35f, 1f), 0.40f);

            // --- RUTA B (Alta): "El Trampolín de las Hojas Doradas" ---
            // Súper rebote (19.0 m/s) disparado sobre aguja de espinas gigante hacia hojas en la estratosfera
            CreateGround("Mushroom_Stump_Z3_High", levelRoot.transform, new Vector3(62.0f, 13.0f, 0f), new Vector2(1.8f, 0.8f), goldenWood, square, unlitMat);
            if (prefabMushroom != null)
            {
                var highMush = (GameObject)PrefabUtility.InstantiatePrefab(prefabMushroom);
                highMush.name = "Bouncy_Mushroom_Z3_High";
                highMush.transform.SetParent(levelRoot.transform);
                highMush.transform.position = new Vector3(62.0f, 14.0f, 0f);

                var bp = highMush.GetComponent<BouncyPlatform2D>();
                if (bp != null)
                {
                    var field = typeof(BouncyPlatform2D).GetField("_bounceVelocity", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field != null) field.SetValue(bp, 19.0f);
                }
            }

            CreateSpikePillar(prefabSpikes, levelRoot.transform, "Spike_Tower_Z3", new Vector3(67.0f, 16.5f, 0f), new Vector2(1.4f, 6.0f), square, darkBranch, unlitMat);
            CreateLeaf(prefabLeaf, levelRoot.transform, "High_Golden_Leaf_1", new Vector3(66.5f, 21.0f, 0f), new Vector3(1.3f, 0.35f, 1f), 0.45f);
            CreateLeaf(prefabLeaf, levelRoot.transform, "High_Golden_Leaf_2", new Vector3(71.0f, 19.5f, 0f), new Vector3(1.3f, 0.35f, 1f), 0.45f);

            // ==========================================
            // ZONA 4: Checkpoint 2 & La Gran Parábola sobre las Nubes (X = 75 a 97, Y = 18 a 22)
            // ==========================================
            CreateGround("Summit_Ante_Ledge", levelRoot.transform, new Vector3(76.5f, 18.0f, 0f), new Vector2(3.2f, 1.2f), goldenWood, square, unlitMat);
            CreateGround("Summit_Ante_Moss", levelRoot.transform, new Vector3(76.5f, 18.65f, 0f), new Vector2(3.2f, 0.15f), emeraldCanopy, square, unlitMat);

            if (prefabCheckpoint != null)
            {
                var cp2 = (GameObject)PrefabUtility.InstantiatePrefab(prefabCheckpoint);
                cp2.name = "Nest_Checkpoint_2";
                cp2.transform.SetParent(levelRoot.transform);
                cp2.transform.position = new Vector3(76.5f, 19.15f, 0f);
            }

            // Rama de lanzamiento hacia el abismo de las nubes
            CreateGround("Launch_Branch_Z4", levelRoot.transform, new Vector3(80.5f, 18.5f, 0f), new Vector2(2.2f, 0.8f), goldenWood, square, unlitMat);
            CreateGround("Launch_Branch_Moss", levelRoot.transform, new Vector3(80.5f, 18.95f, 0f), new Vector2(2.2f, 0.15f), emeraldCanopy, square, unlitMat);

            // Hoja trampolín 1
            CreateLeaf(prefabLeaf, levelRoot.transform, "Crumbling_Leaf_Parabola_1", new Vector3(84.5f, 19.2f, 0f), new Vector3(1.3f, 0.35f, 1f), 0.45f);

            // Percha intermedia con Planta Carnívora 3
            CreateGround("Carnivore_Perch_Z4", levelRoot.transform, new Vector3(88.5f, 18.2f, 0f), new Vector2(1.8f, 1.4f), goldenWood, square, unlitMat);
            if (prefabPlant != null)
            {
                var plant3 = (GameObject)PrefabUtility.InstantiatePrefab(prefabPlant);
                plant3.name = "Carnivorous_Plant_Z4";
                plant3.transform.SetParent(levelRoot.transform);
                plant3.transform.position = new Vector3(88.5f, 19.5f, 0f);
            }

            // Hoja trampolín final (despegue para LA GRAN PARÁBOLA)
            CreateLeaf(prefabLeaf, levelRoot.transform, "Crumbling_Leaf_Launch", new Vector3(92.5f, 20.2f, 0f), new Vector3(1.4f, 0.35f, 1f), 0.45f);

            // ==========================================
            // ZONA 5: El Santuario del Huevo Verde y Salida a Boss 1 (X = 98 a 115, Y = 20 a 25)
            // ==========================================
            CreateGround("Sanctuary_Altar_Base", levelRoot.transform, new Vector3(105.0f, 20.8f, 0f), new Vector2(14.0f, 2.0f), goldenWood, square, unlitMat);
            CreateGround("Sanctuary_Altar_Moss", levelRoot.transform, new Vector3(105.0f, 21.9f, 0f), new Vector2(14.0f, 0.25f), emeraldCanopy, square, unlitMat);
            CreateGround("Wall_Right_Boundary", levelRoot.transform, new Vector3(112.5f, 26.5f, 0f), new Vector2(1.2f, 12.0f), goldenWood, square, unlitMat);

            // Portal hacia Boss 1
            LevelExit2D exitComponent = null;
            if (prefabPortal != null)
            {
                var portalGo = (GameObject)PrefabUtility.InstantiatePrefab(prefabPortal);
                portalGo.name = "Portal_Exit_To_Boss_1";
                portalGo.transform.SetParent(levelRoot.transform);
                portalGo.transform.position = new Vector3(109.0f, 23.2f, 0f);

                exitComponent = portalGo.GetComponent<LevelExit2D>();
                if (exitComponent != null)
                {
                    var nextSceneField = typeof(LevelExit2D).GetField("_nextSceneName", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (nextSceneField != null) nextSceneField.SetValue(exitComponent, "Boss_1");

                    var titleField = typeof(LevelExit2D).GetField("_levelTitle", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (titleField != null) titleField.SetValue(exitComponent, "¡EL PRIMER HIJO ESTÁ A SALVO!");

                    var msgField = typeof(LevelExit2D).GetField("_victoryMessage", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (msgField != null) msgField.SetValue(exitComponent, "Has rescatado el Huevo Verde de las alturas de la copa.\nPero el gran simio bloquea el camino:\n¡Prepárate para la batalla contra el Mono Ladrón Gigante!");
                }
            }

            // Altar del Huevo Verde
            CreateGreenEggSanctuary(levelRoot.transform, new Vector3(103.5f, 22.05f, 0f), exitComponent, circle, unlitMat, prefabCheckpoint);

            // DeadZone KillFloor debajo de todo el nivel (abismo de nubes)
            CreateDeadZone(levelRoot.transform, new Vector3(56f, -7f, 0f), new Vector2(160f, 2f));

            // ==========================================
            // 3. Configuración del Jugador (Alma)
            // ==========================================
            var alma = GameObject.Find("Alma (Player)");
            if (alma != null)
            {
                alma.transform.position = new Vector3(-0.3f, 1.5f, 0f);

                var pc = alma.GetComponent<PlayerController>();
                if (pc != null)
                {
                    var djField = typeof(PlayerController).GetField("_doubleJumpUnlocked", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (djField != null) djField.SetValue(pc, true);
                    EditorUtility.SetDirty(pc);
                }

                var rb = alma.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    EditorUtility.SetDirty(rb);
                }

                SceneSetupValidator.SetupAlmaVisuals(alma);
            }

            // 4. Configurar Cámara y Follow
            var cam = UnityEngine.Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = 6.0f;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.23f, 0.53f, 1.0f, 1f); // #3A86FF High sapphire sky
                if (alma != null)
                {
                    cam.transform.position = new Vector3(alma.transform.position.x, alma.transform.position.y + 1.5f, -10f);
                }
                EditorUtility.SetDirty(cam);

                var follow = cam.GetComponent<AlmaDino.Features.Camera.Camera2DFollow>();
                if (follow != null)
                {
                    if (alma != null) follow.SetTarget(alma.transform);
                    follow.SetBounds(new Vector2(-5f, -4f), new Vector2(116f, 32f));
                    EditorUtility.SetDirty(follow);
                }
            }

            // 5. Configurar Iluminación 2D URP (Sol radiante de mediodía)
            var globalLight = Object.FindAnyObjectByType<Light2D>();
            if (globalLight != null)
            {
                globalLight.lightType = Light2D.LightType.Global;
                globalLight.intensity = 1.10f;
                globalLight.color = new Color(1.0f, 0.96f, 0.88f);
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

            Debug.Log("<color=#00F5D4><b>[AlmaDino]</b> ¡Nivel 1-4 ('La Copa del Gran Árbol' CLÍMAX MUNDO 1) construido con éxito!</color>");
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

        private static void CreateLeaf(GameObject prefabLeaf, Transform parent, string name, Vector3 position, Vector3 scale, float crumbleDelay)
        {
            if (prefabLeaf == null) return;
            var leafGo = (GameObject)PrefabUtility.InstantiatePrefab(prefabLeaf);
            leafGo.name = name;
            leafGo.transform.SetParent(parent);
            leafGo.transform.position = position;
            leafGo.transform.localScale = scale;

            var cp = leafGo.GetComponent<CrumblingPlatform2D>();
            if (cp != null)
            {
                var field = typeof(CrumblingPlatform2D).GetField("_crumbleDelay", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null) field.SetValue(cp, crumbleDelay);
            }
        }

        private static void CreateSpikePillar(GameObject prefabSpikes, Transform parent, string name, Vector3 position, Vector2 size, Sprite square, Color trunkColor, Material material)
        {
            var pillarRoot = new GameObject(name);
            pillarRoot.transform.SetParent(parent);
            pillarRoot.transform.position = position;

            // Tronco / rama central sólido
            var trunk = new GameObject("Trunk");
            trunk.transform.SetParent(pillarRoot.transform);
            trunk.transform.localPosition = Vector3.zero;
            trunk.transform.localScale = new Vector3(size.x, size.y, 1f);

            var sr = trunk.AddComponent<SpriteRenderer>();
            sr.sprite = square;
            sr.sharedMaterial = material;
            sr.color = trunkColor;

            var col = trunk.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;

            // Corona de espinas letales arriba
            if (prefabSpikes != null)
            {
                var topSpikes = (GameObject)PrefabUtility.InstantiatePrefab(prefabSpikes);
                topSpikes.name = "Top_Spikes";
                topSpikes.transform.SetParent(pillarRoot.transform);
                topSpikes.transform.localPosition = new Vector3(0f, size.y * 0.5f + 0.3f, 0f);
                topSpikes.transform.localScale = new Vector3(size.x * 1.1f, 0.9f, 1f);
            }
        }

        private static void CreateGreenEggSanctuary(Transform parent, Vector3 position, LevelExit2D exitPortal, Sprite circle, Material material, GameObject prefabNest)
        {
            var sanctuaryRoot = new GameObject("Green_Egg_Sanctuary_Altar");
            sanctuaryRoot.transform.SetParent(parent);
            sanctuaryRoot.transform.position = position;

            // Nido de cuna sagrada
            if (prefabNest != null)
            {
                var nestVisual = (GameObject)PrefabUtility.InstantiatePrefab(prefabNest);
                nestVisual.name = "Cradle_Nest_Visual";
                nestVisual.transform.SetParent(sanctuaryRoot.transform);
                nestVisual.transform.localPosition = Vector3.zero;

                // Desactivar lógica de checkpoint del nido decorativo
                var cp = nestVisual.GetComponent<Checkpoint2D>();
                if (cp != null) Object.DestroyImmediate(cp);
                var col = nestVisual.GetComponent<Collider2D>();
                if (col != null) col.enabled = false;
            }

            // Huevo Verde Bioluminiscente
            var eggGo = new GameObject("Green_Egg_Item");
            eggGo.transform.SetParent(sanctuaryRoot.transform);
            eggGo.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            eggGo.transform.localScale = new Vector3(0.9f, 1.25f, 1f);

            var sr = eggGo.AddComponent<SpriteRenderer>();
            sr.sprite = circle;
            sr.sharedMaterial = material;
            sr.color = new Color(0.0f, 0.96f, 0.83f, 1f); // #00F5D4 Esmeralda bioluminiscente

            // Luz 2D puntual para el latido cálido
            var lightGo = new GameObject("Egg_Bioluminescent_Light");
            lightGo.transform.SetParent(eggGo.transform);
            lightGo.transform.localPosition = Vector3.zero;

            var light2d = lightGo.AddComponent<Light2D>();
            light2d.lightType = Light2D.LightType.Point;
            light2d.pointLightOuterRadius = 4.5f;
            light2d.pointLightInnerRadius = 0.5f;
            light2d.color = new Color(0.0f, 0.96f, 0.83f, 1f);
            light2d.intensity = 1.2f;

            // Trigger Collider
            var eggCol = eggGo.AddComponent<CircleCollider2D>();
            eggCol.isTrigger = true;
            eggCol.radius = 1.0f;

            // Componente GreenEggRescue2D
            var rescue = eggGo.AddComponent<GreenEggRescue2D>();
            rescue.Configure(
                EggType.GreenEgg,
                "¡PRIMER RESCATE: EL HUEVO VERDE!",
                "«Aún estás tibio...\nMamá llegó a tiempo. Ya estás a salvo.»\n(1 de 4 rescatados)\n\n¡Un rugido colosal sacude la copa del Gran Árbol!\nEl Mono Ladrón Gigante aguarda furioso más adelante...",
                new Color(0.0f, 0.96f, 0.83f, 1f),
                exitPortal
            );
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
                triggerGo.transform.position = new Vector3(0.5f, 1.2f, 0f);

                var col = triggerGo.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(3f, 3f);

                var narrativeTrigger = triggerGo.AddComponent<NarrativePrologueTrigger>();
                narrativeTrigger.Configure(
                    "LA COPA DEL GRAN ÁRBOL",
                    "Has dejado atrás las sombras de la selva y las espinas venenosas.\nPor encima del mar de nubes, el latido del primer huevo resuena en la cima.\nNo hay suelo bajo tus pies: cada salto debe ser certero.",
                    new Color(0.0f, 0.96f, 0.83f, 1f),
                    6.5f
                );
            }
            else
            {
                var narrativeTrigger = triggerGo.GetComponent<NarrativePrologueTrigger>();
                if (narrativeTrigger != null)
                {
                    narrativeTrigger.Configure(
                        "LA COPA DEL GRAN ÁRBOL",
                        "Has dejado atrás las sombras de la selva y las espinas venenosas.\nPor encima del mar de nubes, el latido del primer huevo resuena en la cima.\nNo hay suelo bajo tus pies: cada salto debe ser certero.",
                        new Color(0.0f, 0.96f, 0.83f, 1f),
                        6.5f
                    );
                }
            }
        }
    }
}
#endif
