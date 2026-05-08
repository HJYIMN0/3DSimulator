using UnityEngine;

[CreateAssetMenu(fileName = "State_Customer_Leaving", menuName = "States/Agent/Leaving")]

public class State_Customer_Leaving : StateEntity_SO
{
    public float targetReachedTolerance = 1f;
    public float dynamicTargetSearch = 0.5f;
    public bool runOnGoToDestination = false;
    public bool isDynamicTarget = true;

    public override void Enter(Controller_Entity ce)
    {
        ce.IsRunning = runOnGoToDestination;
        ce.SetMoving(ce.GroundDetector.IsGrounded);
        if (targetReachedTolerance < 1) targetReachedTolerance = 1;

        ce.SetDestinationTarget(ce.CustomersBaseLogic.SpawnPoints[Random.Range(0, ce.CustomersBaseLogic.SpawnPoints.Count)]);
        ce.SetAgentDestinationTimer(dynamicTargetSearch, isDynamicTarget);
        PlayAnimationClip(ce, true, 1);
    }

    public override void StateFixedUpdate(Controller_Entity ce)
    {
        if (!ce.Target) return;

        Vector3 currentFlatPos = new Vector3(ce.transform.position.x, 0, ce.transform.position.z);
        Vector3 targetFlatPos = new Vector3(ce.Target.position.x, 0, ce.Target.position.z);
        float distance = Vector3.Distance(currentFlatPos, targetFlatPos);

        ce.MovingAgent();

        if (distance <= targetReachedTolerance)
        {
            ce.CustomersBaseLogic.ReturnToList(ce);
            return;
        }
    }
}
