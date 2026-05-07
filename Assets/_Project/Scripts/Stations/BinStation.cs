using UnityEngine;

public class BinStation : MonoBehaviour,IInteractable
{
    [Header("interaction")]
    [SerializeField] private WorkstationData workstationData;

    [Header("Bin Settings")]
    [SerializeField] private int binFillAmount = 3;

    private int currentItemCount;

    public bool IsInteractable => true;
    public string InteractionPrompt => workstationData != null ? workstationData.InteractionPrompt : "Put in trash";

    public void Interact(Interactor interactor)
    {
        if (interactor == null || interactor.CarryController == null)
            return;

        if (!interactor.CarryController.HasItem)
        {
           Debug.Log("No item to throw away.");
            return;
        }

        CarryableItem carriedItem = interactor.CarryController.TakeCarriedItem();

        if (carriedItem == null)
            return;

        carriedItem.Consume();

        currentItemCount++;
        Debug.Log($"Item thrown away. Current bin fill: {currentItemCount}/{binFillAmount}");

        if(currentItemCount >= binFillAmount)
        {
            Debug.Log("Bin is full! Please empty it.");
            // Here you could add logic to disable interaction until the bin is emptied
            CreateTrashBag();
        }
    }

    private void CreateTrashBag()
    {
        // Logic to create a trash bag or trigger an event for the full bin
        Debug.Log("Creating trash bag from full bin.");
        currentItemCount = 0; // Reset the bin after creating a trash bag
    }


}
