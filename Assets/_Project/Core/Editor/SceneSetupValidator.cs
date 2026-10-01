#if UNITY_EDITOR
using AlmaDino.Features.MobileUI.Controllers;
using AlmaDino.Features.Player.Components;
using AlmaDino.Features.Player.Controllers;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace AlmaDino.Core.Editor
{
    public static class SceneSetupValidator
    {
        [MenuItem("Alma/📂 Cargar Nivel 1-1")]
        public static void OpenLevel1_1()
        {
            if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
            EditorSceneManager.OpenScene("Assets/Scenes/World_1_Jungle/Level_1_1.unity", OpenSceneMode.Single);
            Debug.Log("<color=#00FF88><b>[AlmaDino]</b> ¡Nivel 1-1 cargado exitosamente!</color>");
        }

        [MenuItem("Alma/📂 Cargar Nivel 1-2")]
        public static void OpenLevel1_2()
        {
            if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
            EditorSceneManager.OpenScene("Assets/Scenes/World_1_Jungle/Level_1_2.unity", OpenSceneMode.Single);
            Debug.Log("<color=#00FF88><b>[AlmaDino]</b> ¡Nivel 1-2 cargado exitosamente!</color>");
        }

        [MenuItem("Alma/📂 Cargar Nivel 1-3")]
        public static void OpenLevel1_3()
        {
            if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
            EditorSceneManager.OpenScene("Assets/Scenes/World_1_Jungle/Level_1_3.unity", OpenSceneMode.Single);
            Debug.Log("<color=#00FF88><b>[AlmaDino]</b> ¡Nivel 1-3 cargado exitosamente!</color>");
        }

        [MenuItem("Alma/📂 Cargar Nivel 1-4")]
        public static void OpenLevel1_4()
        {
            if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
            EditorSceneManager.OpenScene("Assets/Scenes/World_1_Jungle/Level_1_4.unity", OpenSceneMode.Single);
            Debug.Log("<color=#00FF88><b>[AlmaDino]</b> ¡Nivel 1-4 cargado exitosamente!</color>");
        }

        [MenuItem("Alma/📂 Cargar Arena Jefe 1")]
        public static void OpenBoss1()
        {
            if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
            EditorSceneManager.OpenScene("Assets/Scenes/World_1_Jungle/Boss_1.unity", OpenSceneMode.Single);
            Debug.Log("<color=#FF9800><b>[AlmaDino]</b> ¡Arena del Jefe 1 cargada exitosamente!</color>");
        }

        [MenuItem("Alma/📂 Cargar Nivel 2-1")]
        public static void OpenLevel2_1()
        {
            if (EditorApplication.isPlaying) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(LevelBuilder2_1.ScenePath, OpenSceneMode.Single);
        }

        [MenuItem("Alma/📂 Cargar Nivel 2-2")]
        public static void OpenLevel2_2()
        {
            if (EditorApplication.isPlaying) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(LevelBuilder2_2.ScenePath, OpenSceneMode.Single);
        }

        [MenuItem("Alma/📂 Cargar Nivel 2-3")]
        public static void OpenLevel2_3()
        {
            if (EditorApplication.isPlaying) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(LevelBuilder2_3.ScenePath, OpenSceneMode.Single);
        }

        [MenuItem("Alma/🔄 Resetear Progresión de Partida")]
        public static void ResetGameProgression()
        {
            AlmaDino.Core.Progression.GameProgression.ResetProgression();
            EditorUtility.DisplayDialog("Alma: Mother's Roar", "¡Progresión de habilidades reseteada a estado inicial!\nAhora puedes probar el tutorial del Nivel 1-1 desde cero.", "Aceptar");
        }

        // Se ejecuta exclusivamente desde el menú manual para evitar sobreescritura accidental durante recargas de dominio
        [MenuItem("Alma/🛠️ Reparar Escena y Visuales")]
        public static void EnsureSceneVisualsAndReload()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("[AlmaDino] Unity está en modo Play. Detén el Play mode para aplicar la reparación de la escena.");
                return;
            }

            // 1. Force re-import sprites to ensure Sprite sub-assets exist with valid GUIDs
            AssetDatabase.ImportAsset("Assets/Sprites/Square.png", ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset("Assets/Sprites/Circle.png", ImportAssetOptions.ForceUpdate);

            Sprite squareSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Square.png");
            Sprite circleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Circle.png");

            // 2. Obtain Sprite-Unlit material
            Material unlitMat = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat");
            if (unlitMat == null)
            {
                Shader unlitShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit");
                if (unlitShader == null) unlitShader = Shader.Find("Sprites/Default");
                if (unlitShader != null)
                {
                    unlitMat = new Material(unlitShader) { name = "Runtime_Sprite_Unlit" };
                }
            }

            // 3. Keep current scene active if valid, otherwise open Level_1_1
            var currentScene = EditorSceneManager.GetActiveScene();
            if (!currentScene.IsValid() || string.IsNullOrEmpty(currentScene.path))
            {
                currentScene = EditorSceneManager.OpenScene("Assets/Scenes/World_1_Jungle/Level_1_1.unity", OpenSceneMode.Single);
            }

            // 4. Find all SpriteRenderers in scene and assign unlit material and valid sprites
            var renderers = Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include);
            int updatedCount = 0;

            foreach (var sr in renderers)
            {
                if (unlitMat != null)
                {
                    sr.sharedMaterial = unlitMat;
                }

                if (sr.sprite == null)
                {
                    if (sr.gameObject.name.Contains("Eye") || sr.gameObject.name.Contains("Pupil") || sr.gameObject.name.Contains("Circle"))
                    {
                        if (circleSprite != null) sr.sprite = circleSprite;
                    }
                    else
                    {
                        if (squareSprite != null) sr.sprite = squareSprite;
                    }
                }

                EditorUtility.SetDirty(sr);
                updatedCount++;
            }

            // 5. Setup Alma (Player)
            var alma = GameObject.Find("Alma (Player)");
            if (alma != null)
            {
                var inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/Settings/InputSystem_Actions.inputactions");
                var reader = alma.GetComponent<PlayerInputReader>();
                if (reader != null && inputAsset != null)
                {
                    reader.SetInputAsset(inputAsset);
                    EditorUtility.SetDirty(reader);
                }

                var ground = alma.GetComponent<GroundDetector2D>();
                if (ground != null)
                {
                    EditorUtility.SetDirty(ground);
                }

                var rb = alma.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.bodyType = RigidbodyType2D.Dynamic;
                    rb.simulated = true;
                    rb.linearDamping = 0f;
                    rb.angularDamping = 0.05f;
                    rb.gravityScale = 2.2f;
                    rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                    EditorUtility.SetDirty(rb);
                }

                // Configurar Sprite y Animador de Alma
                SetupAlmaVisuals(alma);
            }

            // 6. Setup Mobile UI Canvas
            var joystickBase = GameObject.Find("VirtualJoystick_Base");
            if (joystickBase != null)
            {
                var vj = joystickBase.GetComponent<VirtualJoystick>();
                if (vj == null) vj = joystickBase.AddComponent<VirtualJoystick>();
                EditorUtility.SetDirty(joystickBase);
            }

            SetupTouchControls();

            // 7. Setup EventSystem with StandaloneInputModule (Universal & 100% stable)
            var eventSystem = Object.FindAnyObjectByType<EventSystem>(FindObjectsInactive.Include);
            if (eventSystem != null)
            {
                var inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
                if (inputModule != null)
                {
                    Object.DestroyImmediate(inputModule);
                }

                var standalone = eventSystem.GetComponent<StandaloneInputModule>();
                if (standalone == null)
                {
                    standalone = eventSystem.gameObject.AddComponent<StandaloneInputModule>();
                }
                EditorUtility.SetDirty(eventSystem);
            }

            // 8. Ensure Main Camera configuration
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = 6f;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.12f, 0.16f, 0.24f, 1f); // Deep twilight background
                cam.cullingMask = ~0; // Everything
                
                if (alma != null)
                {
                    Vector3 almaPos = alma.transform.position;
                    cam.transform.position = new Vector3(almaPos.x, almaPos.y + 1.5f, -10f);
                }
                
                EditorUtility.SetDirty(cam);
            }

            // 9. Save scene only if valid and objects were found (safety check against saving empty scenes)
            if (currentScene.IsValid() && alma != null)
            {
                EditorSceneManager.MarkSceneDirty(currentScene);
                EditorSceneManager.SaveScene(currentScene);
            }

            Debug.Log($"<color=#00FF88><b>[AlmaDino]</b> ¡Escena configurada y reparada con éxito! {updatedCount} SpriteRenderers Unlit, Controles Táctiles y EventSystem vinculados.</color>");
        }

        public static void SetupTouchControls()
        {
            SetupButton("Button_Jump", VirtualButtonType.Jump);
            SetupButton("Button_Dash", VirtualButtonType.Dash);
            SetupButton("Button_GroundPound", VirtualButtonType.GroundPound);
            SetupButton("Button_Pound", VirtualButtonType.GroundPound);
            SetupButton("Button_Roar", VirtualButtonType.Roar);
        }

        private static void SetupButton(string gameObjectName, VirtualButtonType buttonType)
        {
            GameObject btnGo = null;
            foreach (var transform in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (transform.name != gameObjectName) continue;
                btnGo = transform.gameObject;
                break;
            }
            if (btnGo != null)
            {
                var vb = btnGo.GetComponent<VirtualTouchButton>();
                if (vb == null) vb = btnGo.AddComponent<VirtualTouchButton>();
                vb.ButtonType = buttonType;
                var legacy = btnGo.GetComponent<UnityEngine.InputSystem.OnScreen.OnScreenButton>();
                if (legacy != null) Object.DestroyImmediate(legacy);
                EditorUtility.SetDirty(vb);
                EditorUtility.SetDirty(btnGo);
            }
        }

        [MenuItem("Alma/🦖 Configurar Sprites de Alma (Escena Actual)")]
        public static void SetupAlmaInCurrentScene()
        {
            var alma = GameObject.Find("Alma (Player)");
            if (alma != null)
            {
                SetupAlmaVisuals(alma);
                var activeScene = EditorSceneManager.GetActiveScene();
                EditorSceneManager.MarkSceneDirty(activeScene);
                EditorSceneManager.SaveScene(activeScene);
                Debug.Log("<color=#00FF88><b>[AlmaDino]</b> ¡Sprites y animador Idle de Alma configurados y guardados en la escena actual!</color>");
            }
            else
            {
                Debug.LogWarning("[AlmaDino] No se encontró el objeto 'Alma (Player)' en la escena actual.");
            }
        }

        [MenuItem("Alma/🦖 Configurar Sprites de Alma en Todas las Escenas")]
        public static void SetupAlmaInAllScenes()
        {
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
            string currentPath = EditorSceneManager.GetActiveScene().path;

            foreach (var guid in sceneGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                var alma = GameObject.Find("Alma (Player)");
                if (alma != null)
                {
                    SetupAlmaVisuals(alma);
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
            }

            if (!string.IsNullOrEmpty(currentPath))
            {
                EditorSceneManager.OpenScene(currentPath, OpenSceneMode.Single);
            }
            Debug.Log("<color=#00FF88><b>[AlmaDino]</b> ¡Todas las escenas actualizadas con los sprites animados de Alma!</color>");
        }

        private static Sprite LoadAlmaFrame(string path)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) return sprite;

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
                throw new System.InvalidOperationException("No se pudo cargar el frame de Alma: " + path);
            return sprite;
        }

        public static void SetupAlmaVisuals(GameObject alma)
        {
            if (alma == null) return;

            // 1. Obtener frames de Idle
            var idleSprites = new Sprite[10];
            for (int i = 1; i <= 10; i++)
            {
                string path = $"Assets/Art/Sprites/Characters/Alma/Idle/Alma_Idle_{i:D2}.png";
                idleSprites[i - 1] = LoadAlmaFrame(path);
            }

            // 1b. Obtener frames de Run
            var runSprites = new Sprite[8];
            for (int i = 1; i <= 8; i++)
            {
                string path = $"Assets/Art/Sprites/Characters/Alma/Run/Alma_Run_{i:D2}.png";
                runSprites[i - 1] = LoadAlmaFrame(path);
            }

            // 1c. Obtener frames de Jump
            var jumpSprites = new Sprite[12];
            for (int i = 1; i <= 12; i++)
            {
                string path = $"Assets/Art/Sprites/Characters/Alma/Jump/Alma_Jump_{i:D2}.png";
                jumpSprites[i - 1] = LoadAlmaFrame(path);
            }

            // 1d. Obtener frames de Fall (frames 8 y 9 del set de salto)
            var fallSprites = new Sprite[] { jumpSprites[7], jumpSprites[8] };

            // 2. Configurar SpriteRenderer en Visual
            var visualTr = alma.transform.Find("Visual");
            SpriteRenderer sr = null;
            if (visualTr != null)
            {
                sr = visualTr.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = Color.white;
                    sr.drawMode = SpriteDrawMode.Simple;
                    if (idleSprites[0] != null)
                    {
                        sr.sprite = idleSprites[0];
                    }
                    sr.sortingOrder = 5;
                    EditorUtility.SetDirty(sr);
                }

                // Ocultar cubos/ojos de prototipo
                for (int i = visualTr.childCount - 1; i >= 0; i--)
                {
                    var child = visualTr.GetChild(i);
                    string cName = child.name.ToLowerInvariant();
                    if (cName.Contains("eye") || cName.Contains("pupil") || cName.Contains("placeholder"))
                    {
                        child.gameObject.SetActive(false);
                        EditorUtility.SetDirty(child.gameObject);
                    }
                }
            }
            else
            {
                sr = alma.GetComponentInChildren<SpriteRenderer>();
            }

            // 3. Añadir o actualizar PlayerSpriteAnimator
            var animator = alma.GetComponent<PlayerSpriteAnimator>();
            if (animator == null)
            {
                animator = alma.AddComponent<PlayerSpriteAnimator>();
            }

            if (animator != null)
            {
                animator.SetSpriteRenderer(sr);
                animator.SetIdleFrames(idleSprites, 8f);
                animator.SetRunFrames(runSprites, 10f);
                animator.SetJumpFrames(jumpSprites, 12f);
                animator.SetFallFrames(fallSprites, 10f);
                EditorUtility.SetDirty(animator);
            }

            EditorUtility.SetDirty(alma);
        }
    }
}
#endif
