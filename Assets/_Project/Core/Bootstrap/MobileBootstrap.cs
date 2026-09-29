using UnityEngine;

namespace AlmaDino.Core.Bootstrap
{
    public static class MobileBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeMobileSettings()
        {
            // Forzar 60 FPS o tasa de refresco nativa de pantalla móvil
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

            // Habilitar multi-touch explícitamente para joystick izquierdo + botones derechos simultáneos
            Input.multiTouchEnabled = true;

            // Evitar que la pantalla del móvil se apague durante la sesión de juego
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            Debug.Log("[MobileBootstrap] Configuración móvil inicializada: 60 FPS, Multi-touch activado, WakeLock activo.");
        }
    }
}
