using UnityEngine;

namespace Character
{
    /// <summary>
    /// Pickup item in world.
    /// Multiplayer-ready: Receives inventory reference from who picks it up.
    /// </summary>
    [RequireComponent(typeof(EquipableItem))]
    public class ItemObject : MonoBehaviour
    {
        [Header("Item Data")]
        public InventoryItemData referenceItem;

        [Header("Gameplay")]
        [Tooltip("If true, picking this up alerts enemies to player position")]
        public bool alertsEnemies = false;

        [Header("Audio")]
        public AudioClip pickupSound;

        /// <summary>
        /// Called by PlayerInteraction when player picks up this item.
        /// Multiplayer-ready: receives inventory of the player who picked it up.
        /// </summary>
        public void OnHandlePickupItem(InventorySystem inventory)
        {
            if (referenceItem == null)
            {
                Debug.LogWarning($"ItemObject '{name}' has no referenceItem assigned!");
                return;
            }

            if (inventory == null)
            {
                Debug.LogError("ItemObject.OnHandlePickupItem() called with NULL inventory!");
                return;
            }

            // Try to add to inventory
            if (inventory.Add(referenceItem, transform.position))
            {
                Debug.Log($" Picked up: {referenceItem.displayName}");

                // Play pickup sound if assigned
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }

                // Trigger AI alert via event
                if (alertsEnemies)
                {
                    Debug.Log($" Item '{referenceItem.displayName}' alerts enemies!");
                }

                // Destroy item in world
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning($" Inventory full! Cannot pick up {referenceItem.displayName}");
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (GetComponent<EquipableItem>() == null)
            {
                gameObject.AddComponent<EquipableItem>();
            }
        }
#endif
    }
}