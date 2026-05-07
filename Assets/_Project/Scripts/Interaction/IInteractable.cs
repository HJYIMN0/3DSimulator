
public interface IInteractable
{

    void Interact( Interactor interactor );

    string InteractionPrompt { get; }
    bool IsInteractable { get; }

}
