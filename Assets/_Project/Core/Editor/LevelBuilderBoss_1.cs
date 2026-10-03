#if UNITY_EDITOR
using System.IO;
using System.Reflection;
using AlmaDino.Core.Events;
using AlmaDino.Core.Interfaces;
using AlmaDino.Core.Progression;
using AlmaDino.Features.Boss.Controllers;
using AlmaDino.Features.Boss.Models;
using AlmaDino.Features.Boss.Projectiles;
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
    public static class LevelBuilderBoss_1
    {
        private const string SCENE_PATH = "Assets/Scenes/World_1_Jungle/Boss_1.unity";
        private const string PREFAB_CHECKPOINT = "Assets/_Project/Prefabs/Universal/Checkpoint_Nest.prefab";
        private const string PREFAB_PORTAL = "Assets/_Project/Prefabs/Universal/Level_Exit_Portal.prefab";
        private const string PREFAB_FRUIT = "Assets/_Project/Prefabs/World_1_Jungle/RollingFruit_Boss1.prefab";
        private const string SHAKE_CHANNEL_PATH = "Assets/_Project/Core/Events/CameraShakeChannel.asset";

        private const string SPRITE_SQUARE = "Assets/Sprites/Square.png";
        private const string SPRITE_CIRCLE = "Assets/Sprites/Circle.png";
        private const string UNLIT_MAT_PATH = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";

        static LevelBuilderBoss_1()
        {
            EditorApplication.delayCall += AutoBuildOnce;
        }

        private static void AutoBuildOnce()
        {
            if (!EditorPrefs.GetBool("Alma_Boss_1_Build_v4", false))
            {
                BuildBoss1Arena();
                EditorPrefs.SetBool("Alma_Boss_1_Build_v4", true);
            }
        }

        [MenuItem("Alma/🏗️ Reestructurar Arena Jefe 1")]
        public static void BuildBoss1Arena()
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

            CameraShakeEventChannelSO shakeChannel = AssetDatabase.LoadAssetAtPath<CameraShakeEventChannelSO>(SHAKE_CHANNEL_PATH);

            // Asegurar la existencia del prefab de Fruto Rodante / Rebotante
            GameObject fruitPrefab = EnsureFruitPrefab(circle, square, unlitMat);

            // 1. Limpiar o configurar contenedor del nivel
            GameObject levelRoot = GameObject.Find("--- LEVEL ---");
            if (levelRoot != null)
            {
                Object.DestroyImmediate(levelRoot);
            }
            levelRoot = new GameObject("--- LEVEL ---");

            // 2. Colores temáticos del Atardecer Tormentoso en la Copa
            Color darkTeak = new Color(0.24f, 0.15f, 0.14f);      // #3E2723 Teca oscura maciza
            Color emeraldMoss = new Color(0.18f, 0.77f, 0.71f);   // #2EC4B6 Musgo esmeralda
            Color vineGreen = new Color(0.18f, 0.32f, 0.12f);     // Lianas de la copa
            Color twilightSky = new Color(0.12f, 0.06f, 0.19f, 1f); // #201030 Cielo crepuscular

            // Cargar prefabs para instanciación
            GameObject prefabCheckpoint = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_CHECKPOINT);
            GameObject prefabPortal = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PORTAL);

            // ==========================================
            // GEOMETRÍA DE LA ARENA DE COMBATE
            // ==========================================
            // Plataforma Central Ancha (20m x 2m)
            CreateGround("Platform_Center_Teak", levelRoot.transform, new Vector3(0f, 0f, 0f), new Vector2(20.0f, 2.0f), darkTeak, square, unlitMat);
            CreateGround("Platform_Center_Moss", levelRoot.transform, new Vector3(0f, 1.05f, 0f), new Vector2(20.0f, 0.15f), emeraldMoss, square, unlitMat);

            // Ramas Elevadas Laterales (Refugios accesibles con salto simple o doble: Y = 2.3m, altura relativa +1.45m)
            // Cuentan con PlatformEffector2D para permitir saltar a través de ellas desde abajo sin golpearse la cabeza.
            CreateOneWayPlatform("Platform_Left_Branch", levelRoot.transform, new Vector3(-7.5f, 2.3f, 0f), new Vector2(5.5f, 0.4f), darkTeak, square, unlitMat);
            CreateVisualDecoration("Platform_Left_Moss", levelRoot.transform, new Vector3(-7.5f, 2.52f, 0f), new Vector2(5.5f, 0.08f), emeraldMoss, square, unlitMat);

            CreateOneWayPlatform("Platform_Right_Branch", levelRoot.transform, new Vector3(7.5f, 2.3f, 0f), new Vector2(5.5f, 0.4f), darkTeak, square, unlitMat);
            CreateVisualDecoration("Platform_Right_Moss", levelRoot.transform, new Vector3(7.5f, 2.52f, 0f), new Vector2(5.5f, 0.08f), emeraldMoss, square, unlitMat);

            // Lianas Visuales Colgantes de la Copa (Puntos de anclaje del simio)
            CreateVisualDecoration("Vine_Hanging_Left", levelRoot.transform, new Vector3(-4.5f, 8.5f, 0f), new Vector2(0.35f, 6.0f), vineGreen, square, unlitMat);
            CreateVisualDecoration("Vine_Hanging_Center", levelRoot.transform, new Vector3(0f, 8.5f, 0f), new Vector2(0.35f, 6.0f), vineGreen, square, unlitMat);
            CreateVisualDecoration("Vine_Hanging_Right", levelRoot.transform, new Vector3(4.5f, 8.5f, 0f), new Vector2(0.35f, 6.0f), vineGreen, square, unlitMat);

            // Muros Delimitadores de la Arena (Evitan que el jugador se salga del encuadre)
            CreateGround("Wall_Left_Boundary", levelRoot.transform, new Vector3(-11.5f, 6.0f, 0f), new Vector2(1.2f, 14.0f), darkTeak, square, unlitMat);
            CreateGround("Wall_Right_Boundary", levelRoot.transform, new Vector3(11.5f, 6.0f, 0f), new Vector2(1.2f, 14.0f), darkTeak, square, unlitMat);

            // DeadZone KillFloor en el abismo inferior
            CreateDeadZone(levelRoot.transform, new Vector3(0f, -7.0f, 0f), new Vector2(60f, 2.0f));

            // ==========================================
            // NIDO DE SPAWN / CHECKPOINT INICIAL
            // ==========================================
            if (prefabCheckpoint != null)
            {
                var nestGo = (GameObject)PrefabUtility.InstantiatePrefab(prefabCheckpoint);
                nestGo.name = "Arena_Spawn_Nest";
                nestGo.transform.SetParent(levelRoot.transform);
                nestGo.transform.position = new Vector3(-8.5f, 1.05f, 0f);
            }

            // ==========================================
            // GRAN PORTAL DE VICTORIA HACIA MUNDO 2 (CUEVAS)
            // ==========================================
            GameObject portalInstance = null;
            if (prefabPortal != null)
            {
                portalInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefabPortal);
                portalInstance.name = "Portal_Victory_To_World_2";
                portalInstance.transform.SetParent(levelRoot.transform);
                portalInstance.transform.position = new Vector3(0f, 2.0f, 0f);

                var exit = portalInstance.GetComponent<LevelExit2D>();
                if (exit != null)
                {
                    var nextSceneField = typeof(LevelExit2D).GetField("_nextSceneName", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (nextSceneField != null) nextSceneField.SetValue(exit, "Level_2_1");

                    var titleField = typeof(LevelExit2D).GetField("_levelTitle", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (titleField != null) titleField.SetValue(exit, "¡MUNDO 1 CONQUISTADO!");

                    var msgField = typeof(LevelExit2D).GetField("_victoryMessage", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (msgField != null) msgField.SetValue(exit, "Has derrotado al temible Mono Ladrón Gigante.\nCon el Huevo Verde a salvo en tu espalda, te adentras en las misteriosas Cuevas de Cristal...");
                }

                // Desactivado hasta que el jefe sea derrotado
                portalInstance.SetActive(false);
            }

            // ==========================================
            // ENTIDAD DEL JEFE: MONO LADRÓN GIGANTE
            // ==========================================
            CreateBossEntity(levelRoot.transform, fruitPrefab, portalInstance, shakeChannel, square, circle, unlitMat);

            // ==========================================
            // CONFIGURACIÓN DEL JUGADOR (ALMA)
            // ==========================================
            var alma = GameObject.Find("Alma (Player)");
            if (alma != null)
            {
                alma.transform.position = new Vector3(-8.5f, 1.73f, 0f);

                var pc = alma.GetComponent<PlayerController>();
                if (pc != null)
                {
                    var djField = typeof(PlayerController).GetField("_doubleJumpUnlocked", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (djField != null) djField.SetValue(pc, true);
                    pc.SetCheckpoint(new Vector2(-8.5f, 1.73f));
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

            // ==========================================
            // CONFIGURAR CÁMARA 2D
            // ==========================================
            var cam = UnityEngine.Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = 6f;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = twilightSky;

                var follow = cam.GetComponent<AlmaDino.Features.Camera.Camera2DFollow>();
                if (follow != null)
                {
                    if (alma != null) follow.SetTarget(alma.transform);
                    follow.SetBounds(new Vector2(-12f, -4f), new Vector2(12f, 12f));
                    EditorUtility.SetDirty(follow);
                }
            }

            // ==========================================
            // ILUMINACIÓN 2D URP: ATARDECER CÁLIDO (0.85)
            // ==========================================
            var globalLight = Object.FindAnyObjectByType<Light2D>();
            if (globalLight != null)
            {
                globalLight.lightType = Light2D.LightType.Global;
                globalLight.intensity = 0.85f;
                globalLight.color = new Color(1.0f, 0.85f, 0.65f); // Ámbar de atardecer
                EditorUtility.SetDirty(globalLight);
            }

            // ==========================================
            // UI MÓVIL Y EVENT SYSTEM
            // ==========================================
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

            var eventSystem = Object.FindAnyObjectByType<EventSystem>(FindObjectsInactive.Include);
            if (eventSystem != null)
            {
                var inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
                if (inputModule != null) Object.DestroyImmediate(inputModule);

                var standalone = eventSystem.GetComponent<StandaloneInputModule>();
                if (standalone == null) standalone = eventSystem.gameObject.AddComponent<StandaloneInputModule>();
                EditorUtility.SetDirty(eventSystem);
            }

            // Guardar la escena
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("<color=#FF9800><b>[AlmaDino]</b> ¡Arena del Jefe 1 ('Rey de la Copa — Mono Ladrón Gigante' v3) construida con éxito!</color>");
        }

        private static GameObject EnsureFruitPrefab(Sprite circle, Sprite square, Material unlitMat)
        {
            if (File.Exists(PREFAB_FRUIT))
            {
                AssetDatabase.DeleteAsset(PREFAB_FRUIT);
            }

            var go = new GameObject("RollingFruit_Boss1");

            var rb = go.AddComponent<Rigidbody2D>();
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.gravityScale = 1.6f;
            rb.freezeRotation = true;

            // Colisionador físico sólido para rodar y rebotar en plataformas
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.50f;

            // Trigger exterior para detección instantánea de daño sobre Alma
            var triggerCol = go.AddComponent<CircleCollider2D>();
            triggerCol.radius = 0.58f;
            triggerCol.isTrigger = true;

            var proj = go.AddComponent<RollingFruitProjectile2D>();

            // Visual
            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform);
            visual.transform.localPosition = Vector3.zero;

            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = circle;
            sr.sharedMaterial = unlitMat;
            sr.color = new Color(0.28f, 0.08f, 0.40f); // Púrpura prehistórico oscuro
            visual.transform.localScale = new Vector3(1.1f, 1.1f, 1f);

            // Espinas verdes
            for (int i = 0; i < 4; i++)
            {
                var spine = new GameObject($"Spine_{i}");
                spine.transform.SetParent(visual.transform);
                float angle = i * 90f;
                spine.transform.localPosition = Quaternion.Euler(0, 0, angle) * new Vector3(0.55f, 0f, 0f);
                spine.transform.localRotation = Quaternion.Euler(0, 0, angle + 45f);
                spine.transform.localScale = new Vector3(0.35f, 0.35f, 1f);

                var spineSr = spine.AddComponent<SpriteRenderer>();
                spineSr.sprite = square;
                spineSr.sharedMaterial = unlitMat;
                spineSr.color = new Color(0.46f, 1.0f, 0.01f); // Verde lima venenoso
            }

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, PREFAB_FRUIT);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static void CreateBossEntity(Transform parent, GameObject fruitPrefab, GameObject exitPortal, CameraShakeEventChannelSO shake, Sprite square, Sprite circle, Material material)
        {
            var bossGo = new GameObject("Boss_GiantMonkey");
            bossGo.transform.SetParent(parent);
            bossGo.transform.position = new Vector3(0f, 6.0f, 0f);

            // 1. Cuerpo Principal con Colisión de Peligro Corporal
            var body = new GameObject("Body");
            body.transform.SetParent(bossGo.transform);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(2.4f, 2.6f, 1f);

            var bodySr = body.AddComponent<SpriteRenderer>();
            bodySr.sprite = square;
            bodySr.sharedMaterial = material;
            bodySr.color = new Color(0.24f, 0.15f, 0.14f); // #3E2723 Pelaje marrón oscuro

            // Colisionador de peligro corporal para evitar contacto horizontal
            var bodyCol = body.AddComponent<BoxCollider2D>();
            bodyCol.size = new Vector2(0.95f, 0.95f);
            bodyCol.isTrigger = true;
            var bodyHazard = body.AddComponent<BossBodyHazard2D>();

            // 2. Cabeza
            var head = new GameObject("Head");
            head.transform.SetParent(bossGo.transform);
            head.transform.localPosition = new Vector3(0f, 1.3f, 0f);
            head.transform.localScale = new Vector3(1.7f, 1.5f, 1f);

            var headSr = head.AddComponent<SpriteRenderer>();
            headSr.sprite = square;
            headSr.sharedMaterial = material;
            headSr.color = new Color(0.31f, 0.20f, 0.18f); // #4E342E

            // 3. Melena Plateada / Corona
            var crest = new GameObject("Silver_Crest");
            crest.transform.SetParent(bossGo.transform);
            crest.transform.localPosition = new Vector3(0f, 2.05f, 0f);
            crest.transform.localScale = new Vector3(1.9f, 0.6f, 1f);

            var crestSr = crest.AddComponent<SpriteRenderer>();
            crestSr.sprite = square;
            crestSr.sharedMaterial = material;
            crestSr.color = new Color(0.92f, 0.94f, 0.95f); // #ECEFF1 Melena plateada

            // 4. Ojos Ámbar Amenazantes
            CreateEye(bossGo.transform, new Vector3(-0.45f, 1.5f, 0f), circle, material);
            CreateEye(bossGo.transform, new Vector3(0.45f, 1.5f, 0f), circle, material);

            // 5. Brazos para colgarse
            CreateArm(bossGo.transform, new Vector3(-1.4f, 0.4f, 0f), square, material);
            CreateArm(bossGo.transform, new Vector3(1.4f, 0.4f, 0f), square, material);

            // 6. Punto de Spawn de Proyectiles
            var spawnPt = new GameObject("Projectile_Spawn_Point");
            spawnPt.transform.SetParent(bossGo.transform);
            spawnPt.transform.localPosition = new Vector3(0f, -0.6f, 0f);

            // 7. Head Hurtbox (Caja de pisada en la cabeza vulnerable)
            var hurtboxGo = new GameObject("Head_Hurtbox");
            hurtboxGo.transform.SetParent(bossGo.transform);
            hurtboxGo.transform.localPosition = new Vector3(0f, 2.1f, 0f);

            var boxCol = hurtboxGo.AddComponent<BoxCollider2D>();
            boxCol.isTrigger = true;
            boxCol.size = new Vector2(1.8f, 0.7f);

            var headHurtbox = hurtboxGo.AddComponent<BossHeadHurtbox2D>();

            // 8. Indicador de Aturdimiento / Fatiga (Estrellas giratorias)
            var dizzyGo = new GameObject("Dizzy_Stars_Indicator");
            dizzyGo.transform.SetParent(bossGo.transform);
            dizzyGo.transform.localPosition = new Vector3(0f, 2.7f, 0f);

            for (int s = 0; s < 3; s++)
            {
                var star = new GameObject($"Star_{s}");
                star.transform.SetParent(dizzyGo.transform);
                float angle = s * 120f;
                star.transform.localPosition = Quaternion.Euler(0, 0, angle) * new Vector3(0.8f, 0f, 0f);
                star.transform.localScale = new Vector3(0.35f, 0.35f, 1f);

                var starSr = star.AddComponent<SpriteRenderer>();
                starSr.sprite = circle;
                starSr.sharedMaterial = material;
                starSr.color = new Color(1.0f, 0.84f, 0.0f); // Amarillo dorado
            }
            dizzyGo.SetActive(false);

            // 9. Componente GiantMonkeyBoss2D con parámetros escalonados
            var boss = bossGo.AddComponent<GiantMonkeyBoss2D>();
            boss.Configure(3, new float[] { 3.2f, 2.8f, 2.4f }, exitPortal, shake);

            // Asignar campos privados de referencias por reflection
            SetPrivateField(boss, "_fruitPrefab", fruitPrefab);
            SetPrivateField(boss, "_projectileSpawnPoint", spawnPt.transform);
            SetPrivateField(boss, "_headHurtbox", headHurtbox);
            SetPrivateField(boss, "_bodyHazard", bodyHazard);
            SetPrivateField(boss, "_dizzyIndicator", dizzyGo);
            SetPrivateField(boss, "_bodyRenderer", bodySr);
            SetPrivateField(boss, "_linkedExitPortal", exitPortal);
            SetPrivateField(boss, "_shakeChannel", shake);
        }

        private static void CreateEye(Transform parent, Vector3 localPos, Sprite circle, Material material)
        {
            var eye = new GameObject("Eye");
            eye.transform.SetParent(parent);
            eye.transform.localPosition = localPos;
            eye.transform.localScale = new Vector3(0.35f, 0.35f, 1f);

            var sr = eye.AddComponent<SpriteRenderer>();
            sr.sprite = circle;
            sr.sharedMaterial = material;
            sr.color = new Color(1.0f, 0.60f, 0.0f); // #FF9800 Ámbar amenazante
        }

        private static void CreateArm(Transform parent, Vector3 localPos, Sprite square, Material material)
        {
            var arm = new GameObject("Arm");
            arm.transform.SetParent(parent);
            arm.transform.localPosition = localPos;
            arm.transform.localScale = new Vector3(0.7f, 2.2f, 1f);

            var sr = arm.AddComponent<SpriteRenderer>();
            sr.sprite = square;
            sr.sharedMaterial = material;
            sr.color = new Color(0.24f, 0.15f, 0.14f);
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

        private static void CreateOneWayPlatform(string name, Transform parent, Vector3 position, Vector2 size, Color color, Sprite sprite, Material material)
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
            col.usedByEffector = true;

            var effector = go.AddComponent<PlatformEffector2D>();
            effector.useOneWay = true;
            effector.useOneWayGrouping = true;
            effector.surfaceArc = 160f;
        }

        private static void CreateVisualDecoration(string name, Transform parent, Vector3 position, Vector2 size, Color color, Sprite sprite, Material material)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sharedMaterial = material;
            sr.color = color;
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

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }

        [MenuItem("Alma/📂 Cargar Arena Jefe 1")]
        public static void LoadBoss1Scene()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
            }
        }
    }
}
#endif
