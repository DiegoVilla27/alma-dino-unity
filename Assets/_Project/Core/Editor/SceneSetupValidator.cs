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
    [InitializeOnLoad]
    public static class SceneSetupValidator
    {
        static SceneSetupValidator()
        {
            EditorApplication.delayCall += EnsureSceneVisualsAndReload;
        }

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
                currentScene = EditorSceneManager.OpenScene("Assets/Scenes/Level_1_1.unity", OpenSceneMode.Single);
            }

            // 4. Find all SpriteRenderers in scene and assign unlit material and valid sprites
            var renderers = Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
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
            }

            // 6. Setup Mobile UI Canvas
            var joystickBase = GameObject.Find("VirtualJoystick_Base");
            if (joystickBase != null)
            {
                var vj = joystickBase.GetComponent<VirtualJoystick>();
                if (vj == null) vj = joystickBase.AddComponent<VirtualJoystick>();
                EditorUtility.SetDirty(joystickBase);
            }

            SetupButton("Button_Jump", VirtualButtonType.Jump);
            SetupButton("Button_Dash", VirtualButtonType.Dash);
            SetupButton("Button_Pound", VirtualButtonType.GroundPound);
            SetupButton("Button_Roar", VirtualButtonType.Roar);

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

            // 9. Save scene
            if (currentScene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(currentScene);
                EditorSceneManager.SaveScene(currentScene);
            }

            Debug.Log($"<color=#00FF88><b>[AlmaDino]</b> ¡Escena configurada y reparada con éxito! {updatedCount} SpriteRenderers Unlit, Controles Táctiles y EventSystem vinculados.</color>");
        }

        private static void SetupButton(string gameObjectName, VirtualButtonType buttonType)
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
