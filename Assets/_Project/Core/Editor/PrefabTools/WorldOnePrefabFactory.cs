using System;
using UnityEditor;
using UnityEngine;

namespace AlmaDino.Core.Editor
{
    internal static class WorldOnePrefabFactory
    {
        internal static GameObject CreateGround(string name, Transform parent, Vector3 position,
            Vector2 size, Color color, Sprite sprite, Material material)
        {
            const string path = LevelPrefabLibrary.Root + "/Resources/Platform_Solid_Universal.prefab";
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null) throw new InvalidOperationException("Build the prefab catalog before building World 1.");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent);
            instance.name = name;
            instance.transform.position = position;
            instance.transform.localScale = new Vector3(size.x, size.y, 1f);
            var renderer = instance.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = material;
            renderer.color = color;
            var collider = instance.GetComponent<BoxCollider2D>();
            collider.size = Vector2.one;
            PrefabUtility.RecordPrefabInstancePropertyModifications(instance.transform);
            PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
            PrefabUtility.RecordPrefabInstancePropertyModifications(collider);
            return instance;
        }
    }
}
