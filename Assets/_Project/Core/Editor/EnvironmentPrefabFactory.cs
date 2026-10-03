#if UNITY_EDITOR
using System.IO;
using AlmaDino.Features.Environment;
using UnityEditor;
using UnityEngine;

namespace AlmaDino.Core.Editor
{
    [InitializeOnLoad]
    public static class EnvironmentPrefabFactory
    {
        private const string PREFABS_ROOT = "Assets/_Project/Prefabs";
        private const string SPRITE_SQUARE = "Assets/Sprites/Square.png";
        private const string SPRITE_CIRCLE = "Assets/Sprites/Circle.png";
        private const string SPRITE_UNLIT_MAT = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";

        static EnvironmentPrefabFactory()
        {
            EditorApplication.delayCall += AutoGenerateIfMissing;
        }

        private static void AutoGenerateIfMissing()
        {
            if (!File.Exists(PREFABS_ROOT + "/Universal/Checkpoint_Nest.prefab"))
            {
                GenerateAllPrefabs();
            }
        }

        [MenuItem("Alma/📦 Generar Biblioteca de Prefabs Ambientales")]
        public static void GenerateAllPrefabs()
        {
            EnsureDirectoryExists(PREFABS_ROOT + "/Universal");
            EnsureDirectoryExists(PREFABS_ROOT + "/World_1_Jungle");
            EnsureDirectoryExists(PREFABS_ROOT + "/World_2_Caves");
            EnsureDirectoryExists(PREFABS_ROOT + "/World_3_Swamp");
            EnsureDirectoryExists(PREFABS_ROOT + "/World_4_Volcano");

            Sprite square = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_SQUARE);
            Sprite circle = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_CIRCLE);
            Material unlitMat = AssetDatabase.LoadAssetAtPath<Material>(SPRITE_UNLIT_MAT);

            if (unlitMat == null)
            {
                Shader unlitShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit");
                if (unlitShader != null) unlitMat = new Material(unlitShader);
            }

            int count = 0;

            // 1. UNIVERSAL
            count += CreatePrefab($"{PREFABS_ROOT}/Universal/Checkpoint_Nest.prefab", () =>
            {
                var go = new GameObject("Checkpoint_Nest");
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = square;
                sr.sharedMaterial = unlitMat;
                sr.color = new Color(0.87f, 0.63f, 0.37f); // Wood/Straw
                go.transform.localScale = new Vector3(1.8f, 0.6f, 1f);

                var orb = new GameObject("Indicator_Orb");
                orb.transform.SetParent(go.transform);
                orb.transform.localPosition = new Vector3(0f, 1.0f, 0f);
                orb.transform.localScale = new Vector3(0.5f, 1.5f, 1f);
                var orbSr = orb.AddComponent<SpriteRenderer>();
                orbSr.sprite = circle;
                orbSr.sharedMaterial = unlitMat;
                orbSr.color = new Color(1f, 0.72f, 0.01f);

                var col = go.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(1f, 2.5f);

                var cp = go.AddComponent<Checkpoint2D>();
                return go;
            });

            count += CreatePrefab($"{PREFABS_ROOT}/Universal/Level_Exit_Portal.prefab", () =>
            {
                var go = new GameObject("Level_Exit_Portal");
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = square;
                sr.sharedMaterial = unlitMat;
                sr.color = new Color(0.23f, 0.05f, 0.64f); // Mystic arch
                go.transform.localScale = new Vector3(2.0f, 3.2f, 1f);

                var core = new GameObject("Portal_Core");
                core.transform.SetParent(go.transform);
                core.transform.localPosition = Vector3.zero;
                core.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
                var coreSr = core.AddComponent<SpriteRenderer>();
                coreSr.sprite = circle;
                coreSr.sharedMaterial = unlitMat;
                coreSr.color = new Color(0.3f, 0.79f, 0.94f);

                var col = go.AddComponent<BoxCollider2D>();
                col.isTrigger = true;

                go.AddComponent<LevelExit2D>();
                return go;
            });

            count += CreatePrefab($"{PREFABS_ROOT}/Universal/Ability_Relic_Altar.prefab", () =>
            {
                var go = new GameObject("Ability_Relic_Altar");
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = square;
                sr.sharedMaterial = unlitMat;
                sr.color = new Color(0.29f, 0.31f, 0.34f); // Stone altar
                go.transform.localScale = new Vector3(1.6f, 0.8f, 1f);

                var gem = new GameObject("Relic_Gem");
                gem.transform.SetParent(go.transform);
                gem.transform.localPosition = new Vector3(0f, 1.2f, 0f);
                gem.transform.localScale = new Vector3(0.6f, 1.2f, 1f);
                var gemSr = gem.AddComponent<SpriteRenderer>();
                gemSr.sprite = circle;
                gemSr.sharedMaterial = unlitMat;
                gemSr.color = new Color(0.97f, 0.15f, 0.52f); // Glowing magenta gem

                var col = go.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(1.2f, 2.5f);

                go.AddComponent<AbilityRelic2D>();
                return go;
            });

            // 2. WORLD 1 (JUNGLE)
            count += CreatePrefab($"{PREFABS_ROOT}/World_1_Jungle/Hazard_Spikes_Jungle.prefab", () =>
            {
                var go = new GameObject("Hazard_Spikes_Jungle");
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = square;
                sr.sharedMaterial = unlitMat;
                sr.color = new Color(0.55f, 0.12f, 0.18f); // Thorny crimson
                go.transform.localScale = new Vector3(3.0f, 0.8f, 1f);

                var col = go.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                go.AddComponent<HazardTrigger2D>();
                return go;
            });

            count += CreatePrefab($"{PREFABS_ROOT}/World_1_Jungle/BouncyMushroom_Jungle.prefab", () =>
            {
                var go = new GameObject("BouncyMushroom_Jungle");

                var stem = new GameObject("Stem");
                stem.transform.SetParent(go.transform);
                stem.transform.localPosition = new Vector3(0f, -0.3f, 0f);
                stem.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
                var stemSr = stem.AddComponent<SpriteRenderer>();
                stemSr.sprite = square;
                stemSr.sharedMaterial = unlitMat;
                stemSr.color = new Color(0.85f, 0.95f, 0.86f);

                var cap = new GameObject("Cap");
                cap.transform.SetParent(go.transform);
                cap.transform.localPosition = new Vector3(0f, 0.15f, 0f);
                cap.transform.localScale = new Vector3(1.8f, 0.7f, 1f);
                var capSr = cap.AddComponent<SpriteRenderer>();
                capSr.sprite = circle;
                capSr.sharedMaterial = unlitMat;
                capSr.color = new Color(0.62f, 0.94f, 0.10f); // Bright jungle green

                var col = go.AddComponent<BoxCollider2D>();
                col.size = new Vector2(1.8f, 0.8f);
                col.offset = new Vector2(0f, 0.1f);

                go.AddComponent<BouncyPlatform2D>();
                return go;
            });

            count += CreatePrefab($"{PREFABS_ROOT}/World_1_Jungle/CrumblingLeaf_Jungle.prefab", () =>
            {
                var go = new GameObject("CrumblingLeaf_Jungle");
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = square;
                sr.sharedMaterial = unlitMat;
                sr.color = new Color(0.39f, 0.45f, 0.23f); // Olive jungle leaf
                go.transform.localScale = new Vector3(2.6f, 0.4f, 1f);

                var col = go.AddComponent<BoxCollider2D>();
                go.AddComponent<CrumblingPlatform2D>();
                return go;
            });

            count += CreatePrefab($"{PREFABS_ROOT}/World_1_Jungle/CarnivorousPlant_Jungle.prefab", () =>
            {
                var go = new GameObject("CarnivorousPlant_Jungle");
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = square;
                sr.sharedMaterial = unlitMat;
                sr.color = new Color(0.84f, 0.13f, 0.27f); // Plant jaws
                go.transform.localScale = new Vector3(1.5f, 1.5f, 1f);

                var col = go.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                go.AddComponent<CarnivorousPlant2D>();
                return go;
            });

            // 3. WORLD 2 (CAVES)
            count += CreatePrefab($"{PREFABS_ROOT}/World_2_Caves/Hazard_Crystals_Caves.prefab", () =>
            {
                var go = new GameObject("Hazard_Crystals_Caves");
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = square;
                sr.sharedMaterial = unlitMat;
                sr.color = new Color(0.45f, 0.04f, 0.72f); // Sharp purple crystal
                go.transform.localScale = new Vector3(3.0f, 1.0f, 1f);

                var col = go.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                go.AddComponent<HazardTrigger2D>();
                return go;
            });

            count += CreatePrefab($"{PREFABS_ROOT}/World_2_Caves/BreakableFloor_Caves.prefab", () =>
            {
                var go = new GameObject("BreakableFloor_Caves");
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = square;
                sr.sharedMaterial = unlitMat;
                sr.color = new Color(0.35f, 0.10f, 0.60f); // Fissured ground
                go.transform.localScale = new Vector3(3.2f, 0.8f, 1f);

                var col = go.AddComponent<BoxCollider2D>();
                go.AddComponent<BreakableGround2D>();
                return go;
            });

            count += CreatePrefab($"{PREFABS_ROOT}/World_2_Caves/SeesawPlatform_Caves.prefab", () =>
            {
                var go = new GameObject("SeesawPlatform_Caves");

                var fulcrum = new GameObject("Fulcrum");
                fulcrum.transform.SetParent(go.transform);
                fulcrum.transform.localPosition = new Vector3(0f, -0.4f, 0f);
                fulcrum.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
                var fSr = fulcrum.AddComponent<SpriteRenderer>();
                fSr.sprite = square;
                fSr.sharedMaterial = unlitMat;
                fSr.color = new Color(0.2f, 0.23f, 0.25f);

                var plank = new GameObject("Plank");
                plank.transform.SetParent(go.transform);
                plank.transform.localPosition = Vector3.zero;
                plank.transform.localScale = new Vector3(4.2f, 0.4f, 1f);
                var pSr = plank.AddComponent<SpriteRenderer>();
                pSr.sprite = square;
                pSr.sharedMaterial = unlitMat;
                pSr.color = new Color(0.42f, 0.46f, 0.49f);

                var col = plank.AddComponent<BoxCollider2D>();
                plank.AddComponent<SeesawPlatform2D>();
                return go;
            });

            count += CreatePrefab($"{PREFABS_ROOT}/World_2_Caves/TimedRuneGate_Caves.prefab", () =>
            {
                var go = new GameObject("TimedRuneGate_Caves");
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = square;
                sr.sharedMaterial = unlitMat;
                sr.color = new Color(0.28f, 0.79f, 0.90f, 1f); // Translucent crystal gate
                go.transform.localScale = new Vector3(0.7f, 3.6f, 1f);

                var col = go.AddComponent<BoxCollider2D>();
                go.AddComponent<TimedRuneGate2D>();
                return go;
            });

            count += CreatePrefab($"{PREFABS_ROOT}/World_2_Caves/BouncyCrystal_Caves.prefab", () =>
            {
                var go = new GameObject("BouncyCrystal_Caves");
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = circle;
                sr.sharedMaterial = unlitMat;
                sr.color = new Color(0f, 0.71f, 0.85f); // Resonant bounce crystal
                go.transform.localScale = new Vector3(1.6f, 1.6f, 1f);

                var col = go.AddComponent<BoxCollider2D>();
                col.size = new Vector2(1.6f, 1.2f);
                col.offset = new Vector2(0f, 0.2f);

                go.AddComponent<BouncyPlatform2D>();
                return go;
            });

            // 4. WORLD 3 (SWAMP)
            count += CreatePrefab($"{PREFABS_ROOT}/World_3_Swamp/Hazard_Brier_Swamp.prefab", () =>
            {
                var go = new GameObject("Hazard_Brier_Swamp");
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = square;
                sr.sharedMaterial = unlitMat;
                sr.color = new Color(0.11f, 0.26f, 0.20f); // Toxic swamp mud
                go.transform.localScale = new Vector3(3.5f, 0.9f, 1f);

                var col = go.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                go.AddComponent<HazardTrigger2D>();
                return go;
            });

            count += CreatePrefab($"{PREFABS_ROOT}/World_3_Swamp/WindCurrent_Swamp.prefab", () =>
            {
                var go = new GameObject("WindCurrent_Swamp");
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = square;
                sr.sharedMaterial = unlitMat;
                sr.color = new Color(0.32f, 0.72f, 0.53f, 0.35f); // Upward wind stream
                go.transform.localScale = new Vector3(2.5f, 6.0f, 1f);

                var col = go.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                go.AddComponent<WindCurrentZone2D>();
                return go;
            });

            count += CreatePrefab($"{PREFABS_ROOT}/World_3_Swamp/DashRefillSpore_Swamp.prefab", () =>
            {
                var go = new GameObject("DashRefillSpore_Swamp");
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = circle;
                sr.sharedMaterial = unlitMat;
                sr.color = new Color(0f, 0.96f, 0.83f); // Glowing teal spore
                go.transform.localScale = new Vector3(1.2f, 1.2f, 1f);

                var col = go.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                go.AddComponent<DashRefillPickup2D>();
                return go;
            });

            count += CreatePrefab($"{PREFABS_ROOT}/World_3_Swamp/ReedBarrier_Swamp.prefab", () =>
            {
                var go = new GameObject("ReedBarrier_Swamp");
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = square;
                sr.sharedMaterial = unlitMat;
                sr.color = new Color(0.25f, 0.57f, 0.42f); // Dense reeds
                go.transform.localScale = new Vector3(0.8f, 3.5f, 1f);

                var col = go.AddComponent<BoxCollider2D>();
                go.AddComponent<DashBreakableBarrier2D>();
                return go;
            });

            count += CreatePrefab($"{PREFABS_ROOT}/World_3_Swamp/CrumblingLilypad_Swamp.prefab", () =>
            {
                var go = new GameObject("CrumblingLilypad_Swamp");
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = circle;
                sr.sharedMaterial = unlitMat;
                sr.color = new Color(0.45f, 0.78f, 0.62f); // Giant floating lilypad
                go.transform.localScale = new Vector3(2.6f, 0.5f, 1f);

                var col = go.AddComponent<BoxCollider2D>();
                col.size = new Vector2(2.6f, 0.5f);
                go.AddComponent<CrumblingPlatform2D>();
                return go;
            });

            // 5. WORLD 4 (VOLCANO)
            count += CreatePrefab($"{PREFABS_ROOT}/World_4_Volcano/Hazard_Lava_Volcano.prefab", () =>
            {
                var go = new GameObject("Hazard_Lava_Volcano");
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = square;
                sr.sharedMaterial = unlitMat;
                sr.color = new Color(0.84f, 0.16f, 0.16f); // Molten lava
                go.transform.localScale = new Vector3(4.0f, 1.0f, 1f);

                var col = go.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                go.AddComponent<HazardTrigger2D>();
                return go;
            });

            count += CreatePrefab($"{PREFABS_ROOT}/World_4_Volcano/PushableBoulder_Volcano.prefab", () =>
            {
                var go = new GameObject("PushableBoulder_Volcano");
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = circle;
                sr.sharedMaterial = unlitMat;
                sr.color = new Color(0.20f, 0.23f, 0.25f); // Basalt boulder
                go.transform.localScale = new Vector3(1.8f, 1.8f, 1f);

                var col = go.AddComponent<CircleCollider2D>();
                var rb = go.AddComponent<Rigidbody2D>();
                rb.mass = 5f;
                go.AddComponent<PushableBoulder2D>();
                return go;
            });

            count += CreatePrefab($"{PREFABS_ROOT}/World_4_Volcano/LavaGeyser_Volcano.prefab", () =>
            {
                var go = new GameObject("LavaGeyser_Volcano");

                var vent = new GameObject("VentBase");
                vent.transform.SetParent(go.transform);
                vent.transform.localPosition = new Vector3(0f, -1.8f, 0f);
                vent.transform.localScale = new Vector3(1.6f, 0.5f, 1f);
                var vSr = vent.AddComponent<SpriteRenderer>();
                vSr.sprite = square;
                vSr.sharedMaterial = unlitMat;
                vSr.color = new Color(0.13f, 0.15f, 0.16f);

                var flame = new GameObject("FlamePillar");
                flame.transform.SetParent(go.transform);
                flame.transform.localPosition = Vector3.zero;
                flame.transform.localScale = new Vector3(1.2f, 3.8f, 1f);
                var fSr = flame.AddComponent<SpriteRenderer>();
                fSr.sprite = square;
                fSr.sharedMaterial = unlitMat;
                fSr.color = new Color(1f, 0.33f, 0.0f); // Bright flame column

                var col = go.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(1.2f, 3.8f);

                go.AddComponent<LavaGeyser2D>();
                return go;
            });

            count += CreatePrefab($"{PREFABS_ROOT}/World_4_Volcano/SinkingBasalt_Volcano.prefab", () =>
            {
                var go = new GameObject("SinkingBasalt_Volcano");
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = square;
                sr.sharedMaterial = unlitMat;
                sr.color = new Color(0.29f, 0.31f, 0.34f); // Basalt platform
                go.transform.localScale = new Vector3(2.8f, 0.6f, 1f);

                var col = go.AddComponent<BoxCollider2D>();
                go.AddComponent<SinkingBasalt2D>();
                return go;
            });

            count += CreatePrefab($"{PREFABS_ROOT}/World_4_Volcano/BouncySteamVent_Volcano.prefab", () =>
            {
                var go = new GameObject("BouncySteamVent_Volcano");
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = square;
                sr.sharedMaterial = unlitMat;
                sr.color = new Color(1f, 0.62f, 0f); // Pressurized steam vent
                go.transform.localScale = new Vector3(1.8f, 0.6f, 1f);

                var col = go.AddComponent<BoxCollider2D>();
                col.size = new Vector2(1.8f, 0.6f);

                go.AddComponent<BouncyPlatform2D>();
                return go;
            });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"<color=#00FF88><b>[AlmaDino]</b> ¡Biblioteca de Prefabs Ambientales generada con éxito! ({count} prefabs creados en {PREFABS_ROOT})</color>");
        }

        private static int CreatePrefab(string assetPath, System.Func<GameObject> buildAction)
        {
            GameObject tempGo = buildAction();
            PrefabUtility.SaveAsPrefabAsset(tempGo, assetPath);
            Object.DestroyImmediate(tempGo);
            return 1;
        }

        private static void EnsureDirectoryExists(string dirPath)
        {
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
        }
    }
}
#endif
