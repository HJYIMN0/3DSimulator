// Invariato — il bug del "sempre open" era causato a cascata
// dal NullReferenceException in SetShopState, non da questa classe.
using UnityEngine;

public class TestingShopSystem : MonoBehaviour, IInteractable
{
    [Header("Reference")]
    [SerializeField] private ShopStateManager shopStateManager;

    public bool IsInteractable => true;

    public string InteractionPrompt => shopStateManager != null
        ? (shopStateManager.IsShopOpen ? "Close Shop" : "Open Shop")
        : "Toggle Shop";

    public void Interact(Interactor interactor)
    {
        ToggleShopState();
    }

    private void OpenShop()
    {
        if (shopStateManager == null) return;
        shopStateManager.SetShopState(shopStateManager.OpenShopState);
        Debug.Log("Shop opened.");
    }

    private void CloseShop()
    {
        if (shopStateManager == null) return;
        shopStateManager.SetShopState(shopStateManager.ClosedShopState);
        Debug.Log("Shop closed.");
    }

    public void ToggleShopState()
    {
        if (shopStateManager == null)
        {
            Debug.LogWarning("ShopStateManager reference missing!");
            return;
        }

        if (shopStateManager.IsShopOpen)
            CloseShop();
        else
            OpenShop();
    }
}