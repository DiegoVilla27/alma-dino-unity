using System;
using UnityEngine;

namespace AlmaDino.Core.Events
{
    /// <summary>
    /// Bus estático global de eventos para solicitar el despliegue de banners narrativos y diálogos diegéticos.
    /// Permite que cualquier módulo (Player, Boss, Combat) solicite un banner sin acoplarse directamente a Features/Environment.
    /// </summary>
    public static class NarrativeBannerEvents
    {
        public static event Action<string, string, Color, float> OnBannerRequested;

        public static void RequestBanner(string title, string body, Color accentColor, float duration = 5.0f)
        {
            OnBannerRequested?.Invoke(title, body, accentColor, duration);
        }
    }
}
