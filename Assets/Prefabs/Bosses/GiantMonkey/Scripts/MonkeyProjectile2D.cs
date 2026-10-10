using AlmaGame.Level;
using AlmaGame.Player;
using UnityEngine;

namespace AlmaGame.Bosses
{
    // Thrown by the Giant Monkey. Rolling fruit: drops to the ground and rolls; coconut: high lob that bounces
    // a few times. It has a circle trigger on a kinematic body: touching Alma kills her and bursts the fruit;
    // hitting a wall (any solid collider at its side) bursts it too. Landing on the ground does not.
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public sealed class MonkeyProjectile2D : MonoBehaviour
    {
        public enum Kind { RollingFruit, Coconut }

        private Kind _kind;
        private Vector2 _velocity;
        private float _radius, _rollSpeed, _gravity = 22f, _limitX;
        private int _bounces;
        private bool _rolling;
        private AlmaMotor2D _alma;
        private Transform _visual;
        private Rigidbody2D _rb;
        private bool _burst;
        private Color _burstColor = new Color(1f, 0.55f, 0.15f);

        public static MonkeyProjectile2D Throw(Kind kind, Vector2 from, Vector2 velocity, float rollSpeed, float radius,
            Sprite sprite, Color tint, float limitX, AlmaMotor2D alma)
        {
            var go = new GameObject(kind.ToString());
            go.transform.position = from;
            var rb = go.AddComponent<Rigidbody2D>(); rb.bodyType = RigidbodyType2D.Kinematic; rb.useFullKinematicContacts = true; rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            var col = go.AddComponent<CircleCollider2D>(); col.isTrigger = true; col.radius = radius;
            var p = go.AddComponent<MonkeyProjectile2D>();
            p._rb = rb;
            if (kind == Kind.Coconut) p._burstColor = new Color(0.55f, 0.36f, 0.2f);
            p._kind = kind; p._velocity = velocity; p._rollSpeed = rollSpeed; p._radius = radius; p._limitX = limitX; p._alma = alma;
            var sr = new GameObject("Visual").AddComponent<SpriteRenderer>();
            sr.transform.SetParent(p.transform, false);
            sr.sprite = sprite; sr.color = tint; sr.sortingOrder = 30;
            sr.transform.localScale = Vector3.one * (radius * 2f / Mathf.Max(0.01f, sprite.bounds.size.x));
            p._visual = sr.transform;
            return p;
        }

        private void FixedUpdate()
        {
            if (_burst) return;
            float dt = Time.fixedDeltaTime;
            Vector2 pos = transform.position;
            if (!_rolling) _velocity.y -= _gravity * dt;
            Vector2 next = pos + _velocity * dt;
            // Ground under the projectile (any solid collider that isn't Alma).
            if (_velocity.y <= 0f && GroundBelow(pos, out float groundY) && next.y - _radius <= groundY)
            {
                next.y = groundY + _radius;
                if (_kind == Kind.RollingFruit || _bounces >= 2)
                {
                    _rolling = true;
                    if (_kind == Kind.RollingFruit) _velocity = new Vector2(Mathf.Sign(_velocity.x == 0f ? 1f : _velocity.x) * _rollSpeed, 0f);
                    else _velocity.y = 0f;
                }
                else { _bounces++; _velocity.y = -_velocity.y * 0.62f; }
            }
            else if (_rolling && !GroundBelow(next + Vector2.up * _radius, out _)) _rolling = false;   // rolled off an edge
            _rb.MovePosition(next);
            _visual.Rotate(0f, 0f, -_velocity.x / _radius * Mathf.Rad2Deg * dt);
            if (Mathf.Abs(next.x) > _limitX || next.y < -30f) Destroy(gameObject);   // safety: never leaves the arena alive
        }

        private void OnTriggerEnter2D(Collider2D other) => Touch(other);
        private void OnTriggerStay2D(Collider2D other) => Touch(other);

        private void Touch(Collider2D other)
        {
            if (_burst) return;
            var body = other.attachedRigidbody;
            if (body != null && body.TryGetComponent(out AlmaMotor2D alma))
            {
                if (!alma.IsDead) alma.Die();
                Burst();
                return;
            }
            if (other.isTrigger || other.GetComponentInParent<MonkeyProjectile2D>() != null) return;
            // A wall: the closest point of that collider is beside the fruit, not under it (the ground).
            Vector2 c = _rb.position, p = other.ClosestPoint(c);
            Vector2 d = p - c;
            if (Mathf.Abs(d.x) > Mathf.Abs(d.y) && Mathf.Sign(d.x) == Mathf.Sign(_velocity.x == 0f ? d.x : _velocity.x)) Burst();
        }

        // Splatters into pulp and shell bits with a flash, then disappears.
        private void Burst()
        {
            _burst = true;
            GetComponent<Collider2D>().enabled = false;
            _visual.gameObject.SetActive(false);
            var mat = new Material(Shader.Find("Sprites/Default"));
            var chunks = HazardFx.CreateParticles("Burst", null, mat, HazardFx.Chunk(), 30, 0, 31, true);
            chunks.transform.position = transform.position;
            var m = chunks.main; m.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.8f); m.startSpeed = new ParticleSystem.MinMaxCurve(3f, 7f);
            m.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.3f); m.gravityModifier = 2.2f; m.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.28f);
            m.startColor = new ParticleSystem.MinMaxGradient(_burstColor, new Color(0.45f, 0.26f, 0.12f));
            var sh = chunks.shape; sh.enabled = true; sh.shapeType = ParticleSystemShapeType.Circle; sh.radius = _radius * 0.6f;
            chunks.Emit(18);
            var juice = HazardFx.CreateParticles("Juice", null, mat, HazardFx.Puff(), 20, 0, 30, true);
            juice.transform.position = transform.position;
            var j = juice.main; j.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.45f); j.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3.5f);
            j.startSize = new ParticleSystem.MinMaxCurve(0.4f, 0.8f); j.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.75f, 0.3f, 0.9f), new Color(1f, 0.5f, 0.15f, 0.8f));
            HazardFx.SetSizeOverLifetime(juice, 0.6f, 1.4f);
            juice.Emit(10);
            Destroy(chunks.gameObject, 1.2f); Destroy(juice.gameObject, 1.2f);
            var flash = new GameObject("Flash").AddComponent<SpriteRenderer>();
            flash.sprite = HazardFx.Glow(); flash.sortingOrder = 32; flash.color = new Color(1f, 0.9f, 0.6f, 0.9f);
            flash.transform.position = transform.position; flash.transform.localScale = Vector3.one * (_radius * 4f / HazardFx.Glow().bounds.size.x);
            Destroy(flash.gameObject, 0.08f);
            Destroy(gameObject, 0.05f);
        }

        private readonly RaycastHit2D[] _hits = new RaycastHit2D[8];

        private bool GroundBelow(Vector2 from, out float y)
        {
            y = float.NegativeInfinity;
            int n = Physics2D.RaycastNonAlloc(from + Vector2.up * 0.2f, Vector2.down, _hits, 3f);
            for (int i = 0; i < n; i++)
            {
                var c = _hits[i].collider;
                if (c.isTrigger || (c.attachedRigidbody != null && c.attachedRigidbody.GetComponent<AlmaMotor2D>() != null)) continue;
                y = _hits[i].point.y; return true;
            }
            return false;
        }
    }
}
