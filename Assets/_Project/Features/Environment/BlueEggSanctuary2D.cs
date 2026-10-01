using AlmaDino.Core.Progression;
using UnityEngine;
namespace AlmaDino.Features.Environment
{
    public class BlueEggSanctuary2D : MonoBehaviour
    {
        [SerializeField] private GreenEggRescue2D _egg;
        [SerializeField] private GameObject _armadillo;
        private void OnEnable() { if (_egg != null) _egg.OnEggRescued += RevealGuardian; }
        private void Start() { if (GameProgression.IsEggRescued(EggType.BlueEgg)) RevealGuardian(EggType.BlueEgg); }
        private void OnDisable() { if (_egg != null) _egg.OnEggRescued -= RevealGuardian; }
        private void RevealGuardian(EggType egg) { if (egg == EggType.BlueEgg && _armadillo != null) _armadillo.SetActive(true); }
    }
}
