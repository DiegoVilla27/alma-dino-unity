using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlmaDino.Core.Editor
{
    internal static class PrefabAssemblies
    {
        // Connected components of the serialized reference graph keep puzzle wiring inside the asset.
        internal static void Extract(GameObject level, LevelPrefabCatalog catalog, string scenePath)
        {
            var roots = level.transform.Cast<Transform>().ToArray();
            var links = roots.ToDictionary(t => t, _ => new HashSet<Transform>());
            foreach (var root in roots)
            foreach (var component in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (component == null) continue;
                var property = new SerializedObject(component).GetIterator();
                while (property.Next(true))
                {
                    if (property.propertyType != SerializedPropertyType.ObjectReference) continue;
                    var target = PrefabStructure.SceneTarget(property.objectReferenceValue);
                    if (target == null) continue;
                    var other = roots.FirstOrDefault(r => target.transform.IsChildOf(r));
                    if (other == null || other == root) continue;
                    links[root].Add(other);
                    links[other].Add(root);
                }
            }
            var visited = new HashSet<Transform>();
            int index = 0;
            foreach (var root in roots)
            {
                if (!visited.Add(root)) continue;
                var group = new List<Transform>();
                var queue = new Queue<Transform>();
                queue.Enqueue(root);
                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    group.Add(current);
                    foreach (var next in links[current]) if (visited.Add(next)) queue.Enqueue(next);
                }
                if (group.Count < 2) continue;
                string folder = LevelPrefabLibrary.Root + (Path.GetFileName(scenePath).StartsWith("Boss_") ? "/Bosses" : "/Resources") + "/Assemblies/" + Path.GetFileName(Path.GetDirectoryName(scenePath));
                Directory.CreateDirectory(folder);
                string path = folder + "/" + Path.GetFileNameWithoutExtension(scenePath) + "_" + PrefabStructure.AssemblyLabel(root.gameObject) + "_" + index++ + ".prefab";
                if (!File.Exists(path)) SaveGroup(group, level.transform, path, catalog, scenePath);
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (!catalog.Assemblies.Contains(asset)) catalog.Assemblies.Add(asset);
            }
        }

        private static void SaveGroup(List<Transform> group, Transform parent, string path, LevelPrefabCatalog catalog, string scenePath)
        {
            var wrapper = new GameObject(Path.GetFileNameWithoutExtension(path));
            var indices = group.ToDictionary(t => t, t => t.GetSiblingIndex());
            try
            {
                // Temporarily group originals before cloning so Unity remaps every internal reference.
                foreach (var item in group) item.SetParent(wrapper.transform, true);
                var copy = Object.Instantiate(wrapper);
                try
                {
                    LevelPrefabLibrary.ConvertChildren(copy, catalog, scenePath);
                    PrefabStructure.AddSpriteSlots(copy);
                    PrefabUtility.SaveAsPrefabAsset(copy, path);
                }
                finally { Object.DestroyImmediate(copy); }
            }
            finally
            {
                foreach (var item in group.OrderBy(t => indices[t]))
                {
                    item.SetParent(parent, true);
                    item.SetSiblingIndex(indices[item]);
                }
                Object.DestroyImmediate(wrapper);
            }
        }
    }
}
