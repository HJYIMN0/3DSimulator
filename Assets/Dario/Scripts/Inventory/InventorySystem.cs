using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Character
{
    /// <summary>
    /// Manages player inventory with ScriptableObject-based items.
    /// Refactored: No singleton pattern, uses events for AI alerts.
    /// </summary>
    public class InventorySystem : MonoBehaviour
    {
        [Header("Inventory Settings")]
        public int maxSlots = 3;

        [Header("Equip Settings")]
        public Transform handSlot;

        [Header("Drop Settings")]
        [Tooltip("Distance to place item when dropping gently")]
        public float dropDistance = 2f;

        [Tooltip("Layers to raycast for drop placement")]
        public LayerMask dropRaycastLayers = -1;

        [Header("Throw Settings")]
        [Tooltip("Minimum force when throwing (0% charge)")]
        public float minThrowForce = 5f;

        [Tooltip("Maximum force when throwing (100% charge)")]
        public float maxThrowForce = 20f;

        [Tooltip("Optional: Curve for throw force scaling")]
        public AnimationCurve throwForceCurve = AnimationCurve.Linear(0, 0, 1, 1);

        // Inventory data
        private Dictionary<InventoryItemData, InventoryItem> m_itemDictionary;
        public List<InventoryItem> inventory { get; private set; }

        // Equipped item tracking
        private GameObject equippedObject;
        private int equippedIndex = -1;

        // Events
        public event System.Action OnInventoryChanged;
        public static event System.Action<Vector3> OnItemPickedUp; // For AI alert

        private void Awake()
        {
            inventory = new List<InventoryItem>();
            m_itemDictionary = new Dictionary<InventoryItemData, InventoryItem>();
        }

        /// <summary>
        /// Get item from inventory by reference data
        /// </summary>
        public InventoryItem Get(InventoryItemData referenceData)
        {
            if (referenceData == null) return null;

            // Check dictionary (stackable items)
            if (m_itemDictionary.TryGetValue(referenceData, out InventoryItem value))
                return value;

            // Check list (non-stackable items)
            foreach (var item in inventory)
            {
                if (item.data == referenceData)
                    return item;
            }

            return null;
        }

        /// <summary>
        /// Add item to inventory
        /// </summary>
        public bool Add(InventoryItemData referenceData, Vector3 pickupPosition = default)
        {
            if (referenceData == null)
            {
                Debug.LogWarning("Cannot add null item to inventory!");
                return false;
            }

            // Check max slots
            if (inventory.Count >= maxSlots && !m_itemDictionary.ContainsKey(referenceData))
            {
                Debug.LogWarning($"Inventory full! Cannot add {referenceData.displayName}");
                return false;
            }

            // Handle non-stackable items
            if (!referenceData.isStackable)
            {
                if (inventory.Count >= maxSlots)
                {
                    Debug.LogWarning("Inventory full!");
                    return false;
                }

                InventoryItem newItem = new InventoryItem(referenceData);
                inventory.Add(newItem);
                OnInventoryChanged?.Invoke();

                // Trigger pickup event (for AI)
                OnItemPickedUp?.Invoke(pickupPosition);

                Debug.Log($"Added {referenceData.displayName} to inventory");
                return true;
            }

            // Handle stackable items
            if (m_itemDictionary.TryGetValue(referenceData, out InventoryItem value))
            {
                value.AddToStack();
                Debug.Log($"Stacked {referenceData.displayName} (x{value.stackSize})");
            }
            else
            {
                InventoryItem newItem = new InventoryItem(referenceData);
                inventory.Add(newItem);
                m_itemDictionary.Add(referenceData, newItem);
                Debug.Log($"Added {referenceData.displayName} to inventory");
            }

            OnInventoryChanged?.Invoke();

            // Trigger pickup event (for AI)
            OnItemPickedUp?.Invoke(pickupPosition);

            return true;
        }

        /// <summary>
        /// Remove item from inventory
        /// </summary>
        public void Remove(InventoryItemData referenceData)
        {
            if (referenceData == null) return;

            InventoryItem itemToRemove = null;

            // Check dictionary (stackable)
            if (m_itemDictionary.TryGetValue(referenceData, out InventoryItem value))
            {
                value.RemoveFromStack();

                if (value.stackSize <= 0)
                {
                    inventory.Remove(value);
                    m_itemDictionary.Remove(referenceData);
                    itemToRemove = value;
                }
            }
            else
            {
                // Check list (non-stackable)
                for (int i = 0; i < inventory.Count; i++)
                {
                    if (inventory[i].data == referenceData)
                    {
                        itemToRemove = inventory[i];
                        inventory.RemoveAt(i);
                        break;
                    }
                }
            }

            if (itemToRemove != null)
                Debug.Log($"Removed: {referenceData.displayName}");

            OnInventoryChanged?.Invoke();
        }

        /// <summary>
        /// Cycle through equipped items (for scroll wheel).
        /// Direction: 1 = next, -1 = previous
        /// </summary>
        public void CycleEquippedItem(int direction)
        {
            if (inventory.Count == 0)
            {
                Debug.Log("Inventory empty, cannot cycle");
                return;
            }

            int newIndex;

            if (equippedIndex < 0)
            {
                // Nothing equipped, equip first item
                newIndex = 0;
            }
            else
            {
                // Cycle to next/previous slot
                newIndex = equippedIndex + direction;

                // Wrap around
                if (newIndex >= inventory.Count)
                    newIndex = 0;
                else if (newIndex < 0)
                    newIndex = inventory.Count - 1;
            }

            EquipItem(newIndex);
        }

        /// <summary>
        /// Equip item at index
        /// </summary>
        public void EquipItem(int index)
        {
            if (index < 0 || index >= inventory.Count)
            {
                Debug.LogWarning($"Invalid inventory index: {index}");
                return;
            }

            // Toggle off if already equipped
            if (index == equippedIndex)
            {
                UnequipItem();
                return;
            }

            // Destroy current equipped object
            if (equippedObject != null)
                Destroy(equippedObject);

            // Instantiate new equipped object
            InventoryItem item = inventory[index];

            if (handSlot == null)
            {
                Debug.LogWarning("Hand slot not assigned! Cannot equip item.");
                return;
            }

            equippedObject = Instantiate(item.data.prefab, handSlot);
            equippedObject.transform.localPosition = Vector3.zero;
            equippedObject.transform.localRotation = Quaternion.identity;

            // Disable physics on equipped item
            var equipScript = equippedObject.GetComponent<EquipableItem>();
            if (equipScript != null)
                equipScript.OnEquipped();

            IgnoreCollisionsWithPlayer(equippedObject, true);

            equippedIndex = index;
            Debug.Log($"Equipped: {item.data.displayName}");

            OnInventoryChanged?.Invoke();
        }

        /// <summary>
        /// Unequip current item
        /// </summary>
        public void UnequipItem()
        {
            if (equippedObject != null)
            {
                Destroy(equippedObject);
                equippedObject = null;
            }

            Debug.Log("Item unequipped");
            equippedIndex = -1;
            OnInventoryChanged?.Invoke();
        }

        /// <summary>
        /// Drop currently equipped item
        /// </summary>
        public void DropItem(bool isThrow = false, float chargePercent = 0f, Vector3 throwDirection = default)
        {
            if (equippedIndex < 0 || equippedIndex >= inventory.Count)
                return;

            InventoryItem item = inventory[equippedIndex];

            // Destroy equipped model
            if (equippedObject != null)
                Destroy(equippedObject);

            // Spawn item in world
            GameObject dropped = Instantiate(item.data.prefab);

            IgnoreCollisionsWithPlayer(dropped, false);

            if (isThrow)
            {
                PlaceAndThrowItem(dropped, chargePercent, throwDirection);
                Debug.Log($"Threw {item.data.displayName} with {chargePercent:P0} charge");
            }
            else
            {
                PlaceItemGentle(dropped);
                Debug.Log($"Dropped {item.data.displayName} gently");
            }

            // Remove from inventory
            Remove(item.data);

            // Handle equip index after drop
            if (inventory.Count == 0)
            {
                equippedIndex = -1;
            }
            else if (equippedIndex >= inventory.Count)
            {
                equippedIndex = inventory.Count - 1;
                EquipItem(equippedIndex);
            }

            OnInventoryChanged?.Invoke();
        }
        /// <summary>
        /// Enable/disable collisions between equipped item and player
        /// </summary>
        private void IgnoreCollisionsWithPlayer(GameObject item, bool ignore)
        {
            if (item == null) return;

            // Get all colliders on item
            Collider[] itemColliders = item.GetComponentsInChildren<Collider>();

            // Get all colliders on player
            Collider[] playerColliders = GetComponentsInChildren<Collider>();

            // Ignore collisions between all combinations
            foreach (var itemCol in itemColliders)
            {
                foreach (var playerCol in playerColliders)
                {
                    Physics.IgnoreCollision(itemCol, playerCol, ignore);
                }
            }

            if (ignore)
                Debug.Log($"Disabled collisions between {item.name} and player");
            else
                Debug.Log($"Enabled collisions between {item.name} and player");
        }

        /// <summary>
        /// Place item gently using raycast from camera
        /// </summary>
        private void PlaceItemGentle(GameObject item)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("No main camera found for drop placement!");
                item.transform.position = transform.position + transform.forward * dropDistance;
                EnablePhysics(item, false);
                return;
            }

            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.5f));

            if (Physics.Raycast(ray, out RaycastHit hit, dropDistance, dropRaycastLayers))
            {
                // Place at hit point with slight offset along normal
                item.transform.position = hit.point + hit.normal * 0.1f;
                item.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            }
            else
            {
                // No hit - place at max distance in look direction
                item.transform.position = ray.origin + ray.direction * dropDistance;
                item.transform.rotation = Quaternion.identity;
            }

            EnablePhysics(item, false);
        }

        /// <summary>
        /// Place item and apply throw force
        /// </summary>
        private void PlaceAndThrowItem(GameObject item, float chargePercent, Vector3 direction)
        {
            // Start position slightly in front of player
            item.transform.position = transform.position + transform.forward * 1f + Vector3.up * 1f;
            item.transform.rotation = Quaternion.identity;

            EnablePhysics(item, true);

            // Apply throw force
            var rb = item.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Use curve if assigned, otherwise linear
                float curveValue = throwForceCurve.Evaluate(chargePercent);
                float force = Mathf.Lerp(minThrowForce, maxThrowForce, curveValue);

                rb.AddForce(direction * force, ForceMode.Impulse);

                // Optional: Add slight upward force for better arc
                rb.AddForce(Vector3.up * force * 0.2f, ForceMode.Impulse);
            }
        }

        /// <summary>
        /// Enable physics on dropped item
        /// </summary>
        private void EnablePhysics(GameObject item, bool withVelocity)
        {
            var equipScript = item.GetComponent<EquipableItem>();
            if (equipScript != null)
                equipScript.OnDropped();

            // Ensure rigidbody exists and is not kinematic
            var rb = item.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;

                if (!withVelocity)
                {
                    // Reset velocities for gentle drop
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }
        /// <summary>
        /// Get currently equipped item index
        /// </summary>
        public int GetEquippedIndex()
        {
            return equippedIndex;
        }

        /// <summary>
        /// Transfer item to another inventory (for multiplayer/NPC trading)
        /// </summary>
        public bool TransferItemTo(InventorySystem target, InventoryItemData data, int amount = 1)
        {
            if (target == null || data == null) return false;

            InventoryItem item = Get(data);
            if (item == null || item.stackSize < amount)
                return false;

            for (int i = 0; i < amount; i++)
                Remove(data);

            target.Add(data);
            OnInventoryChanged?.Invoke();

            return true;
        }
        
    }
}