using AlmaDino.Core.Interfaces;
using UnityEngine;
namespace AlmaDino.Features.Environment.Controllers
{
    public sealed class TempleTrialGates2D : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour _playerSource;
        [SerializeField] private BreakableGround2D[] _pillars;
        [SerializeField] private Collider2D _poundGate;
        [SerializeField] private SpriteRenderer _poundGateVisual;
        [SerializeField] private DashBreakableBarrier2D _dashGate;
        [SerializeField] private TextMesh _poundStatus;
        private IPlayerRespawnable _player;
        public bool IsPoundGateOpen { get; private set; }
        private void Start()
        {
            _player=_playerSource as IPlayerRespawnable;
            if(_player!=null) _player.OnRespawned+=ResetForCheckpoint;
        }
        private void OnDestroy() { if(_player!=null) _player.OnRespawned-=ResetForCheckpoint; }
        private void FixedUpdate()
        {
            bool allBroken=true;
            foreach(var pillar in _pillars) allBroken &= pillar.IsBroken;
            IsPoundGateOpen=allBroken; _poundGate.enabled=!allBroken; _poundGateVisual.enabled=!allBroken;
        }
        private void Update()
        {
            _poundStatus.text=IsPoundGateOpen?"PILARES ROTOS: DOBLE SALTO →":"PISOTÓN ↓ ATRAVIESA LOS DOS PILARES";
            _poundStatus.color=IsPoundGateOpen?Color.cyan:Color.yellow;
        }
        private void ResetForCheckpoint(Vector2 checkpoint)
        {
            if(checkpoint.x<_poundGate.transform.position.x)
            {
                foreach(var pillar in _pillars) pillar.Restore();
                IsPoundGateOpen=false; _poundGate.enabled=true; _poundGateVisual.enabled=true;
            }
            if(checkpoint.x<_dashGate.transform.position.x) _dashGate.Respawn();
        }
    }
}
