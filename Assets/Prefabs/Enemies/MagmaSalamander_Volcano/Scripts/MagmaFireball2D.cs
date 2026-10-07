using AlmaGame.Player;
using UnityEngine;

namespace AlmaGame.Enemies
{
    // Fireball spat by the magma salamander. It flies in a straight line towards where Alma was when
    // it left the mouth, trailing flames. It kills Alma on contact and bursts into embers on any solid
    // collider or after its lifetime. Movement is swept with a circle cast so it never tunnels.
    // All fireballs live under one "Enemy Projectiles" scene object to keep the Hierarchy tidy.
    public sealed class MagmaFireball2D : MonoBehaviour
    {
        private const float Radius = 0.2f;
        private const string ContainerName = "Enemy Projectiles";
        private static Transform s_container;
        private static Sprite s_sprite;

        private readonly RaycastHit2D[] _hits = new RaycastHit2D[4];
        private ParticleSystem _trailFx;
        private ParticleSystem _burstFx;
        private Vector2 _velocity;
        private float _dieAt;
        private float _nextFlameAt;

        // Clears every fireball in flight (e.g. when Alma respawns).
        public static void DestroyAll()
        {
            if (s_container == null) return;
            for (int i = s_container.childCount - 1; i >= 0; i--)
            {
                Transform child = s_container.GetChild(i);
                if (child.TryGetComponent(out MagmaFireball2D _)) Destroy(child.gameObject);
            }
        }

        public static MagmaFireball2D Launch(Vector3 position, Vector2 velocity, float lifetime, Material material,
            int sortingLayerId, int sortingOrder, ParticleSystem trailFx, ParticleSystem burstFx)
        {
            // Shared with other enemies' projectiles (e.g. poison globs).
            if (s_container == null)
            {
                GameObject existing = GameObject.Find(ContainerName);
                s_container = existing != null ? existing.transform : new GameObject(ContainerName).transform;
            }
            var go = new GameObject("MagmaFireball");
            go.transform.SetParent(s_container, false);
            go.transform.position = position;
            var fireball = go.AddComponent<MagmaFireball2D>();
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = FireSprite();
            renderer.sharedMaterial = material;
            renderer.sortingLayerID = sortingLayerId;
            renderer.sortingOrder = sortingOrder;
            fireball._velocity = velocity;
            fireball._dieAt = Time.time + lifetime;
            fireball._trailFx = trailFx;
            fireball._burstFx = burstFx;
            fireball.Flicker();
            return fireball;
        }

        private void FixedUpdate()
        {
            if (Time.time >= _dieAt)
            {
                Burst(transform.position);
                return;
            }

            Vector2 position = transform.position;
            Vector2 step = _velocity * Time.fixedDeltaTime;
            var filter = new ContactFilter2D { useTriggers = false };
            int count = Physics2D.CircleCast(position, Radius, step.normalized, filter, _hits, step.magnitude);
            if (count > 0)
            {
                // Results are sorted by distance: the first hit is what the fireball reaches first.
                RaycastHit2D hit = _hits[0];
                Rigidbody2D body = hit.rigidbody;
                if (body != null && body.TryGetComponent(out AlmaMotor2D alma) && !alma.IsDead) alma.Die();
                Burst(hit.centroid);
                return;
            }
            transform.position = position + step;
            Flicker();
            EmitFlame();
        }

        // Points along its velocity and pulses slightly in size, like a living flame.
        private void Flicker()
        {
            float angle = Mathf.Atan2(_velocity.y, _velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
            float pulse = 1f + 0.1f * Mathf.Sin(Time.time * 35f) + Random.Range(-0.04f, 0.04f);
            transform.localScale = new Vector3(1.15f * pulse, 0.95f * pulse, 1f);
        }

        private void EmitFlame()
        {
            if (_trailFx == null || Time.time < _nextFlameAt) return;
            _nextFlameAt = Time.time + 0.02f;
            var flame = new ParticleSystem.EmitParams
            {
                position = (Vector2)transform.position + Random.insideUnitCircle * Radius * 0.5f,
                velocity = -_velocity * 0.15f + Random.insideUnitCircle * 0.4f,
                applyShapeToPosition = false,
            };
            _trailFx.Emit(flame, 1);
        }

        public void Burst(Vector2 at)
        {
            if (_burstFx != null)
            {
                _burstFx.transform.position = at;
                _burstFx.Emit(14);
            }
            if (_trailFx != null)
            {
                var puff = new ParticleSystem.EmitParams { position = at, applyShapeToPosition = false };
                for (int i = 0; i < 5; i++)
                {
                    puff.velocity = Random.insideUnitCircle * 1.2f;
                    _trailFx.Emit(puff, 1);
                }
            }
            Destroy(gameObject);
        }

        // Soft round glow: white-yellow core fading to orange and transparent red at the edge.
        // 0.6 units wide. Generated once; also used as the particle texture.
        internal static Texture2D FireTexture() => FireSprite().texture;

        private static Sprite FireSprite()
        {
            if (s_sprite != null) return s_sprite;
            const int size = 32;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "MagmaFireball",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            var core = new Color(1f, 0.97f, 0.75f, 1f);
            var middle = new Color(1f, 0.6f, 0.12f, 1f);
            var rim = new Color(0.85f, 0.15f, 0.05f, 0f);
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float nx = (x + 0.5f) / size * 2f - 1f;
                float ny = (y + 0.5f) / size * 2f - 1f;
                float distance = Mathf.Sqrt(nx * nx + ny * ny);
                Color color = distance < 0.45f
                    ? Color.Lerp(core, middle, distance / 0.45f)
                    : Color.Lerp(middle, rim, Mathf.Clamp01((distance - 0.45f) / 0.55f));
                pixels[y * size + x] = color;
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            s_sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 53f);
            s_sprite.name = "MagmaFireball";
            return s_sprite;
        }
    }
}
