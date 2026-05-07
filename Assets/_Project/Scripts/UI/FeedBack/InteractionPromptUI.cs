using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{

    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TMPro.TextMeshProUGUI promptText;
    [SerializeField] private Interactor interaction;

    private bool isPromptActive = false;

    private void Awake()
    {
        if (promptPanel == null)
        {
            Debug.LogError("InteractionPromptUI: No Prompt Panel assigned.");
            return;
        }
        promptPanel.SetActive(false);
    }
    private void Start()
    {
        if (interaction == null)
        {
            Debug.LogError("InteractionPromptUI: No Interactor assigned.");
            return;
        }

    }


    private void Update()
    {
        if (interaction == null || promptPanel == null || promptText == null)
            return;

        Refresh(interaction.CurrentInteractable);
    }

    private void Show()
    {
        if (isPromptActive)
            return;
        promptPanel.SetActive(true);
        isPromptActive = true;
    }

    private void Hide()
    {
        if (!isPromptActive)
            return;
        promptPanel.SetActive(false);
        isPromptActive = false;
    }

    private void Refresh(IInteractable interactable)
    {
        if (interactable != null)
        {
            promptText.text = interactable.InteractionPrompt;
            Show();
        }
        else
        {
            Hide();
        }
    }

}
