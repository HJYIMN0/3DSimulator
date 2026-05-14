using UnityEngine;

[CreateAssetMenu(fileName = "State_Agent_RandomRoaming", menuName = "States/Agent/RandomRoaming")]

public class State_Agent_RandomRoaming_SO : StateEntity_SO
{
    public AnimationClip AnimationOnStart;
    public float TargetReachedTolerance = 1.5f;
    public bool RunWhilePatrolling = false;

    public override void Enter(Controller_Entity ce)
    {
        ce.IsRunning = RunWhilePatrolling;
        ce.SetMoving(true);

        Transform randomPoint = ce.PointsRoaming[Random.Range(0, ce.PointsRoaming.Count)];

        if (randomPoint)
        {
            ce.SetDestinationTarget(randomPoint);
            ce.SetAgentDestinationTimer(0.5f, true);
        }
    }

    public override void StateFixedUpdate(Controller_Entity ce)
    {
        if (!ce.Target) return;

        Vector3 currentFlatPos = new Vector3(ce.transform.position.x, 0, ce.transform.position.z);
        Vector3 targetFlatPos = new Vector3(ce.Target.position.x, 0, ce.Target.position.z);
        float distance = Vector3.Distance(currentFlatPos, targetFlatPos);

        ce.MovingAgent();

        if (distance <= TargetReachedTolerance)
        {
            Transform nextPoint = ce.PointsRoaming[Random.Range(0, ce.PointsRoaming.Count)];
            if (nextPoint != null) ce.SetDestinationTarget(nextPoint);
        }
    }
}
