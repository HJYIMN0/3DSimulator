using UnityEngine;

[CreateAssetMenu(fileName = "State_Agent_IdleOnTarget", menuName = "States/Agent/IdleOnTarget")]
public class State_Agent_IdleOnTarget : StateEntity_SO
{
    [Header("Impostazioni Ritorno")]
    public float maxDistance = 3f;
    public StateEntity_SO nextState;  

    public override void Enter(Controller_Entity ce)
    {
        ce.SetMoving(ce.GroundDetector.IsGrounded);
        ce.AnchorPosition = ce.GroundDetector.PointGround.position;

        ce.Agent.ResetPath();
        ce.Agent.isStopped = true;
        ce.Agent.enabled = false;
        ce.Obstacle.enabled = true;

        if (ce.Animator_Generic) ce.Animator_Generic.SelectAnimation(true, Parameters.ParameterTriggerOnIdle);
    }

    public override void StateUpdate(Controller_Entity ce)
    {
        if (!ce.Target) return;

        Vector3 currentFlatPos = new Vector3(ce.Target.position.x, 0, ce.Target.position.z);
        Vector3 anchorFlatPos = new Vector3(ce.AnchorPosition.x, 0, ce.AnchorPosition.z);

        float distance = Vector3.Distance(currentFlatPos, anchorFlatPos);

        if (distance > maxDistance)
        {
            if(nextState) ce.ChangeState(nextState);
        }
    }

    public override void StateFixedUpdate(Controller_Entity ce)
    {
        ce.IdleRotateTowardsTarget();
    }

    public override void Exit(Controller_Entity ce)
    {
        ce.Obstacle.enabled = false;
        ce.Agent.enabled = true;
        ce.Agent.isStopped = false;
        ce.IsReturning = false;
    }
}
