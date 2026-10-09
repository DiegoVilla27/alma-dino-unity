using System.Collections.Generic;
using UnityEngine;

namespace AlmaGame.Systems
{
    // Global "how tense is this moment" register. Anything dangerous or dramatic (rising gas, a boss phase, a
    // cinematic) sets its own level 0..1 and a tint; the strongest source wins. The HUD's tension vignette
    // (HudTensionVignette) draws it. Sources must clear themselves when they stop (or are disabled).
    public static class TensionState
    {
        private struct Source { public float Level; public Color Tint; }

        private static readonly Dictionary<Object, Source> s_sources = new Dictionary<Object, Source>();

        // Ink-dark default for narrative tension.
        public static readonly Color Narrative = new Color(0.06f, 0.05f, 0.09f);
        // Deep tints per kind of danger.
        public static readonly Color Toxic = new Color(0.12f, 0.3f, 0.04f);
        public static readonly Color Fire = new Color(0.42f, 0.07f, 0.02f);

        public static void Set(Object source, float level, Color tint)
        {
            if (source == null) return;
            if (level <= 0.001f) { s_sources.Remove(source); return; }
            s_sources[source] = new Source { Level = Mathf.Clamp01(level), Tint = tint };
        }

        public static void Clear(Object source)
        {
            if (source != null) s_sources.Remove(source);
        }

        public static void Reset() => s_sources.Clear();

        // Strongest level and its tint (tints of close contenders blend in).
        public static float Current(out Color tint)
        {
            float best = 0f;
            tint = Narrative;
            var dead = (List<Object>)null;
            foreach (var pair in s_sources)
            {
                if (pair.Key == null) { (dead ??= new List<Object>()).Add(pair.Key); continue; }
                if (pair.Value.Level > best) { best = pair.Value.Level; tint = pair.Value.Tint; }
            }
            if (dead != null) foreach (var d in dead) s_sources.Remove(d);
            return best;
        }
    }
}
