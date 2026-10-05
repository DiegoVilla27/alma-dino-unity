using AlmaGame.Player;
using UnityEngine;

namespace AlmaGame.Enemies
{
    // Crystal shard thrown by the crystal beetle. It first "charges" (grows on the beetle's back as
    // a warning), then flies on a ballistic arc. It kills Alma on contact and shatters on any solid
    // collider or after its lifetime. Movement is swept with a circle cast so it never tunnels.
    // All shards live under one "Enemy Projectiles" scene object to keep the Hierarchy tidy.
    public sealed class CrystalShard2D : MonoBehaviour
    {
        private const float Radius = 0.15f;
        private static Transform s_container;
        private static Sprite s_sprite;

        private readonly RaycastHit2D[] _hits = new RaycastHit2D[4];
        private SpriteRenderer _renderer;
        private ParticleSystem _breakFx;
        private Vector2 _velocity;
        private float _gravity;
        private float _dieAt;
        private bool _launched;

        public static CrystalShard2D Create(Color color, Material material, int sortingLayerId, int sortingOrder)
        {
            if (s_container == null) s_container = new GameObject("Enemy Projectiles").transform;
            var go = new GameObject("CrystalShard");
            go.transform.SetParent(s_container, false);
            var shard = go.AddComponent<CrystalShard2D>();
            shard._renderer = go.AddComponent<SpriteRenderer>();
            shard._renderer.sprite = ShardSprite();
            shard._renderer.sharedMaterial = material;
            shard._renderer.color = color;
            shard._renderer.sortingLayerID = sortingLayerId;
            shard._renderer.sortingOrder = sortingOrder;
            return shard;
        }

        // Warning phase: held at the beetle's back, pointing up and growing from 20 % to full size.
        public void Charge(Vector3 position, float progress)
        {
            transform.SetPositionAndRotation(position, Quaternion.Euler(0f, 0f, 90f));
            transform.localScale = Vector3.one * Mathf.Lerp(0.2f, 1f, Mathf.Clamp01(progress));
        }

        public void Launch(Vector2 velocity, float gravity, float lifetime, ParticleSystem breakFx)
        {
            _velocity = velocity;
            _gravity = gravity;
            _dieAt = Time.time + lifetime;
            _breakFx = breakFx;
            transform.localScale = Vector3.one;
            _launched = true;
            FaceVelocity();
        }

        private void FixedUpdate()
        {
            if (!_launched) return;
            if (Time.time >= _dieAt)
            {
                Shatter(transform.position);
                return;
            }

            _velocity.y -= _gravity * Time.fixedDeltaTime;
            Vector2 position = transform.position;
            Vector2 step = _velocity * Time.fixedDeltaTime;
            var filter = new ContactFilter2D { useTriggers = false };
            int count = Physics2D.CircleCast(position, Radius, step.normalized, filter, _hits, step.magnitude);
            if (count > 0)
            {
                // Results are sorted by distance: the first hit is what the shard reaches first.
                RaycastHit2D hit = _hits[0];
                Rigidbody2D body = hit.rigidbody;
                if (body != null && body.TryGetComponent(out AlmaMotor2D alma) && !alma.IsDead) alma.Die();
                Shatter(hit.centroid);
                return;
            }
            transform.position = position + step;
            FaceVelocity();
        }

        private void FaceVelocity()
        {
            float angle = Mathf.Atan2(_velocity.y, _velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        public void Shatter(Vector2 at)
        {
            if (_breakFx != null)
            {
                _breakFx.transform.position = at;
                _breakFx.Emit(6);
            }
            Destroy(gameObject);
        }

        // Elongated crystal pointing to +X with a bright core line, 0.75 × 0.3 units. Generated once.
        internal static Texture2D ShardTexture() => ShardSprite().texture;

        private static Sprite ShardSprite()
        {
            if (s_sprite != null) return s_sprite;
            const int width = 48;
            const int height = 20;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = "CrystalShard",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            var pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                float nx = (x + 0.5f) / width * 2f - 1f;
                float ny = Mathf.Abs((y + 0.5f) / height * 2f - 1f);
                // Hexagonal shard: flat middle, pointed ends (longer point at the front).
                float taper = nx > 0f ? 1f - Mathf.Clamp01((nx - 0.3f) / 0.7f) : 1f - Mathf.Clamp01((-nx - 0.6f) / 0.4f);
                float inside = Mathf.Clamp01((taper - ny) * 8f);
                float core = Mathf.Lerp(0.75f, 1f, Mathf.Clamp01(1f - ny * 3f));
                byte shade = (byte)(core * 255f);
                pixels[y * width + x] = new Color32(shade, shade, shade, (byte)(inside * 255f));
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            s_sprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 64f);
            s_sprite.name = "CrystalShard";
            return s_sprite;
        }
    }
}
