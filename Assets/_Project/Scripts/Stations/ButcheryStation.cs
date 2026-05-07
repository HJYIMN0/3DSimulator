using UnityEngine;

public class ButcheryStation : Workstation
{
    [Header("Meat Placement")]
    [SerializeField] private Transform meatPlacePoint;

    [Header("Cutting Minigame")]
    [SerializeField] private CuttingMinigame cuttingMinigame;

    [Header("Meat Processing")]
    [SerializeField] private MeatData requiredInput;
    [SerializeField] private MeatData outputMeat;

    private CarryableItem placedItem;

    public override bool IsInteractable => placedItem == null;

    protected override void HandleInteraction(Interactor interactor)
    {
        if ( interactor.CarryController == null)
            return;

        if(placedItem != null)
        {
            Debug.Log("There's already an item on the butchery station. Please wait until it's processed.");
            return;
        }

        if (!interactor.CarryController.HasItem)
        {
     
            Debug.Log("No item to place on the butchery station.");
            return;

        }

        CarryableItem carriedItem = interactor.CarryController.CarriedItem;

        if (carriedItem == null)
            return;

        if (requiredInput != null && carriedItem.MeatData != requiredInput)
        {
            Debug.Log("This meat cannot be processed here.");
            return;
        }

        placedItem = interactor.CarryController.ReleaseAt(meatPlacePoint);

        if (cuttingMinigame != null)
        {
            cuttingMinigame.StartMinigame(OnCutSuccess, OnCutFail);
        }
    }

    private void OnCutSuccess()
    {
        Debug.Log("Cut success.");

        if (placedItem != null)
            placedItem.Consume();

        placedItem = null;

        SpawnOutputMeat();
    }

    private void OnCutFail()
    {
        Debug.Log("Cutting failed! Try again.");

        if(cuttingMinigame != null)
        {
            cuttingMinigame.StartMinigame(OnCutSuccess, OnCutFail);
        }

        //per ora lasciamo ripetere poi implementeremo carne rovinata tagli etc...
    }

    private void SpawnOutputMeat()
    {
        if (outputMeat == null)
        {
            Debug.LogError("Output meal is not assigned in the inspector.");
            return;
        }

        if(outputMeat.Prefab == null)
        {
            Debug.LogError("Output meat prefab is not assigned in the inspector.");
            return;
        }

        Vector3 spawnPosition = meatPlacePoint != null ? meatPlacePoint.position : transform.position + transform.forward;
        Instantiate(outputMeat.Prefab, spawnPosition, Quaternion.identity);
    }
}
