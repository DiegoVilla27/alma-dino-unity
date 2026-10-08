using System.Collections.Generic;
using AlmaGame.Level;
using UnityEngine;

namespace AlmaGame.Systems
{
    // HUD egg indicator (GDD 8.1): the four children in the top-left corner. Eggs not rescued yet are small, dim
    // silhouettes; rescued ones are bigger, in full colour, with a soft breathing glow. When Alma picks up an egg
    // it flies from where it was, in an arc, up to its slot, shrinking and trailing sparkles; on arrival the slot
    // pops in with a flash, a ring of sparkles and two heartbeats (FlyIn). Already-rescued eggs (from the save)
    // start lit.
    [DisallowMultipleComponent, RequireComponent(typeof(Hud2D))]
    public sealed class HudEggIndicator : MonoBehaviour
    {
        [System.Serializable]
        public struct Egg
        {
            public string Id;
            public Sprite Sprite;
            public Color Color;
        }

        [SerializeField] private Egg[] _eggs = new Egg[0];
        [Tooltip("Distance of the first egg from the top-left corner (HUD units, camera size 8).")]
        [SerializeField] private Vector2 _inset = new Vector2(0.95f, 0.85f);
        [SerializeField, Min(0.1f)] private float _spacing = 0.95f;
        [Tooltip("Height of a rescued egg (HUD units).")]
        [SerializeField, Min(0.1f)] private float _litHeight = 0.9f;
        [Tooltip("Size of a missing egg, relative to a rescued one.")]
        [SerializeField, Range(0.3f, 1f)] private float _dimScale = 0.75f;
        [SerializeField] private Color _dimColor = new Color(0.7f, 0.7f, 0.78f, 0.5f);
        [Tooltip("Soft dark halo behind every slot, so the eggs read on bright and dark backdrops alike.")]
        [SerializeField, Range(0f, 1f)] private float _backingAlpha = 0.4f;
        [SerializeField, Min(0.1f)] private float _flightTime = 1.1f;

        private class Slot
        {
            public Egg Egg;
            public Transform Root;
            public SpriteRenderer Back, Body, Glow, Flash;
            public bool Lit;
            public float LitAt = -100f;      // time the pop-in started
        }

        private class Flight
        {
            public Slot Slot;
            public SpriteRenderer Body, Glow;
            public Vector3 From;
            public float StartedAt;
        }

        private Hud2D _hud;
        private readonly List<Slot> _slots = new List<Slot>();
        private readonly List<Flight> _flights = new List<Flight>();
        private ParticleSystem _sparkles;
        private ParticleSystem _trail;
        private float _eggScale = 1f;

        public static HudEggIndicator Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            _hud = GetComponent<Hud2D>();
            int order = _hud.SortingOrder;
            var material = new Material(Shader.Find("Sprites/Default"));
            for (int i = 0; i < _eggs.Length; i++)
            {
                var slot = new Slot { Egg = _eggs[i] };
                slot.Root = new GameObject("Egg_" + _eggs[i].Id).transform;
                slot.Root.SetParent(transform, false);
                slot.Back = NewRenderer("Back", slot.Root, HazardFx.Glow(), order - 1);
                slot.Glow = NewRenderer("Glow", slot.Root, HazardFx.Glow(), order);
                slot.Body = NewRenderer("Body", slot.Root, _eggs[i].Sprite, order + 1);
                slot.Flash = NewRenderer("Flash", slot.Root, HazardFx.Glow(), order + 2);
                _slots.Add(slot);
            }
            if (_eggs.Length > 0 && _eggs[0].Sprite != null) _eggScale = _litHeight / _eggs[0].Sprite.bounds.size.y;

            _sparkles = HazardFx.CreateParticles("Sparkles", transform, material, HazardFx.Puff(), 60, 0, order + 3, false);
            var main = _sparkles.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.2f, 2.6f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.16f);
            var shape = _sparkles.shape; shape.shapeType = ParticleSystemShapeType.Circle; shape.radius = 0.15f;
            HazardFx.SetSizeOverLifetime(_sparkles, 1f, 0.1f);

            _trail = HazardFx.CreateParticles("Trail", null, material, HazardFx.Puff(), 120, 0, order + 3, true);
            var tmain = _trail.main;
            tmain.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.6f);
            tmain.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.5f);
            tmain.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.22f);
            HazardFx.SetSizeOverLifetime(_trail, 1f, 0.1f);
        }

        private void Start()
        {
            foreach (var slot in _slots)
            {
                slot.Lit = GameProgress.Instance != null && GameProgress.Instance.IsEggRescued(slot.Egg.Id);
                slot.LitAt = -100f;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            HazardFx.DestroyMaterial(_sparkles);
            HazardFx.DestroyMaterial(_trail);
            if (_trail != null) Destroy(_trail.gameObject);
        }

        private SpriteRenderer NewRenderer(string name, Transform parent, Sprite sprite, int order)
        {
            var sr = new GameObject(name).AddComponent<SpriteRenderer>();
            sr.transform.SetParent(parent, false);
            sr.sprite = sprite;
            sr.sortingOrder = order;
            return sr;
        }

        private Vector2 SlotPosition(int index) => _hud.TopLeft(_inset) + Vector2.right * (_spacing * index);

        // An egg was picked up at `worldPosition`: it flies to its slot, which then lights up.
        public void FlyIn(string eggId, Vector3 worldPosition)
        {
            Slot slot = _slots.Find(s => s.Egg.Id == eggId);
            if (slot == null) return;
            var flight = new Flight { Slot = slot, From = worldPosition, StartedAt = Time.time };
            flight.Glow = NewRenderer("FlyingGlow", null, HazardFx.Glow(), _hud.SortingOrder + 4);
            flight.Body = NewRenderer("FlyingEgg", null, slot.Egg.Sprite, _hud.SortingOrder + 5);
            var main = _trail.main;
            main.startColor = new ParticleSystem.MinMaxGradient(slot.Egg.Color, Color.white);
            _flights.Add(flight);
        }

        private void LateUpdate()
        {
            float vis = _hud.Visibility;
            float scale = _hud.Scale;
            for (int i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];
                slot.Root.localPosition = SlotPosition(i);
                bool arriving = _flights.Exists(f => f.Slot == slot);
                float since = Time.time - slot.LitAt;
                float size, glow, flash = 0f;
                Color body;
                if (!slot.Lit || arriving)
                {
                    size = _dimScale; glow = 0f; body = _dimColor;
                }
                else
                {
                    // Pop-in: overshoot and settle in 0.45 s; a white flash fades in 0.5 s; then two heartbeats.
                    float pop = since < 0.45f ? 1f + 0.45f * Mathf.Sin(since / 0.45f * Mathf.PI) * (1f - since / 0.45f) : 1f;
                    float beat = 0f;
                    for (int b = 0; b < 2; b++)
                    {
                        float t = since - 0.6f - b * 0.55f;
                        if (t > 0f && t < 0.3f) beat = Mathf.Max(beat, Mathf.Sin(t / 0.3f * Mathf.PI));
                    }
                    size = pop + 0.12f * beat;
                    glow = 0.55f + 0.12f * Mathf.Sin(Time.time * 2.2f + i) + 0.35f * beat + (since < 1f ? 0.6f * (1f - since) : 0f);
                    flash = since < 0.5f ? 1f - since / 0.5f : 0f;
                    body = Color.white;
                }
                slot.Back.transform.localScale = Vector3.one * (_litHeight * 1.5f);
                slot.Back.color = new Color(0.05f, 0.04f, 0.07f, _backingAlpha * vis);
                slot.Body.transform.localScale = Vector3.one * (_eggScale * size);
                slot.Body.color = new Color(body.r, body.g, body.b, body.a * vis);
                slot.Glow.transform.localScale = Vector3.one * (_litHeight * 1.9f * size);
                slot.Glow.color = new Color(slot.Egg.Color.r, slot.Egg.Color.g, slot.Egg.Color.b, Mathf.Clamp01(glow) * 0.6f * vis);
                slot.Flash.transform.localScale = Vector3.one * (_litHeight * (1.2f + 1.6f * (1f - flash)));
                slot.Flash.color = new Color(1f, 1f, 1f, flash * 0.9f * vis);
            }

            for (int i = _flights.Count - 1; i >= 0; i--)
            {
                var f = _flights[i];
                float t = Mathf.Clamp01((Time.time - f.StartedAt) / _flightTime);
                int index = _slots.IndexOf(f.Slot);
                Vector3 to = _hud.ToWorld(SlotPosition(index));
                // A lift first (the egg rises out of the level), then an eased arc to the slot.
                float e = t * t * (3f - 2f * t);
                Vector3 control = Vector3.Lerp(f.From, to, 0.25f) + Vector3.up * (2.5f * scale);
                Vector3 p = Vector3.Lerp(Vector3.Lerp(f.From, control, e), Vector3.Lerp(control, to, e), e);
                float wobble = Mathf.Sin(t * Mathf.PI * 3f) * 12f * (1f - t);
                float s = Mathf.Lerp(1f, scale, e) * (1f + 0.35f * Mathf.Sin(t * Mathf.PI));   // grows a little mid-flight
                f.Body.transform.position = p;
                f.Body.transform.rotation = Quaternion.Euler(0f, 0f, wobble);
                f.Body.transform.localScale = Vector3.one * (Mathf.Lerp(1f, _eggScale, e) * s);
                f.Glow.transform.position = p;
                f.Glow.transform.localScale = Vector3.one * (1.6f * s);
                f.Glow.color = new Color(f.Slot.Egg.Color.r, f.Slot.Egg.Color.g, f.Slot.Egg.Color.b, 0.7f);
                _trail.transform.position = p;
                _trail.Emit(2);
                if (t >= 1f)
                {
                    Destroy(f.Body.gameObject); Destroy(f.Glow.gameObject);
                    _flights.RemoveAt(i);
                    f.Slot.Lit = true;
                    f.Slot.LitAt = Time.time;
                    _sparkles.transform.localPosition = SlotPosition(index);
                    var main = _sparkles.main;
                    main.startColor = new ParticleSystem.MinMaxGradient(f.Slot.Egg.Color, Color.white);
                    _sparkles.Emit(22);
                }
            }
        }
    }
}
