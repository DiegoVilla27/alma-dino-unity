using AlmaGame.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AlmaGame.Level
{
    // Ninja vanish/appear effect: a burst of smoke puffs, a few bits (leaves, rock chips...), a flash and a small
    // camera shake. Used by the bosses (intro, hits, burrowing, defeat) and by story moments. Cleans itself up.
    // Its colours follow the world of the current scene (Level_N_* / Boss_N): grey smoke and green leaves in
    // the jungle, black smoke and violet amethyst chips in the caves, and so on.
    public sealed class NinjaSmoke2D : MonoBehaviour
    {
        private struct Palette { public Color Smoke1, Smoke2, Bits1, Bits2, Flash; public bool Chips; }   // Chips: angular rock bits

        private static readonly Palette[] s_worlds =
        {
            // 1 Jungle: grey smoke, green leaves.
            new Palette { Smoke1 = new Color(0.85f, 0.85f, 0.88f), Smoke2 = new Color(0.6f, 0.6f, 0.66f), Bits1 = new Color(0.45f, 0.75f, 0.3f), Bits2 = new Color(0.3f, 0.55f, 0.2f), Flash = Color.white },
            // 2 Caves: black smoke, violet amethyst chips (the colours of the cave floor).
            new Palette { Smoke1 = new Color(0.08f, 0.06f, 0.1f), Smoke2 = new Color(0.2f, 0.17f, 0.24f), Bits1 = new Color(0.6f, 0.3f, 0.9f), Bits2 = new Color(0.38f, 0.1f, 0.55f), Flash = new Color(0.8f, 0.62f, 1f), Chips = true },
            // 3 Swamp: murky green-grey smoke, toxic lime and purple drops.
            new Palette { Smoke1 = new Color(0.45f, 0.5f, 0.4f), Smoke2 = new Color(0.28f, 0.32f, 0.26f), Bits1 = new Color(0.65f, 0.9f, 0.2f), Bits2 = new Color(0.5f, 0.25f, 0.6f), Flash = new Color(0.85f, 1f, 0.6f) },
            // 4 Volcano: charcoal smoke, orange embers (no vegetation in this world).
            new Palette { Smoke1 = new Color(0.12f, 0.1f, 0.1f), Smoke2 = new Color(0.3f, 0.25f, 0.24f), Bits1 = new Color(1f, 0.55f, 0.1f), Bits2 = new Color(1f, 0.85f, 0.35f), Flash = new Color(1f, 0.8f, 0.5f) },
        };

        // World of the active scene (1 to 4) from its name: "Level_2_3" → 2, "Boss_2" → 2, "Boss_Final" → 4.
        private static Palette Current()
        {
            string n = SceneManager.GetActiveScene().name;
            int world = 1;
            if (n == "Boss_Final") world = 4;
            else
            {
                int us = n.IndexOf('_');
                if (us >= 0 && us + 1 < n.Length && char.IsDigit(n[us + 1])) world = n[us + 1] - '0';
            }
            return s_worlds[Mathf.Clamp(world, 1, s_worlds.Length) - 1];
        }

        private Color _flashColor;
        private SpriteRenderer _flash;
        private float _t;
        private const float FlashTime = 0.35f, Life = 1.6f;

        public static void Burst(Vector2 at, float shake = 0.15f)
        {
            Palette pal = Current();
            var go = new GameObject("NinjaSmoke");
            go.transform.position = at;
            var mat = new Material(Shader.Find("Sprites/Default"));
            var smoke = HazardFx.CreateParticles("Smoke", go.transform, mat, HazardFx.Puff(), 80, 0, 40, true);
            var m = smoke.main; m.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.1f); m.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 5f);
            m.startSize = new ParticleSystem.MinMaxCurve(1f, 2.2f); m.startColor = new ParticleSystem.MinMaxGradient(pal.Smoke1, pal.Smoke2);
            m.gravityModifier = -0.15f;
            var sh = smoke.shape; sh.enabled = true; sh.shapeType = ParticleSystemShapeType.Circle; sh.radius = 1.2f;
            HazardFx.SetSizeOverLifetime(smoke, 0.6f, 1.6f);
            var leaves = HazardFx.CreateParticles("Bits", go.transform, mat, pal.Chips ? HazardFx.Chunk() : HazardFx.Puff(), 20, 0, 41, true);
            var lm = leaves.main; lm.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.4f); lm.startSpeed = new ParticleSystem.MinMaxCurve(3f, 7f);
            lm.startSize = new ParticleSystem.MinMaxCurve(0.18f, 0.32f); lm.startColor = new ParticleSystem.MinMaxGradient(pal.Bits1, pal.Bits2);
            lm.gravityModifier = 0.6f; lm.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.28f);
            var lsh = leaves.shape; lsh.enabled = true; lsh.shapeType = ParticleSystemShapeType.Circle; lsh.radius = 0.8f;
            smoke.transform.localPosition = Vector3.zero; leaves.transform.localPosition = Vector3.zero;
            smoke.Emit(45); leaves.Emit(14);
            var fx = go.AddComponent<NinjaSmoke2D>();
            fx._flashColor = pal.Flash;
            fx._flash = new GameObject("VanishFlash").AddComponent<SpriteRenderer>();
            fx._flash.transform.SetParent(go.transform, false);
            fx._flash.sprite = HazardFx.Glow(); fx._flash.sortingOrder = 42;
            var cam = FindAnyObjectByType<AlmaCameraFollow>();
            if (cam != null && shake > 0f) cam.Shake(shake, 0.25f);
        }

        private void Update()
        {
            _t += Time.deltaTime;
            if (_flash != null)
            {
                float u = Mathf.Clamp01(_t / FlashTime);
                _flash.transform.localScale = Vector3.one * (2f + 5f * u) / HazardFx.Glow().bounds.size.x;
                _flash.color = new Color(_flashColor.r, _flashColor.g, _flashColor.b, 1f - u);
                if (u >= 1f) { Destroy(_flash.gameObject); _flash = null; }
            }
            if (_t >= Life) Destroy(gameObject);
        }
    }
}
