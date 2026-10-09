using System.Collections.Generic;
using TMPro;
using UnityEngine.TextCore;
using UnityEngine;

namespace AlmaGame.Systems
{
    public enum HudTextStyle
    {
        Headline,   // big names: abilities, bosses, titles (Luckiest Guy, thick outline + drop shadow)
        Shout,      // huge cries (Luckiest Guy, thicker outline)
        Kicker,     // small spaced line above a headline (Fredoka)
        Body,       // sentences, rescue lines, narration (Fredoka, soft shadow)
        Prompt,     // small hints such as «Toca para continuar» (Fredoka)
    }

    // The HUD's one text style: every on-screen text is TextMeshPro (signed-distance-field, sharp at any size and
    // resolution) with the same two fonts, outline and shadow. Sizes are cap heights in HUD design units.
    public static class HudText
    {
        private static TMP_FontAsset s_headline, s_body;
        private static readonly Dictionary<HudTextStyle, Material> s_materials = new Dictionary<HudTextStyle, Material>();
        private static readonly Dictionary<TMP_FontAsset, float> s_capPerSize = new Dictionary<TMP_FontAsset, float>();
        public static readonly Color Ink = new Color(0.08f, 0.07f, 0.1f, 1f);

        public static bool Ready => s_headline != null && s_body != null;

        public static void Configure(TMP_FontAsset headline, TMP_FontAsset body)
        {
            if (s_headline == headline && s_body == body) return;
            s_headline = headline;
            s_body = body;
            foreach (var m in s_materials.Values) if (m != null) Object.Destroy(m);
            s_materials.Clear();
            s_capPerSize.Clear();
        }

        private static TMP_FontAsset FontFor(HudTextStyle style) =>
            style == HudTextStyle.Headline || style == HudTextStyle.Shout ? s_headline : s_body;

        private static Material MaterialFor(HudTextStyle style)
        {
            if (s_materials.TryGetValue(style, out Material m) && m != null) return m;
            TMP_FontAsset font = FontFor(style);
            m = new Material(font.material) { name = "HUD " + style };
            float outline, softness, offsetY, dilate;
            switch (style)
            {
                case HudTextStyle.Headline: outline = 0.28f; softness = 0.15f; offsetY = -1.1f; dilate = 0.35f; break;
                case HudTextStyle.Shout:    outline = 0.32f; softness = 0.15f; offsetY = -1.3f; dilate = 0.4f; break;
                case HudTextStyle.Kicker:   outline = 0.2f;  softness = 0.5f;  offsetY = -0.6f; dilate = 0.2f; break;
                case HudTextStyle.Prompt:   outline = 0.18f; softness = 0.6f;  offsetY = -0.5f; dilate = 0.15f; break;
                default:                    outline = 0.2f;  softness = 0.55f; offsetY = -0.7f; dilate = 0.2f; break;
            }
            m.SetFloat(ShaderUtilities.ID_FaceDilate, outline * 0.5f);
            m.SetFloat(ShaderUtilities.ID_OutlineWidth, outline);
            m.SetColor(ShaderUtilities.ID_OutlineColor, Ink);
            m.EnableKeyword(ShaderUtilities.Keyword_Underlay);
            m.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(0f, 0f, 0f, 0.75f));
            m.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0.35f);
            m.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, offsetY);
            m.SetFloat(ShaderUtilities.ID_UnderlayDilate, dilate);
            m.SetFloat(ShaderUtilities.ID_UnderlaySoftness, softness);
            ShaderUtilities.UpdateShaderRatios(m);
            s_materials[style] = m;
            return m;
        }

        // TMP font size that gives capital letters a height of `units`.
        public static float FontSizeFor(HudTextStyle style, float units)
        {
            TMP_FontAsset font = FontFor(style);
            if (!s_capPerSize.TryGetValue(font, out float perSize))
            {
                FaceInfo face = font.faceInfo;
                // TextMeshPro (non-UI) draws 1 point as 0.1 units, scaled by pointSize.
                perSize = face.capLine / face.pointSize * 0.1f;
                if (perSize <= 0f) perSize = 0.07f;
                s_capPerSize[font] = perSize;
            }
            return units / perSize;
        }

        // Creates a centred text. `width` > 0 wraps (or, with `fit`, shrinks a single line) to that width.
        public static TextMeshPro Create(Transform parent, string name, HudTextStyle style, string text, float capHeight,
            Color color, int sortingOrder, float width = 0f, bool fit = false)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.font = FontFor(style);
            tmp.fontSharedMaterial = MaterialFor(style);
            tmp.fontSize = FontSizeFor(style, capHeight);
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = color;
            tmp.extraPadding = true;
            tmp.sortingOrder = sortingOrder;
            tmp.richText = true;
            if (style == HudTextStyle.Kicker) tmp.characterSpacing = 28f;
            if (style == HudTextStyle.Headline) tmp.characterSpacing = 4f;
            if (style == HudTextStyle.Shout) tmp.characterSpacing = 7f;
            if (style == HudTextStyle.Body) tmp.lineSpacing = 8f;
            if (width > 0f)
            {
                tmp.rectTransform.sizeDelta = new Vector2(width, capHeight * 3f);
                if (fit)
                {
                    tmp.textWrappingMode = TextWrappingModes.NoWrap;
                    tmp.enableAutoSizing = true;
                    tmp.fontSizeMax = tmp.fontSize;
                    tmp.fontSizeMin = tmp.fontSize * 0.35f;
                }
                else
                {
                    tmp.textWrappingMode = TextWrappingModes.Normal;
                    tmp.overflowMode = TextOverflowModes.Overflow;
                }
            }
            else
            {
                tmp.rectTransform.sizeDelta = new Vector2(100f, capHeight * 3f);
                tmp.textWrappingMode = TextWrappingModes.NoWrap;
            }
            tmp.text = text;
            return tmp;
        }

        // Height actually used by the text (after wrapping), in its local units.
        public static float Height(TextMeshPro tmp)
        {
            tmp.ForceMeshUpdate();
            return tmp.textInfo.lineCount * tmp.fontSize * 0.1f * (tmp.font.faceInfo.lineHeight / tmp.font.faceInfo.pointSize);
        }

        // Width of the widest line, in its local units.
        public static float Width(TextMeshPro tmp)
        {
            tmp.ForceMeshUpdate();
            return tmp.textBounds.size.x;
        }

        public static void Alpha(TextMeshPro tmp, float alpha)
        {
            if (tmp != null) tmp.alpha = Mathf.Clamp01(alpha);
        }

        // Vertical gradient for headlines: light at the top, the accent colour below.
        public static void Gradient(TextMeshPro tmp, Color accent)
        {
            Color top = Color.Lerp(accent, Color.white, 0.6f);
            tmp.color = Color.white;
            tmp.enableVertexGradient = true;
            tmp.colorGradient = new VertexGradient(top, top, accent, accent);
        }
    }
}
