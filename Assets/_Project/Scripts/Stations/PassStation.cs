using UnityEngine;

public class PassStation : MonoBehaviour, IInteractable
{

    [Header("Interaction")]
    [SerializeField] private WorkstationData workstationData;

    [Header("Reference")]
    [SerializeField] private Transform dropPoint;

    [Header("Pass Settings")]
    [SerializeField] private MeatData requiredMeat;
    [SerializeField] private bool acceptAnyCookedMeat = true;

    public GameObject[] TESTOBJ;

    private int deliveredMeatCount = 0;

    public bool IsInteractable => true;
    public string InteractionPrompt => workstationData != null ? workstationData.InteractionPrompt : "Serve Dish";

    [ContextMenu("TESTSPAWNOBJ")]
    public void TESTSPAWNOBJ() => Instantiate(TESTOBJ[Random.Range(0, TESTOBJ.Length)], dropPoint.position, Quaternion.identity);

    public void Interact(Interactor interactor)
    {
    
        if (interactor == null || interactor.CarryController == null)
            return;

        if(!interactor.CarryController.HasItem)
            return;

        CarryableItem carriedItem = interactor.CarryController.CarriedItem;

        if (carriedItem == null || carriedItem.MeatData == null)
            return;

        if (!CanAccept(carriedItem))
        {
            Debug.Log("This station does not accept this item.");
            return;
        }

        carriedItem = interactor.CarryController.TakeCarriedItem();

        if (carriedItem == null)
            return;

        deliveredMeatCount++;
        Debug.Log($"Delivered {carriedItem.MeatData.DisplayName}. Total delivered: {deliveredMeatCount}");
        carriedItem.DropAt(dropPoint);
    }

    private bool CanAccept(CarryableItem item)
    {
        if (item == null || item.MeatData == null)
            return false;

        if (requiredMeat != null)
            return item.MeatData == requiredMeat;

        if (acceptAnyCookedMeat)
            return item.MeatData.ProcessingState == MeatData.ProcessedState.Cooked;

        return false;
    }
}
