using UnityEngine;

public class PlayerCarryController : MonoBehaviour
{

    [SerializeField] private Transform holdPoint;

    private CarryableItem carriedItem;

    public bool HasItem => carriedItem != null;
    public CarryableItem CarriedItem => carriedItem;

    private void Awake()
    {
        if (holdPoint == null)
        {
            GameObject holder = new GameObject("HoldPoint");
            holder.transform.SetParent(transform);
            holder.transform.localPosition = new Vector3(0f, 1.3f, 0.8f);
            holder.transform.localRotation = Quaternion.identity;
            holdPoint = holder.transform;
        }
    }

    public bool TryPickUp(CarryableItem item)
    {
        if (HasItem || item == null || item.IsCarried)
            return false;
        carriedItem = item;
        carriedItem.PickUp(holdPoint);
        return true;
    }

    public CarryableItem ReleaseAt(Transform dropPoint)
    {
        if (!HasItem)
            return null;
        CarryableItem itemToDrop = carriedItem;
        carriedItem = null;

        itemToDrop.DropAt(dropPoint);
        return itemToDrop;
    }

    public CarryableItem TakeCarriedItem()
    {
        if (!HasItem)
            return null;
        CarryableItem item = carriedItem;
        carriedItem = null;
        return item;
    }

}


