using UnityEngine;

public class ShopStateManager : MonoBehaviour
{
    [SerializeField] private CustomersBaseLogic customersBaseLogic;
    [SerializeField] private bool isShopOpenOnStart = true;

    public AbstractShopState CurrentState => _currentState;
    private AbstractShopState _currentState;

    public AbstractShopState OpenShopState { get; private set; }
    public AbstractShopState ClosedShopState { get; private set; }

    public bool IsShopOpen { get; set; }

    private void Start()
    {
        OpenShopState = new OpenShopState();
        ClosedShopState = new ClosedShopState();

        // MODIFICATO: unificato in una riga — SetShopState gestisce
        // già entrambi i casi senza duplicare logica.
        AbstractShopState initialState = isShopOpenOnStart ? OpenShopState : ClosedShopState;
        SetShopState(initialState);
    }

    public void SetShopState(AbstractShopState newState)
    {
        if (newState == null || newState == _currentState)
        {
            Debug.Log("ShopStateManager: SetShopState - Invalid state or same as current. No state change.");
            return;
        }

        // MODIFICATO: aggiunto ?. per gestire _currentState null alla prima chiamata.
        _currentState?.ExitState();

        _currentState = newState;
        _currentState.Setup(this, customersBaseLogic);
        _currentState.EnterState();
    }
}