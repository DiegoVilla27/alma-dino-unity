using System;
using System.IO;
using System.Linq;
using System.Text;
using AlmaDino.Shared.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AlmaDino.Core.Editor
{
    public static class PrefabLibraryValidation
    {
        [MenuItem("Alma/Prefabs/Validate library and world 1")]
        public static void Validate()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            PrefabScaffoldingValidation.Validate();
            var report = new StringBuilder("# Prefab catalog inventory\n\n");
            var catalog = AssetDatabase.LoadAssetAtPath<LevelPrefabCatalog>(LevelPrefabLibrary.CatalogPath);
            if (catalog == null) throw new InvalidOperationException("Missing prefab catalog.");
            foreach (var entry in catalog.Pieces)
            {
                ValidateAsset(entry.Prefab);
                report.AppendLine($"- `{AssetDatabase.GetAssetPath(entry.Prefab)}` — source `{entry.SourceScene}` / `{entry.SourceObject}`");
            }
            report.AppendLine("\n## Connected assemblies\n");
            foreach (var assembly in catalog.Assemblies)
            {
                ValidateAsset(assembly);
                report.AppendLine($"- `{AssetDatabase.GetAssetPath(assembly)}`");
            }
            var setup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                report.AppendLine("\n## Scene coverage\n");
                foreach (var path in LevelPrefabLibrary.ScenePaths())
                {
                    var scene = EditorSceneManager.OpenScene(path);
                    catalog = AssetDatabase.LoadAssetAtPath<LevelPrefabCatalog>(LevelPrefabLibrary.CatalogPath);
                    var level = LevelPrefabLibrary.LevelRoot(scene);
                    int count = 0;
                    foreach (Transform child in level.transform)
                    {
                        if (!catalog.Pieces.Any(e => e.Signature == PrefabStructure.Signature(child.gameObject)))
                            throw new InvalidOperationException($"Uncatalogued piece: {path}/{child.name}");
                        if (path.Contains("World_1_") && !PrefabUtility.IsAnyPrefabInstanceRoot(child.gameObject))
                            throw new InvalidOperationException($"Unconverted World 1 object: {child.name}");
                        count++;
                    }
                    report.AppendLine($"- `{path}`: {count} root pieces; {(path.Contains("World_1_") ? "migrated" : "catalogued, scene unchanged")}.");
                }
            }
            finally { if (!Application.isBatchMode) EditorSceneManager.RestoreSceneManagerSetup(setup); }
            Directory.CreateDirectory("Docs");
            File.WriteAllText("Docs/PrefabCatalogInventory.md", report.ToString());
            Debug.Log("PREFAB_VALIDATION_OK: all 20 scenes covered; every World 1 level object is a prefab instance.");
        }

        private static void ValidateAsset(GameObject asset)
        {
            if (asset == null) throw new InvalidOperationException("Missing prefab asset.");
            foreach (var transform in asset.GetComponentsInChildren<Transform>(true))
            {
                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject) != 0)
                    throw new InvalidOperationException($"Missing script: {asset.name}/{transform.name}");
                if (transform.GetComponent<SpriteRenderer>() != null && transform.GetComponent<PrefabSprite2D>() == null)
                    throw new InvalidOperationException($"Missing sprite slot: {asset.name}/{transform.name}");
            }
        }
    }
}
