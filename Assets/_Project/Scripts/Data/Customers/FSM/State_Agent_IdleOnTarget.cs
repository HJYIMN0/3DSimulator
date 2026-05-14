using UnityEngine;

[CreateAssetMenu(fileName = "State_Agent_IdleOnTarget", menuName = "States/Agent/IdleOnTarget")]
public class State_Agent_IdleOnTarget : StateEntity_SO
{
    [Header("Settings Return")]
    public float MaxDriftDistance = 2f;
    public float TargetReachedTolerance = 1f;
    public bool RunOnReturn = false;

    [Space(3)]

    [Header("Settings Animation")]
    public AnimationClip[] clipsUpperIdle;
    public bool ResetAnimationOnExit = true;
    public bool IsLoopedAnimation = true;

    public override void Enter(Controller_Entity ce)
    {
        ce.IsRunning = RunOnReturn;
        ce.SetMoving(ce.GroundDetector.IsGrounded);

        ce.AnchorPosition = ce.GroundDetector.PointGround.position;
        ce.IsReturning = false;

        ce.Agent.isStopped = true;
        ce.Agent.enabled = false;
        ce.Obstacle.enabled = true;

        if (!ce.Animator_Generic) return;

        ce.Animator_Generic.SelectAnimation(true, Parameters.ParameterTriggerOnIdle);

        if (clipsUpperIdle != null && clipsUpperIdle.Length > 0) 
        {
            if (IsLoopedAnimation) ce.Animator_Generic.PlayLoopAction(clipsUpperIdle, true, 1);
            else ce.Animator_Generic.PlaySingleAction(clipsUpperIdle, true, 1);
        }
    }

    public override void StateUpdate(Controller_Entity ce)
    {
        Vector3 currentFlatPos = new Vector3(ce.transform.position.x, 0, ce.transform.position.z);
        Vector3 anchorFlatPos = new Vector3(ce.AnchorPosition.x, 0, ce.AnchorPosition.z);

        float drift = Vector3.Distance(currentFlatPos, anchorFlatPos);

        if (!ce.IsReturning && drift > MaxDriftDistance)
        {
            if (ce.IsReturning) return;

            ce.IsReturning = true;
            ce.Obstacle.enabled = false;
            ce.Agent.enabled = true;
            ce.Agent.isStopped = false;
            ce.Agent.SetDestination(ce.AnchorPosition);
            ce.SelectMovingAnimation();

            if (ResetAnimationOnExit) ce.Animator_Generic.ResetAnimatios();
        }

        if (ce.IsReturning && drift <= 1f)
        {
            if (!ce.IsReturning) return;

            ce.IsReturning = false;
            ce.Agent.isStopped = true;
            ce.Agent.ResetPath();
            ce.Agent.enabled = false;
            ce.Obstacle.enabled = true;

            if (!ce.Animator_Generic) return;
            ce.Animator_Generic.SelectAnimation(true, Parameters.ParameterTriggerOnIdle);

            if (clipsUpperIdle != null && clipsUpperIdle.Length > 0 && ResetAnimationOnExit)
            {
                if (IsLoopedAnimation) ce.Animator_Generic.PlayLoopAction(clipsUpperIdle, true, 1);
                else ce.Animator_Generic.PlaySingleAction(clipsUpperIdle, true, 1);
            }
        }
    }

    public override void StateFixedUpdate(Controller_Entity ce)
    {
        if (!ce.IsReturning) ce.IdleRotateTowardsTarget();
        else ce.MovingAgent();
    }

    public override void Exit(Controller_Entity ce)
    {
        ce.Obstacle.enabled = false;
        ce.Agent.enabled = true;
        ce.Agent.isStopped = false;
        ce.IsReturning = false;

        if (ResetAnimationOnExit) ce.Animator_Generic.ResetAnimatios();
    }
}
