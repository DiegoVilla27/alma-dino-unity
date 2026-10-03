using System;
using System.Collections.Generic;
using System.Linq;
using AlmaDino.Shared.Visuals;
using UnityEditor;
using UnityEngine;

namespace AlmaDino.Core.Editor
{
    internal sealed class PrefabMigrationSnapshot
    {
        private readonly Dictionary<Component, string> _components = new();
        private readonly Dictionary<GameObject, (string name, bool active, int layer)> _objects = new();

        internal static PrefabMigrationSnapshot Capture(GameObject level)
        {
            var snapshot = new PrefabMigrationSnapshot();
            foreach (var transform in level.GetComponentsInChildren<Transform>(true))
            {
                var go = transform.gameObject;
                snapshot._objects.Add(go, (go.name, go.activeSelf, go.layer));
                foreach (var component in go.GetComponents<Component>())
                    if (component != null && component is not PrefabSprite2D)
                        snapshot._components.Add(component, State(component));
            }
            return snapshot;
        }

        internal void AssertUnchanged(GameObject level)
        {
            foreach (var pair in _objects)
                if (pair.Key == null || (pair.Key.name, pair.Key.activeSelf, pair.Key.layer) != pair.Value)
                    throw new InvalidOperationException($"Prefab conversion changed object: before={pair.Value}, after={(pair.Key == null ? "DESTROYED" : $"{pair.Key.name}, {pair.Key.activeSelf}, {pair.Key.layer}")}");
            foreach (var pair in _components)
            {
                if (pair.Key == null) throw new InvalidOperationException("Prefab conversion destroyed an original component.");
                string after = State(pair.Key);
                if (pair.Value != after)
                    throw new InvalidOperationException($"Prefab conversion changed {pair.Key.name}/{pair.Key.GetType().Name}:\nBefore: {pair.Value}\nAfter: {after}");
            }
            int count = level.GetComponentsInChildren<Component>(true).Count(c => c != null && c is not PrefabSprite2D);
            if (count != _components.Count) throw new InvalidOperationException("Prefab conversion added gameplay components in " + level.name + ": " + string.Join(", ", level.GetComponentsInChildren<Component>(true).Where(c => c != null && c is not PrefabSprite2D && !_components.ContainsKey(c)).Select(c => c.name + "/" + c.GetType().Name)));
        }

        private static string State(Component component)
        {
            // EditorJsonUtility preserves scene instance IDs, including all outgoing object references.
            string json = EditorJsonUtility.ToJson(component);
            return System.Text.RegularExpressions.Regex.Replace(json,
                "\"m_(CorrespondingSourceObject|PrefabInstance|PrefabAsset)\":\\{[^}]*\\},?", "");
        }
    }
}
