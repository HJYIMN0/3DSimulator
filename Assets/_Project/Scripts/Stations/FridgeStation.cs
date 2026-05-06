using UnityEngine;

public class FridgeStation : MonoBehaviour, IInteractable
{
    [SerializeField] private CarryableItem meatPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private string interactionPrompt = "Take meal";

    public bool IsInteractable => true;
    public string InteractionPrompt => interactionPrompt;

    public void Interact(Interactor interactor)
    {
        if (interactor == null || interactor.CarryController == null)
            return;

        if (interactor.CarryController.HasItem)
        {
            Debug.Log("Cannot pick up meat, already carrying something.");
            return;
        }

        if (meatPrefab == null)
        {
            Debug.LogError("Meat prefab is not assigned in the inspector.");
            return;
        }

        CarryableItem newMeat = Instantiate(meatPrefab, spawnPoint != null ? spawnPoint.position : transform.position + transform.forward,
            Quaternion.identity
        );

        interactor.CarryController.TryPickUp(newMeat);
    }

}
