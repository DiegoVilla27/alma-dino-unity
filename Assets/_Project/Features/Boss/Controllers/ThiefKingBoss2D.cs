using System.Collections;
using AlmaDino.Core.Events;
using AlmaDino.Core.Interfaces;
using AlmaDino.Core.Progression;
using AlmaDino.Features.Boss.ScriptableObjects;
using AlmaDino.Features.Boss.Services;
using UnityEngine;
namespace AlmaDino.Features.Boss.Controllers
{
    public sealed class ThiefKingBoss2D : MonoBehaviour
    {
        [SerializeField] private ThiefKingConfigSO _config;
        [SerializeField] private MonoBehaviour _playerSource;
        [SerializeField] private Transform _king;
        [SerializeField] private SpriteRenderer _eye;
        [SerializeField] private GameObject _heatSeal, _plate, _steamSeal, _vent, _anchor;
        [SerializeField] private GameObject _ascent, _arena, _epilogue, _nest;
        [SerializeField] private Transform[] _hatchlings;
        [SerializeField] private Transform _jaw, _meteor, _rainRock, _rainMarker, _lava, _stalactite;
        [SerializeField] private TextMesh _hint;
        [SerializeField] private CameraShakeEventChannelSO _shake;
        private IPlayerRespawnable _player;
        private Rigidbody2D _playerBody;
        private readonly ThiefKingFight _fight = new ThiefKingFight();
        private float _cycle, _window, _rainClock, _rainX, _lavaClock;
        private bool _meteorReflected;
        private Vector2 _meteorPosition;
        private bool _ending;
        public ThiefKingFight Fight => _fight;
        public float LavaHeight => _lava.position.y + 1f;
        public bool MeteorReflected => _meteorReflected;
        private void Start()
        {
            _player = _playerSource as IPlayerRespawnable;
            _playerBody = _playerSource.GetComponent<Rigidbody2D>();
            _player.OnRespawned += ResetAttempt;
            _player.SetCheckpoint(new Vector2(0f,.7f)); ResetAttempt(Vector2.zero);
            NarrativeBannerEvents.RequestBanner("EL REY LADRÓN — TIRANO ANCESTRAL", "Tus cuatro hijos están a salvo. Abre el camino a casa.", Color.yellow, 5f);
        }
        private void OnDestroy() { if(_player != null) _player.OnRespawned -= ResetAttempt; }
        private void ResetAttempt(Vector2 checkpoint)
        {
            if(_fight.IsDefeated) return;
            _fight.ResetAttempt(); _cycle = -2f; _window = 0f; _rainClock = -2f; _lavaClock = 0f;
            _meteorReflected = false; _meteorPosition = new Vector2(18f,.9f);
            _heatSeal.SetActive(_fight.Hits == 0); _plate.SetActive(_fight.Hits < 2);
            _ascent.SetActive(_fight.Hits == 2); _arena.SetActive(_fight.Hits < 2);
            _steamSeal.SetActive(true); _vent.SetActive(false);
            _anchor.GetComponent<SpriteRenderer>().color = Color.yellow;
            _lava.position = new Vector3(28f,-4f,0f);
            _jaw.gameObject.SetActive(false); _rainRock.gameObject.SetActive(false); _rainMarker.gameObject.SetActive(false);
            _meteor.gameObject.SetActive(_fight.Hits == 1);
            _king.position = new Vector3(_fight.Hits == 2 ? 38f : 15f, 1.6f,0f);
        }
        private void FixedUpdate()
        {
            if(_fight.IsDefeated || _playerBody == null) return;
            float dt = Time.fixedDeltaTime;
            if(_fight.Hits == 0) Charge(dt);
            else if(_fight.Hits == 1) Meteors(dt);
            else Collapse(dt);
            if(_window > 0f)
            {
                _window -= dt;
                if(_window <= 0f) { _fight.Close(); if(_fight.Hits == 0) { _heatSeal.SetActive(true); _cycle = -1f; } }
            }
        }
        private void Charge(float dt)
        {
            if(_fight.Vulnerable) { _jaw.gameObject.SetActive(false); return; }
            _cycle += dt;
            float sweep = _cycle - _config.Telegraph;
            bool active = sweep >= 0f && sweep < _config.SweepDuration;
            _jaw.gameObject.SetActive(active);
            if(active)
            {
                _jaw.position = new Vector3(Mathf.Lerp(18f,-3f,sweep/_config.SweepDuration),.65f,0f);

            }
            if(_cycle > _config.Telegraph + _config.SweepDuration + 1f) _cycle = 0f;
        }
        public void CounterCharge()
        {
            if(_fight.Hits != 0 || _playerBody.position.y < 2.2f) return;
            _heatSeal.SetActive(false); Expose();
        }
        private void Expose()
        {
            _fight.Expose(); _window = _config.Recovery;
            _eye.color = Color.cyan; _shake?.Raise(.25f,.15f);
        }
        public void Strike()
        {
            if(_fight.Hits >= 2 || !_fight.Pound()) return;
            _shake?.Raise(.55f,.3f);
            Vector2 checkpoint = _fight.Hits == 1 ? new Vector2(0f,.7f) : new Vector2(20f,.7f);
            _player.SetCheckpoint(checkpoint); ResetAttempt(checkpoint);
            if(_fight.Hits == 2)
            {
                _player.RespawnAt(checkpoint);
                NarrativeBannerEvents.RequestBanner("¡EL CALDERO COLAPSA!", "Sube. DASH entre las columnas. POUND abre el vapor.", Color.yellow, 5f);
            }
            else NarrativeBannerEvents.RequestBanner("CORAZA ROTA — 1/3", "Devuelve el meteorito con ROAR. Después, POUND en la placa.", Color.cyan, 4f);
        }
        private void Meteors(float dt)
        {
            if(!_fight.Vulnerable)
            {
                _meteor.gameObject.SetActive(true);
                if(_meteorReflected)
                {
                    _meteorPosition = Vector2.MoveTowards(_meteorPosition,new Vector2(14f,1.5f),_config.ReflectedSpeed*dt);
                    if(Vector2.Distance(_meteorPosition,new Vector2(14f,1.5f)) < .3f)
                    { _meteor.gameObject.SetActive(false); Expose(); _meteorReflected = false; _meteorPosition = new Vector2(18f,.9f); }
                }
                else
                {
                    _meteorPosition += Vector2.left * (_config.MeteorSpeed*dt);
                    if(_meteorPosition.x < -4f) _meteorPosition = new Vector2(18f,.9f);

                }
                _meteor.position = _meteorPosition;
                _meteor.GetComponent<SpriteRenderer>().color = _meteorReflected ? Color.cyan : new Color(1f,.3f,.03f);
            }
            else _meteor.gameObject.SetActive(false);
            _rainClock += dt;
            if(_rainClock >= 0f && _rainClock < _config.RainWarning)
            {
                if(!_rainMarker.gameObject.activeSelf) _rainX = _playerBody.position.x;
                _rainMarker.gameObject.SetActive(true); _rainMarker.position = new Vector3(_rainX,.05f,0f);
                _rainRock.gameObject.SetActive(false);
            }
            else if(_rainClock >= _config.RainWarning)
            {
                _rainMarker.gameObject.SetActive(false); _rainRock.gameObject.SetActive(true);
                _rainRock.position = new Vector3(_rainX,8f-(_rainClock-_config.RainWarning)*_config.RockSpeed,0f);

                if(_rainRock.position.y < -1f) { _rainClock = -1.2f; _rainRock.gameObject.SetActive(false); }
            }
        }
        public void ReflectMeteor(Vector2 direction)
        {
            if(_fight.Hits != 1 || _fight.Vulnerable || _meteorReflected || direction.x <= 0f || _meteorPosition.x < _playerBody.position.x) return;
            _meteorReflected = true; _shake?.Raise(.2f,.12f);
        }
        private void Collapse(float dt)
        {
            _lavaClock += dt;
            if(_lavaClock > _config.LavaDelay) _lava.position += Vector3.up * (_config.LavaSpeed*dt);
        }
        public void OpenSteam()
        {
            if(_fight.Hits != 2) return;
            _fight.OpenSteam(); _steamSeal.SetActive(false); _vent.SetActive(true); _shake?.Raise(.3f,.2f);
        }
        public void CrackAnchor()
        {
            _fight.CrackAnchor();
            if(_fight.AnchorCracked) _anchor.GetComponent<SpriteRenderer>().color = Color.cyan;
        }
        public void Finish()
        {
            if(_fight.Hits != 2 || !_fight.Pound()) return;
            _shake?.Raise(.7f,.4f); GameProgression.CompleteWorld(4);
            _player.SetCheckpoint(new Vector2(38f,11.9f)); StartCoroutine(Ending());
        }
        private void Update()
        {
            if(_playerSource == null || _fight.IsDefeated) return;
            _king.localRotation = Quaternion.Euler(0f,0f,_fight.Vulnerable ? -8f : Mathf.Sin(Time.time*2f)*2f);
            _king.localScale = new Vector3(1f,1f+Mathf.Sin(Time.time*3f)*.025f,1f);
            _meteor.localRotation = Quaternion.Euler(0f,0f,Time.time*130f);
            _rainRock.localRotation = Quaternion.Euler(0f,0f,Time.time*170f);
            _hint.transform.position = _playerSource.transform.position + Vector3.up * 3.4f;
            _hint.color = _fight.Vulnerable ? Color.cyan : Color.yellow;
            _eye.color = _fight.Vulnerable ? Color.cyan : new Color(1f,.1f,.05f);
            _plate.GetComponent<SpriteRenderer>().color = _fight.Vulnerable ? Color.cyan : new Color(.35f,.3f,.27f);
            _hint.text = _fight.Vulnerable ? "¡PLACA EXPUESTA! POUND ↓  " + Mathf.CeilToInt(_window) + "s"
                : _fight.Hits == 0 ? "1/3  DOBLE SALTO + DASH →\nPLACA DEL LOMO: POUND ↓"
                : _fight.Hits == 1 ? "2/3  METEORITO: ROAR →\nSAL DEL AVISO AMARILLO"
                : !_fight.SteamOpen ? "3/3  MAGMA ↑\nSELLO DE VAPOR: POUND ↓"
                : !_fight.AnchorCracked ? "SUBE CON EL VAPOR\nANCLAJE: ROAR →" : "¡RUGE, ALMA! ¡HAZ TEMBLAR LA TIERRA!\nESTALACTITA: POUND ↓";
        }
        private IEnumerator Ending()
        {
            if(_ending) yield break; _ending = true; _hint.text = "3/3 — EL REY HA CAÍDO";
            _stalactite.gameObject.SetActive(true);
            float t = 0f; Vector3 start = _stalactite.position;
            Vector2 playerStart = _playerBody.position;
            var previousBodyType = _playerBody.bodyType;
            _playerSource.enabled = false; _playerBody.linearVelocity = Vector2.zero; _playerBody.bodyType = RigidbodyType2D.Kinematic;
            _anchor.SetActive(false); _vent.SetActive(false);
            while(t < 1.4f)
            {
                t += Time.deltaTime; float progress = Mathf.SmoothStep(0f,1f,t/1.4f);
                _stalactite.position = Vector3.Lerp(start,new Vector3(38f,1.5f,0f),progress);
                _playerBody.position = Vector2.Lerp(playerStart,new Vector2(38f,3.5f),progress);
                _king.position += Vector3.down * Time.deltaTime*3f; yield return null;
            }
            _playerBody.bodyType = previousBodyType; _playerSource.enabled = true;
            _king.gameObject.SetActive(false); _ascent.SetActive(false); _arena.SetActive(false); _lava.gameObject.SetActive(false);
            _nest.SetActive(false);
            var babyScales = new Vector3[_hatchlings.Length];
            for(int i=0;i<_hatchlings.Length;i++) { babyScales[i] = _hatchlings[i].localScale; _hatchlings[i].localScale = Vector3.zero; }
            _epilogue.SetActive(true); _player.RespawnAt(new Vector2(3f,20.7f));
            Camera.main.backgroundColor = new Color(.95f,.64f,.37f);
            NarrativeBannerEvents.RequestBanner("BIENVENIDOS AL MUNDO, PEQUEÑOS", "No hubo tormenta, abismo ni bestia que pudiera apagar este latido.\n— FIN —",new Color(.5f,1f,.6f),12f);
            for(int i=0;i<_hatchlings.Length;i++)
            {
                var baby = _hatchlings[i]; float birth = 0f;
                while(birth < .5f) { birth += Time.deltaTime; baby.localScale = babyScales[i] * Mathf.SmoothStep(0f,1f,birth/.5f); yield return null; }
                yield return new WaitForSeconds(.25f);
            }
        }
    }
}
