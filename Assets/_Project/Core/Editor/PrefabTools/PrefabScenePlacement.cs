using System.Linq;
using AlmaDino.Core.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AlmaDino.Core.Editor
{
    public static class PrefabScenePlacement
    {
        [MenuItem("Alma/Prefabs/Place selected prefab and bind player-camera")]
        private static void Place()
        {
            var asset = Selection.activeGameObject;
            if (asset == null || !EditorUtility.IsPersistent(asset))
            {
                Debug.LogWarning("Select a prefab asset in the Project window first.");
                return;
            }
            var scene = SceneManager.GetActiveScene();
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, scene);
            Undo.RegisterCreatedObjectUndo(instance, "Place level prefab");
            var level = LevelPrefabLibrary.LevelRoot(scene);
            if (level != null) Undo.SetTransformParent(instance.transform, level.transform, "Parent level prefab");
            Bind(instance);
            Selection.activeGameObject = instance;
            SceneView.lastActiveSceneView?.FrameSelected();
        }

        [MenuItem("Alma/Prefabs/Bind player-camera on selected instance")]
        private static void BindSelection()
        {
            foreach (var root in Selection.gameObjects.Where(g => !EditorUtility.IsPersistent(g))) Bind(root);
        }

        public static void Bind(GameObject instance)
        {
            var sceneRoots = instance.scene.GetRootGameObjects();
            var player = sceneRoots.SelectMany(g => g.GetComponentsInChildren<MonoBehaviour>(true))
                .FirstOrDefault(c => c is IPlayerRespawnable);
            var camera = sceneRoots.SelectMany(g => g.GetComponentsInChildren<Camera>(true))
                .FirstOrDefault(c => c.CompareTag("MainCamera"));
            foreach (var behaviour in instance.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour == null) continue;
                var serialized = new SerializedObject(behaviour);
                AssignIfEmpty(serialized, "_playerSource", player);
                AssignIfEmpty(serialized, "_camera", camera != null ? camera.transform : null);
                serialized.ApplyModifiedProperties();
            }
        }

        private static void AssignIfEmpty(SerializedObject owner, string name, Object target)
        {
            var property = owner.FindProperty(name);
            if (property != null && property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue == null)
                property.objectReferenceValue = target;
        }
    }
}
