using UnityEngine;

[CreateAssetMenu(fileName = "Transition_Customer_SerchChair", menuName = "Transition/Customer/SerchChair")]
public class Transition_Customer_SerchChair : TrasiitionsBase_SO
{
    public override void OnTransition(Controller_Entity ce)
    {
        ce.SetMyChair(ce.CustomersBaseLogic.RequestChair());

        if (ce.MyChair)
        {
            ce.SetDestinationTarget(ce.MyChair.transform);
            ce.ChangeState(NextState);
        }
    }
}
