using UnityEngine;

public class CarryableItem : MonoBehaviour
{
    public bool IsCarried { get; private set; }

    private Rigidbody _rigidbody;
    private Collider _collider;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
    }

    public void PickUp(Transform parent)
    {
        IsCarried = true;
        if (_rigidbody != null)
            _rigidbody.isKinematic = true;


        if (_collider != null)
            _collider.enabled = false;

        transform.SetParent(parent);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void DropAt(Transform dropPoint)
    {
        if (dropPoint == null)
        {
            Debug.LogError("Drop point is null. Cannot drop item.");
            return;
        }
        IsCarried = false;

        transform.SetParent(null);
        transform.position = dropPoint.position;
        transform.rotation = dropPoint.rotation;

        if (_rigidbody != null)
            _rigidbody.isKinematic = false;

        if (_collider != null)
            _collider.enabled = true;
    }

    public void Consume()
    {
        Destroy(gameObject);
    }
}
