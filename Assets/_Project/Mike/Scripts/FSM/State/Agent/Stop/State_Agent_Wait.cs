using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "State_Agent_Wait", menuName = "States/Agent/Wait")]
public class State_Agent_Wait : StateEntity_SO
{
    public float TimeWait = 20f;
    public bool IsKinematic = true;
    public StateEntity_SO NextState;

    public override void Enter(Controller_Entity ce)
    {
        ce.RbEntity.isKinematic = IsKinematic;
    }

    public override void StateUpdate(Controller_Entity ce)
    {
        if (TimeWait < 0) return;

        if (TimeWait <= ce.TimeOnCurrentState) ce.ChangeState(NextState);
    }

    public override void Exit(Controller_Entity ce)
    {
        ce.RbEntity.isKinematic = false;
    }
}
