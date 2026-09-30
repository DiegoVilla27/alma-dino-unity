using System.Collections;
using AlmaDino.Core.Interfaces;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AlmaDino.Features.Environment
{
    [RequireComponent(typeof(Collider2D))]
    public class LevelExit2D : MonoBehaviour
    {
        [Header("Next Destination")]
        [SerializeField] private string _nextSceneName = "Level_1_2";
        [SerializeField] private string _levelTitle = "NIVEL 1-1 COMPLETADO";
        [TextArea]
        [SerializeField] private string _victoryMessage = "Has despertado y dominado el Aleteo Materno.\nEl rastro de tus pequeños se adentra en las copas de la Jungla Esmeralda (1-2).";

        private bool _isCompleted = false;

        private void Awake()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (_isCompleted) return;

            if (collision.GetComponent<IPlayerRespawnable>() != null)
            {
                _isCompleted = true;
                StartCoroutine(CompleteLevelRoutine());
            }
        }

        private IEnumerator CompleteLevelRoutine()
        {
            Debug.Log($"[LevelExit2D] ¡Nivel Completado!");
            if (NarrativeBannerUI.Instance != null)
            {
                NarrativeBannerUI.Instance.ShowBanner(_levelTitle, _victoryMessage, new Color(0.3f, 1f, 0.5f), 6.0f);
            }

            yield return new WaitForSeconds(3.5f);

            // Si la siguiente escena existe en build settings, cargarla; si no, reiniciar o mostrar fin
            if (Application.CanStreamedLevelBeLoaded(_nextSceneName))
            {
                SceneManager.LoadScene(_nextSceneName);
            }
            else
            {
                Debug.Log($"[LevelExit2D] Siguiente escena '{_nextSceneName}' aún no creada. Nivel 1-1 superado con éxito.");
            }
        }

        private void OnGUI()
        {
            if (!_isCompleted) return;

            float w = Screen.width;
            float h = Screen.height;

            GUIStyle style = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
                fontStyle = FontStyle.Bold
            };
            style.normal.textColor = Color.green;

            if (GUI.Button(new Rect(w * 0.5f - 120, h * 0.78f, 240, 50), "REPETIR NIVEL 1-1"))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
}
