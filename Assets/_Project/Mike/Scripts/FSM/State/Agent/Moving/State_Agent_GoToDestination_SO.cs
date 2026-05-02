using UnityEngine;

[CreateAssetMenu(fileName = "State_AgentGoToDestination", menuName = "States/Agent/GoToDestination")]
public class State_Agent_GoToDestination_SO : StateEntity_SO
{
    public StateEntity_SO StateCondicionDynamicDistance;
    public StateEntity_SO OnArrivedToDestination;

    public float targetReachedTolerance = 1f;
    public float dynamicTargetSearch = 0.5f;
    public float dynamicDistanceTolerance = 0f;
    public bool runOnGoToDestination = false;
    public bool isDynamicTarget = true;

    public override void Enter(Controller_Entity ce)
    {
        ce.IsRunning = runOnGoToDestination;
        ce.SetMoving(ce.GroundDetector.IsGrounded);
        if(targetReachedTolerance < 1) targetReachedTolerance = 1;

        if (!ce.Target) return;
        ce.SetAgentDestinationTimer(dynamicTargetSearch, isDynamicTarget);
    }

    public override void StateFixedUpdate(Controller_Entity ce)
    {
        ce.MovingAgent();
        if (!ce.Target) return;

        Vector3 currentFlatPos = new Vector3(ce.transform.position.x, 0, ce.transform.position.z);
        Vector3 targetFlatPos = new Vector3(ce.Target.position.x, 0, ce.Target.position.z);
        float distance = Vector3.Distance(currentFlatPos, targetFlatPos);

        if (OnArrivedToDestination && distance <= targetReachedTolerance)
        {
            ce.ChangeState(OnArrivedToDestination);
            return;
        }

        if (!StateCondicionDynamicDistance || !ce.Target) return;
        if (runOnGoToDestination)
        {
            if (distance < dynamicDistanceTolerance)
            {
                ce.ChangeState(StateCondicionDynamicDistance);
            }
        }
        else
        {
            if (distance > dynamicDistanceTolerance)
            {
                ce.ChangeState(StateCondicionDynamicDistance);
            }
        }
    }
}
