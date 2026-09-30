using System.Collections;
using UnityEngine;

namespace AlmaDino.Features.Environment
{
    public class NarrativeBannerUI : MonoBehaviour
    {
        public static NarrativeBannerUI Instance { get; private set; }

        [Header("Prologue Settings")]
        [SerializeField] private bool _showPrologueOnStart = true;
        [TextArea(3, 6)]
        [SerializeField] private string _prologueText = "La tierra tembló una sola vez.\nCuando regresé al nido con comida, el silencio era absoluto. No estaban.\nSi tengo que cruzar el continente entero a pie,\nmis pequeños volverán a sentir el calor de mis plumas.";

        private string _currentTitle = "";
        private string _currentBody = "";
        private Color _currentAccentColor = Color.white;
        private float _displayAlpha = 0f;
        private bool _isShowing = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (_showPrologueOnStart)
            {
                StartCoroutine(ShowPrologueRoutine());
            }
        }

        private IEnumerator ShowPrologueRoutine()
        {
            yield return new WaitForSeconds(0.4f);
            ShowBanner("ALMA: MOTHER'S ROAR", _prologueText, new Color(0.9f, 0.85f, 0.4f), 5.5f);
        }

        public void ShowRelicUnlock(string title, string body, Color accentColor, float duration = 4.5f)
        {
            ShowBanner("¡NUEVA HABILIDAD DESPERTADA!\n" + title, body, accentColor, duration);
        }

        public void ShowBanner(string title, string body, Color accentColor, float duration)
        {
            StopAllCoroutines();
            StartCoroutine(DisplayRoutine(title, body, accentColor, duration));
        }

        private IEnumerator DisplayRoutine(string title, string body, Color accentColor, float duration)
        {
            _currentTitle = title;
            _currentBody = body;
            _currentAccentColor = accentColor;
            _isShowing = true;

            // Fade In
            float elapsed = 0f;
            float fadeInTime = 0.5f;
            while (elapsed < fadeInTime)
            {
                elapsed += Time.unscaledDeltaTime;
                _displayAlpha = Mathf.Clamp01(elapsed / fadeInTime);
                yield return null;
            }
            _displayAlpha = 1f;

            // Hold
            yield return new WaitForSeconds(duration);

            // Fade Out
            elapsed = 0f;
            float fadeOutTime = 0.6f;
            while (elapsed < fadeOutTime)
            {
                elapsed += Time.unscaledDeltaTime;
                _displayAlpha = Mathf.Clamp01(1f - (elapsed / fadeOutTime));
                yield return null;
            }
            _displayAlpha = 0f;
            _isShowing = false;
        }

        public void Dismiss()
        {
            StopAllCoroutines();
            StartCoroutine(FadeOutFastRoutine());
        }

        private IEnumerator FadeOutFastRoutine()
        {
            float elapsed = 0f;
            float startAlpha = _displayAlpha;
            float dur = 0.2f;
            while (elapsed < dur)
            {
                elapsed += Time.unscaledDeltaTime;
                _displayAlpha = Mathf.Lerp(startAlpha, 0f, elapsed / dur);
                yield return null;
            }
            _displayAlpha = 0f;
            _isShowing = false;
        }

        private void OnGUI()
        {
            if (!_isShowing || _displayAlpha <= 0.01f) return;

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            float bannerWidth = Mathf.Min(screenWidth * 0.88f, 920f);
            float bannerHeight = 185f;
            float bannerX = (screenWidth - bannerWidth) * 0.5f;
            float bannerY = screenHeight * 0.10f;

            // Fondo translúcido elegante
            Color oldColor = GUI.color;
            GUI.color = new Color(0.04f, 0.08f, 0.04f, 0.92f * _displayAlpha);
            GUI.Box(new Rect(bannerX, bannerY, bannerWidth, bannerHeight), GUIContent.none);

            // Borde coloreado
            GUI.color = new Color(_currentAccentColor.r, _currentAccentColor.g, _currentAccentColor.b, _displayAlpha);
            GUI.Box(new Rect(bannerX - 2, bannerY - 2, bannerWidth + 4, 3), GUIContent.none);
            GUI.Box(new Rect(bannerX - 2, bannerY + bannerHeight - 1, bannerWidth + 4, 3), GUIContent.none);

            // Título
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Clamp((int)(screenHeight * 0.028f), 15, 24),
                fontStyle = FontStyle.Bold
            };
            titleStyle.normal.textColor = new Color(_currentAccentColor.r, _currentAccentColor.g, _currentAccentColor.b, _displayAlpha);
            GUI.Label(new Rect(bannerX + 15, bannerY + 10, bannerWidth - 30, 36), _currentTitle, titleStyle);

            // Cuerpo del texto
            GUIStyle bodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Clamp((int)(screenHeight * 0.022f), 13, 19),
                fontStyle = FontStyle.Italic,
                wordWrap = true
            };
            bodyStyle.normal.textColor = new Color(1f, 1f, 1f, 0.95f * _displayAlpha);
            GUI.Label(new Rect(bannerX + 25, bannerY + 48, bannerWidth - 50, bannerHeight - 95), _currentBody, bodyStyle);

            // Botón OK / CONTINUAR
            GUI.color = new Color(1f, 1f, 1f, _displayAlpha);
            float btnW = 160f;
            float btnH = 36f;
            float btnX = bannerX + (bannerWidth - btnW) * 0.5f;
            float btnY = bannerY + bannerHeight - 42f;

            GUIStyle btnStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Clamp((int)(screenHeight * 0.020f), 12, 16),
                fontStyle = FontStyle.Bold
            };
            btnStyle.normal.textColor = Color.white;

            if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), "CONTINUAR ▶", btnStyle))
            {
                Dismiss();
            }

            // Omitir al tocar cualquier parte del banner
            if (Event.current.type == EventType.MouseDown && new Rect(bannerX, bannerY, bannerWidth, bannerHeight).Contains(Event.current.mousePosition))
            {
                Dismiss();
                Event.current.Use();
            }

            GUI.color = oldColor;
        }
    }
}
