using UnityEngine;

namespace Character
{
    public class PlayerManager : MonoBehaviour
    {
        [Header("Components")]
        public RbPlayerController Movement;
        public PlayerInteraction Interaction;
        public InventorySystem Inventory;

        [Header("Camera")]
        public Transform CameraFollowPoint;

        public CharacterState CurrentState { get; private set; } = CharacterState.Default;

        private void Awake()
        {
            // Auto-assign components
            if (Movement == null)
                Movement = GetComponent<RbPlayerController>();

            if (Interaction == null)
                Interaction = GetComponent<PlayerInteraction>();

            if (Inventory == null)
                Inventory = GetComponent<InventorySystem>();

            // Link Inventory to Interaction
            if (Interaction != null && Inventory != null)
                Interaction.Inventory = Inventory;

            // Setup Camera Follow Point if not assigned
            if (CameraFollowPoint == null)
            {
                // Create CameraFollowPoint at eye height
                GameObject followPoint = new GameObject("CameraFollowPoint");
                followPoint.transform.SetParent(transform);
                followPoint.transform.localPosition = new Vector3(0, 1.7f, 0); // Eye height
                followPoint.transform.localRotation = Quaternion.identity;
                CameraFollowPoint = followPoint.transform;
            }

            ApplyState(CurrentState);
        }

        public void SetState(CharacterState newState)
        {
            if (CurrentState == newState)
                return;

            CurrentState = newState;
            ApplyState(newState);
        }

        private void ApplyState(CharacterState state)
        {
            // Notify movement of state change
            if (Movement != null)
                Movement.OnStateChanged(state);

            // Notify interaction of state change
            if (Interaction != null)
                Interaction.OnStateChanged(state);
        }
    }
}