using AlmaDino.Core.Progression;
using UnityEngine;

namespace AlmaDino.Features.Environment.Controllers
{
    public class PurpleEggSanctuary2D : MonoBehaviour
    {
        [SerializeField] private GreenEggRescue2D _egg;
        [SerializeField] private GameObject _guardian;

        private void OnEnable()
        {
            if (_egg != null) _egg.OnEggRescued += RevealGuardian;
        }

        private void Start()
        {
            if (GameProgression.IsEggRescued(EggType.PurpleEgg)) RevealGuardian(EggType.PurpleEgg);
        }

        private void OnDisable()
        {
            if (_egg != null) _egg.OnEggRescued -= RevealGuardian;
        }

        private void RevealGuardian(EggType egg)
        {
            if (egg == EggType.PurpleEgg && _guardian != null) _guardian.SetActive(true);
        }
    }
}
