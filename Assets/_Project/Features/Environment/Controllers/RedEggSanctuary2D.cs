using System.Collections;
using AlmaDino.Core.Events;
using AlmaDino.Core.Interfaces;
using AlmaDino.Core.Progression;
using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace AlmaDino.Features.Environment.Controllers
{
    public sealed class RedEggSanctuary2D : MonoBehaviour
    {
        [SerializeField] private GreenEggRescue2D _egg;
        [SerializeField] private MonoBehaviour _playerSource;
        [SerializeField] private GameObject _combatRoot;
        [SerializeField] private GameObject _fireRoot;
        [SerializeField] private GameObject _guardian;
        [SerializeField] private SpriteRenderer[] _auraEggs;
        [SerializeField] private Light2D[] _auraLights;
        [SerializeField] private CameraShakeEventChannelSO _shake;
        [SerializeField] private Vector2 _rescueCheckpoint=new Vector2(90f,.7f);
        public bool IsCalm { get; private set; }
        public bool GuardianRevealed { get; private set; }
        private void OnEnable() { if(_egg!=null) _egg.OnEggRescued+=OnRescue; }
        private void OnDisable() { if(_egg!=null) _egg.OnEggRescued-=OnRescue; }
        private void Start()
        {
            if(GameProgression.IsEggRescued(EggType.RedEgg))
            {
                CalmSanctuary(); _guardian.transform.position=new Vector3(93.5f,1.5f,0f);
                _guardian.SetActive(true); GuardianRevealed=true;
            }
        }
        private void Update()
        {
            for(int i=0;i<_auraEggs.Length;i++)
            {
                bool rescued=GameProgression.IsEggRescued((EggType)(i+1));
                _auraEggs[i].gameObject.SetActive(rescued); _auraLights[i].gameObject.SetActive(rescued);
                _auraLights[i].intensity=.15f+Mathf.Sin(Time.time*3f)*.03f;
            }
        }
        private void OnRescue(EggType egg) { if(egg==EggType.RedEgg) StartCoroutine(RescueMoment()); }
        private void CalmSanctuary()
        {
            IsCalm=true; _combatRoot.SetActive(false); _fireRoot.SetActive(false);
            (_playerSource as IPlayerRespawnable)?.SetCheckpoint(_rescueCheckpoint);
        }
        private IEnumerator RescueMoment()
        {
            CalmSanctuary();
            yield return new WaitForSeconds(2.5f);
            _guardian.SetActive(true); _shake?.Raise(.3f,.35f);
            Vector3 start=new Vector3(93.5f,-2f,0f), end=new Vector3(93.5f,1.5f,0f);
            float elapsed=0f;
            while(elapsed<1.2f)
            {
                elapsed+=Time.deltaTime; _guardian.transform.position=Vector3.Lerp(start,end,Mathf.SmoothStep(0f,1f,elapsed/1.2f));
                yield return null;
            }
            GuardianRevealed=true;
        }
    }
}
