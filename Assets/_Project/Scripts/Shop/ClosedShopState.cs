using UnityEngine;

public class ClosedShopState : AbstractShopState
{
    public override void EnterState()
    {
        Debug.Log("Entering Closed Shop State: Shop is now closed.");
        _customersBaseLogic.DismissAllCustomers();
    }

    public override void ExitState()
    {
        Debug.Log("Exiting Closed Shop State: Shop is no more closed.");
    }
}