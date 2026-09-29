using UnityEngine;

namespace AlmaDino.Core.Utilities
{
    public static class VirtualInputBridge
    {
        public static Vector2 MoveVector { get; set; }
        public static bool JumpDown { get; set; }
        public static bool JumpHeld { get; set; }
        public static bool JumpUp { get; set; }
        public static bool DashDown { get; set; }
        public static bool GroundPoundDown { get; set; }
        public static bool RoarDown { get; set; }

        public static void ConsumeFrameTriggers()
        {
            JumpDown = false;
            JumpUp = false;
            DashDown = false;
            GroundPoundDown = false;
            RoarDown = false;
        }

        public static void TriggerJump()
        {
            JumpDown = true;
            JumpHeld = true;
        }

        public static void ReleaseJump()
        {
            JumpHeld = false;
            JumpUp = true;
        }

        public static void TriggerDash()
        {
            DashDown = true;
        }

        public static void TriggerGroundPound()
        {
            GroundPoundDown = true;
        }

        public static void TriggerRoar()
        {
            RoarDown = true;
        }
    }
}
