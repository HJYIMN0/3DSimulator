using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUIController : MonoBehaviour
{
    [System.Serializable]
    public class SlotUI
    {
        public GameObject slotRoot;     // il contenitore dello slot
        public Image iconImage;         // icona oggetto
        public TMP_Text nameText;       // nome oggetto
        public TMP_Text amountText;     // quantità
        public GameObject highlight;    // bordo o effetto di selezione
    }

    [Header("Riferimenti")]
    public Character.InventorySystem inventory;   // collegalo al player
    public SlotUI[] slots;              // 3 slot configurabili

    private void Start()
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<Character.InventorySystem>();

        UpdateUI();
        inventory.OnInventoryChanged += UpdateUI;
    }

    private void OnDestroy()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= UpdateUI;
    }

    public void UpdateUI()
    {
        if (inventory == null || slots == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (i < inventory.inventory.Count)
            {
                Character.InventoryItem item = inventory.inventory[i];
                var slot = slots[i];

                slot.slotRoot.SetActive(true);
                slot.iconImage.sprite = item.data.icon;
                slot.iconImage.enabled = true;
                slot.nameText.text = item.data.displayName;

                slot.amountText.text = item.stackSize > 1 ? item.stackSize.ToString() : "";

                // Attiva il bordo se è quello equipaggiato
                bool isEquipped = (i == inventory.GetEquippedIndex());

                if (slot.highlight != null)
                    slot.highlight.SetActive(isEquipped);
            }
            else
            {
                // slot vuoto → nascondi tutto
                slots[i].slotRoot.SetActive(false);
            }
        }
    }
}
