using UnityEngine;

namespace AlmaGame.Level
{
    // Shared, cheap visual helpers for hazards: textures generated once in code and small
    // particle systems with fixed budgets. No art assets needed; swap for real art later.
    public static class HazardFx
    {
        private static Texture2D s_puff;
        private static Texture2D s_chunk;
        private static Sprite s_glow;
        private static Texture2D s_streak;
        private static Texture2D s_bubble;

        // Soft round puff (smoke, gas, dust, glow).
        public static Texture2D Puff()
        {
            if (s_puff != null) return s_puff;
            const int size = 32;
            s_puff = NewTexture("HazardPuff", size, size);
            var pixels = new Color32[size * size];
            float radius = size * 0.5f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(radius, radius)) / radius;
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(1f - d * d) * 255f));
            }
            return Apply(s_puff, pixels);
        }

        // Irregular rock chunk with a darker edge (debris).
        public static Texture2D Chunk()
        {
            if (s_chunk != null) return s_chunk;
            const int size = 16;
            s_chunk = NewTexture("HazardChunk", size, size);
            var pixels = new Color32[size * size];
            Vector2[] corners = { new(0.15f, 0.25f), new(0.55f, 0.05f), new(0.92f, 0.35f), new(0.8f, 0.85f), new(0.3f, 0.92f), new(0.05f, 0.6f) };
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                var p = new Vector2((x + 0.5f) / size, (y + 0.5f) / size);
                float inside = InsidePolygon(p, corners);
                byte shade = (byte)(inside > 0.85f ? 255 : 170);
                pixels[y * size + x] = new Color32(shade, shade, shade, (byte)(inside > 0f ? 255 : 0));
            }
            return Apply(s_chunk, pixels);
        }

        // Bubble: bright rim, faint see-through fill and a highlight at the top left (liquid bubbles).
        public static Texture2D Bubble()
        {
            if (s_bubble != null) return s_bubble;
            const int size = 32;
            s_bubble = NewTexture("HazardBubble", size, size);
            var pixels = new Color32[size * size];
            float radius = size * 0.5f;
            var highlight = new Vector2(size * 0.36f, size * 0.66f);
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                var p = new Vector2(x + 0.5f, y + 0.5f);
                float d = Vector2.Distance(p, new Vector2(radius, radius)) / radius;
                float rim = Mathf.Clamp01(1f - Mathf.Abs(d - 0.84f) / 0.12f);
                float fill = d < 0.84f ? 0.28f : 0f;
                float shine = Mathf.Clamp01(1f - Vector2.Distance(p, highlight) / (size * 0.13f));
                float alpha = Mathf.Max(Mathf.Max(rim, fill), shine);
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
            }
            return Apply(s_bubble, pixels);
        }

        // Thin horizontal streak with soft ends (wind lines).
        public static Texture2D Streak()
        {
            if (s_streak != null) return s_streak;
            const int width = 64;
            const int height = 8;
            s_streak = NewTexture("HazardStreak", width, height);
            var pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                float across = 1f - Mathf.Abs((y + 0.5f) / height * 2f - 1f);
                float along = Mathf.Sin((x + 0.5f) / width * Mathf.PI);
                pixels[y * width + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(across * along) * 255f));
            }
            return Apply(s_streak, pixels);
        }

        // Puff as a 1×1-unit sprite (fireball glow, cores).
        public static Sprite Glow()
        {
            if (s_glow != null) return s_glow;
            Texture2D texture = Puff();
            s_glow = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), texture.width);
            s_glow.name = "HazardGlow";
            return s_glow;
        }

        // World- or local-space particle system with a radial start, fading out over its life.
        public static ParticleSystem CreateParticles(string name, Transform parent, Material baseMaterial,
            Texture2D texture, int maxParticles, int sortingLayerId, int sortingOrder, bool worldSpace = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var system = go.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = system.main;
            main.loop = true;
            main.playOnAwake = false;
            main.maxParticles = maxParticles;
            main.simulationSpace = worldSpace ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local;

            var emission = system.emission;
            emission.rateOverTime = 0f;
            emission.enabled = false;

            var fade = new Gradient();
            fade.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
            var color = system.colorOverLifetime;
            color.enabled = true;
            color.color = fade;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = new Material(baseMaterial) { mainTexture = texture };
            renderer.sortingLayerID = sortingLayerId;
            renderer.sortingOrder = sortingOrder;
            system.Play();
            return system;
        }

        // Unity forbids resizing sprites/colliders inside OnValidate (SendMessage warning), so editor
        // layout updates run on the next editor tick instead. Runtime layout is applied in Awake.
        public static void DeferInEditor(Object owner, System.Action action)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (owner != null) action();
            };
#endif
        }

        public static void SetSizeOverLifetime(ParticleSystem system, float start, float end)
        {
            var size = system.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, start, 1f, end));
        }

        public static void DestroyMaterial(ParticleSystem system)
        {
            if (system == null) return;
            var renderer = system.GetComponent<ParticleSystemRenderer>();
            if (renderer != null && renderer.sharedMaterial != null) Object.Destroy(renderer.sharedMaterial);
        }

        private static float InsidePolygon(Vector2 p, Vector2[] poly)
        {
            bool inside = false;
            float minEdge = float.MaxValue;
            for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
            {
                Vector2 a = poly[i], b = poly[j];
                if ((a.y > p.y) != (b.y > p.y) && p.x < (b.x - a.x) * (p.y - a.y) / (b.y - a.y) + a.x) inside = !inside;
                Vector2 ab = b - a;
                float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
                minEdge = Mathf.Min(minEdge, Vector2.Distance(p, a + ab * t));
            }
            return inside ? Mathf.Clamp01(minEdge * 8f) : 0f;
        }

        private static Texture2D NewTexture(string name, int width, int height) => new(width, height, TextureFormat.RGBA32, false)
        {
            name = name,
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
        };

        private static Texture2D Apply(Texture2D texture, Color32[] pixels)
        {
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }
    }
}
