using AlmaGame.Player;
using UnityEngine;

namespace AlmaGame.Level
{
    // Ninja vanish/appear effect: a burst of grey smoke puffs, a few leaves, a white flash and a small camera
    // shake. Used by the Giant Monkey (boss intro, hits, defeat) and by story moments. Cleans itself up.
    public sealed class NinjaSmoke2D : MonoBehaviour
    {
        private SpriteRenderer _flash;
        private float _t;
        private const float FlashTime = 0.35f, Life = 1.6f;

        public static void Burst(Vector2 at, float shake = 0.15f)
        {
            var go = new GameObject("NinjaSmoke");
            go.transform.position = at;
            var mat = new Material(Shader.Find("Sprites/Default"));
            var smoke = HazardFx.CreateParticles("Smoke", go.transform, mat, HazardFx.Puff(), 80, 0, 40, true);
            var m = smoke.main; m.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.1f); m.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 5f);
            m.startSize = new ParticleSystem.MinMaxCurve(1f, 2.2f); m.startColor = new ParticleSystem.MinMaxGradient(new Color(0.85f, 0.85f, 0.88f), new Color(0.6f, 0.6f, 0.66f));
            m.gravityModifier = -0.15f;
            var sh = smoke.shape; sh.enabled = true; sh.shapeType = ParticleSystemShapeType.Circle; sh.radius = 1.2f;
            HazardFx.SetSizeOverLifetime(smoke, 0.6f, 1.6f);
            var leaves = HazardFx.CreateParticles("Leaves", go.transform, mat, HazardFx.Puff(), 20, 0, 41, true);
            var lm = leaves.main; lm.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.4f); lm.startSpeed = new ParticleSystem.MinMaxCurve(3f, 7f);
            lm.startSize = new ParticleSystem.MinMaxCurve(0.18f, 0.32f); lm.startColor = new ParticleSystem.MinMaxGradient(new Color(0.45f, 0.75f, 0.3f), new Color(0.3f, 0.55f, 0.2f));
            lm.gravityModifier = 0.6f; lm.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.28f);
            var lsh = leaves.shape; lsh.enabled = true; lsh.shapeType = ParticleSystemShapeType.Circle; lsh.radius = 0.8f;
            smoke.transform.localPosition = Vector3.zero; leaves.transform.localPosition = Vector3.zero;
            smoke.Emit(45); leaves.Emit(14);
            var fx = go.AddComponent<NinjaSmoke2D>();
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
                _flash.color = new Color(1f, 1f, 1f, 1f - u);
                if (u >= 1f) { Destroy(_flash.gameObject); _flash = null; }
            }
            if (_t >= Life) Destroy(gameObject);
        }
    }
}
