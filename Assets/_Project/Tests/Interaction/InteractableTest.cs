using UnityEngine;

public class InteractableTest : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionPrompt = "Interact";

    public bool IsInteractable => true;
    public string InteractionPrompt => interactionPrompt;

    public void Interact(Interactor interactor)
    {
        Debug.Log($"Interacted with {gameObject.name}");
    }
}