using AlmaDino.Core.Events;
using AlmaDino.Core.Interfaces;
using AlmaDino.Features.Environment.ScriptableObjects;
using AlmaDino.Features.Environment.Services;
using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace AlmaDino.Features.Environment.Controllers
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public sealed class FractureEruption2D : MonoBehaviour, IConditionalHazard2D
    {
        [SerializeField] private FractureConfigSO _config;
        [SerializeField] private MonoBehaviour _playerSource;
        [SerializeField] private BreakableGround2D _seal;
        [SerializeField] private Collider2D _barrier;
        [SerializeField] private SpriteRenderer _barrierVisual;
        [SerializeField] private TextMesh _status;
        [SerializeField] private Light2D _glow;
        [SerializeField] private CameraShakeEventChannelSO _shake;
        private RisingGasCycle _cycle;
        private Rigidbody2D _body;
        private BoxCollider2D _hazard;
        private IPlayerRespawnable _player;
        public RisingGasPhase Phase => _cycle.Phase;
        public float SurfaceY => _cycle.Height;
        public bool IsDangerous => _cycle.IsDangerous;
        private void Awake()
        {
            _body=GetComponent<Rigidbody2D>(); _hazard=GetComponent<BoxCollider2D>();
            _cycle=new RisingGasCycle(_config.EruptionWarning,_config.EruptionRiseSpeed);
            ResetEruption();
        }
        private void Start()
        {
            _player=_playerSource as IPlayerRespawnable;
            if(_player!=null) _player.OnRespawned+=OnRespawn;
        }
        private void OnDestroy() { if(_player!=null) _player.OnRespawned-=OnRespawn; }
        private void FixedUpdate()
        {
            if(Phase==RisingGasPhase.Dormant && _seal.IsBroken)
            {
                _cycle.Arm(_config.EruptionInitialHeight,_config.EruptionCeiling);
                _barrier.enabled=false; _barrierVisual.enabled=false;
                _shake?.Raise(.2f,.35f);
            }
            if(_playerSource.transform.position.x>=106f && _playerSource.transform.position.y>=4.1f) _cycle.Stop();
            _cycle.Tick(Time.fixedDeltaTime); _hazard.enabled=IsDangerous;
            _body.MovePosition(new Vector2(_body.position.x,SurfaceY-_hazard.size.y*.5f));
        }
        private void Update()
        {
            _glow.intensity=Phase==RisingGasPhase.Dormant?.15f:1f+Mathf.Sin(Time.time*8f)*.15f;
            _status.text=Phase==RisingGasPhase.Dormant?"PISOTÓN ↓ ROMPE EL SELLO":Phase==RisingGasPhase.Warning?"¡LA FRACTURA DESPIERTA! SUBE →":Phase==RisingGasPhase.Rising?"¡LAVA ↑! NO TE DETENGAS":"LA CIMA ESTÁ A SALVO";
            _status.color=Phase==RisingGasPhase.Stopped?Color.cyan:Color.yellow;
            if(Phase!=RisingGasPhase.Dormant) _status.transform.position=_playerSource.transform.position+Vector3.up*3.3f;
        }
        private void OnRespawn(Vector2 checkpoint) { _seal.Restore(); ResetEruption(); }
        private void ResetEruption()
        {
            _cycle.Reset(_config.EruptionInitialHeight); _hazard.enabled=false;
            _body.position=new Vector2(_body.position.x,SurfaceY-_hazard.size.y*.5f);
            _barrier.enabled=true; _barrierVisual.enabled=true;
            _status.transform.position=new Vector3(70f,2.8f,0f);
        }
        public void OnHazardTouch() { }
    }
}
