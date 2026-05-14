using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "State_Agent_Wait", menuName = "States/Agent/Wait")]
public class State_Agent_Wait : StateEntity_SO
{
    public bool IsKinematic = true;

    public override void Enter(Controller_Entity ce)
    {
        ce.Animator_Generic.AnimationMoving(0f, ce.Entity.RunSpeed, ce.RbEntity);

        ce.RbEntity.isKinematic = IsKinematic;
    }

    public override void Exit(Controller_Entity ce)
    {
        ce.RbEntity.isKinematic = false;
    }
}
