using System;

namespace AlmaGame.Systems
{
    // Global "a cinematic is playing" flag. Cinematics call Begin() when they start and End() when they finish
    // (nested ones are counted); the HUD hides while it is on.
    public static class CinematicState
    {
        private static int s_depth;

        public static bool IsPlaying => s_depth > 0;
        public static event Action Changed;

        public static void Begin()
        {
            s_depth++;
            if (s_depth == 1) Changed?.Invoke();
        }

        public static void End()
        {
            if (s_depth == 0) return;
            s_depth--;
            if (s_depth == 0) Changed?.Invoke();
        }

        // Scene changes never leave the HUD hidden by a cinematic that did not finish.
        public static void Reset()
        {
            bool was = s_depth > 0;
            s_depth = 0;
            if (was) Changed?.Invoke();
        }
    }
}
