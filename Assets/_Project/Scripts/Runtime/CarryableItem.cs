using UnityEngine;

public class CarryableItem : MonoBehaviour, IInteractable
{
    [SerializeField] private MeatData meatData;

    private Rigidbody _rigidbody;
    private Collider _collider;

    public MeatData MeatData => meatData;
    public bool IsCarried { get; private set; }

    public bool IsInteractable => !IsCarried;

    public string InteractionPrompt =>
        meatData != null ? $"Pick up {meatData.DisplayName}" : "Pick up item";

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
    }

    public void Interact(Interactor interactor)
    {
        if (interactor == null || interactor.CarryController == null)
            return;

        if (interactor.CarryController.HasItem)
        {
            Debug.Log("Cannot pick up item, already carrying something.");
            return;
        }

        interactor.CarryController.TryPickUp(this);
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

    public void Throw(Vector3 direction, float force)
    {
        if (_rigidbody == null)
        {
            Debug.LogWarning("Cannot throw item: missing Rigidbody.");
            return;
        }

        _rigidbody.AddForce(direction.normalized * force, ForceMode.Impulse);
    }
    public void Consume()
    {
        Destroy(gameObject);
    }
}