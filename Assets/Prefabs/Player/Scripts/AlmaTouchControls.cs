namespace AlmaGame.Player
{
    // On-screen (touch) controls feed Alma through here; AlmaInput merges them with keyboard and gamepad.
    // Button presses are latched until AlmaInput consumes them, so a tap shorter than a frame is never lost.
    public static class AlmaTouchControls
    {
        private static bool s_jumpPressed, s_poundPressed, s_dashPressed, s_roarPressed;

        public static bool JumpHeld { get; private set; }
        // Horizontal stick/pad value in -1..1 (for the future movement control).
        public static float Move { get; set; }

        public static void PressJump() { s_jumpPressed = true; JumpHeld = true; }
        public static void ReleaseJump() => JumpHeld = false;
        public static void PressPound() => s_poundPressed = true;
        public static void PressDash() => s_dashPressed = true;
        public static void PressRoar() => s_roarPressed = true;

        public static bool ConsumeJump() { bool v = s_jumpPressed; s_jumpPressed = false; return v; }
        public static bool ConsumePound() { bool v = s_poundPressed; s_poundPressed = false; return v; }
        public static bool ConsumeDash() { bool v = s_dashPressed; s_dashPressed = false; return v; }
        public static bool ConsumeRoar() { bool v = s_roarPressed; s_roarPressed = false; return v; }

        public static void Clear()
        {
            s_jumpPressed = s_poundPressed = s_dashPressed = s_roarPressed = false;
            JumpHeld = false;
            Move = 0f;
        }
    }
}
