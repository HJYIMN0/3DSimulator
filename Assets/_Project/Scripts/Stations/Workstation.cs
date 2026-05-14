using UnityEngine;

public class Workstation : MonoBehaviour, IInteractable
{
    [Header("Workstation Data")]
    [SerializeField] private WorkstationData workstationData;

    [Header("Meat Placement")]
    [SerializeField] private Transform meatPlacePoint;

    [Header("Processing Minigame")]
    [SerializeField] private MonoBehaviour minigameBehaviour;

    [Header("Meat Processing")]
    [SerializeField] private MeatData requiredInput;
    [SerializeField] private MeatData successOutput;
    [SerializeField] private MeatData failOutput;

    private CarryableItem placedItem;
    private IProcessingMinigame processingMinigame;

    public bool IsInteractable => placedItem == null;

    public string DisplayName =>
        workstationData != null ? workstationData.DisplayName : gameObject.name;

    private void Awake()
    {
        if (minigameBehaviour == null)
            return;

        processingMinigame = minigameBehaviour as IProcessingMinigame;

        if (processingMinigame == null)
            Debug.LogError($"{minigameBehaviour.name} does not implement IProcessingMinigame.");
    }

    public void Interact(Interactor interactor)
    {
        if (!IsInteractable)
            return;

        if (interactor == null || interactor.CarryController == null)
            return;

        if (!interactor.CarryController.HasItem)
        {
            Debug.Log("No item to place on workstation.");
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

        if (processingMinigame != null)
            processingMinigame.StartMinigame(OnProcessingCompleted);
        else
            OnProcessingCompleted(true);
    }

    private void OnProcessingCompleted(bool success)
    {
        MeatData output = success ? successOutput : failOutput;

        if (placedItem != null)
            placedItem.Consume();

        placedItem = null;

        SpawnOutput(output);
    }

    private void SpawnOutput(MeatData output)
    {
        if (output == null)
        {
            Debug.LogWarning("No output meat assigned for this processing result.");
            return;
        }

        if (output.Prefab == null)
        {
            Debug.LogError($"Output meat prefab is missing in MeatData: {output.name}");
            return;
        }

        Vector3 spawnPosition = meatPlacePoint != null
            ? meatPlacePoint.position
            : transform.position + transform.forward;

        Instantiate(output.Prefab, spawnPosition, Quaternion.identity);
    }

    public string GetInteractionPrompt(Interactor interactor)
    {
        if (!IsInteractable)
            return workstationData != null ? workstationData.BlockedPrompt : "";

        if (interactor == null || interactor.CarryController == null)
            return "";

        if (!interactor.CarryController.HasItem)
            return workstationData != null ? workstationData.EmptyHandPrompt : "Prendi qualcosa prima";

        CarryableItem carriedItem = interactor.CarryController.CarriedItem;

        if (carriedItem == null)
            return "";

        if (requiredInput != null && carriedItem.MeatData != requiredInput)
            return workstationData != null ? workstationData.InvalidItemPrompt : "Oggetto non valido";

        string basePrompt = workstationData != null ? workstationData.InteractionPrompt : "interagire";
        return $"Premi E per {basePrompt}";
    }
}