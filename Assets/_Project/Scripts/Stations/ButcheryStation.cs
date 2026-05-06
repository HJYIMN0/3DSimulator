using UnityEngine;

public class ButcheryStation : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionPrompt = "Cut the meat";
    [SerializeField] private Transform meatPlacePoint;
    [SerializeField] private CuttingMinigame cuttingMinigame;

    private CarryableItem placedItem;

    public bool IsInteractable => true;
    public string InteractionPrompt => interactionPrompt;

    public void Interact(Interactor interactor)
    {
        if (interactor == null || interactor.CarryController == null)
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

        placedItem = interactor.CarryController.ReleaseAt(meatPlacePoint);

        if (cuttingMinigame != null)
        {
            cuttingMinigame.StartMinigame(OnCutSuccess, OnCutFail);
        }
    }

    private void OnCutSuccess()
    {
        if (placedItem != null)
        {
            Debug.Log("Cut success.");

            if (placedItem != null)
                placedItem.Consume();
            //Per ora lo consumo poi implementerò il rilascio del taglio

            placedItem = null;
        }
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
}
