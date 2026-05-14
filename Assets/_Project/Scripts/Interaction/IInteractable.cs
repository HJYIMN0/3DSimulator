public interface IInteractable
{
    void Interact(Interactor interactor);
    string GetInteractionPrompt(Interactor interactor);
    bool IsInteractable { get; }
}