using AlmaGame.Player;
using UnityEngine;

namespace AlmaGame.Enemies
{
    // Poison glob spat by the poison toad. It flies on a fixed lob (forward and a bit up, pulled down
    // by gravity), dripping poison as it goes. It kills Alma on contact and splashes on any solid
    // collider or after its lifetime. Movement is swept with a circle cast so it never tunnels.
    // All globs live under one "Enemy Projectiles" scene object to keep the Hierarchy tidy.
    public sealed class PoisonSpit2D : MonoBehaviour
    {
        private const float Radius = 0.18f;
        private const string ContainerName = "Enemy Projectiles";
        private static Transform s_container;
        private static Sprite s_sprite;

        private readonly RaycastHit2D[] _hits = new RaycastHit2D[4];
        private SpriteRenderer _renderer;
        private ParticleSystem _trailFx;
        private ParticleSystem _splashFx;
        private Vector2 _velocity;
        private float _gravity;
        private float _dieAt;
        private float _bornAt;
        private float _nextDripAt;

        // Clears every glob in flight (e.g. when Alma respawns).
        public static void DestroyAll()
        {
            if (s_container == null) return;
            for (int i = s_container.childCount - 1; i >= 0; i--)
            {
                Transform child = s_container.GetChild(i);
                if (child.TryGetComponent(out PoisonSpit2D _)) Destroy(child.gameObject);
            }
        }

        public static PoisonSpit2D Launch(Vector3 position, Vector2 velocity, float gravity, float lifetime,
            Color color, Material material, int sortingLayerId, int sortingOrder,
            ParticleSystem trailFx, ParticleSystem splashFx)
        {
            // Shared with other enemies' projectiles (e.g. crystal shards).
            if (s_container == null)
            {
                GameObject existing = GameObject.Find(ContainerName);
                s_container = existing != null ? existing.transform : new GameObject(ContainerName).transform;
            }
            var go = new GameObject("PoisonSpit");
            go.transform.SetParent(s_container, false);
            go.transform.position = position;
            var spit = go.AddComponent<PoisonSpit2D>();
            spit._renderer = go.AddComponent<SpriteRenderer>();
            spit._renderer.sprite = GlobSprite();
            spit._renderer.sharedMaterial = material;
            spit._renderer.color = color;
            spit._renderer.sortingLayerID = sortingLayerId;
            spit._renderer.sortingOrder = sortingOrder;
            spit._velocity = velocity;
            spit._gravity = gravity;
            spit._bornAt = Time.time;
            spit._dieAt = Time.time + lifetime;
            spit._trailFx = trailFx;
            spit._splashFx = splashFx;
            spit.UpdateShape();
            return spit;
        }

        private void FixedUpdate()
        {
            if (Time.time >= _dieAt)
            {
                Splash(transform.position);
                return;
            }

            _velocity.y -= _gravity * Time.fixedDeltaTime;
            Vector2 position = transform.position;
            Vector2 step = _velocity * Time.fixedDeltaTime;
            var filter = new ContactFilter2D { useTriggers = false };
            int count = Physics2D.CircleCast(position, Radius, step.normalized, filter, _hits, step.magnitude);
            if (count > 0)
            {
                // Results are sorted by distance: the first hit is what the glob reaches first.
                RaycastHit2D hit = _hits[0];
                Rigidbody2D body = hit.rigidbody;
                if (body != null && body.TryGetComponent(out AlmaMotor2D alma) && !alma.IsDead) alma.Die();
                Splash(hit.centroid);
                return;
            }
            transform.position = position + step;
            UpdateShape();
            Drip();
        }

        // Points along its velocity, stretched a little by speed, with a quick wobble right after leaving the mouth.
        private void UpdateShape()
        {
            float angle = Mathf.Atan2(_velocity.y, _velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
            float age = Time.time - _bornAt;
            float wobble = Mathf.Sin(age * 40f) * 0.12f * Mathf.Clamp01(1f - age * 3f);
            float stretch = Mathf.Clamp(_velocity.magnitude / 8f, 0.8f, 1.25f);
            transform.localScale = new Vector3(stretch + wobble, 1f / stretch - wobble, 1f);
        }

        private void Drip()
        {
            if (_trailFx == null || Time.time < _nextDripAt) return;
            _nextDripAt = Time.time + 0.03f;
            var drip = new ParticleSystem.EmitParams
            {
                position = (Vector2)transform.position + Random.insideUnitCircle * Radius * 0.6f,
                velocity = -_velocity * 0.08f + Random.insideUnitCircle * 0.3f,
                applyShapeToPosition = false,
            };
            _trailFx.Emit(drip, 1);
        }

        public void Splash(Vector2 at)
        {
            if (_splashFx != null)
            {
                _splashFx.transform.position = at;
                _splashFx.Emit(10);
            }
            Destroy(gameObject);
        }

        // Soft round blob with a bright highlight, 0.5 units wide. Generated once.
        internal static Texture2D GlobTexture() => GlobSprite().texture;

        private static Sprite GlobSprite()
        {
            if (s_sprite != null) return s_sprite;
            const int size = 32;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "PoisonGlob",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float nx = (x + 0.5f) / size * 2f - 1f;
                float ny = (y + 0.5f) / size * 2f - 1f;
                float distance = Mathf.Sqrt(nx * nx + ny * ny);
                float alpha = Mathf.Clamp01((1f - distance) * 6f);
                // Darker rim, lighter centre and a white highlight towards the upper front.
                float shade = Mathf.Lerp(1f, 0.6f, Mathf.Clamp01(distance * distance));
                float highlight = Mathf.Clamp01(1f - Vector2.Distance(new Vector2(nx, ny), new Vector2(0.3f, 0.35f)) / 0.3f);
                byte value = (byte)(Mathf.Lerp(shade, 1f, highlight) * 255f);
                pixels[y * size + x] = new Color32(value, value, value, (byte)(alpha * 255f));
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            s_sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 64f);
            s_sprite.name = "PoisonGlob";
            return s_sprite;
        }
    }
}
