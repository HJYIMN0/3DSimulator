using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemSO : ScriptableObject
{
    public string ItemName;
    public int ItemBuyCost;
    public int ItemSellCost;
}
