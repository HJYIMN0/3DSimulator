using UnityEngine;

public class OpenShopState : AbstractShopState
{
    public override void EnterState()
    {
        _customersBaseLogic.StartCoroutine(_customersBaseLogic.TryCustomersRoutine());
        _shopStateManager.IsShopOpen = true;

        Debug.Log("Entering Open Shop State: Shop is now open.");
    }

    public override void ExitState()
    {
        _shopStateManager.IsShopOpen = false;

        Debug.Log("Exiting Open Shop State: Shop is no more open.");
    }
}