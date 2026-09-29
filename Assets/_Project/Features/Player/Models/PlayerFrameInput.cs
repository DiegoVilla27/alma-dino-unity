using UnityEngine;

namespace AlmaDino.Features.Player.Models
{
    public struct PlayerFrameInput
    {
        public Vector2 MoveVector;
        public bool JumpDown;
        public bool JumpHeld;
        public bool JumpUp;
        public bool DashDown;
        public bool GroundPoundDown;
        public bool RoarDown;
    }
}
