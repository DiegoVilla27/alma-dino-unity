using System;
using AlmaDino.Core.Interfaces;
using AlmaDino.Core.Progression;
using AlmaDino.Features.Environment.ScriptableObjects;
using AlmaDino.Features.Environment.Services;
using UnityEngine;

namespace AlmaDino.Features.Environment.Controllers
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class RisingHazardFloor2D : MonoBehaviour, IConditionalHazard2D
    {
        [Serializable]
        private struct Stage
        {
            public float StartX;
            public float EndX;
            public float FloorY;
            public float EndFloorY;
        }

        [SerializeField] private RisingGasConfigSO _config;
        [SerializeField] private MonoBehaviour _playerSource;
        [SerializeField] private GreenEggRescue2D _egg;
        [SerializeField] private Stage[] _stages;
        [SerializeField] private SpriteRenderer _surface;
        [SerializeField] private TextMesh _warningLabel;
        private IPlayerRespawnable _player;
        private Rigidbody2D _body;
        private BoxCollider2D _collider;
        private RisingGasCycle _cycle;
        private int _stageIndex;

        public RisingGasPhase Phase => _cycle != null ? _cycle.Phase : RisingGasPhase.Dormant;
        public float SurfaceY => _cycle != null ? _cycle.Height : 0f;
        public int StageIndex => _stageIndex;
        public bool IsDangerous => _cycle != null && _cycle.IsDangerous;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _collider = GetComponent<BoxCollider2D>();
            _collider.enabled = false;
            if (_config != null) _cycle = new RisingGasCycle(_config.WarningDuration, _config.RiseSpeed);
        }

        private void Start()
        {
            _player = _playerSource as IPlayerRespawnable;
            if (_player != null) _player.OnRespawned += ResetForCheckpoint;
            if (_egg != null) _egg.OnEggRescued += StopForRescue;
            if (_playerSource != null) ResetForCheckpoint(_playerSource.transform.position);
        }

        private void OnDestroy()
        {
            if (_player != null) _player.OnRespawned -= ResetForCheckpoint;
            if (_egg != null) _egg.OnEggRescued -= StopForRescue;
        }

        private void FixedUpdate()
        {
            if (_cycle == null || _playerSource == null || _stages == null || _stages.Length == 0) return;
            if (_stageIndex < _stages.Length && Phase != RisingGasPhase.Stopped)
            {
                var stage = _stages[_stageIndex];
                Vector2 position = _playerSource.transform.position;
                if (position.x >= stage.EndX && position.y >= stage.EndFloorY + 0.3f)
                {
                    _stageIndex++;
                    if (_stageIndex == _stages.Length) _cycle.Stop();
                    else _cycle.Reset(_stages[_stageIndex].FloorY - _config.CheckpointClearance);
                }
                else if (Phase == RisingGasPhase.Dormant && position.x >= stage.StartX)
                    _cycle.Arm(stage.FloorY - _config.CheckpointClearance, stage.EndFloorY - 0.3f);
            }
            _cycle.Tick(Time.fixedDeltaTime);
            _collider.enabled = IsDangerous;
            _body.MovePosition(new Vector2(_body.position.x, SurfaceY - _collider.size.y * 0.5f));
        }

        private void Update()
        {
            if (_surface != null)
                _surface.color = Phase == RisingGasPhase.Warning ? Color.yellow : new Color(0.22f, 0.69f, 0f);
            if (_warningLabel != null)
            {
                _warningLabel.gameObject.SetActive(Phase == RisingGasPhase.Warning || Phase == RisingGasPhase.Rising);
                if (_playerSource != null) _warningLabel.transform.position = _playerSource.transform.position + Vector3.up * 3.5f;
                _warningLabel.text = Phase == RisingGasPhase.Warning ? "¡EL GAS VA A SUBIR!" : "GAS ↑";
            }
        }

        private void ResetForCheckpoint(Vector2 position)
        {
            if (_cycle == null || _stages == null || _stages.Length == 0) return;
            _stageIndex = 0;
            while (_stageIndex < _stages.Length && position.x >= _stages[_stageIndex].EndX) _stageIndex++;
            _cycle.Reset((_stageIndex < _stages.Length ? _stages[_stageIndex].FloorY : _stages[^1].EndFloorY)
                - _config.CheckpointClearance);
            _collider.enabled = false;
            _body.position = new Vector2(_body.position.x, SurfaceY - _collider.size.y * 0.5f);
        }

        private void StopForRescue(EggType egg)
        {
            if (egg != EggType.PurpleEgg || _cycle == null) return;
            _cycle.Stop();
            _collider.enabled = false;
        }

        public void OnHazardTouch() { }
    }
}
