using UnityEngine;
[CreateAssetMenu(fileName = "State_AgentRagdoll", menuName = "States/Agent/Ragdoll")]

public class State_Agent_Ragdoll_SO : StateEntity_SO
{
    [Header("Setting CheckForStandUp")]
    [SerializeField] protected float timeForTryToStandUp = 5f;
    [SerializeField] private float timeToResetBones = 0.5f;
    [SerializeField] private StateEntity_SO selectState;

    public override void Enter(Controller_Entity ce)
    {
        ce.IsImpacted = true;

        ce.Agent.enabled = false;
        ce.IKAnimator.ClearAllLimbs();

        ce.Generic_RagDoll.IsStandingUp = false;
        ce.Generic_RagDoll.ElepsedResetBones = 0f;
        ce.Generic_RagDoll.TimerForWakeUp = 0f;
        ce.Generic_RagDoll.TimerForTryToStandUp = 0f;
        ce.Generic_RagDoll.EnableRagdoll();
    }

    public override void StateUpdate(Controller_Entity ce)
    {
        if (!ce.Generic_RagDoll.IsStandingUp)
        {
            ce.Generic_RagDoll.TimerForWakeUp += Time.deltaTime;

            if (ce.Generic_RagDoll.TimerForWakeUp >= timeForTryToStandUp)
            {
                bool isStoppedFalling = ce.Animator_Generic.GetComponentInChildren<Rigidbody>().linearVelocity.y >= -1.5f;

                if (isStoppedFalling)
                {
                    if (ce.Generic_RagDoll.ElepsedResetBones == 0) ce.Generic_RagDoll.AllingPositionToHips();

                    ce.Generic_RagDoll.ElepsedResetBones += Time.deltaTime;
                    float percent = ce.Generic_RagDoll.ElepsedResetBones / timeToResetBones;

                    ce.Generic_RagDoll.ResetBones(percent);

                    if (percent <= 1) return;
                    ce.Generic_RagDoll.IsStandingUp = true;
                }
                else
                {
                    ce.Generic_RagDoll.IsStandingUp = false;
                    ce.Generic_RagDoll.ElepsedResetBones = 0f;
                    ce.Generic_RagDoll.TimerForWakeUp = 0f;
                    ce.Generic_RagDoll.TimerForTryToStandUp = 0f;
                }
            }
        }
        else
        {
            if (ce.Generic_RagDoll.TimerForTryToStandUp <= 0f)
            {
                ce.Generic_RagDoll.DisableRagdoll();
                ce.Generic_RagDoll.GetUp();
            }

            ce.Generic_RagDoll.TimerForTryToStandUp += Time.deltaTime;
            if (ce.Animator_Generic.Animator.GetCurrentAnimatorStateInfo(0).length < 0.1f) return;

            if (ce.Generic_RagDoll.TimerForTryToStandUp > ce.Animator_Generic.Animator.GetCurrentAnimatorStateInfo(0).length)
            {
                ce.ChangeState(selectState);
            }
        }
    }

    public override void Exit(Controller_Entity ce)
    {
        ce.IsImpacted = false;

        ce.Agent.enabled = true;
        ce.Generic_RagDoll.IsStandingUp = false;
        ce.Generic_RagDoll.ElepsedResetBones = 0f;
        ce.Generic_RagDoll.TimerForWakeUp = 0f;
        ce.Generic_RagDoll.TimerForTryToStandUp = 0f;
    }
}
