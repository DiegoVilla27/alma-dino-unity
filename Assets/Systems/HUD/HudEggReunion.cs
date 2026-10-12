using UnityEngine;
using UnityEngine.Rendering;

namespace AlmaGame.Systems
{
    // «El Reencuentro»: the moment Alma gets an egg back after beating a boss. The room itself is a prefab made by
    // hand, one per boss (root: EggReunionRoom); this only shows it: a white flash takes us out of the arena, the
    // room fills the screen (on the HUD layer, over everything, with no scene load: the arena keeps its state
    // underneath) for the room's Duration, and another flash brings us back. The egg is saved when the room opens
    // and flies to its HUD slot once the boss's cinematic is over. Bosses call HudEggReunion.Play(...) right after
    // they leave or die, and continue in `onDone` (the exit portal).
    [DisallowMultipleComponent, RequireComponent(typeof(Hud2D))]
    public sealed class HudEggReunion : MonoBehaviour
    {
        public static HudEggReunion Instance { get; private set; }
        public bool IsPlaying => _playing;

        private const float FlashIn = 0.3f, FlashOut = 0.5f, DefaultDuration = 9f;

        private Hud2D _hud;
        private GameObject _room;
        private SpriteRenderer _flash;
        private bool _playing, _roomShown, _closing;
        private float _startedAt, _duration, _closeAt;
        private string _eggId, _pendingFlight;
        private GameObject _prefab;
        private System.Action _onDone;

        // Shows `roomPrefab` for `eggId`; `onDone` runs when we are back in the arena (behind the closing flash, at full white). Without a HUD or a prefab
        // the egg is saved and onDone runs right away (returns false).
        public static bool Play(string eggId, GameObject roomPrefab, System.Action onDone)
        {
            var hud = Hud2D.Instance;
            var player = hud != null ? hud.GetComponent<HudEggReunion>() : null;
            if (player == null || roomPrefab == null)
            {
                if (GameProgress.Instance != null) GameProgress.Instance.RescueEgg(eggId);
                onDone?.Invoke();
                return false;
            }
            player.Open(eggId, roomPrefab, onDone);
            return true;
        }

        private void Awake()
        {
            Instance = this;
            _hud = GetComponent<Hud2D>();
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        private int Order => _hud.SortingOrder + 60;

        private void Open(string eggId, GameObject prefab, System.Action onDone)
        {
            _eggId = eggId; _prefab = prefab; _onDone = onDone;
            _playing = true; _roomShown = _closing = false;
            _startedAt = Time.time;
            if (_flash == null)
            {
                _flash = new GameObject("EggReunionFlash").AddComponent<SpriteRenderer>();
                _flash.transform.SetParent(transform, false);
                _flash.sprite = AlmaGame.Level.HazardFx.Pixel();
                Vector2 s = _flash.sprite.bounds.size;
                _flash.transform.localScale = new Vector3(80f / s.x, 40f / s.y, 1f);
            }
            _flash.sortingOrder = Order + 1;
            _flash.color = new Color(1f, 1f, 1f, 0f);
        }

        // The room: the prefab under the HUD (which follows the camera view and scales with it), centred on the
        // screen, drawn over everything as one sorting group.
        private void ShowRoom()
        {
            _roomShown = true;
            _room = Instantiate(_prefab, transform, false);
            _room.name = _prefab.name;
            _room.transform.localPosition = Vector3.zero;
            var group = _room.GetComponent<SortingGroup>();
            if (group == null) group = _room.AddComponent<SortingGroup>();
            group.sortingOrder = Order;
            var room = _room.GetComponent<EggReunionRoom>();
            _duration = room != null ? room.Duration : DefaultDuration;
            _closeAt = Time.time + _duration;
            if (GameProgress.Instance != null) GameProgress.Instance.RescueEgg(_eggId);
        }

        private void LateUpdate()
        {
            if (_pendingFlight != null && !CinematicState.IsPlaying && HudEggIndicator.Instance != null && _hud.Visibility > 0.6f)
            {
                // Back in the game: the egg flies from the middle of the screen to its slot.
                var cam = _hud.Camera;
                Vector3 from = cam != null ? cam.transform.position : transform.position;
                HudEggIndicator.Instance.FlyIn(_pendingFlight, new Vector3(from.x, from.y, 0f), 1.5f * _hud.Scale);
                _pendingFlight = null;
            }
            if (!_playing) return;
            float now = Time.time;
            if (!_roomShown)
            {
                // Flash in, then the room appears behind the flash.
                float u = Mathf.Clamp01((now - _startedAt) / FlashIn);
                _flash.color = new Color(1f, 1f, 1f, u);
                if (u >= 1f) ShowRoom();
                return;
            }
            if (!_closing)
            {
                _flash.color = new Color(1f, 1f, 1f, 1f - Mathf.Clamp01((now - _startedAt - FlashIn) / FlashOut));
                if (now >= _closeAt) _closing = true;
                return;
            }
            // Flash in, back to the arena behind it (the boss continues right then, still behind the white, so the
            // view is already on Alma when the flash clears), flash out.
            float c = (now - _closeAt) / FlashIn;
            if (c < 1f) { _flash.color = new Color(1f, 1f, 1f, c); return; }
            if (_room != null)
            {
                Destroy(_room); _room = null;
                var done = _onDone; _onDone = null;
                done?.Invoke();
            }
            float o = Mathf.Clamp01((now - _closeAt - FlashIn) / FlashOut);
            _flash.color = new Color(1f, 1f, 1f, 1f - o);
            if (o < 1f) return;
            _playing = false;
            _pendingFlight = _eggId;
        }
    }
}
