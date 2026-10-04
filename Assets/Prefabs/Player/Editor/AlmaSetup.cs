using System;
using System.IO;
using System.Linq;
using AlmaGame.Player;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace AlmaGame.Editor
{
    public static class AlmaSetup
    {
        private const string Root = "Assets/Prefabs/Player";
        private const string ScenePath = "Assets/Scenes/World_01/Level_1_1.unity";
        private const string ControllerPath = Root + "/Animations/Idle/Player_Idle_Sheet_0.controller";

        [MenuItem("Alma/Configure player and practice scene")]
        public static void Configure()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Directory.CreateDirectory(Root + "/Configuration");
            AssetDatabase.Refresh();
            var settings = GetOrCreate<AlmaMovementSettings>(Root + "/Configuration/AlmaMovement.asset");
            var friction = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(Root + "/Configuration/AlmaFrictionless.physicsMaterial2D");
            if (friction == null)
            {
                friction = new PhysicsMaterial2D("AlmaFrictionless") { friction = 0f, bounciness = 0f };
                AssetDatabase.CreateAsset(friction, Root + "/Configuration/AlmaFrictionless.physicsMaterial2D");
            }
            var idle = SetClip(Root + "/Animations/Idle/Player_Idle_Animation.anim", Frames("Idle"), 8f, true);
            var run = SetClip(Root + "/Animations/Run/Player_Run_Animation.anim", Frames("Run"), 12f, true);
            var jumpFrames = Frames("Jump");
            var rise = SetClip(Root + "/Animations/Jump/Alma_Rise.anim", jumpFrames.Skip(3).Take(2).ToArray(), 12f, false);
            var fall = SetClip(Root + "/Animations/Jump/Alma_Fall.anim", jumpFrames.Skip(5).Take(1).ToArray(), 12f, false);
            var controller = ConfigureAnimator(idle, run, rise, fall);

            var prefab = PrefabUtility.LoadPrefabContents(Root + "/Alma.prefab");
            try
            {
                foreach (var oldCollider in prefab.GetComponents<PolygonCollider2D>()) Object.DestroyImmediate(oldCollider);
                var capsule = GetOrAdd<CapsuleCollider2D>(prefab);
                capsule.size = new Vector2(1.2f, 2.42f);
                capsule.offset = new Vector2(0f, -0.02f);
                capsule.sharedMaterial = friction;
                var body = GetOrAdd<Rigidbody2D>(prefab);
                body.bodyType = RigidbodyType2D.Dynamic;
                body.gravityScale = settings.GravityScale;
                body.constraints = RigidbodyConstraints2D.FreezeRotation;
                body.interpolation = RigidbodyInterpolation2D.Interpolate;
                body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                var motor = GetOrAdd<AlmaMotor2D>(prefab);
                var serialized = new SerializedObject(motor);
                serialized.FindProperty("_settings").objectReferenceValue = settings;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                GetOrAdd<AlmaInput>(prefab);
                GetOrAdd<AlmaAnimation>(prefab);
                prefab.GetComponent<Animator>().runtimeAnimatorController = controller;
                prefab.GetComponent<Animator>().applyRootMotion = false;
                PrefabUtility.SaveAsPrefabAsset(prefab, Root + "/Alma.prefab");
            }
            finally { PrefabUtility.UnloadPrefabContents(prefab); }

            ConfigurePracticeScene();
            AssetDatabase.SaveAssets();
            Debug.Log("ALMA_SETUP_OK: configured prefab, four animation states and practice scene.");
        }

        private static AnimatorController ConfigureAnimator(AnimationClip idle, AnimationClip run, AnimationClip rise, AnimationClip fall)
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            var machine = controller.layers[0].stateMachine;
            foreach (var transition in machine.anyStateTransitions) machine.RemoveAnyStateTransition(transition);
            foreach (var state in machine.states) machine.RemoveState(state.state);
            controller.parameters = Array.Empty<AnimatorControllerParameter>();
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("VerticalSpeed", AnimatorControllerParameterType.Float);
            controller.AddParameter("RunRate", AnimatorControllerParameterType.Float);
            var idleState = AddState(machine, "Idle", idle, new Vector3(200, 50));
            var runState = AddState(machine, "Run", run, new Vector3(420, 50));
            var riseState = AddState(machine, "Jump", rise, new Vector3(200, 180));
            var fallState = AddState(machine, "Fall", fall, new Vector3(420, 180));
            runState.speedParameter = "RunRate";
            runState.speedParameterActive = true;
            machine.defaultState = idleState;
            AddTransition(machine, idleState, true, AnimatorConditionMode.Less, 0.2f, "Speed");
            AddTransition(machine, runState, true, AnimatorConditionMode.Greater, 0.2f, "Speed");
            AddTransition(machine, riseState, false, AnimatorConditionMode.Greater, 0f, "VerticalSpeed");
            AddTransition(machine, fallState, false, AnimatorConditionMode.Less, 0.001f, "VerticalSpeed");
            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static AnimatorState AddState(AnimatorStateMachine machine, string name, AnimationClip clip, Vector3 position)
        {
            var state = machine.AddState(name, position);
            state.motion = clip;
            state.writeDefaultValues = false;
            return state;
        }

        private static void AddTransition(AnimatorStateMachine machine, AnimatorState state, bool grounded,
            AnimatorConditionMode mode, float threshold, string parameter)
        {
            var transition = machine.AddAnyStateTransition(state);
            transition.hasExitTime = false;
            // Sprite frames cannot crossfade; immediate state changes keep controls responsive.
            transition.duration = 0f;
            transition.canTransitionToSelf = false;
            transition.AddCondition(grounded ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, "Grounded");
            transition.AddCondition(mode, threshold, parameter);
        }

        private static Sprite[] Frames(string animation)
        {
            var frames = AssetDatabase.LoadAllAssetsAtPath($"{Root}/Animations/{animation}/Player_{animation}_Sheet.png")
                .OfType<Sprite>().OrderBy(sprite => int.Parse(sprite.name.Substring(sprite.name.LastIndexOf('_') + 1))).ToArray();
            if (frames.Length != 8) throw new InvalidOperationException($"Expected eight {animation} sprites, got {frames.Length}.");
            return frames;
        }

        private static AnimationClip SetClip(string path, Sprite[] frames, float fps, bool loop)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, path);
            }
            clip.frameRate = fps;
            var keys = frames.Select((sprite, i) => new ObjectReferenceKeyframe { time = i / fps, value = sprite }).ToArray();
            AnimationUtility.SetObjectReferenceCurve(clip, new EditorCurveBinding
            { path = "", type = typeof(SpriteRenderer), propertyName = "m_Sprite" }, keys);
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            settings.startTime = 0f;
            settings.stopTime = frames.Length / fps;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static void ConfigurePracticeScene()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var player = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<AlmaMotor2D>()).Single();
            var camera = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Camera>()).First();
            var follow = GetOrAdd<AlmaCameraFollow>(camera.gameObject);
            follow.Target = player;
            camera.orthographic = true;
            camera.orthographicSize = 6f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.77f, 0.86f, 0.89f);
            foreach (var listener in camera.GetComponents<AudioListener>()) Object.DestroyImmediate(listener);
            var existing = scene.GetRootGameObjects().FirstOrDefault(root => root.name == "Alma_Practice");
            if (existing == null)
            {
                var practice = new GameObject("Alma_Practice");
                var material = new Material(Shader.Find("Unlit/Color")) { color = new Color(0.26f, 0.36f, 0.30f) };
                AssetDatabase.CreateAsset(material, Root + "/Configuration/PracticeGround.mat");
                Ground(practice.transform, "Ground", new Vector2(0f, -2f), new Vector2(40f, 1f), material);
                Ground(practice.transform, "Step_1", new Vector2(6f, -0.8f), new Vector2(3f, 0.5f), material);
                Ground(practice.transform, "Step_2", new Vector2(10f, 0.2f), new Vector2(3f, 0.5f), material);
                Ground(practice.transform, "Step_Left", new Vector2(-7f, -0.8f), new Vector2(3f, 0.5f), material);
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            var scenes = EditorBuildSettings.scenes.ToList();
            if (!scenes.Any(entry => entry.path == ScenePath)) scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void Ground(Transform parent, string name, Vector2 position, Vector2 size, Material material)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = name;
            ground.transform.SetParent(parent);
            ground.transform.position = position;
            ground.transform.localScale = new Vector3(size.x, size.y, 1f);
            Object.DestroyImmediate(ground.GetComponent<BoxCollider>());
            ground.AddComponent<BoxCollider2D>();
            ground.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        private static T GetOrAdd<T>(GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }

        private static T GetOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }
}
