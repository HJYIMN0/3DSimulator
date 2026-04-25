using UnityEngine;

namespace Character
{
    public enum OrientationMethod
    {
        TowardsCamera,
        TowardsMovement,
    }

    public enum BonusOrientationMethod
    {
        None,
        TowardsGravity,
        TowardsGroundSlopeAndGravity,
    }

    public struct PlayerCharacterInputs
    {
        public float MoveAxisForward;
        public float MoveAxisRight;
        public Quaternion CameraRotation;
        public bool JumpDown;
        public bool CrouchDown;
        public bool CrouchUp;
        public bool SprintHeld;
        public bool Interact;
        public bool Drop;
        public bool Item1;
        public bool Item2;
        public bool Item3;
    }

    public struct AICharacterInputs
    {
        public Vector3 MoveVector;
        public Vector3 LookVector;
    }
}
