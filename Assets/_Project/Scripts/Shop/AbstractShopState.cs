using UnityEngine;
/// <summary>
/// Remember if you need to add more states, you need to update ShopStateManager to include the new state
/// </summary>
public abstract class AbstractShopState
{
    protected CustomersBaseLogic _customersBaseLogic;
    protected ShopStateManager _shopStateManager;

    public abstract void EnterState();
    public abstract void ExitState();

    public void Setup(ShopStateManager shopStateManager, CustomersBaseLogic customersBaseLogic)
    {
        _shopStateManager = shopStateManager;
        _customersBaseLogic = customersBaseLogic;

        Debug.Log("AbstractShopState Setup called by " + GetType().Name);
    }
}