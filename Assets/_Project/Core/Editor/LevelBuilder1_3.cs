#if UNITY_EDITOR
using System.IO;
using System.Reflection;
using AlmaDino.Core.Interfaces;
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
    public static class LevelBuilder1_3
    {
        private const string SCENE_PATH = "Assets/Scenes/World_1_Jungle/Level_1_3.unity";
        private const string PREFAB_CHECKPOINT = "Assets/_Project/Prefabs/Universal/Checkpoint_Nest.prefab";
        private const string PREFAB_PORTAL = "Assets/_Project/Prefabs/Universal/Level_Exit_Portal.prefab";
        private const string PREFAB_SPIKES = "Assets/_Project/Prefabs/World_1_Jungle/Hazard_Spikes_Jungle.prefab";
        private const string PREFAB_MUSHROOM = "Assets/_Project/Prefabs/World_1_Jungle/BouncyMushroom_Jungle.prefab";
        private const string PREFAB_LEAF = "Assets/_Project/Prefabs/World_1_Jungle/CrumblingLeaf_Jungle.prefab";
        private const string PREFAB_PLANT = "Assets/_Project/Prefabs/World_1_Jungle/CarnivorousPlant_Jungle.prefab";

        private const string SPRITE_SQUARE = "Assets/Sprites/Square.png";
        private const string SPRITE_CIRCLE = "Assets/Sprites/Circle.png";
        private const string UNLIT_MAT_PATH = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";

        static LevelBuilder1_3()
        {
            EditorApplication.delayCall += AutoBuildOnce;
        }

        private static void AutoBuildOnce()
        {
            if (!EditorPrefs.GetBool("Alma_Level_1_3_Camera_Bounds_v6", false))
            {
                BuildLevel1_3WithPrefabs();
                EditorPrefs.SetBool("Alma_Level_1_3_Camera_Bounds_v6", true);
            }
        }

        [MenuItem("Alma/🏗️ Reestructurar Nivel 1-3 con Prefabs")]
        public static void BuildLevel1_3WithPrefabs()
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

            // 2. Colores temáticos de Las Zarzas Profundas
            Color thornBedColor = new Color(0.18f, 0.08f, 0.17f);  // #2E142B Púrpura oscuro venenoso
            Color darkStone = new Color(0.17f, 0.16f, 0.15f);      // #2B2A27 Pardo pizarra oscuro
            Color mossLedge = new Color(0.23f, 0.28f, 0.16f);      // #3B4828 Musgo marchito

            // Cargar prefabs para instanciación
            GameObject prefabCheckpoint = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_CHECKPOINT);
            GameObject prefabPortal = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PORTAL);
            GameObject prefabSpikes = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_SPIKES);
            GameObject prefabMushroom = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_MUSHROOM);
            GameObject prefabLeaf = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_LEAF);
            GameObject prefabPlant = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PLANT);

            // ==========================================
            // FILOSOFÍA "SUELO CERO": FOSO CONTINUO DE ESPINAS INFERIOR
            // ==========================================
            CreateGround("Thorn_Pit_Bed", levelRoot.transform, new Vector3(52f, -3.5f, 0f), new Vector2(120f, 2.5f), thornBedColor, square, unlitMat);

            // Tira continua de púas venenosas a lo largo de TODO el fondo (Y = -2.1)
            if (prefabSpikes != null)
            {
                for (float sx = 2.0f; sx <= 102f; sx += 5.5f)
                {
                    var spikes = (GameObject)PrefabUtility.InstantiatePrefab(prefabSpikes);
                    spikes.name = $"Thorn_Bed_Spikes_{sx:F0}";
                    spikes.transform.SetParent(levelRoot.transform);
                    spikes.transform.position = new Vector3(sx, -2.1f, 0f);
                    spikes.transform.localScale = new Vector3(5.6f, 1.2f, 1f);
                }
            }

            // ==========================================
            // ZONA 0: El Borde del Foso (Spawn) (X = -2 a 4)
            // ==========================================
            CreateGround("Spawn_Rock_Cliff", levelRoot.transform, new Vector3(0f, 0.0f, 0f), new Vector2(3.5f, 1.5f), darkStone, square, unlitMat);
            CreateGround("Spawn_Moss_Top", levelRoot.transform, new Vector3(0f, 0.7f, 0f), new Vector2(3.5f, 0.2f), mossLedge, square, unlitMat);
            CreateGround("Wall_Left_Boundary", levelRoot.transform, new Vector3(-2.2f, 6.0f, 0f), new Vector2(1f, 14f), darkStone, square, unlitMat);

            // Nido de spawn
            if (prefabCheckpoint != null)
            {
                var nestGo = (GameObject)PrefabUtility.InstantiatePrefab(prefabCheckpoint);
                nestGo.name = "Nest_Cradle_Spawn";
                nestGo.transform.SetParent(levelRoot.transform);
                nestGo.transform.position = new Vector3(-0.3f, 1.0f, 0f);
            }

            EnsurePrologueNarrativeTrigger(levelRoot.transform);

            // ==========================================
            // ZONA 1: El Desfiladero de Espinas & Doble Salto Obligatorio (X = 2 a 25)
            // Desafío: 3 hojas quebradizas estrechas (1.4m, timer 0.45s) con columnas de espinas que
            // OBLIGAN a saltar alto, arquear y aletear con el doble salto sobre las columnas.
            // ==========================================
            // Hoja 1: Salto largo desde el borde (X = 1.7 ➔ X = 7.0 = 5.3m de salto obligatorio con doble salto!)
            CreateLeaf(prefabLeaf, levelRoot.transform, "Crumbling_Leaf_Z1_1", new Vector3(7.0f, 1.5f, 0f), new Vector3(1.4f, 0.35f, 1f), 0.45f);

            // Columna de espinas 1: se alza entre hoja 1 y hoja 2
            CreateSpikePillar(prefabSpikes, levelRoot.transform, "Spike_Pillar_Z1_A", new Vector3(10.0f, 1.8f, 0f), new Vector2(1.2f, 3.6f), square, darkStone, unlitMat);

            // Hoja 2: tras sobrevolar la columna 1
            CreateLeaf(prefabLeaf, levelRoot.transform, "Crumbling_Leaf_Z1_2", new Vector3(13.0f, 2.0f, 0f), new Vector3(1.4f, 0.35f, 1f), 0.45f);

            // Columna de espinas 2: se alza entre hoja 2 y hoja 3
            CreateSpikePillar(prefabSpikes, levelRoot.transform, "Spike_Pillar_Z1_B", new Vector3(16.0f, 2.4f, 0f), new Vector2(1.2f, 4.2f), square, darkStone, unlitMat);

            // Hoja 3: elevada, exige doble salto al ápice
            CreateLeaf(prefabLeaf, levelRoot.transform, "Crumbling_Leaf_Z1_3", new Vector3(19.0f, 2.8f, 0f), new Vector3(1.4f, 0.35f, 1f), 0.45f);

            // Pilar de descanso estrecho (1.8m de ancho)
            CreateGround("Pillar_Rest_1", levelRoot.transform, new Vector3(23.5f, 2.2f, 0f), new Vector2(1.8f, 0.8f), darkStone, square, unlitMat);
            CreateGround("Pillar_Rest_1_Moss", levelRoot.transform, new Vector3(23.5f, 2.65f, 0f), new Vector2(1.8f, 0.15f), mossLedge, square, unlitMat);

            // ==========================================
            // ZONA 2: Vuelo del Hongo entre Agujas de Espinas hacia Hojas Aéreas (X = 24 a 43)
            // Desafío: El hongo te dispara alto (17.0 m/s). Debes sobrevolar una aguja gigante de espinas,
            // aterrizar en caída sobre una hoja quebradiza aérea (1.3m, 0.45s) y de inmediato saltar a otra!
            // ==========================================
            CreateGround("Mushroom_Stump_1", levelRoot.transform, new Vector3(26.2f, 1.0f, 0f), new Vector2(2.0f, 0.8f), darkStone, square, unlitMat);
            if (prefabMushroom != null)
            {
                var mush1 = (GameObject)PrefabUtility.InstantiatePrefab(prefabMushroom);
                mush1.name = "Bouncy_Mushroom_Combo";
                mush1.transform.SetParent(levelRoot.transform);
                mush1.transform.position = new Vector3(26.2f, 2.0f, 0f);

                var bp = mush1.GetComponent<BouncyPlatform2D>();
                if (bp != null)
                {
                    var field = typeof(BouncyPlatform2D).GetField("_bounceVelocity", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field != null) field.SetValue(bp, 17.0f);
                }
            }

            // Aguja gigante de espinas en medio del vuelo
            CreateSpikePillar(prefabSpikes, levelRoot.transform, "Spike_Tower_Z2", new Vector3(30.2f, 3.8f, 0f), new Vector2(1.4f, 7.0f), square, darkStone, unlitMat);

            // Hoja aérea 1 (en descenso tras sobrevolar la aguja)
            CreateLeaf(prefabLeaf, levelRoot.transform, "Crumbling_Leaf_Air_1", new Vector3(33.5f, 5.0f, 0f), new Vector3(1.3f, 0.35f, 1f), 0.45f);

            // Hoja aérea 2 (salto inmediato desde hoja 1)
            CreateLeaf(prefabLeaf, levelRoot.transform, "Crumbling_Leaf_Air_2", new Vector3(37.5f, 4.0f, 0f), new Vector3(1.3f, 0.35f, 1f), 0.45f);

            // Isla Checkpoint 1 (estrecha: 2.2m)
            CreateGround("Mid_Island_Cliff", levelRoot.transform, new Vector3(41.5f, 3.2f, 0f), new Vector2(2.2f, 1.0f), darkStone, square, unlitMat);
            CreateGround("Mid_Island_Moss", levelRoot.transform, new Vector3(41.5f, 3.75f, 0f), new Vector2(2.2f, 0.15f), mossLedge, square, unlitMat);

            if (prefabCheckpoint != null)
            {
                var cp1 = (GameObject)PrefabUtility.InstantiatePrefab(prefabCheckpoint);
                cp1.name = "Nest_Checkpoint_1";
                cp1.transform.SetParent(levelRoot.transform);
                cp1.transform.position = new Vector3(41.5f, 4.05f, 0f);
            }

            // ==========================================
            // ZONA 3: La Gran Bifurcación (Campo Minado vs Vértigo en el Dosel) (X = 42 a 65)
            // ==========================================
            // --- RUTA BAJA: "El Campo Minado de Zarzas" ---
            // 4 hojas quebradizas (1.3m, 0.45s) separadas por dientes verticales de espinas que debes saltar por encima!
            CreateLeaf(prefabLeaf, levelRoot.transform, "Crumbling_Leaf_Low_1", new Vector3(45.0f, 1.6f, 0f), new Vector3(1.3f, 0.35f, 1f), 0.45f);
            CreateSpikePillar(prefabSpikes, levelRoot.transform, "Spike_Tooth_Low_1", new Vector3(47.2f, 1.8f, 0f), new Vector2(1.0f, 2.2f), square, darkStone, unlitMat);

            CreateLeaf(prefabLeaf, levelRoot.transform, "Crumbling_Leaf_Low_2", new Vector3(49.5f, 1.6f, 0f), new Vector3(1.3f, 0.35f, 1f), 0.45f);
            CreateSpikePillar(prefabSpikes, levelRoot.transform, "Spike_Tooth_Low_2", new Vector3(51.7f, 1.8f, 0f), new Vector2(1.0f, 2.2f), square, darkStone, unlitMat);

            CreateLeaf(prefabLeaf, levelRoot.transform, "Crumbling_Leaf_Low_3", new Vector3(54.0f, 1.6f, 0f), new Vector3(1.3f, 0.35f, 1f), 0.45f);
            CreateSpikePillar(prefabSpikes, levelRoot.transform, "Spike_Tooth_Low_3", new Vector3(56.2f, 1.8f, 0f), new Vector2(1.0f, 2.2f), square, darkStone, unlitMat);

            CreateLeaf(prefabLeaf, levelRoot.transform, "Crumbling_Leaf_Low_4", new Vector3(58.5f, 1.6f, 0f), new Vector3(1.3f, 0.35f, 1f), 0.45f);

            // --- RUTA ALTA: "El Paso del Vértigo en el Dosel" ---
            // Súper Rebote (18.5 m/s) disparado hacia hojas en la corona del bosque (Y = 9.8)
            CreateGround("High_Route_Stump", levelRoot.transform, new Vector3(43.5f, 3.2f, 0f), new Vector2(1.8f, 0.8f), darkStone, square, unlitMat);
            if (prefabMushroom != null)
            {
                var highMush = (GameObject)PrefabUtility.InstantiatePrefab(prefabMushroom);
                highMush.name = "Bouncy_Mushroom_High";
                highMush.transform.SetParent(levelRoot.transform);
                highMush.transform.position = new Vector3(43.5f, 4.2f, 0f);

                var bp = highMush.GetComponent<BouncyPlatform2D>();
                if (bp != null)
                {
                    var field = typeof(BouncyPlatform2D).GetField("_bounceVelocity", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field != null) field.SetValue(bp, 18.5f);
                }
            }

            CreateLeaf(prefabLeaf, levelRoot.transform, "High_Crumbling_Leaf_1", new Vector3(48.0f, 9.8f, 0f), new Vector3(1.3f, 0.35f, 1f), 0.45f);
            CreateSpikePillar(prefabSpikes, levelRoot.transform, "High_Spike_Divider", new Vector3(51.0f, 10.5f, 0f), new Vector2(1.0f, 2.5f), square, darkStone, unlitMat);
            CreateLeaf(prefabLeaf, levelRoot.transform, "High_Crumbling_Leaf_2", new Vector3(54.0f, 9.2f, 0f), new Vector3(1.3f, 0.35f, 1f), 0.45f);

            // Plataforma de convergencia de ambas rutas (2.4m)
            CreateGround("Rest_Rock_Zone3", levelRoot.transform, new Vector3(62.5f, 3.5f, 0f), new Vector2(2.4f, 1.0f), darkStone, square, unlitMat);
            CreateGround("Rest_Rock_Zone3_Moss", levelRoot.transform, new Vector3(62.5f, 4.05f, 0f), new Vector2(2.4f, 0.15f), mossLedge, square, unlitMat);

            // ==========================================
            // ZONA 4: El Pasaje de las Dos Plantas Carnívoras y Hojas Trampa (X = 64 a 85)
            // Desafío brutal: Checkpoint 2 ➔ Hoja 1 (0.45s) ➔ Salto sobre Planta 1 ➔ Hoja Central 2 (0.45s) ➔ Salto sobre Planta 2 ➔ Hoja 3!
            // No puedes parar ni un milisegundo: o saltas sobre las fauces al ritmo exacto o la hoja te tira a las espinas!
            // ==========================================
            CreateGround("Cliff_Checkpoint_2", levelRoot.transform, new Vector3(65.5f, 3.8f, 0f), new Vector2(2.2f, 1.0f), darkStone, square, unlitMat);
            CreateGround("Cliff_Checkpoint_2_Moss", levelRoot.transform, new Vector3(65.5f, 4.35f, 0f), new Vector2(2.2f, 0.15f), mossLedge, square, unlitMat);

            if (prefabCheckpoint != null)
            {
                var cp2 = (GameObject)PrefabUtility.InstantiatePrefab(prefabCheckpoint);
                cp2.name = "Nest_Checkpoint_2";
                cp2.transform.SetParent(levelRoot.transform);
                cp2.transform.position = new Vector3(65.5f, 4.65f, 0f);
            }

            // Hoja 1 antes de Planta 1
            CreateLeaf(prefabLeaf, levelRoot.transform, "Crumbling_Leaf_Carnivore_1", new Vector3(69.2f, 4.2f, 0f), new Vector3(1.3f, 0.35f, 1f), 0.45f);

            // Planta Carnívora 1
            CreateGround("Carnivorous_Pillar_1", levelRoot.transform, new Vector3(72.5f, 3.4f, 0f), new Vector2(1.8f, 1.4f), darkStone, square, unlitMat);
            if (prefabPlant != null)
            {
                var plant1 = (GameObject)PrefabUtility.InstantiatePrefab(prefabPlant);
                plant1.name = "Carnivorous_Plant_1";
                plant1.transform.SetParent(levelRoot.transform);
                plant1.transform.position = new Vector3(72.5f, 4.7f, 0f);
            }

            // Hoja 2 central (flota entre ambas plantas carnívoras!)
            CreateLeaf(prefabLeaf, levelRoot.transform, "Crumbling_Leaf_Carnivore_2", new Vector3(76.0f, 4.4f, 0f), new Vector3(1.3f, 0.35f, 1f), 0.45f);

            // Planta Carnívora 2 (ciclo ligeramente desfasado)
            CreateGround("Carnivorous_Pillar_2", levelRoot.transform, new Vector3(79.5f, 3.4f, 0f), new Vector2(1.8f, 1.4f), darkStone, square, unlitMat);
            if (prefabPlant != null)
            {
                var plant2 = (GameObject)PrefabUtility.InstantiatePrefab(prefabPlant);
                plant2.name = "Carnivorous_Plant_2";
                plant2.transform.SetParent(levelRoot.transform);
                plant2.transform.position = new Vector3(79.5f, 4.7f, 0f);

                var cpComp = plant2.GetComponent<CarnivorousPlant2D>();
                if (cpComp != null)
                {
                    var openField = typeof(CarnivorousPlant2D).GetField("_openDuration", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (openField != null) openField.SetValue(cpComp, 2.4f);
                }
            }

            // Hoja 3 posterior a Planta 2
            CreateLeaf(prefabLeaf, levelRoot.transform, "Crumbling_Leaf_Carnivore_3", new Vector3(83.0f, 4.4f, 0f), new Vector3(1.3f, 0.35f, 1f), 0.45f);

            // ==========================================
            // ZONA 5: El Gran Ascenso de la Cumbre entre Espinas (X = 85 a 104)
            // Desafío: Repisa ascensor ➔ Hongo ➔ 2 hojas quebradizas de cumbre en ascenso sobre pinchos ➔ Portal
            // ==========================================
            CreateGround("Ascent_Ledge", levelRoot.transform, new Vector3(86.5f, 3.0f, 0f), new Vector2(2.0f, 0.8f), darkStone, square, unlitMat);
            if (prefabMushroom != null)
            {
                var summitMush = (GameObject)PrefabUtility.InstantiatePrefab(prefabMushroom);
                summitMush.name = "Bouncy_Mushroom_Summit";
                summitMush.transform.SetParent(levelRoot.transform);
                summitMush.transform.position = new Vector3(86.5f, 4.0f, 0f);

                var bp = summitMush.GetComponent<BouncyPlatform2D>();
                if (bp != null)
                {
                    var field = typeof(BouncyPlatform2D).GetField("_bounceVelocity", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field != null) field.SetValue(bp, 16.5f);
                }
            }

            // Hojas quebradizas de ascenso hacia la cumbre
            CreateLeaf(prefabLeaf, levelRoot.transform, "Crumbling_Leaf_Summit_1", new Vector3(90.5f, 6.8f, 0f), new Vector3(1.3f, 0.35f, 1f), 0.45f);
            CreateLeaf(prefabLeaf, levelRoot.transform, "Crumbling_Leaf_Summit_2", new Vector3(94.5f, 8.2f, 0f), new Vector3(1.3f, 0.35f, 1f), 0.45f);

            // Peñasco de la cumbre y portal hacia 1-4
            CreateGround("Exit_Ridge_Zone5", levelRoot.transform, new Vector3(99.0f, 9.2f, 0f), new Vector2(4.5f, 1.2f), darkStone, square, unlitMat);
            CreateGround("Exit_Ridge_Moss", levelRoot.transform, new Vector3(99.0f, 9.85f, 0f), new Vector2(4.5f, 0.15f), mossLedge, square, unlitMat);
            CreateGround("Wall_Right_Boundary", levelRoot.transform, new Vector3(102.0f, 14.0f, 0f), new Vector2(1f, 12f), darkStone, square, unlitMat);

            // Portal de salida hacia 1-4
            if (prefabPortal != null)
            {
                var portalGo = (GameObject)PrefabUtility.InstantiatePrefab(prefabPortal);
                portalGo.name = "Portal_Exit_To_1_4";
                portalGo.transform.SetParent(levelRoot.transform);
                portalGo.transform.position = new Vector3(100.0f, 11.2f, 0f);

                var exit = portalGo.GetComponent<LevelExit2D>();
                if (exit != null)
                {
                    var nextSceneField = typeof(LevelExit2D).GetField("_nextSceneName", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (nextSceneField != null) nextSceneField.SetValue(exit, "Level_1_4");

                    var titleField = typeof(LevelExit2D).GetField("_levelTitle", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (titleField != null) titleField.SetValue(exit, "NIVEL 1-3 COMPLETADO");

                    var msgField = typeof(LevelExit2D).GetField("_victoryMessage", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (msgField != null) msgField.SetValue(exit, "¡Has conquistado las Zarzas Profundas desafiando toda adversidad!\nEl llanto de tus pequeños resuena desde las alturas del Gran Árbol Sagrado (1-4).");
                }
            }

            // DeadZone KillFloor debajo de todo el nivel
            CreateDeadZone(levelRoot.transform, new Vector3(52f, -8f, 0f), new Vector2(160f, 2f));

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
                cam.backgroundColor = new Color(0.06f, 0.10f, 0.08f, 1f); // Deep gloom jungle
                if (alma != null)
                {
                    cam.transform.position = new Vector3(alma.transform.position.x, alma.transform.position.y + 1.5f, -10f);
                }
                EditorUtility.SetDirty(cam);

                var follow = cam.GetComponent<AlmaDino.Features.Camera.Camera2DFollow>();
                if (follow != null)
                {
                    if (alma != null) follow.SetTarget(alma.transform);
                    follow.SetBounds(new Vector2(-5f, -4f), new Vector2(108f, 40f));
                    EditorUtility.SetDirty(follow);
                }
            }

            // 5. Configurar Iluminación 2D URP
            var globalLight = Object.FindAnyObjectByType<Light2D>();
            if (globalLight != null)
            {
                globalLight.lightType = Light2D.LightType.Global;
                globalLight.intensity = 0.50f;
                globalLight.color = new Color(0.72f, 0.85f, 0.75f);
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

            Debug.Log("<color=#FF2200><b>[AlmaDino]</b> ¡Nivel 1-3 ('Las Zarzas Profundas' MODO HARDCORE) reconstruido con saltos dobles obligatorios y obstáculos letales!</color>");
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

            // Tronco central sólido
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
                    "LAS ZARZAS PROFUNDAS",
                    "El suelo firme ha desaparecido bajo un manto de espinas impenetrables.\nLas hojas marchitas no soportarán tu peso por mucho tiempo.\nConfía en tu aleteo y no te detengas: vacilar es caer.",
                    new Color(0.85f, 0.25f, 0.35f, 1f),
                    6.5f
                );
            }
            else
            {
                var narrativeTrigger = triggerGo.GetComponent<NarrativePrologueTrigger>();
                if (narrativeTrigger != null)
                {
                    narrativeTrigger.Configure(
                        "LAS ZARZAS PROFUNDAS",
                        "El suelo firme ha desaparecido bajo un manto de espinas impenetrables.\nLas hojas marchitas no soportarán tu peso por mucho tiempo.\nConfía en tu aleteo y no te detengas: vacilar es caer.",
                        new Color(0.85f, 0.25f, 0.35f, 1f),
                        6.5f
                    );
                }
            }
        }
    }
}
#endif
