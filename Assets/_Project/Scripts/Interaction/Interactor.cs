using UnityEngine;

public class Interactor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;

    [Header("Settings")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask interactionRaycastMask = ~0;
    [SerializeField] private float checkInterval = 0.1f;

    private float nextCheckTime;

    private IInteractable currentInteractable;

    public IInteractable CurrentInteractable => currentInteractable;
    public PlayerCarryController CarryController { get; private set; }

    private void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        
        CarryController = GetComponentInParent<PlayerCarryController>();
        
        if(CarryController == null)
            CarryController = GetComponent<PlayerCarryController>();

    }

    private void Update()
    {
        if (Time.time < nextCheckTime) //Metto un intervallo di tempo tra un controllo e l'altro per evitare di fare troppi raycast ma non serve davvero
            return;

        nextCheckTime = Time.time + checkInterval;
        UpdateCurrentInteractable();
    }

    public void TryInteract()
    {
        if (currentInteractable == null)
            return;
        if (!currentInteractable.IsInteractable)
            return;

        currentInteractable.Interact(this);
    }

    private void UpdateCurrentInteractable()
    {
        currentInteractable = null;

        if (playerCamera == null)
            return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionRaycastMask))
            return;

        IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();

        if (interactable == null)
            return;

        if (!interactable.IsInteractable)
            return;

        currentInteractable = interactable;
    }

    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        if (playerCamera == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactionDistance);
    }

    #endregion

    #region Test

    [ContextMenu("Test Interact")]
    public void TestInteract()
    {
        UpdateCurrentInteractable();
        TryInteract();
    }

    #endregion

}

