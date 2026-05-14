using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Transition_Agent_DistanceToTarget", menuName = "Transition/Agent/DistanceToTarget")]

public class Transition_Agent_DistanceToTarget : TrasiitionsBase_SO
{
    public float MaxDistance = 3f;

    public override void OnTransition(Controller_Entity ce)
    {
        float distance = Vector3.Distance(ce.AnchorPosition, ce.Target.position);
        if (distance > MaxDistance) ce.ChangeState(NextState);
    }
}
