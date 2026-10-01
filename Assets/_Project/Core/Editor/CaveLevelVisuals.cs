#if UNITY_EDITOR
using AlmaDino.Features.Environment;
using UnityEngine;

namespace AlmaDino.Core.Editor
{
    internal static class CaveLevelVisuals
    {
        public static void Build(CaveLevelSceneFactory factory, Transform root, Transform camera)
        {
            var far = Layer("Parallax_0_Deep_Void", root, camera, new Vector2(0.85f, 0.85f));
            var backdrop = factory.Visual("Cave_Void", far, new Vector2(28f, -3f),
                new Vector2(130f, 80f), new Color(0.025f, 0.045f, 0.1f), true);
            backdrop.GetComponent<SpriteRenderer>().sortingOrder = -30;
            var middle = Layer("Parallax_1_Geode_Columns", root, camera, new Vector2(0.35f, 0.25f));
            for (int i = 0; i < 13; i++)
            {
                var column = factory.Visual("Distant_Column_" + i, middle,
                    new Vector2(-9f + i * 7f, -3f + (i % 3) * 3f),
                    new Vector2(1.8f + (i % 2), 35f), new Color(0.055f, 0.085f, 0.17f), true);
                column.transform.localRotation = Quaternion.Euler(0f, 0f, i % 2 == 0 ? 7f : -7f);
                column.GetComponent<SpriteRenderer>().sortingOrder = -20;
            }
            var foreground = Layer("Parallax_3_Foreground_Stalactites", root, camera, new Vector2(-0.08f, -0.05f));
            for (int i = 0; i < 10; i++)
            {
                var tip = factory.Visual("Foreground_Stalactite_" + i, foreground,
                    new Vector2(-5f + i * 8f, 16f - i * 3.4f), new Vector2(0.4f, 2.3f),
                    new Color(0.06f, 0.09f, 0.16f, 0.3f), true);
                tip.transform.localRotation = Quaternion.Euler(0f, 0f, 12f);
                tip.GetComponent<SpriteRenderer>().sortingOrder = 7;
            }
            Vector2[] positions =
            {
                new Vector2(1.3f, 13.1f), new Vector2(8.2f, 5.2f), new Vector2(6.8f, -1.8f),
                new Vector2(16.3f, -1.8f), new Vector2(25.5f, -7.3f), new Vector2(31.5f, -7.3f),
                new Vector2(38.2f, -6.5f), new Vector2(43.2f, -13.8f), new Vector2(54f, -12.8f),
                new Vector2(60f, -12.8f)
            };
            for (int i = 0; i < positions.Length; i++) CrystalCluster(factory, root, positions[i], i);
            LandingMarker(factory, root, new Vector2(4.5f, -1f));
            LandingMarker(factory, root, new Vector2(20f, -6.5f));
            LandingMarker(factory, root, new Vector2(44f, -13f));
        }

        private static Transform Layer(string name, Transform parent, Transform camera, Vector2 factor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var parallax = go.AddComponent<ParallaxLayer2D>();
            CaveLevelSceneFactory.Set(parallax, "_camera", camera);
            CaveLevelSceneFactory.Set(parallax, "_cameraMotionFactor", factor);
            return go.transform;
        }

        private static void CrystalCluster(CaveLevelSceneFactory factory, Transform root, Vector2 position, int index)
        {
            Color color = index % 2 == 0 ? CaveLevelSceneFactory.Crystal : new Color(0.58f, 0.27f, 0.85f);
            for (int i = 0; i < 3; i++)
            {
                var crystal = factory.Visual("Decorative_Geode_" + index + "_" + i, root,
                    position + new Vector2((i - 1) * 0.28f, 0f),
                    new Vector2(0.3f, 0.9f - Mathf.Abs(i - 1) * 0.3f), color, true);
                crystal.transform.localRotation = Quaternion.Euler(0f, 0f, (i - 1) * -18f);
                crystal.GetComponent<SpriteRenderer>().sortingOrder = -1;
            }
            CaveLevelSceneFactory.Light(root, position, color, 4f, 0.7f);
        }

        private static void LandingMarker(CaveLevelSceneFactory factory, Transform root, Vector2 position)
        {
            for (int i = 0; i < 2; i++)
            {
                var marker = factory.Visual("Safe_Descent_Arrow", root,
                    position + new Vector2(i == 0 ? -0.2f : 0.2f, 0f),
                    new Vector2(0.08f, 0.6f), CaveLevelSceneFactory.Crystal, true);
                marker.transform.localRotation = Quaternion.Euler(0f, 0f, i == 0 ? 40f : -40f);
            }
        }
    }
}
#endif
