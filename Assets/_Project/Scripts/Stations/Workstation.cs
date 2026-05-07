using UnityEngine;

public abstract class Workstation : MonoBehaviour, IInteractable
{
    [SerializeField] private WorkstationData workstationData;

    public virtual bool IsInteractable => true;
    public string InteractionPrompt => workstationData != null ? workstationData.InteractionPrompt : "interact";

    public string DisplayName => workstationData != null ? workstationData.DisplayName : gameObject.name;

    public void Interact (Interactor interactor)
    {
        if(!IsInteractable)
            return;

        if (interactor == null)
            return;

        HandleInteraction(interactor);
    }

    protected abstract void HandleInteraction(Interactor interactor);
}
