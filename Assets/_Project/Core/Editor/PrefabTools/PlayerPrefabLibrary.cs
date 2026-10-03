using System;
using System.Linq;
using AlmaDino.Features.Player.Components;
using AlmaDino.Features.Player.Controllers;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlmaDino.Core.Editor
{
    public static class PlayerPrefabLibrary
    {
        public const string Path = "Assets/_Project/Prefabs/Player/Alma/Player_Alma.prefab";

        [MenuItem("Alma/Prefabs/Create Alma prefab if missing")]
        public static void Create()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Path) != null) { Validate(); return; }
            var scene = EditorSceneManager.OpenPreviewScene("Assets/Scenes/World_1_Jungle/Level_1_1.unity");
            GameObject clone = null;
            try
            {
                var source = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<PlayerController>(true)).Single();
                clone = Object.Instantiate(source.gameObject);
                clone.name = "Player_Alma";
                clone.transform.SetParent(null);
                clone.transform.position = Vector3.zero;
                if (PrefabUtility.IsPartOfPrefabInstance(clone))
                    PrefabUtility.UnpackPrefabInstance(clone, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                var animator = clone.GetComponent<PlayerSpriteAnimator>();
                foreach (var renderer in clone.GetComponentsInChildren<SpriteRenderer>(true))
                {
                    // Animated visuals already expose their SpriteRenderer and frame arrays.
                    if (renderer == animator.SpriteRenderer) continue;
                    var slot = renderer.GetComponent<AlmaDino.Shared.Visuals.PrefabSprite2D>();
                    if (slot == null) slot = renderer.gameObject.AddComponent<AlmaDino.Shared.Visuals.PrefabSprite2D>();
                    slot.Initialize();
                }
                if (PrefabUtility.SaveAsPrefabAsset(clone, Path) == null)
                    throw new InvalidOperationException("Could not save Alma prefab.");
            }
            finally
            {
                if (clone != null) Object.DestroyImmediate(clone);
                EditorSceneManager.ClosePreviewScene(scene);
            }
            Validate();
            PrefabScaffoldingValidation.Validate();
        }

        [MenuItem("Alma/Prefabs/Validate Alma prefab")]
        public static void Validate()
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(Path);
            if (asset == null) throw new InvalidOperationException("Alma prefab is missing.");
            var player = asset.GetComponent<PlayerController>();
            var animation = asset.GetComponent<PlayerSpriteAnimator>();
            if (player == null || player.Config == null || asset.GetComponent<PlayerInputReader>() == null
                || asset.GetComponent<GroundDetector2D>() == null || asset.GetComponent<Rigidbody2D>() == null
                || asset.GetComponent<Collider2D>() == null || animation == null || animation.SpriteRenderer == null)
                throw new InvalidOperationException("Alma requires controller, physics, input, ground detector and animation references.");
            var input = new SerializedObject(asset.GetComponent<PlayerInputReader>());
            if (input.FindProperty("_inputActions").objectReferenceValue == null)
                throw new InvalidOperationException("Alma input asset is missing.");
            foreach (var frames in new[] { animation.IdleFrames, animation.RunFrames, animation.JumpFrames, animation.FallFrames })
                if (frames == null || frames.Length == 0 || frames.Any(f => f == null))
                    throw new InvalidOperationException("Alma has missing animation frames.");
            foreach (var node in asset.GetComponentsInChildren<Transform>(true))
                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(node.gameObject) != 0)
                    throw new InvalidOperationException("Alma has a missing script.");
            if (asset.GetComponentsInChildren<AudioSource>(true).Length != 0 || asset.GetComponentsInChildren<Camera>(true).Length != 0)
                throw new InvalidOperationException("Alma prefab should contain neither audio nor a camera.");
            Debug.Log("PLAYER_PREFAB_OK: configured input, physics and Idle/Run/Jump/Fall frames.");
        }
    }
}
