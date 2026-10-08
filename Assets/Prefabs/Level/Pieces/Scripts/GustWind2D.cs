using UnityEngine;

namespace AlmaGame.Level
{
    // Turns a WindCurrentZone2D into a gale that blows in gusts: `Gust` seconds of wind ramping up to
    // `Strength`, then `Calm` seconds of rest. Before each gust the backdrop's wind particles speed up for
    // `Warning` seconds, so the player sees it coming; during the gust they keep racing. Strength 0 = no wind.
    // Boss backdrops drive Strength and Calm per phase, and the wind's side through BossBackdropPhases.
    [DisallowMultipleComponent]
    public sealed class GustWind2D : MonoBehaviour
    {
        [SerializeField] private WindCurrentZone2D _zone;
        [Tooltip("Peak push (units/s²). 0 = calm, the wind is only scenery.")]
        [SerializeField, Min(0f)] private float _strength;
        [SerializeField, Min(0.1f)] private float _gust = 2.5f;
        [SerializeField, Min(0f)] private float _calm = 2.5f;
        [SerializeField, Min(0f)] private float _warning = 0.7f;
        [Tooltip("Seconds to reach full strength and to die down.")]
        [SerializeField, Min(0.01f)] private float _ramp = 0.4f;
        [Tooltip("Particle systems that speed up before and during each gust (the visible wind).")]
        [SerializeField] private ParticleSystem[] _gustParticles = new ParticleSystem[0];
        [SerializeField, Min(1f)] private float _gustParticleSpeed = 2f;

        private float _time;
        private float _side = 1f;

        public float Strength { get => _strength; set => _strength = Mathf.Max(0f, value); }
        public float Calm { get => _calm; set => _calm = Mathf.Max(0f, value); }
        // +1 = towards the right, -1 = towards the left.
        public float Side { get => _side; set => _side = value < 0f ? -1f : 1f; }
        // 0..1: how hard the wind is blowing right now (0 during the calm).
        public float Gust { get; private set; }

        private void Update()
        {
            float cycle = _calm + _gust;
            _time = (_time + Time.deltaTime) % cycle;
            float intoGust = _time - _calm;   // < 0 during the calm
            Gust = intoGust < 0f ? 0f : Mathf.Clamp01(Mathf.Min(intoGust, _gust - intoGust) / _ramp);
            bool warning = intoGust > -_warning;
            float push = _strength * Gust;
            if (_zone != null)
            {
                _zone.Direction = new Vector2(_side, 0f);
                _zone.Strength = push;
                _zone.IsActive = push > 0.01f;
            }
            float speed = _strength > 0f && warning ? _gustParticleSpeed : 1f;
            foreach (var ps in _gustParticles)
            {
                if (ps == null) continue;
                var main = ps.main;
                main.simulationSpeed = Mathf.MoveTowards(main.simulationSpeed, speed, Time.deltaTime * 3f);
            }
        }
    }
}
