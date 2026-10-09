using UnityEngine;
using UnityEngine.SceneManagement;

namespace AlmaGame.Systems
{
    // Shows the level's name when the scene starts («MUNDO 1 · JUNGLA ESMERALDA», «1-1», «DESPERTAR EN EL NIDO»),
    // through the HUD banner layer (BannerKind.Level). One table for the 20 playable scenes, with the titles of
    // their design sheets; a scene that isn't in it shows nothing.
    [DisallowMultipleComponent, RequireComponent(typeof(HudBanners))]
    public sealed class HudLevelTitle : MonoBehaviour
    {
        [System.Serializable]
        public struct Entry
        {
            public string Scene;      // scene name, e.g. Level_1_1
            public string Code;       // «1-1», «Jefe»
            public string Title;
            public string World;      // «Mundo 1 · Jungla Esmeralda»
            public Color Accent;
        }

        [SerializeField] private Entry[] _levels = new Entry[0];
        [Tooltip("Seconds after the scene starts before the title appears.")]
        [SerializeField, Min(0f)] private float _delay = 0.6f;
        [SerializeField] private bool _showOnStart = true;

        private float _showAt = -1f;
        private Entry _entry;

        public static HudLevelTitle Instance { get; private set; }

        private void Awake() => Instance = this;

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            if (_showOnStart && TryGet(SceneManager.GetActiveScene().name, out _entry)) _showAt = Time.unscaledTime + _delay;
        }

        private void Update()
        {
            if (_showAt < 0f || Time.unscaledTime < _showAt) return;
            _showAt = -1f;
            Show(_entry);
        }

        public bool TryGet(string scene, out Entry entry)
        {
            foreach (var e in _levels)
                if (e.Scene == scene) { entry = e; return true; }
            entry = default;
            return false;
        }

        // Shows a level title now (also used by tests).
        public void Show(Entry e)
        {
            var banners = GetComponent<HudBanners>();
            banners.Show(new Banner { Kind = BannerKind.Level, Code = e.Code, Headline = e.Title, Kicker = e.World, Accent = e.Accent });
        }
    }
}
