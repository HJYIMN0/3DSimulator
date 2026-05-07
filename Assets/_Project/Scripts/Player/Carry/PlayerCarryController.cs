using UnityEngine;

public class PlayerCarryController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Transform dropPoint;
    [SerializeField] private Transform throwDirectionReference;

    [Header("Throw Settings")]
    [SerializeField] private float throwForce = 8f;

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

        if (dropPoint == null)
        {
            GameObject drop = new GameObject("DropPoint");
            drop.transform.SetParent(transform);
            drop.transform.localPosition = new Vector3(0f, 0.8f, 1.2f);
            drop.transform.localRotation = Quaternion.identity;
            dropPoint = drop.transform;
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

    public CarryableItem ReleaseAt(Transform targetPoint)
    {
        if (!HasItem)
            return null;

        CarryableItem itemToDrop = carriedItem;
        carriedItem = null;

        itemToDrop.DropAt(targetPoint);
        return itemToDrop;
    }

    public void DropCarriedItem()
    {
        if (!HasItem)
        {
            Debug.Log("Drop failed: no carried item.");
            return;
        }

        ReleaseAt(dropPoint);
        Debug.Log("Dropped carried item.");
    }

    public void ThrowCarriedItem()
    {
        if (!HasItem)
        {
            Debug.Log("Throw failed: no carried item.");
            return;
        }

        CarryableItem itemToThrow = ReleaseAt(dropPoint);

        if (itemToThrow == null)
            return;

        Vector3 throwDirection = throwDirectionReference != null
            ? throwDirectionReference.forward
            : transform.forward;

        itemToThrow.Throw(throwDirection, throwForce);

        Debug.Log("Thrown carried item.");
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