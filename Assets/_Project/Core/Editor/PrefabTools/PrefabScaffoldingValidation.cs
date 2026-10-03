using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AlmaDino.Shared.Visuals;
using UnityEditor;
using UnityEngine;

namespace AlmaDino.Core.Editor
{
    public static class PrefabScaffoldingValidation
    {
        private const string IndexPath = "Assets/_Project/Core/Editor/PrefabTools/GameplayPrefabIndex.json";
        private static readonly string[] Categories = { "Enemies", "Traps", "Projectiles", "Bosses", "Resources", "Narrative", "Player" };
        [Serializable] private sealed class Item { public string Name; public string Path; }
        [Serializable] private sealed class Index { public Item[] Items; }

        [MenuItem("Alma/Prefabs/Validate named inventory and folders")]
        public static void Validate()
        {
            var index = JsonUtility.FromJson<Index>(File.ReadAllText(IndexPath));
            var names = new HashSet<string>();
            foreach (var item in index.Items)
            {
                if (!names.Add(item.Name)) throw new InvalidOperationException("Duplicate inventory name: " + item.Name);
                if (Path.GetFileNameWithoutExtension(item.Path) != item.Name)
                    throw new InvalidOperationException("Inventory filename differs: " + item.Path);
                if (AssetDatabase.LoadAssetAtPath<GameObject>(item.Path) == null)
                    throw new InvalidOperationException("Missing named inventory prefab: " + item.Path);
            }
            var paths = Directory.GetFiles(LevelPrefabLibrary.Root, "*.prefab", SearchOption.AllDirectories);
            foreach (var path in paths)
            {
                var relative = path.Substring(LevelPrefabLibrary.Root.Length + 1).Replace('\\', '/');
                if (!Categories.Contains(relative.Split('/')[0]))
                    throw new InvalidOperationException("Prefab outside canonical folders: " + path);
                if (!Regex.IsMatch(Path.GetFileNameWithoutExtension(path), "^[A-Za-z0-9_]+$"))
                    throw new InvalidOperationException("Invalid prefab name: " + path);
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                foreach (var node in asset.GetComponentsInChildren<Transform>(true))
                {
                    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(node.gameObject) != 0)
                        throw new InvalidOperationException("Missing script: " + path + "/" + node.name);
                    if (node.GetComponent<SpriteRenderer>() != null && node.GetComponent<PrefabSprite2D>() == null && !(asset.GetComponent<AlmaDino.Features.Player.Components.PlayerSpriteAnimator>() is { } animator && animator.SpriteRenderer == node.GetComponent<SpriteRenderer>()))
                        throw new InvalidOperationException("Missing sprite slot: " + path + "/" + node.name);
                }
            }
            Debug.Log($"SCAFFOLD_VALIDATION_OK: {index.Items.Length} named inventory prefabs; {paths.Length} total pieces/variants/assemblies in the canonical folders.");
        }
    }
}
