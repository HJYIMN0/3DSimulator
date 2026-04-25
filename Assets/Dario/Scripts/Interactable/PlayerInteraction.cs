using UnityEngine;

namespace Character
{
    /// <summary>
    /// Handles player interaction with world objects.
    /// Multiplayer-ready: each player has own interaction component.
    /// </summary>
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private float interactRange = 3f;
        [SerializeField] private float interactRadius = 0.2f;
        [SerializeField] private LayerMask interactLayers = -1;

        [Header("UI Feedback")]
        [SerializeField] private GameObject interactIcon;
        [SerializeField] private UnityEngine.UI.Text interactText;

        [Header("References")]
        public InventorySystem Inventory; // Assigned by PlayerManager

        [Header("Debug")]
        public bool showDebugLogs = false;

        private Camera playerCamera;
        private GameObject currentPointedObject;

        private void Awake()
        {
            playerCamera = Camera.main;
        }

        private void Start()
        {
            // Fallback: try to get inventory if not assigned
            if (Inventory == null)
                Inventory = GetComponent<InventorySystem>();

            if (showDebugLogs && Inventory == null)
            {
                Debug.LogWarning($"PlayerInteraction on '{gameObject.name}' has no Inventory reference!");
            }
        }

        private void Update()
        {
            HandleInteractionRaycast();
        }

        /// <summary>
        /// Called from InputManager when interact button pressed
        /// </summary>
        public void TryInteract()
        {
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
                if (playerCamera == null) return;
            }

            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.5f));

            if (Physics.SphereCast(ray, interactRadius, out RaycastHit hit, interactRange, interactLayers))
            {
                if (showDebugLogs)
                    Debug.Log($"Interacted with: {hit.collider.name}");

                // Check for ItemObject (pickup)
                if (hit.collider.TryGetComponent<ItemObject>(out var item))
                {
                    if (Inventory != null)
                    {
                        // MULTIPLAYER-READY: Pass THIS player's inventory to item
                        item.OnHandlePickupItem(Inventory);
                    }
                    else
                    {
                        Debug.LogError("Cannot pickup item: PlayerInteraction has no Inventory reference!");
                    }
                    return;
                }

                // Check for Interactable (door, lever, etc)
                if (hit.collider.TryGetComponent<Interactable>(out var interactable))
                {
                    interactable.Interact();
                    return;
                }
            }
        }

        /// <summary>
        /// Continuous raycast for UI feedback
        /// </summary>
        private void HandleInteractionRaycast()
        {
            if (playerCamera == null) return;

            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.5f));
            bool foundInteractable = false;
            GameObject hitObject = null;

            if (Physics.SphereCast(ray, interactRadius, out RaycastHit hit, interactRange, interactLayers))
            {
                if (hit.collider.GetComponent<ItemObject>() != null ||
                    hit.collider.GetComponent<Interactable>() != null)
                {
                    foundInteractable = true;
                    hitObject = hit.collider.gameObject;

                    // Update interact text
                    if (interactText != null)
                    {
                        var interactable = hit.collider.GetComponent<Interactable>();
                        if (interactable != null && !string.IsNullOrEmpty(interactable.promptMessage))
                            interactText.text = interactable.promptMessage;
                        else
                            interactText.text = "Press [E] to interact";
                    }
                }
            }

            // Update pointed object tracking
            if (hitObject != currentPointedObject)
            {
                currentPointedObject = hitObject;
            }

            // Update UI
            if (interactIcon != null)
                interactIcon.SetActive(foundInteractable);

            if (interactText != null)
                interactText.gameObject.SetActive(foundInteractable);
        }

        /// <summary>
        /// Called when state changes
        /// </summary>
        public void OnStateChanged(CharacterState state)
        {
            enabled = (state == CharacterState.Default);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (playerCamera == null) return;

            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.5f));
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(ray.origin, ray.direction * interactRange);
            Gizmos.DrawWireSphere(ray.origin + ray.direction * interactRange, interactRadius);
        }
#endif
    }
}