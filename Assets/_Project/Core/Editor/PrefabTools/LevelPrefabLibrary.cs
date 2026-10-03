using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace AlmaDino.Core.Editor
{
    public static class LevelPrefabLibrary
    {
        public const string Root = "Assets/_Project/Prefabs";
        public const string CatalogPath = Root + "/Resources/LevelPrefabCatalog.asset";

        [MenuItem("Alma/Prefabs/1. Build catalog from all worlds")]
        public static void BuildCatalog()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var setup = EditorSceneManager.GetSceneManagerSetup();
            try { BuildCatalogInternal(); }
            finally { if (!Application.isBatchMode) EditorSceneManager.RestoreSceneManagerSetup(setup); }
        }

        private static void BuildCatalogInternal()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<LevelPrefabCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<LevelPrefabCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }
            // Existing assets retain their GUIDs and designer artwork on subsequent runs.
            foreach (var path in Directory.GetFiles(Root, "*.prefab", SearchOption.AllDirectories))
            {
                var contents = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    PrefabStructure.MakeNamesUnique(contents);
                    PrefabStructure.AddSpriteSlots(contents);
                    PrefabUtility.SaveAsPrefabAsset(contents, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(contents); }
            }
            foreach (var path in ScenePaths())
            {
                var scene = EditorSceneManager.OpenScene(path);
                catalog = AssetDatabase.LoadAssetAtPath<LevelPrefabCatalog>(CatalogPath);
                var level = LevelRoot(scene);
                if (level == null) throw new InvalidOperationException($"Missing level root: {path}");
                foreach (Transform child in level.transform) EnsurePiece(child.gameObject, catalog, path);
                PrefabAssemblies.Extract(level, catalog, path);
                EditorUtility.SetDirty(catalog);
                AssetDatabase.SaveAssets();
            }
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log($"Prefab catalog: {catalog.Pieces.Count} reusable piece types, {catalog.Assemblies.Count} connected assemblies.");
        }

        internal static GameObject EnsurePiece(GameObject source, LevelPrefabCatalog catalog, string scenePath)
        {
            var signature = PrefabStructure.Signature(source);
            var existing = catalog.Pieces.FirstOrDefault(e => e.Signature == signature && e.Prefab != null);
            if (existing != null) return existing.Prefab;
            string path = PrefabUtility.IsAnyPrefabInstanceRoot(source)
                ? PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(source) : null;
            if (string.IsNullOrEmpty(path))
            {
                string folder = Root + "/" + PrefabStructure.Category(source);
                Directory.CreateDirectory(folder);
                path = PrefabStructure.UniquePath(folder, PrefabStructure.Label(source));
                var copy = Object.Instantiate(source);
                try
                {
                    copy.name = Path.GetFileNameWithoutExtension(path);
                    copy.transform.SetParent(null);
                    copy.transform.position = Vector3.zero;
                    PrefabStructure.MakeNamesUnique(copy);
                    PrefabStructure.AddSpriteSlots(copy);
                    PrefabUtility.SaveAsPrefabAsset(copy, path);
                }
                finally { Object.DestroyImmediate(copy); }
            }
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            catalog.Pieces.Add(new LevelPrefabCatalog.Entry
            {
                Signature = signature, Prefab = prefab, SourceScene = scenePath, SourceObject = source.name
            });
            return prefab;
        }

        [MenuItem("Alma/Prefabs/2. Migrate world 1 scenes")]
        public static void MigrateWorldOne()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var setup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                foreach (var path in ScenePaths().Where(p => p.Contains("World_1_")))
                {
                    var scene = EditorSceneManager.OpenScene(path);
                    ConvertLevel(scene);
                    EditorSceneManager.SaveScene(scene);
                }
                AssetDatabase.SaveAssets();
            }
            finally { if (!Application.isBatchMode) EditorSceneManager.RestoreSceneManagerSetup(setup); }
        }

        // Called by all world 1 builders before saving: regeneration also uses the shared library.
        public static void ConvertLevel(Scene scene)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<LevelPrefabCatalog>(CatalogPath);
            if (catalog == null) throw new InvalidOperationException("Build the prefab catalog before rebuilding World 1.");
            var level = LevelRoot(scene);
            ConvertChildren(level, catalog, scene.path);
            EditorUtility.SetDirty(catalog);
        }

        internal static void ConvertChildren(GameObject level, LevelPrefabCatalog catalog, string sourceScene)
        {
            var before = PrefabMigrationSnapshot.Capture(level);
            foreach (Transform child in level.transform)
            {
                if (PrefabUtility.IsAnyPrefabInstanceRoot(child.gameObject)) continue;
                var prefab = EnsurePiece(child.gameObject, catalog, sourceScene);
                var names = PrefabStructure.MakeNamesUnique(child.gameObject);
                PrefabUtility.ConvertToPrefabInstance(child.gameObject, prefab, new ConvertToPrefabInstanceSettings
                {
                    objectMatchMode = ObjectMatchMode.ByHierarchy,
                    componentsNotMatchedBecomesOverride = true,
                    gameObjectsNotMatchedBecomesOverride = true,
                    recordPropertyOverridesOfMatches = true,
                    changeRootNameToAssetName = false,
                    logInfo = false
                }, InteractionMode.AutomatedAction);
                foreach (var pair in names) pair.Key.name = pair.Value;
            }
            before.AssertUnchanged(level);
        }

        internal static string[] ScenePaths() => Directory.GetFiles("Assets/Scenes", "*.unity", SearchOption.AllDirectories)
            .Where(p => p.Contains("World_")).OrderBy(p => p).ToArray();
        internal static GameObject LevelRoot(Scene scene) => scene.GetRootGameObjects().FirstOrDefault(g => g.name == "--- LEVEL ---");

        public static void RunAll()
        {
            BuildCatalog();
            MigrateWorldOne();
            PrefabLibraryValidation.Validate();
        }
    }
}
