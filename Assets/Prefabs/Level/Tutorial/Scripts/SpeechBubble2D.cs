using AlmaGame.Systems;
using TMPro;
using UnityEngine;

namespace AlmaGame.Level
{
    // Comic speech bubble above a character (created by StoryMoment2D). Pops in with a bounce, types its line
    // letter by letter, shakes while a line shouts («…!»), bobs gently, and pops out. Keeps the same size on
    // screen whatever the camera zoom, and never gets wider than the view. Runs on unscaled time.
    public sealed class SpeechBubble2D : MonoBehaviour
    {
        const float TextCap = 0.46f, MaxWidth = 9f, PadX = 0.55f, PadY = 0.45f;
        const int Order = 900;   // over the level, under the HUD

        private Transform _anchor;
        private SpriteRenderer _body, _tail;
        private TextMeshPro _text, _name;
        private float _shownAt = -1f, _hiddenAt = -1f, _typeSpeed = 32f;
        private bool _shout;
        private Camera _camera;

        public static SpeechBubble2D Create(Transform anchor, Sprite body, Sprite tail, string speaker, Color accent)
        {
            var b = new GameObject("SpeechBubble").AddComponent<SpeechBubble2D>();
            b._anchor = anchor;
            b._camera = Camera.main;
            b._body = b.NewSprite("Body", body, Order);
            b._body.drawMode = SpriteDrawMode.Sliced;
            b._tail = b.NewSprite("Tail", tail, Order + 1);
            b._text = HudText.Create(b.transform, "Line", HudTextStyle.Bubble, "", TextCap, HudText.Ink, Order + 2, MaxWidth);
            if (!string.IsNullOrEmpty(speaker))
            {
                b._name = HudText.Create(b.transform, "Speaker", HudTextStyle.Kicker, speaker.ToUpperInvariant(), 0.3f, accent, Order + 3, MaxWidth, true);
                b._name.alignment = TextAlignmentOptions.Left;
            }
            b.gameObject.SetActive(false);
            return b;
        }

        private SpriteRenderer NewSprite(string n, Sprite s, int order)
        {
            var sr = new GameObject(n).AddComponent<SpriteRenderer>();
            sr.transform.SetParent(transform, false); sr.sprite = s; sr.sortingOrder = order;
            return sr;
        }

        public float TypeTime(string line) => line.Length / _typeSpeed;

        public void Say(string line)
        {
            gameObject.SetActive(true);
            _shout = line.TrimEnd().EndsWith("!");
            float k = Fit();
            _text.rectTransform.sizeDelta = new Vector2(Mathf.Min(MaxWidth, k), TextCap * 3f);
            // Measure the whole line first (bounds are invalid while no character is visible), then type it.
            _text.text = line;
            _text.maxVisibleCharacters = 99999;
            _text.ForceMeshUpdate();
            Vector2 size = _text.textBounds.size;
            _text.maxVisibleCharacters = 0;
            _body.size = new Vector2(Mathf.Max(size.x + PadX * 2f, 2.2f), size.y + PadY * 2f);
            _body.transform.localPosition = new Vector3(0f, _body.size.y * 0.5f + 0.32f, 0f);
            _text.transform.localPosition = _body.transform.localPosition;
            _tail.transform.localPosition = new Vector3(-0.15f, 0.22f, 0f);
            _tail.transform.localScale = Vector3.one * (0.45f / _tail.sprite.bounds.size.y);
            if (_name != null) _name.transform.localPosition = new Vector3(-_body.size.x * 0.5f + _name.rectTransform.sizeDelta.x * 0.5f + 0.1f, _body.size.y + 0.78f, 0f);
            _shownAt = Time.unscaledTime; _hiddenAt = -1f;
        }

        public void Hide() { if (_hiddenAt < 0f) _hiddenAt = Time.unscaledTime; }

        // Widest the text may be (HUD units) so the bubble stays inside the view.
        private float Fit()
        {
            if (_camera == null) return MaxWidth;
            float viewW = _camera.orthographicSize * 2f * _camera.aspect / Scale();
            return Mathf.Max(3f, viewW * 0.7f - PadX * 2f);
        }

        // World units per HUD unit, so the bubble keeps its screen size during zooms.
        private float Scale()
        {
            float zoom = _camera != null ? _camera.orthographicSize / 8f : 1f;
            return zoom * (Hud2D.Instance != null ? Hud2D.Instance.TextScale : 1f);
        }

        private static float Back(float t) { const float c = 1.9f; t -= 1f; return 1f + (c + 1f) * t * t * t + c * t * t; }

        private void LateUpdate()
        {
            if (_shownAt < 0f) return;
            float now = Time.unscaledTime, t = now - _shownAt;
            if (_anchor != null)
            {
                // Sit just above the speaker's top.
                Vector3 top = _anchor.position;
                var r = _anchor.GetComponentInChildren<Renderer>();
                if (r != null && r.enabled) top = new Vector3(r.bounds.center.x, r.bounds.max.y, 0f);
                transform.position = top + Vector3.up * (0.25f + 0.06f * Mathf.Sin(now * 2.4f));
            }
            int shown = Mathf.Min(_text.text.Length, Mathf.FloorToInt(Mathf.Max(0f, t - 0.15f) * _typeSpeed));
            _text.maxVisibleCharacters = shown;
            float pop = Mathf.Clamp01(t / 0.32f);
            float scale = pop <= 0f ? 0.01f : Mathf.LerpUnclamped(0.2f, 1f, Back(pop));
            if (_hiddenAt >= 0f)
            {
                float h = Mathf.Clamp01((now - _hiddenAt) / 0.22f);
                scale *= 1f - h * h;
                if (h >= 1f) { gameObject.SetActive(false); _shownAt = -1f; return; }
            }
            transform.localScale = Vector3.one * Scale() * Mathf.Max(0.01f, scale);
            // Shouting lines shake while they are being typed.
            bool typing = shown < _text.text.Length;
            Vector3 shake = _shout && typing ? (Vector3)(Random.insideUnitCircle * 0.05f) : Vector3.zero;
            _body.transform.localPosition = new Vector3(0f, _body.size.y * 0.5f + 0.32f, 0f) + shake;
            _text.transform.localPosition = _body.transform.localPosition;
        }
    }
}
